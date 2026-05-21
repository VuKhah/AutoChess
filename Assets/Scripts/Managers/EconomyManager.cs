public class EconomyManager
{
    public int CurrentCoin { get; private set; }
    private int bonusNextTurn = 0;

    public void ResetEconomy()
    {
        CurrentCoin = 10 + bonusNextTurn;
        bonusNextTurn = 0;
    }

    // Dùng cho GameManager: mua/bán với giá biến đổi, cộng coin trực tiếp
    public bool TrySpend(int cost)
    {
        if (CurrentCoin < cost) return false;
        CurrentCoin -= cost;
        return true;
    }

    public void Earn(int amount) => CurrentCoin += amount;
    public void AddBonus(int amt) => bonusNextTurn += amt;

    // Dùng cho BotAgent
    public bool TryBuy(int cost) => TrySpend(cost);
    public void Sell() => CurrentCoin += 1;
}