using System.Collections.Generic;
using UnityEngine;

public class CombatResolver
{
    public void ResolveTurn(List<CardInstance> pBoard, List<CardInstance> eBoard, TurnRecord log)
    {
        // 1. Kích hoạt Synergy một lần duy nhất lúc đầu (Buff theo Tộc)
        ApplyTribeSynergies(pBoard);
        ApplyTribeSynergies(eBoard);

        // 2. Kích hoạt kỹ năng đầu hiệp & Aura
        for (int i = 0; i < 6; i++)
        {
            if (pBoard[i] != null && !pBoard[i].IsDead)
            {
                TriggerAbility(TriggerType.StartOfBattle, pBoard[i], null, pBoard, eBoard);
                TriggerAbility(TriggerType.Aura, pBoard[i], null, pBoard, eBoard); // Kích hoạt Aura sau Growth để buff kịp thời
            }

            if (eBoard[i] != null && !eBoard[i].IsDead)
            {
                TriggerAbility(TriggerType.StartOfBattle, eBoard[i], null, eBoard, pBoard);
                TriggerAbility(TriggerType.Aura, eBoard[i], null, eBoard, pBoard); // Kích hoạt Aura sau Growth để buff kịp thời
            }
        }

        int maxRounds = 50;
        int currentRound = 0;

        while (currentRound < maxRounds)
        {
            bool actionTakenInThisRound = false;

            for (int i = 0; i < 6; i++)
            {
                // --- PLAYER TẤN CÔNG ---
                if (pBoard[i] != null && !pBoard[i].IsDead && pBoard[i].currentATK > 0)
                {
                    CardInstance target = FindTarget(eBoard, i);
                    if (target != null)
                    {
                        ExecuteClash(pBoard[i], target, i, eBoard.IndexOf(target), log, true, pBoard, eBoard);
                        actionTakenInThisRound = true;
                    }
                }

                // --- ENEMY TẤN CÔNG ---
                if (eBoard[i] != null && !eBoard[i].IsDead && eBoard[i].currentATK > 0)
                {
                    CardInstance target = FindTarget(pBoard, i);
                    if (target != null)
                    {
                        ExecuteClash(eBoard[i], target, i, pBoard.IndexOf(target), log, false, eBoard, pBoard);
                        actionTakenInThisRound = true;
                    }
                }

                // Dọn dẹp xác chết / Kích hoạt Reborn, SlainEffect sau mỗi cặp đấu
                CleanupBoard(pBoard, eBoard);
                CleanupBoard(eBoard, pBoard);
            }

            currentRound++;

            // KIỂM TRA ĐIỀU KIỆN KẾT THÚC
            if (IsSideEliminated(pBoard) || IsSideEliminated(eBoard)) break;

            // TRƯỜNG HỢP HÒA: Không ai có ATK > 0 để tấn công tiếp
            if (!actionTakenInThisRound)
            {
                Debug.Log("<color=gray>[COMBAT]</color> Trận đấu hòa: Không bên nào còn quân có khả năng tấn công!");
                break;
            }
        }

        RecordSnapshots(pBoard, eBoard, log);
    }

    private bool IsSideEliminated(List<CardInstance> board)
    {
        return !board.Exists(u => u != null && !u.IsDead);
    }

    private void ExecuteClash(CardInstance attacker, CardInstance defender, int atkIdx, int defIdx, TurnRecord log, bool isPlayerAttacking, List<CardInstance> atkBoard, List<CardInstance> defBoard)
    {
        // Lưu máu trước khi đấm cho TurnRecord
        int aBefore = attacker.currentHP;
        int dBefore = defender.currentHP;

        // 1. Sát thương vật lý cơ bản (đánh đồng thời)
        int dmgToDefender = attacker.currentATK;
        int dmgToAttacker = defender.currentATK;

        defender.currentHP -= dmgToDefender;
        attacker.currentHP -= dmgToAttacker;

        // 2. Check Triggers sau va chạm
        TriggerAbility(TriggerType.OnAttack, attacker, defender, atkBoard, defBoard);

        // Trigger kỹ năng Khi Bị Đánh (OnTakeDamage)
        if (dmgToAttacker > 0)
            TriggerAbility(TriggerType.OnTakeDamage, attacker, defender, atkBoard, defBoard);
        if (dmgToDefender > 0)
            TriggerAbility(TriggerType.OnTakeDamage, defender, attacker, defBoard, atkBoard);

        // 3. XỬ LÝ CÁI CHẾT VÀ BẮN TIN CHO ĐỒNG MINH (OnAllyDeath)
        HandlePotentialDeath(defender, attacker, defBoard, atkBoard);
        HandlePotentialDeath(attacker, defender, atkBoard, defBoard);

        // Ghi log ra Console và TurnRecord
        Debug.Log($"<color=white>[CLASH]</color> {attacker.Data.cardName} đấm {defender.Data.cardName}. " +
                  $"HP Atk: {aBefore}->{attacker.currentHP}, HP Def: {dBefore}->{defender.currentHP}");

        log.AddAction(new CombatAction(atkIdx, defIdx, isPlayerAttacking,
            attacker.Data.cardName, defender.Data.cardName,
            aBefore, attacker.currentHP, dBefore, defender.currentHP));
    }

    private void HandlePotentialDeath(CardInstance victim, CardInstance killer, List<CardInstance> victimBoard, List<CardInstance> killerBoard)
    {
        if (!victim.IsDead) return;

        // Bản thân victim kích OnDeath
        TriggerAbility(TriggerType.OnDeath, victim, killer, victimBoard, killerBoard);

        // [SYNERGY] Toàn bộ đồng đội nhìn thấy cái chết này và phản ứng
        foreach (var ally in victimBoard)
        {
            if (ally != null && !ally.IsDead && ally != victim)
            {
                TriggerAbility(TriggerType.OnAllyDeath, ally, victim, victimBoard, killerBoard);
            }
        }
    }

    // Giờ chỉ có nhiệm vụ duy nhất: dọn dẹp các thi thể (đã chết và không thể cứu vãn)
    private void CleanupBoard(List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        for (int i = 0; i < allyBoard.Count; i++)
        {
            if (allyBoard[i] != null && allyBoard[i].IsDead)
            {
                // Chỉ dọn dẹp xác chết thực sự.
                allyBoard[i] = null;
            }
        }
    }

    private CardInstance FindTarget(List<CardInstance> board, int prefSlot)
    {
        // Ưu tiên 1: Taunt
        var taunt = board.Find(u => u != null && !u.IsDead && u.isTaunt);
        if (taunt != null) return taunt;

        // Ưu tiên 2: Ripple Search (Loang từ đối diện ra 2 bên)
        for (int d = 0; d < 6; d++)
        {
            int left = prefSlot - d;
            int right = prefSlot + d;

            if (left >= 0 && left < board.Count && board[left] != null && !board[left].IsDead) return board[left];
            if (right >= 0 && right < board.Count && board[right] != null && !board[right].IsDead) return board[right];
        }
        return null;
    }

    private void ApplyTribeSynergies(List<CardInstance> board)
    {
        int babylonCount = board.FindAll(u => u != null && u.Data.tribe == Tribe.Babylon).Count;
        if (babylonCount >= 2)
        {
            foreach (var unit in board)
            {
                if (unit != null && unit.Data.tribe == Tribe.Babylon)
                {
                    unit.currentHP += 1; // Buff HP tạm thời cho combat này, reset sau trận
                }
            }
        }
    }

    private void RecordSnapshots(List<CardInstance> pBoard, List<CardInstance> eBoard, TurnRecord log)
    {
        foreach (var unit in pBoard)
        {
            if (unit != null)
                log.playerBoardFinal.Add(new CardSnapshot(unit.Data.cardID, unit.currentHP, unit.currentATK));
        }

        foreach (var unit in eBoard)
        {
            if (unit != null)
                log.enemyBoardFinal.Add(new CardSnapshot(unit.Data.cardID, unit.currentHP, unit.currentATK));
        }
    }

    // ==========================================
    // HỆ THỐNG ENGINE TTE (TRIGGER - TARGET - EFFECT)
    // ==========================================

    // Public để CardSlot.cs có thể gọi OnDeploy/OnSell
    public void TriggerAbility(TriggerType triggerContext, CardInstance source, CardInstance directEnemy, List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        if (source == null || source.Data.ability == null) return;
        if (source.Data.ability.trigger != triggerContext) return;

        AbilityData ability = source.Data.ability;

        // Kiểm tra giới hạn kích hoạt (0 = không giới hạn)
        if (ability.triggerLimit > 0 && source.abilityTriggerCount >= ability.triggerLimit) return;

        // StartOfBattle + AddStats = Growth (tăng trưởng vĩnh viễn, lưu vào growthBonus)
        bool isGrowth = triggerContext == TriggerType.StartOfBattle && ability.effect == EffectType.AddStats;

        int scaleFactor = source.mergeLevel + 1;
        List<CardInstance> targets = FindTargets(ability, source, directEnemy, allyBoard, enemyBoard);

        foreach (var target in targets)
        {
            if (isGrowth) ApplyGrowth(ability, target, scaleFactor);
            else ExecuteEffect(ability, target, source, allyBoard, enemyBoard, scaleFactor);
        }

        if (ability.triggerLimit > 0) source.abilityTriggerCount++;
    }

    private void ApplyGrowth(AbilityData ability, CardInstance target, int scaleFactor)
    {
        if (target == null || target.IsDead) return;
        int atkGain = ability.effectValue1 * scaleFactor;
        int hpGain  = ability.effectValue2 * scaleFactor;
        target.growthATKBonus += atkGain;
        target.growthHPBonus  += hpGain;
        target.currentATK = target.Data.baseATK + target.permanentATKBonus + target.growthATKBonus;
        target.currentHP  = target.Data.baseHP  + target.permanentHPBonus  + target.growthHPBonus;
        Debug.Log($"<color=lime>[GROWTH]</color> {target.Data.cardName} tăng trưởng +{atkGain}ATK/+{hpGain}HP (tổng tăng: +{target.growthATKBonus}/+{target.growthHPBonus})");
    }

    private void ExecuteEffect(AbilityData ability, CardInstance target, CardInstance source, List<CardInstance> allyBoard, List<CardInstance> enemyBoard, int scaleFactor = 1)
    {
        if (target == null) return;

        // Chỉ hồi sinh (Reborn) mới được thực thi trên xác chết
        if (target.IsDead && ability.effect != EffectType.Reborn) return;

        switch (ability.effect)
        {
            case EffectType.AddStats:
            {
                int atk = ability.effectValue1 * scaleFactor;
                int hp  = ability.effectValue2 * scaleFactor;
                target.currentATK += atk;
                target.currentHP  += hp;
                if (ability.isPermanent)
                {
                    target.permanentATKBonus += atk;
                    target.permanentHPBonus  += hp;
                }
                Debug.Log($"<color=cyan>[ABILITY]</color> {target.Data.cardName} được buff +{atk}ATK / +{hp}HP");
                break;
            }

            case EffectType.GiveBuff:
                if (ability.isTaunt) target.isTaunt = true;
                Debug.Log($"<color=cyan>[ABILITY]</color> {target.Data.cardName} nhận buff trạng thái (Taunt={target.isTaunt})");
                break;

            case EffectType.DealDamage:
            {
                int dmg = ability.effectValue1 * scaleFactor;
                target.currentHP -= dmg;
                Debug.Log($"<color=orange>[ABILITY]</color> {target.Data.cardName} chịu {dmg} sát thương kỹ năng");
                break;
            }

            case EffectType.Destroy:
                target.currentHP = 0;
                Debug.Log($"<color=red>[ABILITY]</color> {target.Data.cardName} bị tiêu diệt (Banish)!");
                break;

            case EffectType.Reborn:
                if (!target.hasRebornUsed)
                {
                    int reviveHP = ability.effectValue1 * scaleFactor;
                    target.Revive(reviveHP);
                    Debug.Log($"<color=magenta>[ABILITY]</color> {target.Data.cardName} HỒI SINH với {reviveHP} HP!");
                    // [SYNERGY] Bắn tin cho đồng minh: "Có đứa vừa sống lại kìa!"
                    BroadcastAllyEvent(TriggerType.OnAllyReborn, target, allyBoard, enemyBoard);
                }
                break;

            case EffectType.Summon:
                // Logic triệu hồi quái vật vào ô trống gần nhất
                CardInstance summoned = SummonUnit(ability.summonCardID, allyBoard);
                if (summoned != null)
                {
                    Debug.Log($"<color=green>[SUMMON]</color> {source.Data.cardName} triệu hồi {summoned.Data.cardName}!");
                    // [SYNERGY] Bắn tin cho đồng minh: "Có đứa vừa được triệu hồi nè!"
                    BroadcastAllyEvent(TriggerType.OnAllySummon, summoned, allyBoard, enemyBoard);
                }
                break;

            case EffectType.TriggerAbility:
                // Kích hoạt ability của chính target (copy battlecry / deathrattle của đồng minh)
                // Guard: không chain nếu target cũng là TriggerAbility (tránh vòng lặp vô hạn)
                if (target.Data.ability != null && target.Data.ability.effect != EffectType.TriggerAbility)
                    TriggerAbility(target.Data.ability.trigger, target, null, allyBoard, enemyBoard);
                break;
        }
    }

    private void BroadcastAllyEvent(TriggerType context, CardInstance subject, List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        foreach (var unit in allyBoard)
        {
            if (unit != null && !unit.IsDead && unit != subject)
            {
                TriggerAbility(context, unit, subject, allyBoard, enemyBoard);
            }
        }
    }

    private List<CardInstance> FindTargets(AbilityData ability, CardInstance source, CardInstance directEnemy, List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        List<CardInstance> validTargets = new List<CardInstance>();

        switch (ability.target)
        {
            case TargetType.Self:
                validTargets.Add(source);
                break;

            case TargetType.DirectEnemy:
                if (directEnemy != null && !directEnemy.IsDead)
                    validTargets.Add(directEnemy);
                break;

            case TargetType.AllAllies:
            {
                var pool = allyBoard.FindAll(u => u != null && !u.IsDead);
                if (ability.targetTribe != 0) pool = pool.FindAll(u => u.Data.tribe == (Tribe)ability.targetTribe);
                validTargets.AddRange(pool);
                break;
            }

            case TargetType.RandomAlly:
            {
                var pool = allyBoard.FindAll(u => u != null && !u.IsDead && u != source);
                if (ability.targetTribe != 0) pool = pool.FindAll(u => u.Data.tribe == (Tribe)ability.targetTribe);
                validTargets.AddRange(GetRandomFromPool(pool, ability.targetCount > 0 ? ability.targetCount : 1, null));
                break;
            }

            case TargetType.RandomEnemy:
            {
                var pool = enemyBoard.FindAll(u => u != null && !u.IsDead);
                if (ability.targetTribe != 0) pool = pool.FindAll(u => u.Data.tribe == (Tribe)ability.targetTribe);
                validTargets.AddRange(GetRandomFromPool(pool, ability.targetCount > 0 ? ability.targetCount : 1, null));
                break;
            }

            case TargetType.LowestHealthAlly:
                CardInstance lowestHPUnit = null;
                int minHP = int.MaxValue;
                foreach (var unit in allyBoard)
                {
                    if (unit != null && !unit.IsDead && unit.currentHP < minHP)
                    {
                        minHP = unit.currentHP;
                        lowestHPUnit = unit;
                    }
                }
                if (lowestHPUnit != null) validTargets.Add(lowestHPUnit);
                break;

            case TargetType.LeftAlly:
                int leftIndex = source.slotIndex - 1;
                if (leftIndex >= 0)
                {
                    CardInstance leftUnit = allyBoard[leftIndex];
                    if (leftUnit != null && !leftUnit.IsDead)
                        validTargets.Add(leftUnit);
                }
                break;

            case TargetType.RightAlly:
                int rightIndex = source.slotIndex + 1;
                if (rightIndex < 6)
                {
                    CardInstance rightUnit = allyBoard[rightIndex];
                    if (rightUnit != null && !rightUnit.IsDead)
                        validTargets.Add(rightUnit);
                }
                break;

            case TargetType.AllNilesAllies:
                validTargets.AddRange(allyBoard.FindAll(u => u != null && !u.IsDead && u.Data.tribe == Tribe.Niles));
                break;

            case TargetType.AllBabylonAllies:
                validTargets.AddRange(allyBoard.FindAll(u => u != null && !u.IsDead && u.Data.tribe == Tribe.Babylon));
                break;
        }

        return validTargets;
    }

    private CardInstance SummonUnit(string cardID, List<CardInstance> board)
    {
        for (int i = 0; i < 6; i++)
        {
            if (board[i] == null || board[i].IsDead)
            {
                CardDefinition data = CardDatabase.Instance.GetCard(cardID);
                if (data == null) return null;
                board[i] = new CardInstance(data, i);
                Debug.Log($"<color=green>[SUMMON]</color> Đã triệu hồi {data.cardName} vào slot {i}");
                return board[i];
            }
        }
        return null;
    }

    private List<CardInstance> GetRandomFromPool(List<CardInstance> pool, int count, CardInstance exclude)
    {
        List<CardInstance> results = new List<CardInstance>();
        List<CardInstance> temp = new List<CardInstance>(pool.FindAll(u => u != null && !u.IsDead && u != exclude));
        for (int i = 0; i < count && temp.Count > 0; i++)
        {
            int r = Random.Range(0, temp.Count);
            results.Add(temp[r]);
            temp.RemoveAt(r); // Chống trùng lặp mục tiêu
        }
        return results;
    }

    public void ApplyMagicToUnit(CardInstance magic, CardInstance unit)
    {
        if (magic == null || unit == null) return;

        switch (magic.Data.magicGroup)
        {
            case "StatBoost":
                unit.permanentATKBonus += magic.Data.statBonusATK;
                unit.permanentHPBonus += magic.Data.statBonusHP;
                break;

            case "AddAbility":
                if (magic.Data.ability != null)
                    // Cẩn thận: Nếu sau này cần Deep Copy AbilityData để không đè data gốc,
                    // ta sẽ xử lý ở đây. Tạm thời gán reference.
                    unit.Data.ability = magic.Data.ability;
                break;

            case "AddTaunt":
                unit.isTaunt = true;
                break;

            case "RemoveTaunt":
                unit.isTaunt = false;
                break;

            case "Economy":
                // Ví dụ: Thêm 1 vàng vào túi (cần gọi GameManager để cập nhật UI)
                GameManager.Instance.bonusCoinNextTurn += 1;
                Debug.Log($"<color=yellow>[ECONOMY]</color> Đã nhận 1 vàng từ phép {magic.Data.cardName}");
                break;

            default:
                Debug.LogWarning($"Chưa có logic xử lý cho magicGroup: {magic.Data.magicGroup}");
                break;
        }

        // Core logic: Luôn gọi ResetStats từ Engine sau khi thay đổi base data
        unit.ResetStats();

        Debug.Log($"<color=cyan>[MAGIC]</color> Áp dụng phép {magic.Data.cardName} lên {unit.Data.cardName}. ATK/HP mới: {unit.currentATK}/{unit.currentHP}");
    }
}