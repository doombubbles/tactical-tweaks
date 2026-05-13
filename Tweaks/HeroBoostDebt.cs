using System;
using System.Linq;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models.SimulationBehaviors;
using Il2CppAssets.Scripts.Simulation.SimulationBehaviors;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;

namespace TacticalTweaks.Tweaks;

public class HeroBoostDebt : ToggleableTweak
{
    protected override bool DefaultEnabled => true;

    public override string Description =>
        "Replaces the Monkey Money Hero Boost with a button that lets you purchase the next level up of the hero by going into debt.";

    protected override string Icon => VanillaSprites.HeroBoost;

    [HarmonyPatch(typeof(TowerSelectionMenu), nameof(TowerSelectionMenu.UpdateHeroBooster))]
    internal static class TowerSelectionMenu_UpdateHeroBooster
    {
        [HarmonyPostfix]
        internal static void Postfix(TowerSelectionMenu __instance)
        {
            if (!GetInstance<HeroBoostDebt>().Enabled) return;

            __instance.heroBoosterButtonText.SetText("▼$");

            if (__instance.Bridge.simulation.GetBehaviors().ToArray().OfIl2CppType<ImfLoanCollection>().Any())
            {
                __instance.heroBoosterButton.interactable = false;
            }
        }
    }

    [HarmonyPatch(typeof(TowerSelectionMenu), nameof(TowerSelectionMenu.OnHeroBoosterButtonClicked))]
    internal static class TowerSelectionMenu_OnHeroBoosterButtonClicked
    {
        [HarmonyPrefix]
        internal static bool Prefix(TowerSelectionMenu __instance)
        {
            if (!GetInstance<HeroBoostDebt>().Enabled) return true;

            var tower = __instance.selectedTower;

            PopupScreen.instance.ShowPopup(PopupScreen.Placement.inGameCenter, "Hero Boost Debt",
                $"Do you want to boost to level {tower.hero.level + 1}?\nYou will gain a debt of ${tower.hero.costToLevelUp:N0}",
                new Action(() =>
                {
                    __instance.Bridge.UpgradeHeroToLevel(tower.Id, new Action<bool>(_ => { }), tower.hero.level + 1);

                    __instance.Bridge.Simulation.AddBehavior<ImfLoanCollection>(
                        new ImfLoanCollectionModel(nameof(HeroBoostDebt), .5f, tower.hero.costToLevelUp));

                    __instance.UpdateHeroBooster(tower);
                }), "Yes", new Action(() =>
                {
                    __instance.UpdateHeroBooster(tower);
                }), "No", Popup.TransitionAnim.Scale, PopupScreen.BackGround.Grey);

            return false;
        }
    }
}