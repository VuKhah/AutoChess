using System.Collections.Generic;
using UnityEngine;

public class CombatResolver
{
    private const int THORNS_DMG = 2;
    private const int ENRAGE_BUFF = 2;

    public void ResolveTurn(List<CardInstance> pBoard, List<CardInstance> eBoard, TurnRecord log)
    {
        
        // 1. Kích hoạt Synergy một lần duy nhất lúc đầu
        ApplyTribeSynergies(pBoard);
        ApplyTribeSynergies(eBoard);

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
                        ExecuteClash(pBoard[i], target, i, eBoard.IndexOf(target), log, true);
                        actionTakenInThisRound = true;
                    }
                }

                // --- BOT TẤN CÔNG ---
                if (eBoard[i] != null && !eBoard[i].IsDead && eBoard[i].currentATK > 0)
                {
                    CardInstance target = FindTarget(pBoard, i);
                    if (target != null)
                    {
                        ExecuteClash(eBoard[i], target, i, pBoard.IndexOf(target), log, false);
                        actionTakenInThisRound = true;
                    }
                }

                // Dọn dẹp xác chết/Reborn sau mỗi cặp đấu
                CleanupBoard(pBoard, log);
                CleanupBoard(eBoard, log);
            }

            currentRound++;

            // KIỂM TRA ĐIỀU KIỆN KẾT THÚC
            if (IsSideEliminated(pBoard) || IsSideEliminated(eBoard)) break;

            // TRƯỜNG HỢP HÒA: Không ai có ATK > 0 để tấn công tiếp
            if (!actionTakenInThisRound)
            {
                Debug.Log("<color=gray>Trận đấu hòa: Không bên nào còn quân có khả năng tấn công!</color>");
                break;
            }
        }

        RecordSnapshots(pBoard, eBoard, log);
    }

    private bool IsSideEliminated(List<CardInstance> board)
    {
        return !board.Exists(u => u != null && !u.IsDead);
    }

    private void ExecuteClash(CardInstance attacker, CardInstance defender, int atkIdx, int defIdx, TurnRecord log, bool isPlayerAttacking)
    {
        // Lưu máu trước khi đấm
        int aBefore = attacker.currentHP;
        int dBefore = defender.currentHP;

        int dmgToDef = attacker.currentATK;
        int dmgToAtk = defender.currentATK;
        // Áp sát thương
        defender.currentHP -= dmgToAtk;
        attacker.currentHP -= dmgToDef;

        // Pipeline Hiệu ứng tức thì
        // Thorns (Gai): Trả sát thương khi bị đấm
        if (defender.Data.ability == AbilityType.Thorns)
            attacker.currentHP -= THORNS_DMG;

        // Enrage (Cuồng nộ): Tăng công khi bị mất máu
        if (attacker.Data.ability == AbilityType.Enrage && dmgToAtk > 0)
            attacker.currentATK += ENRAGE_BUFF;

        if (defender.Data.ability == AbilityType.Enrage && dmgToDef > 0)
            defender.currentATK += ENRAGE_BUFF;

        // Ghi log chi tiết ra Console để Khanh kiểm tra
        Debug.Log($"<color=white>[LOG]</color> {attacker.Data.cardName} đấm {defender.Data.cardName}. " +
                  $"HP Atk: {aBefore}->{attacker.currentHP}, HP Def: {dBefore}->{defender.currentHP}");

        log.AddAction(new CombatAction(atkIdx, defIdx, isPlayerAttacking,
            attacker.Data.cardName, defender.Data.cardName,
            aBefore, attacker.currentHP, dBefore, defender.currentHP));
    }

    private CardInstance FindTarget(List<CardInstance> board, int prefSlot)
    {
        // Luật ưu tiên 1: Taunt (ID 5)
        var taunt = board.Find(u => u != null && !u.IsDead && u.Data.ability == AbilityType.Taunt);
        if (taunt != null) return taunt;

        // d là khoảng cách lan tỏa (0: đối diện, 1: sát bên, 2: cách 1 ô...)
        // 2. Ưu tiên 2: Ripple Search (Loang từ đối diện ra 2 bên)
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
        // Babylon Synergy
        int babylonCount = board.FindAll(u => u != null && u.Data.tribe == Tribe.Babylon).Count;
        if (babylonCount >= 2)
        {
            foreach (var unit in board)
            {
                if (unit != null && unit.Data.tribe == Tribe.Babylon)
                {
                    unit.currentHP += 1;
                    // Tạm thời tăng máu tối đa để UI không bị hiện màu đỏ ngay lập tức
                    unit.Data.baseHP += 1;
                }
            }
        }
    }

    private void CleanupBoard(List<CardInstance> board, TurnRecord log)
    {
        for (int i = 0; i < board.Count; i++)
        {
            if (board[i] != null && board[i].IsDead)
            {
                if (board[i].Data.ability == AbilityType.Reborn && !board[i].hasRebornUsed)
                {
                    board[i].Revive(1); // Hồi sinh
                }
                else
                {
                    if (board[i].Data.ability == AbilityType.SlainEffect)
                    {
                        TriggerSlainEffect(board, i);
                    }
                    board[i] = null; // Xóa khỏi bộ nhớ
                }
            }
        }
    }

    private void TriggerSlainEffect(List<CardInstance> board, int deadIndex)
    {
        List<CardInstance> allies = board.FindAll(u => u != null && !u.IsDead);
        if (allies.Count > 0)
        {
            CardInstance randomAlly = allies[Random.Range(0, allies.Count)];
            randomAlly.currentATK += 3;
        }
    }

    private void RecordSnapshots(List<CardInstance> pBoard, List<CardInstance> eBoard, TurnRecord log)
    {
        // Lưu trạng thái board người chơi
        foreach (var unit in pBoard)
        {
            if (unit != null)
                log.playerBoardFinal.Add(new CardSnapshot(unit.Data.cardID, unit.currentHP, unit.currentATK));
        }

        // Lưu trạng thái board đối thủ
        foreach (var unit in eBoard)
        {
            if (unit != null)
                log.enemyBoardFinal.Add(new CardSnapshot(unit.Data.cardID, unit.currentHP, unit.currentATK));
        }
    }
}