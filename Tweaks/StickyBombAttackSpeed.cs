using System.Linq;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Bloons.Behaviors;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers;

namespace TacticalTweaks.Tweaks;

public class StickyBombAttackSpeed : ToggleableTweak
{
    protected override bool DefaultEnabled => false;

    public override string Description =>
        "Makes attack speed buffs cause Sticky Bombs to blow up quicker. (Also affects Master Bomber)";

    protected override string Icon => VanillaSprites.StickyBombUpgradeIcon;

    public static void Apply(Tower? tower)
    {
        if (tower?.towerModel == null ||
            !GetInstance<StickyBombAttackSpeed>().Enabled ||
            tower.towerModel.appliedUpgrades?.Contains(UpgradeType.StickyBomb) != true ||
            tower.IsMutatedBy(nameof(StickyBombAttackSpeed))) return;

        tower.AddMutator(new RateSupportModel.RateSupportMutator(true, nameof(StickyBombAttackSpeed), 1, -100, null)
        {
            cantBeAbsorbed = true
        });
    }

    [HarmonyPatch(typeof(Tower), nameof(Tower.OnUpgraded))]
    internal static class Tower_OnUpgraded
    {
        [HarmonyPostfix]
        internal static void Postfix(Tower __instance) => Apply(__instance);
    }

    [HarmonyPatch(typeof(Tower), nameof(Tower.OnPlace))]
    internal static class Tower_OnPlace
    {
        [HarmonyPostfix]
        internal static void Postfix(Tower __instance) => Apply(__instance);
    }

    [HarmonyPatch(typeof(RateSupportModel.RateSupportMutator), nameof(RateSupportModel.RateSupportMutator.Mutate))]
    internal static class RateSupportMutator_Mutate
    {
        [HarmonyPrefix]
        internal static bool Prefix(RateSupportModel.RateSupportMutator __instance, Model baseModel, Model model,
            ref bool __result)
        {
            if (__instance.id != nameof(StickyBombAttackSpeed) || !baseModel.Is(out TowerModel baseTower) ||
                !model.Is(out TowerModel tower) || !GetInstance<StickyBombAttackSpeed>().Enabled) return true;

            var stickyBombs = tower.GetDescendants<AddBehaviorToBloonModel>().AsIEnumerable()
                .Where(m => m.mutationId is "StickyBomb" or "MasterSticky" or "StickyBombSplash");

            var originalSpeed = baseTower.GetAttackModel().weapons[0]!.Rate;
            var modifiedSpeed = tower.GetAttackModel().weapons[0]!.Rate;
            var reduction = modifiedSpeed / originalSpeed;

            foreach (var stickyBomb in stickyBombs)
            {
                stickyBomb.lifespan *= reduction;
                if (stickyBomb.lifespan < 0.05f) stickyBomb.lifespan = .05f;
                stickyBomb.lifespanFrames = (int) (stickyBomb.lifespan * 60);

                foreach (var damage in stickyBomb.GetBehaviors<DamageOverTimeModel>())
                {
                    damage.Interval = stickyBomb.lifespan - .02f;
                }
                foreach (var proj in stickyBomb.GetBehaviors<ProjectileOverTimeModel>())
                {
                    proj.Interval = stickyBomb.lifespan - .02f;

                }
            }

            __result = true;
            return false;
        }
    }
}