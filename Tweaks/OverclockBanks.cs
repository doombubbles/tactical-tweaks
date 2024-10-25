using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;

namespace TacticalTweaks.Tweaks;

public class OverclockBanks : ToggleableTweak
{
    protected override bool DefaultEnabled => true;

    public override string Description => "Makes the Overclock buff affect the interest rates of Monkey Banks.";

    protected override string Icon => VanillaSprites.MonkeyBankUpgradeIcon;

    protected override ModSettingCategory Category => TacticalTweaksMod.Overclock;

    [HarmonyPatch(typeof(OverclockModel.OverclockMutator), nameof(OverclockModel.OverclockMutator.Mutate))]
    internal static class OverclockMutator_Mutate
    {
        [HarmonyPostfix]
        internal static void Postfix(OverclockModel.OverclockMutator __instance, Model model)
        {
            if (!GetInstance<OverclockBanks>().Enabled || !model.Is(out TowerModel towerModel)) return;

            if (towerModel.HasBehavior(out BankModel bankModel))
            {
                bankModel.interest /= __instance.overclockModel.rateModifier;
            }
        }
    }

    [HarmonyPatch(typeof(OverclockPermanentModel.OverclockPermanentMutator),
        nameof(OverclockPermanentModel.OverclockPermanentMutator.Mutate))]
    internal static class OverclockPermanentModel_Mutate
    {
        [HarmonyPostfix]
        internal static void Postfix(OverclockPermanentModel.OverclockPermanentMutator __instance, Model model)
        {
            if (!GetInstance<OverclockMonkeyopolis>().Enabled || !model.Is(out TowerModel towerModel)) return;

            var rateModifier = 1 - __instance.stacks * (1 - __instance.overclockPermanentModel.rateModifier);

            if (towerModel.HasBehavior(out BankModel bankModel))
            {
                bankModel.interest /= rateModifier;
            }
        }
    }
}