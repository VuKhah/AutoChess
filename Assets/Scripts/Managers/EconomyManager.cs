public class EconomyManager
{
    public int CurrentCoin { get; private set; }
    private int bonusNextTurn = 0;

    public void ResetEconomy()
    {
        CurrentCoin = 10 + bonusNextTurn;
        bonusNextTurn = 0;
    }

    public void Buy() => CurrentCoin -= 3;
    public void Sell() => CurrentCoin += 1;
    public void AddBonus(int amt) => bonusNextTurn += amt;
}