using System.Collections.Generic;
using UnityEngine;

// TTE Engine: Trigger → Target → Effect
public partial class AbilityEngine
{
    // Stack-based summon queue: mỗi unit chỉ summon 1 lần ngay lập tức,
    // các unit còn lại trong batch đợi CombatResolver.FlushDeathStack pop ra
    // SAU KHI death chain của unit trước đó resolve hoàn toàn.
    private struct PendingSummonEntry
    {
        public string cardID;
        public List<CardInstance> allyBoard;
        public List<CardInstance> enemyBoard;
    }
    private readonly Queue<PendingSummonEntry> _pendingSummons = new Queue<PendingSummonEntry>();

    public bool HasPendingSummons => _pendingSummons.Count > 0;
    public void ClearPendingSummons() => _pendingSummons.Clear();

    public void ProcessNextPendingSummon()
    {
        if (_pendingSummons.Count == 0) return;
        var e = _pendingSummons.Dequeue();
        CardInstance summoned = SummonUnit(e.cardID, e.allyBoard);
        if (summoned == null)
        {
            Debug.Log($"<color=gray>[SUMMON]</color> Pending {e.cardID}: hết slot, unit biến mất theo stack.");
            return;
        }
        Debug.Log($"<color=green>[SUMMON]</color> (pending) triệu hồi {summoned.Data.cardName}!");
        TriggerAbility(TriggerType.OnDeploy, summoned, null, e.allyBoard, e.enemyBoard);
        BroadcastAllyEvent(TriggerType.OnAllySummon, summoned, e.allyBoard, e.enemyBoard);
    }


    public void TriggerAbility(TriggerType triggerContext, CardInstance source, CardInstance directEnemy, List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        if (source == null || source.Data.abilities == null || source.Data.abilities.Count == 0) return;

        int scaleFactor = source.mergeLevel + 1;

        for (int i = 0; i < source.Data.abilities.Count; i++)
        {
            AbilityData ability = source.Data.abilities[i];
            if (ability == null || ability.trigger != triggerContext) continue;

            if (ability.triggerLimit > 0 && source.abilityTriggerCounts[i] >= ability.triggerLimit) continue;

            // StartOfBattle + AddStats = Growth (tăng trưởng vĩnh viễn)
            bool isGrowth = triggerContext == TriggerType.StartOfBattle && ability.effect == EffectType.AddStats;

            List<CardInstance> targets = FindTargets(ability, source, directEnemy, allyBoard, enemyBoard);
            int esc = (ability.isEscalating && i < source.abilityEscalationBonuses.Count)
                      ? source.abilityEscalationBonuses[i] : 0;

            // triggerLimit: chỉ đếm khi thực sự có mục tiêu hợp lệ.
            // Tránh tình huống unit thứ hai (SekhmetB) bị trừ lượt khi mục tiêu đã bị
            // unit thứ nhất (SekhmetA) consume mất — TriggerSubject trả rỗng → skip, không mất count.
            // conditionCount: luôn đếm (semantics = "fire mỗi N lần trigger").
            if (ability.triggerLimit > 0)
            {
                if (targets.Count == 0) continue;
                source.abilityTriggerCounts[i]++;
            }
            else
            {
                source.abilityTriggerCounts[i]++;
                // conditionCount > 0: chỉ thực thi vào đúng lần thứ N, 2N, 3N...
                if (ability.conditionCount > 0 && source.abilityTriggerCounts[i] % ability.conditionCount != 0) continue;
            }

            foreach (var target in targets)
            {
                if (isGrowth) ApplyGrowth(ability, target, scaleFactor, esc);
                else ExecuteEffect(ability, target, source, allyBoard, enemyBoard, scaleFactor, directEnemy, esc);
            }

            // Escalating: tăng effectValue mỗi lần fire — dùng counter riêng, không mutate AbilityData
            if (ability.isEscalating && i < source.abilityEscalationBonuses.Count)
                source.abilityEscalationBonuses[i]++;
        }
    }

    private void ApplyGrowth(AbilityData ability, CardInstance target, int scaleFactor, int escalationBonus = 0)
    {
        if (target == null || target.IsDead) return;
        int atkGain = (ability.effectValue1 + escalationBonus) * scaleFactor;
        int hpGain  = (ability.effectValue2 + escalationBonus) * scaleFactor;
        target.growthATKBonus += atkGain;
        target.growthHPBonus  += hpGain;
        // BUG FIX: Cộng dồn vào currentHP/ATK hiện tại thay vì recalculate từ base,
        // giữ nguyên bonus từ tribe synergy và các buff trước đó trong cùng combat.
        target.currentATK += atkGain;
        target.currentHP  += hpGain;
        Debug.Log($"<color=lime>[GROWTH]</color> {target.Data.cardName} tăng trưởng +{atkGain}ATK/+{hpGain}HP (tổng: +{target.growthATKBonus}/+{target.growthHPBonus})");
    }

    private void ExecuteEffect(AbilityData ability, CardInstance target, CardInstance source,
        List<CardInstance> allyBoard, List<CardInstance> enemyBoard, int scaleFactor = 1, CardInstance directEnemy = null, int escalationBonus = 0)
    {
        if (target == null) return;

        // Reborn, Summon, SummonConsumed, GiveBuff được phép thực thi dù target đã chết.
        // GiveBuff vào target dead: nếu grant Reborn → CleanupBoard sẽ revive;
        // nếu grant Taunt/Safeguard → được reset ở ResetStats() đầu turn sau, không gây hại.
        bool isDeathSafeEffect = ability.effect == EffectType.Reborn
                              || ability.effect == EffectType.Summon
                              || ability.effect == EffectType.SummonConsumed
                              || ability.effect == EffectType.GiveBuff;
        if (target.IsDead && !isDeathSafeEffect) return;

        switch (ability.effect)
        {
            case EffectType.AddStats:
            {
                int atk = (ability.effectValue1 + escalationBonus) * scaleFactor;
                int hp  = (ability.effectValue2 + escalationBonus) * scaleFactor;
                target.currentATK += atk;
                target.currentHP  += hp;
                if (ability.isPermanent)
                {
                    target.permanentATKBonus += atk;
                    target.permanentHPBonus  += hp;
                    // BUG FIX: Giữ maxHP đồng bộ với permanent HP buff trong combat.
                    // Không cập nhật → IsDamaged sai, heal-to-max tính thiếu HP.
                    if (hp > 0) target.maxHP += hp;
                }
                Debug.Log($"<color=cyan>[ABILITY]</color> {target.Data.cardName} được buff +{atk}ATK / +{hp}HP");
                break;
            }

            case EffectType.GiveBuff:
            {
                if (ability.isTaunt)      { target.isTaunt = true;         Debug.Log($"<color=cyan>[BUFF]</color> {target.Data.cardName} nhận Taunt!"); }
                if (ability.isReborn)     { target.isReborn = true; target.hasRebornUsed = false; Debug.Log($"<color=cyan>[BUFF]</color> {target.Data.cardName} nhận Reborn!"); }
                if (ability.isSafeguard)  { target.safeguardActive = true; Debug.Log($"<color=cyan>[BUFF]</color> {target.Data.cardName} nhận Safeguard!"); }
                break;
            }

            case EffectType.DealDamage:
            {
                if (target.safeguardActive)
                {
                    target.safeguardActive = false;
                    Debug.Log($"<color=cyan>[SAFEGUARD]</color> {target.Data.cardName} chặn sát thương kỹ năng!");
                    break;
                }
                int dmg = ability.effectValue1 * scaleFactor;
                target.currentHP -= dmg;
                // Track kẻ gây đòn chết để death stack dùng làm directEnemy trong OnDeath
                if (target.IsDead) target.lastAttacker = source;
                Debug.Log($"<color=orange>[ABILITY]</color> {target.Data.cardName} chịu {dmg} sát thương kỹ năng");
                break;
            }

            case EffectType.Destroy:
                target.currentHP = 0;
                target.lastAttacker = source;
                if (ability.isConsume)
                {
                    source.consumedCardIDs.Add(target.Data.cardID);
                    // BUG FIX: Unit bị Consume không được phép hồi sinh qua Reborn passive.
                    // Nếu không chặn, CleanupBoard sẽ revive unit đó → unit vừa sống trên board
                    // vừa nằm trong consumedCardIDs, gây nhân bản khi consumer chết.
                    target.hasRebornUsed = true;
                }
                Debug.Log($"<color=red>[ABILITY]</color> {source.Data.cardName} tiêu diệt {target.Data.cardName}{(ability.isConsume ? " (Consumed!)" : "")}");
                break;

            case EffectType.Reborn:
                if (!target.hasRebornUsed)
                {
                    // Mathf.Max(1,...): đảm bảo reviveHP > 0 tránh IsDead ngay sau revive → OnDeath fire 2 lần
                    int reviveHP = Mathf.Max(1, ability.effectValue1 * scaleFactor);
                    target.Revive(reviveHP);
                    Debug.Log($"<color=magenta>[ABILITY]</color> {target.Data.cardName} HỒI SINH với {reviveHP} HP!");
                    BroadcastAllyEvent(TriggerType.OnAllyReborn, target, allyBoard, enemyBoard);
                }
                break;

            case EffectType.Summon:
            {
                int summonCount = (ability.effectValue1 > 0 ? ability.effectValue1 : 1) * scaleFactor;
                // Summon unit đầu tiên ngay lập tức
                CardInstance first = SummonUnit(ability.summonCardID, allyBoard);
                if (first != null)
                {
                    Debug.Log($"<color=green>[SUMMON]</color> {source.Data.cardName} triệu hồi {first.Data.cardName} (1/{summonCount})!");
                    TriggerAbility(TriggerType.OnDeploy, first, null, allyBoard, enemyBoard);
                    BroadcastAllyEvent(TriggerType.OnAllySummon, first, allyBoard, enemyBoard);
                }
                // BUG FIX: stack-based summon — các unit tiếp theo đi vào pending queue.
                // CombatResolver.FlushDeathStack chỉ pop 1 unit SAU KHI death chain của unit
                // trước đó (bao gồm cả Sekhmet consume + OnDeath chain) đã resolve hoàn toàn.
                // Tránh tình huống Sekhmet nuốt cả NW1 lẫn NW2 trong cùng một lượt broadcast.
                for (int s = 1; s < summonCount; s++)
                    _pendingSummons.Enqueue(new PendingSummonEntry
                        { cardID = ability.summonCardID, allyBoard = allyBoard, enemyBoard = enemyBoard });
                break;
            }

            case EffectType.GainCoin:
            {
                int coins = ability.effectValue1 * scaleFactor;
                GameManager.Instance.AddCoin(coins);
                Debug.Log($"<color=yellow>[ABILITY]</color> {source.Data.cardName} cộng {coins} Coin! (Deploy)");
                break;
            }

            case EffectType.TriggerAbility:
                // Replay OnDeath abilities của target scaleFactor lần
                // Guard: không cho phép OnDeath abilities có effect TriggerAbility gây chain vô hạn
                if (target.Data.abilities != null)
                {
                    for (int r = 0; r < scaleFactor; r++)
                        TriggerAbility(TriggerType.OnDeath, target, directEnemy, allyBoard, enemyBoard);
                }
                break;

            case EffectType.SummonConsumed:
                if (source.consumedCardIDs != null && source.consumedCardIDs.Count > 0)
                {
                    Debug.Log($"<color=magenta>[CONSUME]</color> {source.Data.cardName} giải phóng {source.consumedCardIDs.Count} unit đã tiêu thụ!");
                    foreach (var cardID in source.consumedCardIDs)
                    {
                        CardInstance released = SummonUnit(cardID, allyBoard);
                        if (released != null)
                        {
                            TriggerAbility(TriggerType.OnDeploy, released, null, allyBoard, enemyBoard);
                            BroadcastAllyEvent(TriggerType.OnAllySummon, released, allyBoard, enemyBoard);
                        }
                    }
                    source.consumedCardIDs.Clear();
                }
                break;
        }
    }

    public void BroadcastAllyEvent(TriggerType context, CardInstance subject, List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        // Snapshot tránh unit được summon trong broadcast nhận event của chính lần summon đó
        var snapshot = new List<CardInstance>(allyBoard);
        foreach (var unit in snapshot)
        {
            if (unit == null || unit.IsDead || unit == subject) continue;
            // BUG FIX: OnAllySummon / OnAllyReborn chỉ kích hoạt khi đồng minh CÙNG BỘ TỘC liên quan
            // (theo đặc tả trong AbilityData.cs enum comment)
            if ((context == TriggerType.OnAllySummon || context == TriggerType.OnAllyReborn)
                && unit.Data.tribe != subject.Data.tribe) continue;
            TriggerAbility(context, unit, subject, allyBoard, enemyBoard);
        }
    }

    private CardInstance SummonUnit(string cardID, List<CardInstance> board)
    {
        CardDefinition data = CardDatabase.Instance.GetCard(cardID);
        if (data == null) return null;

        // Ưu tiên 1: slot trống hoàn toàn
        for (int i = 0; i < 6; i++)
        {
            if (board[i] == null)
            {
                board[i] = new CardInstance(data, i) { isBattleSpawned = true };
                Debug.Log($"<color=green>[SUMMON]</color> Đã triệu hồi {data.cardName} vào slot {i}");
                return board[i];
            }
        }

        // BUG FIX: Ưu tiên 2: slot dead + đã xử lý OnDeath + KHÔNG có Reborn chờ
        // (tránh overwrite unit đang chờ Reborn trong CleanupBoard)
        for (int i = 0; i < 6; i++)
        {
            if (board[i] != null && board[i].IsDead && board[i].onDeathProcessed && !board[i].isReborn)
            {
                board[i] = new CardInstance(data, i) { isBattleSpawned = true };
                Debug.Log($"<color=green>[SUMMON]</color> Đã triệu hồi {data.cardName} vào slot {i} (thay chỗ dead)");
                return board[i];
            }
        }

        return null;
    }

    public void ApplyMagicToUnit(CardInstance magic, CardInstance unit)
    {
        if (magic == null || unit == null) return;

        switch (magic.Data.magicGroup)
        {
            case "StatBoost":
                unit.permanentATKBonus += magic.Data.statBonusATK;
                unit.permanentHPBonus  += magic.Data.statBonusHP;
                break;

            case "AddAbility":
                if (magic.Data.abilities != null && magic.Data.abilities.Count > 0)
                {
                    if (unit.Data.abilities == null) unit.Data.abilities = new List<AbilityData>();
                    foreach (var ab in magic.Data.abilities)
                        if (ab != null) unit.Data.abilities.Add(ab);
                }
                break;

            case "AddTaunt":
                unit.isTaunt = true;
                break;

            case "RemoveTaunt":
                unit.isTaunt = false;
                break;

            case "Economy":
                GameManager.Instance.AddBonusCoin(1);
                Debug.Log($"<color=yellow>[ECONOMY]</color> Đã nhận 1 vàng từ phép {magic.Data.cardName}");
                break;

            default:
                Debug.LogWarning($"Chưa có logic xử lý cho magicGroup: {magic.Data.magicGroup}");
                break;
        }

        unit.ResetStats();
        Debug.Log($"<color=cyan>[MAGIC]</color> Áp dụng phép {magic.Data.cardName} lên {unit.Data.cardName}. ATK/HP mới: {unit.currentATK}/{unit.currentHP}");
    }
}
