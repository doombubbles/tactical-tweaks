using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Attack.Behaviors;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppSystem.IO;

namespace TacticalTweaks.Tweaks;

public class BuffableParagons : ToggleableTweak
{
    protected override float RegistrationPriority => -1;

    protected override bool DefaultEnabled => false;

    public override string Description => "Makes Paragons able to receive buffs like normal towers.";

    protected override string Icon => VanillaSprites.ParagonPip;

    public override void OnNewGameModel(GameModel gameModel)
    {
        if (!Enabled) return;

        gameModel.GetParagonTower(TowerType.BoomerangMonkey)
            .GetDescendants<SupportModel>()
            .ForEach(model => model.onlyAffectParagon = false);
    }

    /// <summary>
    /// Method that's used for checking paragon-ness for buffing, and not too many other places
    /// </summary>
    [HarmonyPatch(typeof(Tower), nameof(Tower.IsParagonBased))]
    internal static class Tower_IsParagonBased
    {
        [HarmonyPrefix]
        internal static bool Prefix(ref bool __result)
        {
            if (!GetInstance<BuffableParagons>().Enabled) return true;

            __result = false;
            return false;
        }
    }

    /// <summary>
    /// Make Overclock and Take Aim work
    /// </summary>
    [HarmonyPatch(typeof(TapTowerAbilityBehavior), nameof(TapTowerAbilityBehavior.IsBanned))]
    internal static class TapTowerAbilityBehavior_IsBanned
    {
        [HarmonyPrefix]
        internal static void Prefix(Tower tower, ref bool __state)
        {
            if (GetInstance<BuffableParagons>().Enabled)
            {
                __state = tower.towerModel.isParagon;
                tower.towerModel.isParagon = false;
            }
        }

        [HarmonyPostfix]
        internal static void Postfix(Tower tower, ref bool __state)
        {
            if (GetInstance<BuffableParagons>().Enabled)
            {
                tower.towerModel.isParagon = __state;
            }
        }
    }

    /// <summary>
    /// Make Alchemists work
    /// </summary>
    [HarmonyPatch(typeof(BrewTargetting), nameof(BrewTargetting.FilterTower))]
    internal static class BrewTargetting_FilterTower
    {
        [HarmonyPrefix]
        internal static void Prefix(Tower tower, ref bool __state)
        {
            if (GetInstance<BuffableParagons>().Enabled)
            {
                __state = tower.towerModel.isParagon;
                tower.towerModel.isParagon = false;
            }
        }

        [HarmonyPostfix]
        internal static void Postfix(Tower tower, ref bool __state)
        {
            if (GetInstance<BuffableParagons>().Enabled)
            {
                tower.towerModel.isParagon = __state;
            }
        }
    }

    /// <summary>
    /// Make Alchemists work
    /// </summary>
    [HarmonyPatch]
    internal static class TargetFriendly_GetTargets
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return MoreAccessTools.SafeGetNestedClassMethod(typeof(TargetFriendly),
                nameof(TargetFriendly.GetTargets));
        }


        [HarmonyPrefix]
        internal static void Prefix(ref Tower[] __state)
        {
            if (!GetInstance<BuffableParagons>().Enabled) return;

            __state = InGame.Bridge.Simulation.factory.GetUncast<Tower>()
                .ToArray()
                .Where(tower => tower.towerModel.isParagon)
                .ToArray();

            foreach (var tower in __state)
            {
                tower.towerModel.isParagon = false;
            }
        }

        [HarmonyPostfix]
        internal static void Postfix(ref Tower[] __state)
        {
            if (!GetInstance<BuffableParagons>().Enabled) return;

            foreach (var tower in __state)
            {
                tower.towerModel.isParagon = true;
            }
        }
    }

    /// <summary>
    /// Make sure Paragon specific mutators still apply
    /// </summary>
    [HarmonyPatch(typeof(Tower), nameof(Tower.AddParagonMutator))]
    internal static class Tower_AddParagonMutator
    {
        [HarmonyPrefix]
        internal static bool Prefix(Tower __instance,
            BehaviorMutator mutator,
            int time,
            bool updateDuration,
            bool applyMutation,
            bool onlyTimeoutWhenActive,
            bool useRoundTime,
            int roundsRemaining)
        {
            if (!GetInstance<BuffableParagons>().Enabled || !__instance.towerModel.isParagon) return true;

            __instance.AddMutator(mutator, time: time, updateDuration: updateDuration,
                applyMutation: applyMutation, onlyTimeoutWhenActive, useRoundTime, roundsRemaining: roundsRemaining);
            return false;
        }
    }

    /// <summary>
    /// Make sure Paragon specific mutators still apply
    /// </summary>
    [HarmonyPatch(typeof(Tower), nameof(Tower.AddParagonMutatorFromParent))]
    internal static class Tower_AddParagonMutatorFromParent
    {
        [HarmonyPrefix]
        internal static bool Prefix(Tower __instance,
            BehaviorMutator mutator,
            int time,
            bool updateDuration,
            bool applyMutation,
            bool onlyTimeoutWhenActive,
            bool useRoundTime,
            int roundsRemaining)
        {
            if (!GetInstance<BuffableParagons>().Enabled || !__instance.towerModel.isParagon) return true;

            __instance.AddMutatorFromParent(mutator, time: time, updateDuration: updateDuration,
                applyMutation: applyMutation, onlyTimeoutWhenActive, useRoundTime, roundsRemaining: roundsRemaining);
            return false;
        }
    }

    /// <summary>
    /// Make sure Paragon specific mutators still apply
    /// </summary>
    [HarmonyPatch(typeof(Tower), nameof(Tower.AddParagonMutatorIncludeSubTowers))]
    internal static class Tower_AddParagonMutatorIncludeSubTowers
    {
        [HarmonyPrefix]
        internal static bool Prefix(Tower __instance,
            BehaviorMutator mutator,
            int time,
            bool updateDuration,
            bool applyMutation,
            bool onlyTimeoutWhenActive,
            bool useRoundTime,
            int roundsRemaining)
        {
            if (!GetInstance<BuffableParagons>().Enabled || !__instance.towerModel.isParagon) return true;

            __instance.AddMutatorIncludeSubTowers(mutator, time: time, updateDuration: updateDuration,
                applyMutation: applyMutation, onlyTimeoutWhenActive, useRoundTime, roundsRemaining: roundsRemaining);
            return false;
        }
    }
}