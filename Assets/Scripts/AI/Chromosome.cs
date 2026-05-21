using UnityEngine;

[System.Serializable]
public class Chromosome
{
    public const int GeneCount = 24;

    // ── NHÓM 1: Chỉ số gốc (4 genes) ───────────────────────────────────────
    // [0]  wATK          Trọng số ATK cơ bản
    // [1]  wHP           Trọng số HP cơ bản
    // [2]  wTierBonus    Bonus thêm mỗi cấp Tier vượt 1  (score += (tier-1)*gene*5)
    // [3]  wCostEff      Hệ số hiệu quả chi phí          (score / cost * (1+gene))

    // ── NHÓM 2: Passive keywords (3 genes) ──────────────────────────────────
    // [4]  wTaunt        Giá trị của passive Taunt
    // [5]  wReborn       Giá trị của passive Reborn
    // [6]  wSafeguard    Giá trị của passive Safeguard

    // ── NHÓM 3: Trigger weights (6 genes) ───────────────────────────────────
    // [7]  tStartBattle   StartOfBattle — buff trước trận
    // [8]  tOnDeath       OnDeath — deathrattle
    // [9]  tOnAttack      OnAttack — phản ứng mỗi đòn tấn công
    // [10] tOnTakeDmg     OnTakeDamage — phản ứng khi bị đánh
    // [11] tEndTurnShop   EndTurnShop — buff tích lũy qua turn
    // [12] tOnDeploy      OnDeploy / OnAlly events

    // ── NHÓM 4: Effect weights (5 genes) ────────────────────────────────────
    // [13] eAddStats      AddStats — tăng ATK/HP
    // [14] eSummon        Summon / SummonConsumed / Consume chain
    // [15] eDealDmg       DealDamage — sát thương trực tiếp
    // [16] eGainCoin      GainCoin / kinh tế
    // [17] eGiveBuff      GiveBuff / Reborn / status effects

    // ── NHÓM 5: Tribe synergy (3 genes) ─────────────────────────────────────
    // [18] sBabylon       Bonus per đồng đội Babylon đã có (+HP synergy)
    // [19] sOlympus       Bonus per đồng đội Olympus đã có (+ATK synergy)
    // [20] sNiles         Bonus per đồng đội Niles đã có  (+HP synergy ≥3)

    // ── NHÓM 6: Board context (3 genes) ─────────────────────────────────────
    // [21] wMerge         Bonus per bản sao cùng cardID đang có trên sân (merge proximity)
    // [22] wFrontline     Bonus khi mua Taunt cho vị trí frontline còn trống
    // [23] wSaveThreshold Ngưỡng tối thiểu để quyết định mua (≤ gene*3 → tiết kiệm tiền)

    public float[] genes = new float[GeneCount];
    public float fitness = 0f;

    private static readonly System.Random _rng = new System.Random();

    public Chromosome()
    {
        for (int i = 0; i < GeneCount; i++)
            genes[i] = (float)_rng.NextDouble();
    }

    public Chromosome Clone()
    {
        Chromosome copy = new Chromosome();
        System.Array.Copy(genes, copy.genes, GeneCount);
        copy.fitness = this.fitness;
        return copy;
    }
}
