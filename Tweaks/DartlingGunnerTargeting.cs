using System.Linq;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Weapons.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Attack.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers.Weapons.Behaviors;

namespace TacticalTweaks.Tweaks;

public class DartlingGunnerTargeting : ToggleableTweak
{
    protected override bool DefaultEnabled => true;

    public override string Description => "Gives all Dartling Gunners First/Last/Close/Strong targeting";

    protected override ModSettingCategory Category => TacticalTweaksMod.Targeting;

    protected override string Icon => VanillaSprites.DartlingGunnerIcon;

    protected override string DisableIfModPresent => "MegaKnowledge";

    public override void OnNewGameModel(GameModel gameModel)
    {
        if (!Enabled) return;

        foreach (var model in gameModel.towers.Where(model =>
                     model.baseId == TowerType.DartlingGunner &&
                     !model.appliedUpgrades.Contains(UpgradeType.BloonAreaDenialSystem)))
        {
            var attackModel = model.GetAttackModel();

            var rotate = !model.appliedUpgrades.Contains(UpgradeType.RayOfDoom) &&
                         !model.appliedUpgrades.Contains(UpgradeType.Buckshot);

            TacticalTweaksMod.UpdatePointer(attackModel, rotate);
            TacticalTweaksMod.AddAllTargets(attackModel);

            model.UpdateTargetProviders();
        }
    }

    [HarmonyPatch(typeof(RotateToPointer), nameof(RotateToPointer.SetRotation))]
    internal static class RotateToPointer_SetRotation
    {
        [HarmonyPrefix]
        internal static bool Prefix(RotateToPointer __instance)
        {
            var attack = __instance.attack;
            if (!GetInstance<DartlingGunnerTargeting>().Enabled ||
                !attack.activeTargetSupplier.Is(out var target)) return true;

            var hasPointer = false;
            var hasTarget = false;

            foreach (var attackBehavior in attack.attackBehaviors)
            {
                hasPointer |= attackBehavior.Is<RotateToPointer>();
                hasTarget |= attackBehavior.Is<RotateToTarget>();
            }

            if (!hasPointer || !hasTarget) return true;

            var targetsBloon = target.Is<TargetFirst>() || target.Is<TargetLast>() ||
                               target.Is<TargetClose>() || target.Is<TargetStrong>() || target.Is<TargetCamo>();

            return !targetsBloon;
        }
    }

    [HarmonyPatch(typeof(RotateToTarget), nameof(RotateToTarget.ApplyRotation))]
    internal static class RotateToTarget_ApplyRotation
    {
        [HarmonyPrefix]
        internal static bool Prefix(RotateToTarget __instance)
        {
            var attack = __instance.attack;
            if (!GetInstance<DartlingGunnerTargeting>().Enabled ||
                !attack.activeTargetSupplier.Is(out var target)) return true;

            var hasPointer = false;
            var hasTarget = false;

            foreach (var attackBehavior in attack.attackBehaviors)
            {
                hasPointer |= attackBehavior.Is<RotateToPointer>();
                hasTarget |= attackBehavior.Is<RotateToTarget>();
            }

            if (!hasPointer || !hasTarget) return true;

            var targetsBloon = target.Is<TargetFirst>() || target.Is<TargetLast>() ||
                               target.Is<TargetClose>() || target.Is<TargetStrong>() || target.Is<TargetCamo>();

            return targetsBloon;
        }
    }
}