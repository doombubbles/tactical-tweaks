using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
namespace TacticalTweaks.Tweaks;

public class BetterMonkeyCity : ToggleableTweak
{
    protected override bool DefaultEnabled => false;

    public override string Description =>
        "Makes the Monkey City income buff affect more things, including Lead to Gold, IMF Loan, Monkey-Nomics, Monkey Pirates, and similar effects.";

    protected override string Icon => VanillaSprites.MonkeyCityUpgradeIcon;

    [HarmonyPatch(typeof(MonkeyCityIncomeSupport.MutatorTower), nameof(MonkeyCityIncomeSupport.MutatorTower.Mutate))]
    internal static class MonkeyCityIncomeSupport_Mutate
    {
        [HarmonyPostfix]
        internal static void Postfix(MonkeyCityIncomeSupport.MutatorTower __instance, Model model, ref bool __result)
        {
            if (!GetInstance<BetterMonkeyCity>().Enabled) return;

            foreach (var increaseBloonWorthModel in model.GetDescendants<IncreaseBloonWorthModel>().AsIEnumerable())
            {
                if (increaseBloonWorthModel.cash > 0)
                {
                    increaseBloonWorthModel.cash *= __instance.multiplier;
                }
                else
                {
                    increaseBloonWorthModel.cashMultiplier *= __instance.multiplier;
                }
                __result = true;
            }

            foreach (var imfLoanModel in model.GetDescendants<ImfLoanModel>().AsIEnumerable())
            {
                imfLoanModel.amount *= __instance.multiplier;
                imfLoanModel.imfLoanCollection.amount = imfLoanModel.amount;
                __result = true;
            }

            foreach (var moabTakedownModel in model.GetDescendants<MoabTakedownModel>().AsIEnumerable())
            {
                moabTakedownModel.multiplier *= __instance.multiplier;
                __result = true;
            }
        }
    }
}