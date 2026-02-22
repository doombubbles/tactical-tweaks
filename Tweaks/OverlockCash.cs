using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;

namespace TacticalTweaks.Tweaks;

public class OverlockCash : ToggleableTweak
{
    protected override bool DefaultEnabled => true;

    public override string Description =>
        "Makes the Overclock buff increase other forms of cash generation for towers such as from end of round or from abilities.";

    protected override string Icon => VanillaSprites.MerchantmanUpgradeIcon;

    protected override ModSettingCategory Category => TacticalTweaksMod.Overclock;

    public static void Apply(TowerModel towerModel, float rateModifier)
    {
        foreach (var model in towerModel.GetDescendants<PerRoundCashBonusTowerModel>().AsIEnumerable())
        {
            model.cashPerRound /= rateModifier;
            model.cashRoundBonusMultiplier /= rateModifier;
        }

        foreach (var model in towerModel.GetDescendants<ImfLoanModel>().AsIEnumerable())
        {
            model.amount /= rateModifier;
        }

        foreach (var cashPerFarm in towerModel.GetDescendants<CashPerBananaFarmInRangeModel>().AsIEnumerable())
        {
            cashPerFarm.baseCash /= rateModifier;
            for (var i = 0; i < cashPerFarm.extraCashPerTier.Count; i++)
            {
                cashPerFarm.extraCashPerTier[i] /= rateModifier;
            }
        }

        if (towerModel.baseId != TowerType.BananaFarm && towerModel.baseId != TowerType.MonkeyVillage)
        {
            foreach (var cash in towerModel.GetDescendants<CashModel>().AsIEnumerable())
            {
                cash.bonusMultiplier += 1 / rateModifier - 1;
            }
        }
    }

    [HarmonyPatch(typeof(OverclockModel.OverclockMutator), nameof(OverclockModel.OverclockMutator.Mutate))]
    internal static class OverclockMutator_Mutate
    {
        [HarmonyPostfix]
        internal static void Postfix(OverclockModel.OverclockMutator __instance, Model model)
        {
            if (!GetInstance<OverlockCash>().Enabled || !model.Is(out TowerModel towerModel)) return;

            var rateModifier = __instance.overclockModel.rateModifier;

            Apply(towerModel, rateModifier);
        }
    }

    [HarmonyPatch(typeof(OverclockPermanentModel.OverclockPermanentMutator),
        nameof(OverclockPermanentModel.OverclockPermanentMutator.Mutate))]
    internal static class OverclockPermanentModel_Mutate
    {
        [HarmonyPostfix]
        internal static void Postfix(OverclockPermanentModel.OverclockPermanentMutator __instance, Model model)
        {
            if (!GetInstance<OverlockCash>().Enabled || !model.Is(out TowerModel towerModel)) return;

            var rateModifier = 1 - __instance.stacks * (1 - __instance.overclockPermanentModel.rateModifier);

            Apply(towerModel, rateModifier);
        }
    }
}