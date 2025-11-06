using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;

namespace TacticalTweaks.Tweaks;

public class AutoDepositBanks : ToggleableTweak
{
    protected override bool DefaultEnabled => false;

    public override string Description =>
        "When making a full collection from a Monkey Bank that has Deposits enabled, " +
        "half of the amount collected will be automatically deposited back.";

    protected override string Icon => VanillaSprites.IMFLoanUpgradeIcon;

    [HarmonyPatch(typeof(Bank), nameof(Bank.Collect))]
    internal static class Bank_Collect
    {
        [HarmonyPrefix]
        internal static void Prefix(Bank __instance, ref float __state)
        {
            __state = __instance.cash;
        }

        [HarmonyPostfix]
        internal static void Postfix(Bank __instance, float __state)
        {
            if (!GetInstance<AutoDepositBanks>().Enabled ||
                !__instance.tower.towerModel.HasBehavior<BankDepositsModel>() ||
                __state < __instance.bankModel.capacity) return;

            __instance.Sim.RemoveCash(__state / 2, Simulation.CashType.Ability, InGame.Bridge.MyPlayerNumber,
                Simulation.CashSource.BankDeposit);
            __instance.DepositCash(__state / 2);
        }
    }
}