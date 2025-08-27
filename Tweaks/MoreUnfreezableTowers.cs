using BTD_Mod_Helper.Api.Enums;
using HarmonyLib;
using Il2CppAssets.Scripts;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Simulation.SimulationBehaviors;
using Il2CppAssets.Scripts.Simulation.Towers.TowerFilters;

namespace TacticalTweaks.Tweaks;

public class MoreUnfreezableTowers : ToggleableTweak
{
    protected override bool DefaultEnabled => true;

    public override string Description =>
        """
        Fire Towers and certain other Ice Towers also no longer get frozen on Glacial Trail.
        This includes: Gwen (Any Skin), Fusty the Snowman, Ring of Fire Tack Shooters, Dragon's Breath Wizards, Signal Flare Mortar Monkeys, Tidal Chill Mermonkeys, The Blazing Sun.
        """;

    protected override string Icon => VanillaSprites.MapSelectGlacialTrailButton;

    [HarmonyPatch(typeof(ApplyTowerFreeze), nameof(ApplyTowerFreeze.FreezeTower))]
    internal static class ApplyTowerFreeze_FreezeTower
    {
        [HarmonyPrefix]
        internal static bool Prefix(ApplyTowerFreeze __instance, ObjectId towerID)
        {
            if (!GetInstance<MoreUnfreezableTowers>().Enabled) return true;

            var tower = __instance.Sim.towerManager.GetTowerById(towerID);
            var towerModel = tower.towerModel;

            return towerModel.appliedUpgrades?.Contains(UpgradeType.RingOfFire) != true &&
                   towerModel.appliedUpgrades?.Contains(UpgradeType.SignalFlare) != true &&
                   towerModel.appliedUpgrades?.Contains(UpgradeType.DragonsBreath) != true &&
                   towerModel.appliedUpgrades?.Contains(UpgradeType.TidalChill) != true &&
                   towerModel.appliedUpgrades?.Contains(UpgradeType.TheBlazingSun) != true &&
                   towerModel.baseId != TowerType.Gwendolin &&
                   tower.towerSkinName != "Fusty the Snowman";
        }
    }

    [HarmonyPatch(typeof(PyrotechnicsSupportFilter), nameof(PyrotechnicsSupportFilter.FilterTowerModel))]
    internal static class PyrotechnicsSupportFilter_FilterTowerModel
    {
        [HarmonyPostfix]
        internal static void Postfix(TowerModel towerModel, ref bool __result)
        {
            __result |= towerModel.baseId == TowerType.Desperado &&
                        towerModel.appliedUpgrades?.Contains(UpgradeType.TheBlazingSun) == true;
        }
    }
}