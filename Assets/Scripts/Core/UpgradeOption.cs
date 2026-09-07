using System;
using System.Collections.Generic;

public class UpgradeOption
{
    public string Title;
    public string Description;
    public Action<BattleSystem> Apply;

    public UpgradeOption(string title, string description, Action<BattleSystem> apply)
    {
        Title = title;
        Description = description;
        Apply = apply;
    }
}

public static class UpgradePool
{
    public static List<UpgradeOption> All()
    {
        return new List<UpgradeOption>
        {
            new UpgradeOption("Vigor", "+10 Max Health, heal 10 now.", bs => bs.ApplyVigor(10)),
            new UpgradeOption("Focus", "+3 Attack Power.", bs => bs.ApplyAttackBonus(3)),
            new UpgradeOption("Ward", "+2 Defense.", bs => bs.ApplyDefenseBonus(2)),
            new UpgradeOption("Overflow", "+20 Max Mana.", bs => bs.ApplyMaxManaBonus(20)),
            new UpgradeOption("Efficient Casting", "Heal costs 5 less Mana (min 10).", bs => bs.ReduceHealCost(5)),
            new UpgradeOption("Bloodpact", "Attacks heal you for 25% of the damage dealt.", bs => bs.AddLifesteal(0.25f)),
            new UpgradeOption("Second Wind", "Defend also restores 10 Mana.", bs => bs.AddDefendManaRestore(10)),
        };
    }
}
