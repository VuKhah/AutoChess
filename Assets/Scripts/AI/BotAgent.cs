using System.Collections.Generic;

public class BotAgent
{
    public Chromosome brain;
    public EconomyManager economy;
    public List<CardInstance> board = new List<CardInstance>(new CardInstance[6]);

    public BotAgent(Chromosome brain)
    {
        this.brain = brain;
        this.economy = new EconomyManager();
    }

    public void DecidePrepPhase(List<CardDefinition> shop)
    {
        economy.ResetEconomy();

        // Mua cho đến khi hết tiền (Giả sử thẻ giá 3) hoặc hết chỗ
        while (economy.CurrentCoin >= 3)
        {
            CardDefinition bestCard = null;
            float bestScore = -1f;

            foreach (var card in shop)
            {
                float score = Evaluate(card);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCard = card;
                }
            }

            if (bestCard != null && HasEmptySlot())
            {
                BuyAndPlace(bestCard);
            }
            else break;
        }
    }

    private float Evaluate(CardDefinition c)
    {
        // 1. Chấm điểm dựa trên chỉ số gốc
        float s = c.baseATK * brain.genes[0] + c.baseHP * brain.genes[1];

        // 2. Chấm điểm dựa trên hệ thống Kỹ năng TTE mới
        if (c.ability != null)
        {

            // Đánh giá Taunt (Bắt buộc tấn công)
            if (c.ability.isTaunt)
            {
                s += 8 * brain.genes[3];
            }

            // Đánh giá Thorns (Phản sát thương: Bị đánh -> DealDamage)
            if (c.ability.trigger == TriggerType.OnTakeDamage && c.ability.effect == EffectType.DealDamage)
            {
                s += 8 * brain.genes[3];
            }

            // Đánh giá Growth (Tăng trưởng: Đầu hiệp -> AddStats)
            if (c.ability.trigger == TriggerType.StartOfBattle && c.ability.effect == EffectType.AddStats)
            {
                s += 12 * brain.genes[4];
            }

            // Đánh giá Reborn (Hồi sinh)
            if (c.ability.effect == EffectType.Reborn)
            {
                s += 10 * brain.genes[5];
            }

            // Đánh giá SlainEffect (Khi chết -> Buff đồng minh) 
            // Tạm dùng gene[2] (gene Economy cũ) để chấm điểm cho hiệu ứng buff khi chết
            if (c.ability.trigger == TriggerType.OnDeath && c.ability.effect == EffectType.AddStats)
            {
                s += 10 * brain.genes[2];
            }
        }

        return s;
    }

    private bool HasEmptySlot() => board.Exists(slot => slot == null);

    private void BuyAndPlace(CardDefinition c)
    {
        int idx = board.FindIndex(slot => slot == null);
        if (idx != -1)
        { // Thêm check an toàn
            board[idx] = new CardInstance(c, idx);
            economy.Buy();
        }
    }
}