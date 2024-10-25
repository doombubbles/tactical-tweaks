using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Simulation.SMath;

namespace TacticalTweaks.Tweaks;

public class OverclockPeriodicAbilities : ToggleableTweak
{
    protected override bool DefaultEnabled => true;

    public override string Description =>
        "Makes the Overclock buff affect the frequency of periodic ability activations like Carpet of Spikes.";

    protected override string Icon => VanillaSprites.CarpetOfSpikesUpgradeIcon;

    protected override ModSettingCategory Category => TacticalTweaksMod.Overclock;

    [HarmonyPatch(typeof(OverclockModel.OverclockMutator), nameof(OverclockModel.OverclockMutator.Mutate))]
    internal static class OverclockMutator_Mutate
    {
        [HarmonyPostfix]
        internal static void Postfix(OverclockModel.OverclockMutator __instance, Model model)
        {
            if (!GetInstance<OverclockPeriodicAbilities>().Enabled) return;

            model.GetDescendants<ActivateAbilityAfterIntervalModel>().ForEach(i =>
            {
                i.interval *= __instance.overclockModel.rateModifier;
                i.intervalFrames = Math.CeilToInt(i.intervalFrames * __instance.overclockModel.rateModifier);
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
            if (!GetInstance<OverclockPeriodicAbilities>().Enabled) return;

            var rateModifier = 1 - __instance.stacks * (1 - __instance.overclockPermanentModel.rateModifier);

            model.GetDescendants<ActivateAbilityAfterIntervalModel>().ForEach(i =>
            {
                i.interval *= rateModifier;
                i.intervalFrames = Math.CeilToInt(i.intervalFrames * rateModifier);
            });
        }
    }
}