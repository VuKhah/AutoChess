public class GameSimulator {
    private CombatResolver resolver = new CombatResolver();

    public int SimulateMatch(BotAgent botA, BotAgent botB) {
        int hpA = 7, hpB = 7;
        int maxTurns = 15;

        for (int i = 0; i < maxTurns; i++) {
            // Pha chuẩn bị (Giả sử shop có 3 lá ngẫu nhiên)
            botA.DecidePrepPhase(CardDatabase.Instance.GetRandomShop(3));
            botB.DecidePrepPhase(CardDatabase.Instance.GetRandomShop(3));

            // Pha chiến đấu
            resolver.ResolveTurn(botA.board, botB.board, new TurnRecord());

            // Trừ HP
            bool aAlive = botA.board.Exists(u => u != null);
            bool bAlive = botB.board.Exists(u => u != null);

            if (!aAlive && bAlive) hpA--;
            else if (aAlive && !bAlive) hpB--;

            if (hpA <= 0 || hpB <= 0) break;
        }

        if (hpA > hpB) return 1; // A thắng
        if (hpB > hpA) return -1; // B thắng
        return 0; // Hòa
    }
}