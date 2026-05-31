using System.Collections.Generic;
using UnityEngine;

public class CombatResolver
{
    private readonly AbilityEngine engine = new AbilityEngine();

    // ==========================================
    // DEATH STACK — trung tâm xử lý mọi loại cái chết
    // ==========================================

    private struct DeathEvent
    {
        public CardInstance victim;
        public CardInstance killer;          // lastAttacker của victim (có thể null với AOE)
        public List<CardInstance> victimBoard;
        public List<CardInstance> killerBoard;

        public DeathEvent(CardInstance v, CardInstance k, List<CardInstance> vb, List<CardInstance> kb)
        { victim = v; killer = k; victimBoard = vb; killerBoard = kb; }
    }

    private struct AttackEntry
    {
        public CardInstance attacker;
        public int atkSlot;
        public List<CardInstance> atkBoard, defBoard;
    }

    private readonly Stack<DeathEvent> deathStack = new Stack<DeathEvent>();

    // Dùng trong ResolveTurn để CleanupBoard có thể log Death action khi cần
    private TurnRecord              _currentLog;
    private List<CardInstance>      _pBoard;
    private readonly HashSet<CardInstance> _clashDeaths    = new HashSet<CardInstance>();
    private readonly HashSet<CardInstance> _revivedThisFlush = new HashSet<CardInstance>();

    // ==========================================
    // PUBLIC API — gọi từ CardSlot / GameManager
    // ==========================================

    public void TriggerAbility(TriggerType trigger, CardInstance source, CardInstance directEnemy,
        List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
        => engine.TriggerAbility(trigger, source, directEnemy, allyBoard, enemyBoard);

    public void BroadcastAllyEvent(TriggerType trigger, CardInstance subject,
        List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
        => engine.BroadcastAllyEvent(trigger, subject, allyBoard, enemyBoard);

    public void SetSummonObserver(System.Action<CardInstance, List<CardInstance>> observer)
        => engine.SetSummonObserver(observer);

    public void SetStatChangeObserver(System.Action<CardInstance, List<CardInstance>, FlashType> observer)
        => engine.SetStatChangeObserver(observer);

    // Xử lý toàn bộ pending summons ngay lập tức — dùng cho shop phase
    // (combat dùng FlushDeathStack để đảm bảo death chain resolve trước)
    public void FlushShopPendingSummons(List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        while (engine.HasPendingSummons)
            engine.ProcessNextPendingSummon();
    }

    public void ApplySpellToUnit(CardInstance spell, CardInstance targetUnit)
        => engine.ApplySpellToUnit(spell, targetUnit);

    // ==========================================
    // COMBAT FLOW
    // ==========================================

    public void ResolveTurn(List<CardInstance> pBoard, List<CardInstance> eBoard, TurnRecord log)
    {
        _currentLog = log;
        _pBoard     = pBoard;
        _clashDeaths.Clear();
        deathStack.Clear();
        engine.SetCombatPhase(true);
        engine.ClearPendingSummons();
        engine.SetSummonObserver((unit, board) =>
        {
            if (unit == null || board == null || log == null) return;
            bool isPlayerSide = ReferenceEquals(board, pBoard);
            log.AddAction(CombatAction.Summon(unit.slotIndex, isPlayerSide, unit));
        });
        engine.SetStatChangeObserver((unit, board, flash) =>
        {
            if (unit == null || board == null || log == null) return;
            bool isPlayerSide = ReferenceEquals(board, pBoard);
            int slotIdx = board.IndexOf(unit);
            if (slotIdx < 0) return;
            log.AddAction(CombatAction.StatChange(slotIdx, isPlayerSide, unit.currentATK, unit.currentHP, flash,
                unit.isReborn, unit.isTaunt, unit.safeguardActive));
        });

        // --- Phase 1: Setup ---
        int setupSlotCount = Mathf.Min(pBoard.Count, eBoard.Count);
        for (int i = 0; i < setupSlotCount; i++)
        {
            if (pBoard[i] != null && !pBoard[i].IsDead)
            {
                engine.TriggerAbility(TriggerType.StartOfBattle, pBoard[i], null, pBoard, eBoard);
                engine.TriggerAbility(TriggerType.Aura,          pBoard[i], null, pBoard, eBoard);
            }
            if (eBoard[i] != null && !eBoard[i].IsDead)
            {
                engine.TriggerAbility(TriggerType.StartOfBattle, eBoard[i], null, eBoard, pBoard);
                engine.TriggerAbility(TriggerType.Aura,          eBoard[i], null, eBoard, pBoard);
            }
        }

        // Xử lý cái chết phát sinh từ StartOfBattle/Aura (ví dụ: DealDamage AOE)
        ScanAllBoardsForNewDeaths(pBoard, eBoard);
        FlushDeathStack(pBoard, eBoard);

        // --- Phase 2: Battle Loop (tối đa 50 round) ---
        for (int round = 0; round < 50; round++)
        {
            // Bước A: Build queue tấn công theo slot tăng dần (slot 0 → 6).
            // Mỗi slot: enemy trước, player sau — giữ đúng thứ tự hiện tại.
            List<AttackEntry> attackQueue = BuildInitialAttackQueue(pBoard, eBoard);
            if (attackQueue.Count == 0)
            {
                if (!Application.isBatchMode) Debug.Log("<color=gray>[COMBAT]</color> Trận đấu hòa: Không còn đòn tấn công hợp lệ!");
                break;
            }

            // queued: ngăn unit đánh 2 lần trong cùng round
            var queued = new HashSet<CardInstance>();
            foreach (var e in attackQueue) queued.Add(e.attacker);

            // Bước B: Lần lượt từng entry trong queue.
            //   - FindTarget gọi động → phản ánh board hiện tại (Taunt/Frontline đúng lúc).
            //   - Sau FlushDeathStack: unit mới summon/reborn được insert đúng vị trí slot.
            int qi = 0;
            while (qi < attackQueue.Count)
            {
                AttackEntry entry = attackQueue[qi++];
                if (entry.attacker.IsDead) continue;

                CardInstance defender = FindTarget(entry.defBoard, entry.atkSlot);
                if (defender == null) continue;

                bool isPlayerAttacking = ReferenceEquals(entry.atkBoard, pBoard);
                ExecuteClash(entry.attacker, defender,
                             entry.atkSlot, entry.defBoard.IndexOf(defender),
                             log, isPlayerAttacking,
                             entry.atkBoard, entry.defBoard);

                ScanAllBoardsForNewDeaths(pBoard, eBoard);
                _revivedThisFlush.Clear();
                FlushDeathStack(pBoard, eBoard);

                // Reborn unit đã "chết rồi sống lại" → cho phép vào queue lại
                foreach (var u in _revivedThisFlush) queued.Remove(u);

                // Insert unit mới (summon / reborn) vào đúng vị trí slot trong phần chưa xử lý
                InsertNewUnits(attackQueue, queued, pBoard, eBoard, qi);

                if (IsSideEliminated(pBoard) || IsSideEliminated(eBoard)) break;
            }

            if (IsSideEliminated(pBoard) || IsSideEliminated(eBoard)) break;
        }

        RecordSnapshots(pBoard, eBoard, log);
        engine.SetSummonObserver(null);
        engine.SetStatChangeObserver(null);
        engine.SetCombatPhase(false);
        _currentLog = null;
        _pBoard     = null;
    }

    // ==========================================
    // ATTACK QUEUE BUILDER
    // Thứ tự: slot tăng dần (0→6). Cùng slot: enemy trước, player sau.
    // Target KHÔNG được fix sẵn — FindTarget gọi động lúc execute để luôn
    // phản ánh board hiện tại (Taunt / Frontline mới summon che đúng lúc).
    // ==========================================

    private List<AttackEntry> BuildInitialAttackQueue(List<CardInstance> pBoard, List<CardInstance> eBoard)
    {
        var queue = new List<AttackEntry>();
        int slotCount = Mathf.Min(pBoard.Count, eBoard.Count);
        for (int slot = 0; slot < slotCount; slot++)
        {
            if (eBoard[slot] != null && !eBoard[slot].IsDead && eBoard[slot].currentATK > 0)
                queue.Add(new AttackEntry { attacker = eBoard[slot], atkSlot = slot, atkBoard = eBoard, defBoard = pBoard });
            if (pBoard[slot] != null && !pBoard[slot].IsDead && pBoard[slot].currentATK > 0)
                queue.Add(new AttackEntry { attacker = pBoard[slot], atkSlot = slot, atkBoard = pBoard, defBoard = eBoard });
        }
        return queue;
    }

    // Unit mới xuất hiện trên board (summon / reborn) được insert vào đúng vị trí slot
    // trong phần chưa xử lý của queue (index >= nextIndex), không đánh 2 lần/round.
    private void InsertNewUnits(List<AttackEntry> queue, HashSet<CardInstance> queued,
        List<CardInstance> pBoard, List<CardInstance> eBoard, int nextIndex)
    {
        // Thu thập trước rồi insert — tránh modify queue trong lúc scan
        var toInsert = new List<AttackEntry>();
        int slotCount = Mathf.Min(pBoard.Count, eBoard.Count);
        for (int slot = 0; slot < slotCount; slot++)
        {
            var eu = eBoard[slot];
            if (eu != null && !eu.IsDead && eu.currentATK > 0 && !queued.Contains(eu))
            {
                toInsert.Add(new AttackEntry { attacker = eu, atkSlot = slot, atkBoard = eBoard, defBoard = pBoard });
                queued.Add(eu);
            }
            var pu = pBoard[slot];
            if (pu != null && !pu.IsDead && pu.currentATK > 0 && !queued.Contains(pu))
            {
                toInsert.Add(new AttackEntry { attacker = pu, atkSlot = slot, atkBoard = pBoard, defBoard = eBoard });
                queued.Add(pu);
            }
        }

        // toInsert đã sắp xếp theo slot tăng dần (vì loop slot 0→N).
        // Mỗi entry được insert vào vị trí đúng trong phần chưa xử lý.
        foreach (var entry in toInsert)
        {
            int insertAt = queue.Count;
            for (int i = nextIndex; i < queue.Count; i++)
            {
                if (entry.atkSlot < queue[i].atkSlot)
                {
                    insertAt = i;
                    break;
                }
            }
            queue.Insert(insertAt, entry);
        }
    }

    // ==========================================
    // EXECUTE CLASH
    // ==========================================

    private void ExecuteClash(CardInstance attacker, CardInstance defender,
        int atkIdx, int defIdx, TurnRecord log, bool isPlayerAttacking,
        List<CardInstance> atkBoard, List<CardInstance> defBoard)
    {
        int aBefore = attacker.currentHP;
        int dBefore = defender.currentHP;

        int dmgToDefender = attacker.currentATK;
        int dmgToAttacker = defender.currentATK;

        // Track kẻ tấn công để death stack dùng làm directEnemy trong OnDeath
        defender.lastAttacker = attacker;
        attacker.lastAttacker = defender; // attacker có thể chết từ counterattack

        if (defender.safeguardActive)
        {
            dmgToDefender = 0;
            defender.safeguardActive = false;
            if (!Application.isBatchMode) Debug.Log($"<color=cyan>[SAFEGUARD]</color> {defender.Data.cardName} chặn đòn tấn công!");
        }
        if (attacker.safeguardActive)
        {
            dmgToAttacker = 0;
            attacker.safeguardActive = false;
            if (!Application.isBatchMode) Debug.Log($"<color=cyan>[SAFEGUARD]</color> {attacker.Data.cardName} chặn phản đòn!");
        }

        defender.currentHP -= dmgToDefender;
        attacker.currentHP -= dmgToAttacker;

        // BUG FIX: OnAttack chỉ kích nếu attacker còn sống
        // (tránh unit chết từ counterattack vẫn gây effect lên kẻ địch)
        if (!attacker.IsDead)
            engine.TriggerAbility(TriggerType.OnAttack, attacker, defender, atkBoard, defBoard);

        // OnTakeDamage chỉ kích khi unit còn sống sau đòn — unit chết không còn "phản ứng"
        if (dmgToAttacker > 0 && !attacker.IsDead)
            engine.TriggerAbility(TriggerType.OnTakeDamage, attacker, defender, atkBoard, defBoard);
        if (dmgToDefender > 0 && !defender.IsDead)
            engine.TriggerAbility(TriggerType.OnTakeDamage, defender, attacker, defBoard, atkBoard);

        if (!Application.isBatchMode) Debug.Log($"<color=white>[CLASH]</color> {attacker.Data.cardName} đấm {defender.Data.cardName}. " +
                  $"HP Atk: {aBefore}->{attacker.currentHP}, HP Def: {dBefore}->{defender.currentHP}");

        var act = new CombatAction(atkIdx, defIdx, isPlayerAttacking,
            attacker.Data.cardName, defender.Data.cardName,
            aBefore, attacker.currentHP, dBefore, defender.currentHP);

        // Đánh dấu: death này sẽ được Clash action xử lý — CleanupBoard không cần log thêm Death action
        if (attacker.IsDead) _clashDeaths.Add(attacker);
        if (defender.IsDead) _clashDeaths.Add(defender);

        // Đánh dấu Reborn để visualizer hồi sinh card sau DieAnimation
        if (defender.IsDead && defender.isReborn && !defender.hasRebornUsed)
        {
            act.defenderReborn     = true;
            act.defenderRevivedHP  = ComputeRevivedHP(defender);
        }
        if (attacker.IsDead && attacker.isReborn && !attacker.hasRebornUsed)
        {
            act.attackerReborn     = true;
            act.attackerRevivedHP  = ComputeRevivedHP(attacker);
        }
        log.AddAction(act);
    }

    // ==========================================
    // DEATH STACK SYSTEM
    // BUG FIX: Mọi nguồn damage (trực tiếp + ability) đều đi qua đây,
    // đảm bảo OnDeath / OnAllyDeath / Reborn luôn được xử lý đúng.
    // ==========================================

    private void RegisterDeath(CardInstance victim, List<CardInstance> victimBoard, List<CardInstance> killerBoard)
    {
        if (victim == null || victim.onDeathProcessed) return;
        victim.onDeathProcessed = true;
        deathStack.Push(new DeathEvent(victim, victim.lastAttacker, victimBoard, killerBoard));
    }

    private void ScanAllBoardsForNewDeaths(List<CardInstance> pBoard, List<CardInstance> eBoard)
    {
        int slotCount = Mathf.Min(pBoard.Count, eBoard.Count);
        for (int i = 0; i < slotCount; i++)
        {
            if (pBoard[i] != null && pBoard[i].IsDead && !pBoard[i].onDeathProcessed)
                RegisterDeath(pBoard[i], pBoard, eBoard);
            if (eBoard[i] != null && eBoard[i].IsDead && !eBoard[i].onDeathProcessed)
                RegisterDeath(eBoard[i], eBoard, pBoard);
        }
    }

    private void FlushDeathStack(List<CardInstance> pBoard, List<CardInstance> eBoard)
    {
        int safetyCounter = 0;
        bool hasMore = true;
        while (hasMore && safetyCounter++ < 500)
        {
            if (safetyCounter == 499)
                Debug.LogError("[CombatResolver] FlushDeathStack hit safety limit (500) — infinite loop detected! Check OnAllySummon/Summon chains.");
            // --- Xử lý toàn bộ death stack (LIFO: death mới nhất được xử lý trước) ---
            while (deathStack.Count > 0)
            {
                DeathEvent evt = deathStack.Pop();

                // OnDeath của unit vừa chết (có thể gây deaths mới)
                engine.TriggerAbility(TriggerType.OnDeath, evt.victim, evt.killer,
                                      evt.victimBoard, evt.killerBoard);
                ScanAllBoardsForNewDeaths(pBoard, eBoard);

                // Broadcast OnAllyDeath cho từng đồng minh còn sống
                // Snapshot list tránh modification khi đang duyệt
                var allySnapshot = new List<CardInstance>(evt.victimBoard);
                foreach (var ally in allySnapshot)
                {
                    if (ally == null || ally.IsDead || ally == evt.victim) continue;
                    // OnAllyDeath chỉ kích hoạt khi đồng minh CÙNG BỘ TỘC chết,
                    // TRỪ KHI ability có anyAllyTrigger = true (VD: Anubis react với bất kỳ ally chết)
                    if (ally.Data.tribe != evt.victim.Data.tribe)
                    {
                        bool hasAnyTrigger = ally.Data.abilities != null &&
                            ally.Data.abilities.Exists(a => a.trigger == TriggerType.OnAllyDeath && a.anyAllyTrigger);
                        if (!hasAnyTrigger) continue;
                    }
                    engine.TriggerAbility(TriggerType.OnAllyDeath, ally, evt.victim,
                                          evt.victimBoard, evt.killerBoard);
                    ScanAllBoardsForNewDeaths(pBoard, eBoard);
                }
            }

            // --- CleanupBoard: Apply Reborn hoặc xóa unit ---
            // BroadcastAllyEvent(OnAllyReborn) bên trong có thể gây deaths mới
            CleanupBoard(pBoard, eBoard);
            CleanupBoard(eBoard, pBoard);

            // Quét sau cleanup: OnAllyReborn effects có thể kill thêm
            ScanAllBoardsForNewDeaths(pBoard, eBoard);

            // --- Stack-based summon: pop 1 pending summon sau khi board sạch ---
            // Chỉ xử lý khi death stack đã hết — đảm bảo chain của unit trước
            // (NW1 → Mummy → ...) hoàn toàn resolve trước khi NW2 xuất hiện.
            // Nếu NW2 bị Sekhmet nuốt (gây death mới) → vòng lặp ngoài sẽ tiếp tục.
            // Nếu không còn slot → SummonUnit trả null, NW2 biến mất theo đúng luật.
            if (deathStack.Count == 0 && engine.HasPendingSummons)
            {
                engine.ProcessNextPendingSummon();
                ScanAllBoardsForNewDeaths(pBoard, eBoard);
            }

            hasMore = deathStack.Count > 0 || engine.HasPendingSummons;
        }
    }

    // ==========================================
    // CLEANUP BOARD — chỉ apply Reborn / xóa slot
    // (OnDeath đã xử lý trong FlushDeathStack, không xử lý ở đây)
    // ==========================================

    private void CleanupBoard(List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        bool isPlayerBoard = _pBoard != null && ReferenceEquals(allyBoard, _pBoard);
        for (int i = 0; i < allyBoard.Count; i++)
        {
            var unit = allyBoard[i];
            if (unit == null || !unit.IsDead) continue;

            if (unit.isReborn && !unit.hasRebornUsed)
            {
                unit.ReviveDefault();
                _clashDeaths.Remove(unit);
                _revivedThisFlush.Add(unit); // cho phép unit vào queue lại trong round hiện tại
                if (!Application.isBatchMode) Debug.Log($"<color=magenta>[REBORN]</color> {unit.Data.cardName} hồi sinh với chỉ số mặc định (ATK {unit.currentATK} / HP {unit.currentHP})!");
                engine.BroadcastAllyEvent(TriggerType.OnAllyReborn, unit, allyBoard, enemyBoard);
                // Reborn = đơn vị "xuất hiện lại" → Sekhmet và các listener OnAllySummon cũng phản ứng
                engine.BroadcastAllyEvent(TriggerType.OnAllySummon, unit, allyBoard, enemyBoard);
                // BUG FIX: OnAllyReborn có thể kill unit ở slot sau trong cùng pass này.
                // Scan ngay để đăng ký vào death stack — tránh CleanupBoard null chúng trước khi
                // OnDeath/OnAllyDeath kịp fire.
                ScanAllBoardsForNewDeaths(allyBoard, enemyBoard);
            }
            else if (unit.onDeathProcessed)
            {
                // Nếu unit không chết từ Clash trực tiếp, visualizer chưa có action nào để chạy
                // DieAnimation — log thêm Death action để xóa card khỏi UI.
                if (_currentLog != null && !_clashDeaths.Contains(unit))
                    _currentLog.AddAction(CombatAction.Death(i, isPlayerBoard));
                allyBoard[i] = null;
            }
        }
    }

    // ==========================================
    // TARGET SELECTION
    // Layout: Frontline = index 0-3 | Backline = index 4-6
    // Ưu tiên 1: Taunt — bypass mọi thứ, kể cả frontline protection
    // Ưu tiên 2: Frontline (index 0-3) còn sống
    // Ưu tiên 3: Backline (index 4-6) — chỉ lộ khi TOÀN BỘ frontline đã chết
    // Trong mỗi nhóm: chọn unit gần nhất với vị trí attacker.
    // ==========================================

    private const int FrontlineCount = 4; // index 0-3

    private CardInstance FindTarget(List<CardInstance> board, int attackerSlot)
    {
        // Ưu tiên 1: Taunt — bypass hoàn toàn frontline protection
        var tauntPool = new List<(int slot, CardInstance unit)>();
        for (int i = 0; i < board.Count; i++)
        {
            var u = board[i];
            if (u != null && !u.IsDead && u.isTaunt)
                tauntPool.Add((i, u));
        }
        if (tauntPool.Count > 0) return ClosestTo(tauntPool, attackerSlot);

        // Ưu tiên 2: Frontline còn sống (index 0-3)
        var frontPool = new List<(int slot, CardInstance unit)>();
        for (int i = 0; i < FrontlineCount && i < board.Count; i++)
        {
            if (IsAlive(board, i))
                frontPool.Add((i, board[i]));
        }
        if (frontPool.Count > 0) return ClosestTo(frontPool, attackerSlot);

        // Ưu tiên 3: Backline (index 4-6) — chỉ lộ khi toàn bộ frontline đã chết
        var backPool = new List<(int slot, CardInstance unit)>();
        for (int i = FrontlineCount; i < board.Count; i++)
        {
            if (IsAlive(board, i))
                backPool.Add((i, board[i]));
        }
        return backPool.Count > 0 ? ClosestTo(backPool, attackerSlot) : null;
    }

    private CardInstance ClosestTo(List<(int slot, CardInstance unit)> candidates, int referenceSlot)
    {
        CardInstance best = null;
        int minDist = int.MaxValue;
        foreach (var (slot, unit) in candidates)
        {
            int dist = Mathf.Abs(slot - referenceSlot);
            if (dist < minDist) { minDist = dist; best = unit; }
        }
        return best;
    }

    private bool IsAttackableTarget(List<CardInstance> board, int slot)
    {
        if (slot < 0 || slot >= board.Count) return false;
        var unit = board[slot];
        if (unit == null || unit.IsDead) return false;

        if (slot < FrontlineCount) return true; // Frontline: luôn lộ ra

        // Backline: chỉ lộ khi toàn bộ frontline đã chết
        for (int i = 0; i < FrontlineCount && i < board.Count; i++)
            if (IsAlive(board, i)) return false;
        return true;
    }

    private bool IsAlive(List<CardInstance> board, int slot)
    {
        if (slot < 0 || slot >= board.Count) return false;
        var unit = board[slot];
        return unit != null && !unit.IsDead;
    }

    // ==========================================
    // HELPERS
    // ==========================================

    private int ComputeRevivedHP(CardInstance unit) => 1;

    private bool IsSideEliminated(List<CardInstance> board)
        => !board.Exists(u => u != null && !u.IsDead);

    private void RecordSnapshots(List<CardInstance> pBoard, List<CardInstance> eBoard, TurnRecord log)
    {
        foreach (var unit in pBoard)
            if (unit != null && !unit.IsDead) log.playerBoardFinal.Add(new CardSnapshot(unit.Data.cardID, unit.currentHP, unit.currentATK));
        foreach (var unit in eBoard)
            if (unit != null && !unit.IsDead) log.enemyBoardFinal.Add(new CardSnapshot(unit.Data.cardID, unit.currentHP, unit.currentATK));
    }
}
