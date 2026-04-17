using System.Collections.Generic;

public class CombatResolver
{
    private const int THORNS_DMG = 2;
    private const int ENRAGE_BUFF = 2;

    public void ResolveTurn(List<CardInstance> pBoard, List<CardInstance> eBoard, TurnRecord log)
    {
        // 1. Pha tấn công theo Slot (0-5)
        for (int i = 0; i < 6; i++)
        {
            if (pBoard[i] != null && !pBoard[i].IsDead)
                ExecuteClash(pBoard[i], FindTarget(eBoard, i), log);

            if (eBoard[i] != null && !eBoard[i].IsDead)
                ExecuteClash(eBoard[i], FindTarget(pBoard, i), log);

            // Dọn dẹp xác chết & Reborn ngay sau mỗi cặp đấu
            CleanupBoard(pBoard, log);
            CleanupBoard(eBoard, log);
        }
    }

    private void ExecuteClash(CardInstance attacker, CardInstance defender, TurnRecord log)
    {
        if (defender == null) return;

        // Sát thương đồng thời
        int dmgToDef = attacker.currentATK;
        int dmgToAtk = defender.currentATK;

        defender.currentHP -= dmgToDef;
        attacker.currentHP -= dmgToAtk;

        // Pipeline Hiệu ứng tức thì: Thorns & Enrage
        if (defender.Data.ability == AbilityType.Thorns) attacker.currentHP -= THORNS_DMG;
        if (attacker.Data.ability == AbilityType.Enrage && dmgToAtk > 0) attacker.currentATK += ENRAGE_BUFF;
        if (defender.Data.ability == AbilityType.Enrage && dmgToDef > 0) defender.currentATK += ENRAGE_BUFF;
    }

    private CardInstance FindTarget(List<CardInstance> board, int prefSlot)
    {
        // Luật: Taunt trước -> Slot tương ứng -> Slot thấp nhất
        var taunt = board.Find(u => u != null && !u.IsDead && u.Data.ability == AbilityType.Taunt);
        if (taunt != null) return taunt;
        if (board[prefSlot] != null && !board[prefSlot].IsDead) return board[prefSlot];
        return board.Find(u => u != null && !u.IsDead);
    }

    private void CleanupBoard(List<CardInstance> board, TurnRecord log)
    {
        for (int i = 0; i < board.Count; i++)
        {
            if (board[i] != null && board[i].IsDead)
            {
                if (board[i].Data.ability == AbilityType.Reborn && !board[i].hasRebornUsed)
                    board[i].Revive(1);
                else
                    board[i] = null;
            }
        }
    }
}