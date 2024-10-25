using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
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

    public static MelonPreferences_Category Preferences { get; private set; } = null!;

    public override void OnApplicationStart()
    {
        Preferences = MelonPreferences.CreateCategory("TacticalTweaksPreferences");

        AccessTools.GetTypesFromAssembly(MelonAssembly.Assembly)
            .Where(type => !type.IsNested)
            .Do(ApplyHarmonyPatches);
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

    public override void OnRestart()
    {
        foreach (var usefulUtility in TacticalTweaks.Values)
        {
            usefulUtility.OnRestart();
        }
    }

    public override void OnNewGameModel(GameModel gameModel)
    {
        foreach (var usefulUtility in TacticalTweaks.Values)
        {
            usefulUtility.OnNewGameModel(gameModel);
        }

        var strikerJones = gameModel.GetHeroWithNameAndLevel(TowerType.StrikerJones, 20);
        strikerJones.AddBehavior(new SupportRemoveFilterOutTagModel("", "Striker:DdtDamageModifier",
            "Striker:Level9BlackBuff", null, true, false, 0, "", ""));
    }
}