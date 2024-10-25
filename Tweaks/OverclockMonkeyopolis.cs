using System.Collections.Generic;
using System.Linq;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Weapons.Behaviors;
using Il2CppAssets.Scripts.Simulation.SMath;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Il2CppSystem.Linq;

namespace TacticalTweaks.Tweaks;

public class OverclockMonkeyopolis : ToggleableTweak
{
    protected override bool DefaultEnabled => true;

    public override string Description =>
        "Makes the Overclock buff affect Monkeyopolis income in the same way it affects Banana Farms.";

    protected override string Icon => VanillaSprites.MetropolisUpgradeIcon;

    protected override ModSettingCategory Category => TacticalTweaksMod.Overclock;

    [HarmonyPatch(typeof(OverclockModel.OverclockMutator), nameof(OverclockModel.OverclockMutator.Mutate))]
    internal static class OverclockMutator_Mutate
    {
        [HarmonyPostfix]
        internal static void Postfix(OverclockModel.OverclockMutator __instance, Model model)
        {
            if (!GetInstance<OverclockMonkeyopolis>().Enabled || !model.Is(out TowerModel towerModel) ||
                towerModel.baseId == TowerType.BananaFarm) return;

            model.GetDescendants<EmissionsPerRoundFilterModel>().ForEach(filter =>
            {
                var current = towerModel.HasBehavior(out MonkeyopolisModel monkeyopolis)
                    ? monkeyopolis.cratesPerRound
                    : filter.count;
                filter.count = Math.CeilToInt(current / __instance.overclockModel.rateModifier);
            });
        }
    }

    [HarmonyPatch(typeof(OverclockPermanentModel.OverclockPermanentMutator),
        nameof(OverclockPermanentModel.OverclockPermanentMutator.Mutate))]
    internal static class OverclockPermanentModel_Mutate
    {
        [HarmonyPostfix]
        internal static void Postfix(OverclockPermanentModel.OverclockPermanentMutator __instance, Model model)
        {
            if (!GetInstance<OverclockMonkeyopolis>().Enabled || !model.Is(out TowerModel towerModel) ||
                towerModel.baseId == TowerType.BananaFarm) return;

            var rateModifier = 1 - __instance.stacks * (1 - __instance.overclockPermanentModel.rateModifier);

            model.GetDescendants<EmissionsPerRoundFilterModel>().ForEach(filter =>
            {
                var current = towerModel.HasBehavior(out MonkeyopolisModel monkeyopolis)
                    ? monkeyopolis.cratesPerRound
                    : filter.count;
                filter.count = Math.CeilToInt(current / rateModifier);
            });
        }
    }

    [HarmonyPatch(typeof(Monkeyopolis.EmissionMutator), nameof(Monkeyopolis.EmissionMutator.Mutate))]
    internal static class EmissionMutator_Mutate
    {
        [HarmonyPrefix]
        internal static void Prefix(Monkeyopolis.EmissionMutator __instance, Model model, ref List<int> __state)
        {
            __state = model.GetDescendants<EmissionsPerRoundFilterModel>()
                .ToArray()
                .Select(filter => filter.count)
                .ToList();
        }

        [HarmonyPostfix]
        internal static void Postfix(Monkeyopolis.EmissionMutator __instance, Model model, ref List<int> __state)
        {
            foreach (var (filter, beforeCount) in model.GetDescendants<EmissionsPerRoundFilterModel>().ToArray()
                         .Zip(__state))
            {
                filter.count = Math.Max(beforeCount, filter.count);
            }
        }
    }
}