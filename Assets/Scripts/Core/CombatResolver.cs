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

    private struct AttackIntent
    {
        public CardInstance attacker, defender;
        public int atkIdx, defIdx;
        public bool isPlayerAttacking;
        public List<CardInstance> atkBoard, defBoard;

        public AttackIntent(CardInstance a, CardInstance d, int ai, int di, bool ip,
                            List<CardInstance> ab, List<CardInstance> db)
        { attacker = a; defender = d; atkIdx = ai; defIdx = di; isPlayerAttacking = ip; atkBoard = ab; defBoard = db; }
    }

    private readonly Stack<DeathEvent> deathStack = new Stack<DeathEvent>();

    // ==========================================
    // PUBLIC API — gọi từ CardSlot / GameManager
    // ==========================================

    public void TriggerAbility(TriggerType trigger, CardInstance source, CardInstance directEnemy,
        List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
        => engine.TriggerAbility(trigger, source, directEnemy, allyBoard, enemyBoard);

    public void ApplySpellToUnit(CardInstance spell, CardInstance targetUnit)
        => engine.ApplySpellToUnit(spell, targetUnit);

    // ==========================================
    // COMBAT FLOW
    // ==========================================

    public void ResolveTurn(List<CardInstance> pBoard, List<CardInstance> eBoard, TurnRecord log)
    {
        deathStack.Clear();
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
            log.AddAction(CombatAction.StatChange(slotIdx, isPlayerSide, unit.currentATK, unit.currentHP, flash));
        });

        // --- Phase 1: Setup ---
        ApplyTribeSynergies(pBoard, true, log);
        ApplyTribeSynergies(eBoard, false, log);

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
            // Bước A: Build Stack tấn công xen kẽ P[0], E[0], P[1], E[1], ...
            Stack<AttackIntent> attackStack = BuildAttackStack(pBoard, eBoard);

            if (attackStack.Count == 0)
            {
                Debug.Log("<color=gray>[COMBAT]</color> Trận đấu hòa: Không còn đòn tấn công hợp lệ!");
                break;
            }

            // Bước B: Pop từng intent ra khỏi Stack và thực thi
            //         Nếu attacker đã chết → skip
            //         Nếu defender chết trước → tìm mục tiêu thay thế
            //         Flush Death Stack ngay sau mỗi đòn → deathrattle/reborn xử lý tức thì
            while (attackStack.Count > 0)
            {
                AttackIntent intent = attackStack.Pop();
                if (intent.attacker.IsDead) continue;

                CardInstance defender = intent.defender;
                int defIdx = intent.defIdx;
                if (defender.IsDead)
                {
                    defender = FindTarget(intent.defBoard, intent.atkIdx);
                    if (defender == null) continue;
                    defIdx = intent.defBoard.IndexOf(defender);
                }

                ExecuteClash(intent.attacker, defender,
                             intent.atkIdx, defIdx,
                             log, intent.isPlayerAttacking,
                             intent.atkBoard, intent.defBoard);

                ScanAllBoardsForNewDeaths(pBoard, eBoard);
                // Flush ngay sau mỗi đòn: deathrattle / reborn / OnAllyDeath chain xử lý tức thì,
                // không chờ đến cuối round — tránh unit đã chết vẫn "block slot" cho round sau
                FlushDeathStack(pBoard, eBoard);

                // BUG FIX: Early-exit ngay khi một bên bị xóa sổ giữa round,
                // tránh pop thêm AttackIntent thừa rồi skip từng cái.
                if (IsSideEliminated(pBoard) || IsSideEliminated(eBoard)) break;
            }

            if (IsSideEliminated(pBoard) || IsSideEliminated(eBoard)) break;
        }

        RecordSnapshots(pBoard, eBoard, log);
        engine.SetSummonObserver(null);
        engine.SetStatChangeObserver(null);
    }

    // ==========================================
    // ATTACK STACK BUILDER
    // Layout: Frontline = index 0-3 (slot 1-4), Backline = index 4-6 (slot 5-7).
    // Thứ tự tấn công: Frontline (0→3) trước, Backline (4→6) sau.
    // Stack LIFO → push ngược để pop ra đúng thứ tự.
    // ==========================================

    private Stack<AttackIntent> BuildAttackStack(List<CardInstance> pBoard, List<CardInstance> eBoard)
    {
        var stack = new Stack<AttackIntent>();
        int slotCount = Mathf.Min(pBoard.Count, eBoard.Count);

        // Push ngược (slot cuối → 0) để pop ra 0,1,2,...,6
        for (int slot = slotCount - 1; slot >= 0; slot--)
        {
            if (eBoard[slot] != null && !eBoard[slot].IsDead && eBoard[slot].currentATK > 0)
            {
                CardInstance target = FindTarget(pBoard, slot);
                if (target != null)
                    stack.Push(new AttackIntent(eBoard[slot], target, slot, pBoard.IndexOf(target), false, eBoard, pBoard));
            }
            if (pBoard[slot] != null && !pBoard[slot].IsDead && pBoard[slot].currentATK > 0)
            {
                CardInstance target = FindTarget(eBoard, slot);
                if (target != null)
                    stack.Push(new AttackIntent(pBoard[slot], target, slot, eBoard.IndexOf(target), true, pBoard, eBoard));
            }
        }
        return stack;
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
            Debug.Log($"<color=cyan>[SAFEGUARD]</color> {defender.Data.cardName} chặn đòn tấn công!");
        }
        if (attacker.safeguardActive)
        {
            dmgToAttacker = 0;
            attacker.safeguardActive = false;
            Debug.Log($"<color=cyan>[SAFEGUARD]</color> {attacker.Data.cardName} chặn phản đòn!");
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

        Debug.Log($"<color=white>[CLASH]</color> {attacker.Data.cardName} đấm {defender.Data.cardName}. " +
                  $"HP Atk: {aBefore}->{attacker.currentHP}, HP Def: {dBefore}->{defender.currentHP}");

        var act = new CombatAction(atkIdx, defIdx, isPlayerAttacking,
            attacker.Data.cardName, defender.Data.cardName,
            aBefore, attacker.currentHP, dBefore, defender.currentHP);

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
        // Lặp cho đến khi cả death stack lẫn CleanupBoard không còn sinh ra death mới
        bool hasMore = true;
        while (hasMore)
        {
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
                    // BUG FIX: OnAllyDeath chỉ kích hoạt khi đồng minh CÙNG BỘ TỘC chết
                    // (theo đặc tả trong AbilityData.cs enum comment)
                    if (ally.Data.tribe != evt.victim.Data.tribe) continue;
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
        for (int i = 0; i < allyBoard.Count; i++)
        {
            var unit = allyBoard[i];
            if (unit == null || !unit.IsDead) continue;

            if (unit.isReborn && !unit.hasRebornUsed)
            {
                unit.ReviveDefault();
                Debug.Log($"<color=magenta>[REBORN]</color> {unit.Data.cardName} hồi sinh với chỉ số mặc định (ATK {unit.currentATK} / HP {unit.currentHP})!");
                engine.BroadcastAllyEvent(TriggerType.OnAllyReborn, unit, allyBoard, enemyBoard);
                // BUG FIX: OnAllyReborn có thể kill unit ở slot sau trong cùng pass này.
                // Scan ngay để đăng ký vào death stack — tránh CleanupBoard null chúng trước khi
                // OnDeath/OnAllyDeath kịp fire.
                ScanAllBoardsForNewDeaths(allyBoard, enemyBoard);
            }
            else if (unit.onDeathProcessed)
            {
                // BUG FIX: Chỉ null unit đã qua death stack. Unit chết từ BroadcastAllyReborn
                // (onDeathProcessed=false) được giữ lại để ScanAllBoardsForNewDeaths bắt được,
                // FlushDeathStack xử lý OnDeath/OnAllyDeath, rồi CleanupBoard pass sau mới null.
                allyBoard[i] = null;
            }
        }
    }

    // ==========================================
    // TRIBE SYNERGIES
    // Babylon  ≥2: +1 HP  — nền văn minh, phòng thủ vững chắc
    // Olympus  ≥2: +1 ATK — thần linh, công kích thần thánh
    // Niles    ≥3: +2 HP  — dòng sông sự sống, hồi phục mạnh hơn nhưng yêu cầu đội hình lớn hơn
    // ==========================================

    private void ApplyTribeSynergies(List<CardInstance> board, bool isPlayerSide, TurnRecord log)
    {
        var alive = board.FindAll(u => u != null && !u.IsDead);

        // Babylon ≥2 → +1 HP
        int babylonCount = alive.FindAll(u => u.Data.tribe == Tribe.Babylon).Count;
        if (babylonCount >= 2)
        {
            foreach (var u in alive)
            {
                if (u.Data.tribe != Tribe.Babylon) continue;
                u.currentHP += 1; u.maxHP += 1;
                int idx = board.IndexOf(u);
                if (idx >= 0) log?.AddAction(CombatAction.StatChange(idx, isPlayerSide, u.currentATK, u.currentHP, FlashType.SynergyBabylon));
            }
            Debug.Log($"<color=yellow>[SYNERGY]</color> Babylon x{babylonCount}: mỗi Babylon unit +1 HP");
        }

        // Olympus ≥2 → +1 ATK
        int olympusCount = alive.FindAll(u => u.Data.tribe == Tribe.Olympus).Count;
        if (olympusCount >= 2)
        {
            foreach (var u in alive)
            {
                if (u.Data.tribe != Tribe.Olympus) continue;
                u.currentATK += 1;
                int idx = board.IndexOf(u);
                if (idx >= 0) log?.AddAction(CombatAction.StatChange(idx, isPlayerSide, u.currentATK, u.currentHP, FlashType.SynergyOlympus));
            }
            Debug.Log($"<color=cyan>[SYNERGY]</color> Olympus x{olympusCount}: mỗi Olympus unit +1 ATK");
        }

        // Niles ≥3 → +2 HP
        int nilesCount = alive.FindAll(u => u.Data.tribe == Tribe.Niles).Count;
        if (nilesCount >= 3)
        {
            foreach (var u in alive)
            {
                if (u.Data.tribe != Tribe.Niles) continue;
                u.currentHP += 2; u.maxHP += 2;
                int idx = board.IndexOf(u);
                if (idx >= 0) log?.AddAction(CombatAction.StatChange(idx, isPlayerSide, u.currentATK, u.currentHP, FlashType.SynergyNiles));
            }
            Debug.Log($"<color=green>[SYNERGY]</color> Niles x{nilesCount}: mỗi Niles unit +2 HP");
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

    private int ComputeRevivedHP(CardInstance unit)
    {
        const float keepRatio = 0.7f;
        int tier = unit.mergeLevel + 1;
        return Mathf.RoundToInt(unit.Data.baseHP * tier
               + keepRatio * (unit.growthHPBonus + unit.permanentHPBonus));
    }

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
