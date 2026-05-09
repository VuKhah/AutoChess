using System.Collections.Generic;
using UnityEngine;

public class CombatResolver
{
    private readonly AbilityEngine engine = new AbilityEngine();

    // Public API: CardSlot.cs gọi trực tiếp để kích trigger OnDeploy/OnSell
    public void TriggerAbility(TriggerType trigger, CardInstance source, CardInstance directEnemy, List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
        => engine.TriggerAbility(trigger, source, directEnemy, allyBoard, enemyBoard);

    // Public API: CardSlot.cs gọi để áp dụng phép lên unit
    public void ApplyMagicToUnit(CardInstance magic, CardInstance unit)
        => engine.ApplyMagicToUnit(magic, unit);

    // ==========================================
    // COMBAT FLOW
    // ==========================================

    public void ResolveTurn(List<CardInstance> pBoard, List<CardInstance> eBoard, TurnRecord log)
    {
        ApplyTribeSynergies(pBoard);
        ApplyTribeSynergies(eBoard);

        for (int i = 0; i < 6; i++)
        {
            if (pBoard[i] != null && !pBoard[i].IsDead)
            {
                engine.TriggerAbility(TriggerType.StartOfBattle, pBoard[i], null, pBoard, eBoard);
                engine.TriggerAbility(TriggerType.Aura, pBoard[i], null, pBoard, eBoard);
            }
            if (eBoard[i] != null && !eBoard[i].IsDead)
            {
                engine.TriggerAbility(TriggerType.StartOfBattle, eBoard[i], null, eBoard, pBoard);
                engine.TriggerAbility(TriggerType.Aura, eBoard[i], null, eBoard, pBoard);
            }
        }

        int maxRounds = 50;
        int currentRound = 0;

        while (currentRound < maxRounds)
        {
            bool actionTakenInThisRound = false;

            for (int i = 0; i < 6; i++)
            {
                if (pBoard[i] != null && !pBoard[i].IsDead && pBoard[i].currentATK > 0)
                {
                    CardInstance target = FindTarget(eBoard, i);
                    if (target != null)
                    {
                        ExecuteClash(pBoard[i], target, i, eBoard.IndexOf(target), log, true, pBoard, eBoard);
                        actionTakenInThisRound = true;
                    }
                }

                if (eBoard[i] != null && !eBoard[i].IsDead && eBoard[i].currentATK > 0)
                {
                    CardInstance target = FindTarget(pBoard, i);
                    if (target != null)
                    {
                        ExecuteClash(eBoard[i], target, i, pBoard.IndexOf(target), log, false, eBoard, pBoard);
                        actionTakenInThisRound = true;
                    }
                }

                CleanupBoard(pBoard, eBoard);
                CleanupBoard(eBoard, pBoard);
            }

            currentRound++;

            if (IsSideEliminated(pBoard) || IsSideEliminated(eBoard)) break;

            if (!actionTakenInThisRound)
            {
                Debug.Log("<color=gray>[COMBAT]</color> Trận đấu hòa: Không bên nào còn quân có khả năng tấn công!");
                break;
            }
        }

        RecordSnapshots(pBoard, eBoard, log);
    }

    private void ExecuteClash(CardInstance attacker, CardInstance defender, int atkIdx, int defIdx, TurnRecord log, bool isPlayerAttacking, List<CardInstance> atkBoard, List<CardInstance> defBoard)
    {
        int aBefore = attacker.currentHP;
        int dBefore = defender.currentHP;

        int dmgToDefender = attacker.currentATK;
        int dmgToAttacker = defender.currentATK;

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

        engine.TriggerAbility(TriggerType.OnAttack, attacker, defender, atkBoard, defBoard);

        if (dmgToAttacker > 0) engine.TriggerAbility(TriggerType.OnTakeDamage, attacker, defender, atkBoard, defBoard);
        if (dmgToDefender > 0) engine.TriggerAbility(TriggerType.OnTakeDamage, defender, attacker, defBoard, atkBoard);

        HandlePotentialDeath(defender, attacker, defBoard, atkBoard);
        HandlePotentialDeath(attacker, defender, atkBoard, defBoard);

        Debug.Log($"<color=white>[CLASH]</color> {attacker.Data.cardName} đấm {defender.Data.cardName}. " +
                  $"HP Atk: {aBefore}->{attacker.currentHP}, HP Def: {dBefore}->{defender.currentHP}");

        log.AddAction(new CombatAction(atkIdx, defIdx, isPlayerAttacking,
            attacker.Data.cardName, defender.Data.cardName,
            aBefore, attacker.currentHP, dBefore, defender.currentHP));
    }

    private void HandlePotentialDeath(CardInstance victim, CardInstance killer, List<CardInstance> victimBoard, List<CardInstance> killerBoard)
    {
        if (!victim.IsDead) return;

        engine.TriggerAbility(TriggerType.OnDeath, victim, killer, victimBoard, killerBoard);

        foreach (var ally in victimBoard)
        {
            if (ally != null && !ally.IsDead && ally != victim)
                engine.TriggerAbility(TriggerType.OnAllyDeath, ally, victim, victimBoard, killerBoard);
        }
    }

    private void CleanupBoard(List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        for (int i = 0; i < allyBoard.Count; i++)
        {
            var unit = allyBoard[i];
            if (unit == null || !unit.IsDead) continue;

            if (unit.isReborn && !unit.hasRebornUsed)
            {
                unit.Revive(1);
                Debug.Log($"<color=magenta>[REBORN]</color> {unit.Data.cardName} hồi sinh với 1 HP!");
                engine.BroadcastAllyEvent(TriggerType.OnAllyReborn, unit, allyBoard, enemyBoard);
            }
            else
            {
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
                    unit.currentHP += 1;
            }
        }
    }

    private CardInstance FindTarget(List<CardInstance> board, int prefSlot)
    {
        // Ưu tiên 1: Taunt
        var taunt = board.Find(u => u != null && !u.IsDead && u.isTaunt);
        if (taunt != null) return taunt;

        // Ưu tiên 2: Ripple Search (loang từ đối diện ra 2 bên)
        for (int d = 0; d < 6; d++)
        {
            int left  = prefSlot - d;
            int right = prefSlot + d;
            if (left  >= 0 && left  < board.Count && board[left]  != null && !board[left].IsDead)  return board[left];
            if (right >= 0 && right < board.Count && board[right] != null && !board[right].IsDead) return board[right];
        }
        return null;
    }

    private bool IsSideEliminated(List<CardInstance> board)
        => !board.Exists(u => u != null && !u.IsDead);

    private void RecordSnapshots(List<CardInstance> pBoard, List<CardInstance> eBoard, TurnRecord log)
    {
        foreach (var unit in pBoard)
            if (unit != null) log.playerBoardFinal.Add(new CardSnapshot(unit.Data.cardID, unit.currentHP, unit.currentATK));

        foreach (var unit in eBoard)
            if (unit != null) log.enemyBoardFinal.Add(new CardSnapshot(unit.Data.cardID, unit.currentHP, unit.currentATK));
    }
}
