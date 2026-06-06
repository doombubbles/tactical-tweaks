using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Weapons.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers;
using Newtonsoft.Json.Linq;
using TacticalTweaks;

[assembly: MelonInfo(typeof(TacticalTweaksMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
[assembly: HarmonyDontPatchAll]

namespace TacticalTweaks;

public class TacticalTweaksMod : BloonsTD6Mod
{
    public static readonly Dictionary<string, TacticalTweak> TacticalTweaks = new();

    public static readonly ModSettingCategory Overclock = new("Overclock")
    {
        icon = VanillaSprites.OverclockUpgradeIcon,
        order = 1
    };

    public static readonly ModSettingCategory Targeting = new("Targeting")
    {
        icon = VanillaSprites.CamoTargetIconCross,
        order = 2
    };

    public static readonly ModSettingBool DefaultToNewTargeting = new(true)
    {
        description = "Whether to default to the new targeting options, aka setting these towers to be First by default",
        category = Targeting
    };

    public static MelonPreferences_Category Preferences { get; private set; } = null!;

    public override void OnApplicationStart()
    {
        Preferences = MelonPreferences.CreateCategory("TacticalTweaksPreferences");

        AccessTools.GetTypesFromAssembly(MelonAssembly.Assembly)
            .Where(type => !type.IsNested)
            .Do(ApplyHarmonyPatches);
    }

    public static void AddAllTargets(AttackModel attackModel)
    {
        var prevTargets = attackModel.GetBehaviors<TargetSupplierModel>().ToList();

        attackModel.AddBehavior(TargetFirstModel.Create(new() { isSelectable = true }));
        attackModel.AddBehavior(TargetLastModel.Create(new() { isSelectable = true }));
        attackModel.AddBehavior(TargetCloseModel.Create(new() { isSelectable = true }));
        attackModel.AddBehavior(TargetStrongModel.Create(new() { isSelectable = true }));

        if (!DefaultToNewTargeting) return;

        foreach (var target in prevTargets)
        {
            attackModel.RemoveBehavior(target);
            attackModel.AddBehavior(target);
        }
    }

    public static void UpdatePointer(AttackModel attackModel, bool? rotateTower = null)
    {
        var pointer = attackModel.GetBehavior<RotateToPointerModel>();
        attackModel.AddBehavior(RotateToTargetModel.Create(new()
        {
            rotateOnlyOnThrow = pointer.rotateOnlyOnEmit,
            rotateTower = rotateTower ?? pointer.rotateTower
        }));

        if (attackModel.HasDescendant(out LineEffectModel lineEffectModel))
        {
            lineEffectModel.useRotateToPointer = false;
        }
    }

    public override void OnSaveSettings(JObject settings)
    {
        foreach (var usefulUtility in TacticalTweaks.Values)
        {
            usefulUtility.OnSaveSettings();
        }
    }

    public override void OnUpdate()
    {
        foreach (var usefulUtility in TacticalTweaks.Values)
        {
            usefulUtility.OnUpdate();
        }
    }

    public override void OnGameObjectsReset()
    {
        foreach (var usefulUtility in TacticalTweaks.Values)
        {
            usefulUtility.OnGameObjectsReset();
        }
    }

    public override void OnNewGameModel(GameModel gameModel)
    {
        foreach (var tacticalTweak in TacticalTweaks.Values)
        {
            tacticalTweak.OnNewGameModel(gameModel);
        }

        var strikerJones = gameModel.GetHeroWithNameAndLevel(TowerType.StrikerJones, 20);

        strikerJones?.AddBehavior(SupportRemoveFilterOutTagModel.Create(new()
        {
            mutatorId = "Striker:DdtDamageModifier",
            removeScriptWithSupportMutatorId = "Striker:Level9BlackBuff",
            isGlobal = true
        }));
    }

    public override void OnTowerSaved(Tower tower, TowerSaveDataModel saveData)
    {
        foreach (var tacticalTweak in TacticalTweaks.Values)
        {
            tacticalTweak.OnTowerSaved(tower, saveData);
        }
    }

    public override void OnTowerLoaded(Tower tower, TowerSaveDataModel saveData)
    {
        foreach (var tacticalTweak in TacticalTweaks.Values)
        {
            tacticalTweak.OnTowerLoaded(tower, saveData);
        }
    }
}
