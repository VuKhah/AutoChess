using UnityEngine; // Thêm UnityEngine để dùng Mathf.Clamp

public class GameSimulator
{
    private CombatResolver resolver = new CombatResolver();

    public int SimulateMatch(BotAgent botA, BotAgent botB)
    {
        int hpA = 7, hpB = 7;
        int maxTurns = 20;

        for (int i = 0; i < maxTurns; i++)
        {
            // Trong vòng lặp i chạy từ 0 -> 19, Turn thực tế sẽ là i + 1
            int currentTurn = i + 1;

            // Công thức tính Tier y hệt như GameManager (Cứ 2 Turn lên 1 Tier, tối đa 6)
            int currentTier = Mathf.Clamp((currentTurn + 1) / 2, 1, 6);

            // Pha chuẩn bị: Shop roll ra bài phải tuân thủ maxTier của Turn hiện tại
            botA.DecidePrepPhase(CardDatabase.Instance.GetRandomShop(3, currentTier));
            botB.DecidePrepPhase(CardDatabase.Instance.GetRandomShop(3, currentTier));

            // Pha chiến đấu
            resolver.ResolveTurn(botA.board, botB.board, new TurnRecord());

            // Trừ HP
            bool aAlive = botA.board.Exists(u => u != null && !u.IsDead); // [HOTFIX] Thêm !u.IsDead để kiểm tra chính xác lính còn sống
            bool bAlive = botB.board.Exists(u => u != null && !u.IsDead); // [HOTFIX] Thêm !u.IsDead

            if (!aAlive && bAlive) hpA--;
            else if (aAlive && !bAlive) hpB--;

            if (hpA <= 0 || hpB <= 0) break;
        }

        if (hpA > hpB) return 1; // A thắng
        if (hpB > hpA) return -1; // B thắng
        return 0; // Hòa
    }
}