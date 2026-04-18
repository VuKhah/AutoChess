using System.Collections.Generic;
using UnityEngine;

public class CombatResolver
{
    public void ResolveTurn(List<CardInstance> pBoard, List<CardInstance> eBoard, TurnRecord log)
    {
        // 1. Kích hoạt Synergy một lần duy nhất lúc đầu (Buff theo Tộc)
        ApplyTribeSynergies(pBoard);
        ApplyTribeSynergies(eBoard);

        // 2. Kích hoạt kỹ năng đầu hiệp (Ví dụ: Growth - Tăng trưởng)
        for (int i = 0; i < 6; i++)
        {
            if (pBoard[i] != null && !pBoard[i].IsDead)
                TriggerAbility(TriggerType.OnTurnStart, pBoard[i], null, pBoard, eBoard);

            if (eBoard[i] != null && !eBoard[i].IsDead)
                TriggerAbility(TriggerType.OnTurnStart, eBoard[i], null, eBoard, pBoard);
        }

        int maxRounds = 50; // Giới hạn an toàn để tránh vòng lặp vô tận (Infinity Loop)
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

                // --- BOT TẤN CÔNG ---
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

        int dmgToDef = attacker.currentATK;
        int dmgToAtk = defender.currentATK;


        // Áp sát thương (Đánh đồng thời)
        defender.currentHP -= dmgToDef; // Defender nhận sát thương từ Attacker
        attacker.currentHP -= dmgToAtk; // Attacker nhận sát thương từ Defender

        // Trigger kỹ năng Khi Bị Đánh (OnTakeDamage) - Thay thế cho Enrage/Thorns cũ
        if (dmgToAtk > 0)
            TriggerAbility(TriggerType.OnTakeDamage, defender, attacker, defBoard, atkBoard);

        if (dmgToDef > 0)
            TriggerAbility(TriggerType.OnTakeDamage, attacker, defender, atkBoard, defBoard);

        // 2. [FIX PHANTOM COMBAT] Trigger OnDeath NGAY LẬP TỨC để xử lý Reborn/SlainEffect
        if (defender.IsDead)
            TriggerAbility(TriggerType.OnDeath, defender, attacker, defBoard, atkBoard);
        if (attacker.IsDead)
            TriggerAbility(TriggerType.OnDeath, attacker, defender, atkBoard, defBoard);
        // Ghi log ra Console và TurnRecord
        Debug.Log($"<color=white>[CLASH]</color> {attacker.Data.cardName} đấm {defender.Data.cardName}. " +
                  $"HP Atk: {aBefore}->{attacker.currentHP}, HP Def: {dBefore}->{defender.currentHP}");

        log.AddAction(new CombatAction(atkIdx, defIdx, isPlayerAttacking,
            attacker.Data.cardName, defender.Data.cardName,
            aBefore, attacker.currentHP, dBefore, defender.currentHP));
    }

    private CardInstance FindTarget(List<CardInstance> board, int prefSlot)
    {
        // Cập nhật tìm Taunt bằng biến isTaunt từ AbilityData
        var taunt = board.Find(u => u != null && !u.IsDead && u.Data.ability != null && u.Data.ability.isTaunt);
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

    private void ApplyTribeSynergies(List<CardInstance> board)
    {
        int babylonCount = board.FindAll(u => u != null && u.Data.tribe == Tribe.Babylon).Count;
        if (babylonCount >= 2)
        {
            foreach (var unit in board)
            {
                if (unit != null && unit.Data.tribe == Tribe.Babylon)
                {
                    unit.currentHP += 1;
                    unit.Data.baseHP += 1; // Tạm thời
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

    private void TriggerAbility(TriggerType triggerContext, CardInstance source, CardInstance directEnemy, List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        // Bỏ qua nếu quân bài không có AbilityData hoặc không khớp Trigger
        if (source == null || source.Data.ability == null) return;

        if (source.Data.ability.trigger == triggerContext)
        {
            // 1. Lấy danh sách mục tiêu
            List<CardInstance> targets = GetTargets(source.Data.ability, source, directEnemy, allyBoard, enemyBoard);

            // 2. Thực thi hiệu ứng lên từng mục tiêu
            foreach (var target in targets)
            {
                ExecuteEffect(source.Data.ability, target);
            }
        }
    }

    private List<CardInstance> GetTargets(AbilityData ability, CardInstance source, CardInstance directEnemy, List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        List<CardInstance> validTargets = new List<CardInstance>();

        if (ability.target == TargetType.Self)
        {
            validTargets.Add(source);
        }
        else if (ability.target == TargetType.Enemy && directEnemy != null && !directEnemy.IsDead)
        {
            validTargets.Add(directEnemy);
        }
        else if (ability.target == TargetType.AllAllies)
        {
            validTargets.AddRange(allyBoard.FindAll(u => u != null && !u.IsDead));
        }
        else if (ability.target == TargetType.RandomAlly || ability.target == TargetType.RandomEnemy)
        {
            List<CardInstance> pool = ability.target == TargetType.RandomAlly
                ? allyBoard.FindAll(u => u != null && !u.IsDead && u != source)
                : enemyBoard.FindAll(u => u != null && !u.IsDead);

            int maxTargets = Mathf.Min(ability.targetCount > 0 ? ability.targetCount : 1, pool.Count);

            for (int i = 0; i < maxTargets; i++)
            {
                int r = Random.Range(0, pool.Count);
                validTargets.Add(pool[r]);
                pool.RemoveAt(r); // Chống trùng lặp mục tiêu
            }
        }

        return validTargets;
    }

    private void ExecuteEffect(AbilityData ability, CardInstance target)
    {
        if (target == null) return;

        // Chỉ hồi sinh (Reborn) mới được thực thi trên xác chết
        if (target.IsDead && ability.effect != EffectType.Reborn) return;

        switch (ability.effect)
        {
            case EffectType.AddStats:
                target.currentATK += ability.effectValue1;
                target.currentHP += ability.effectValue2;
                Debug.Log($"<color=cyan>[ABILITY]</color> {target.Data.cardName} được buff +{ability.effectValue1}ATK / +{ability.effectValue2}HP");
                break;

            case EffectType.Heal:
                target.currentHP += ability.effectValue1;
                Debug.Log($"<color=green>[ABILITY]</color> {target.Data.cardName} hồi {ability.effectValue1} HP");
                break;

            case EffectType.DealDamage:
                target.currentHP -= ability.effectValue1;
                Debug.Log($"<color=orange>[ABILITY]</color> {target.Data.cardName} chịu {ability.effectValue1} sát thương kỹ năng");
                break;

            case EffectType.Reborn:
                if (!target.hasRebornUsed)
                {
                    target.Revive(ability.effectValue1);
                    Debug.Log($"<color=magenta>[ABILITY]</color> {target.Data.cardName} HỒI SINH với {ability.effectValue1} HP!");
                }
                break;
        }
    }
}