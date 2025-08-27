using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;

namespace TacticalTweaks.Tweaks;

public class ParagonSentryTargeting : ToggleableTweak
{
    protected override bool DefaultEnabled => true;

    public override string Description =>
        "Gives the Master Builder's Green Sentry First/Last/Close/Strong/Locked targeting.";

    protected override ModSettingCategory Category => TacticalTweaksMod.Targeting;

    protected override string Icon => VanillaSprites.SentryGreenAAIcon;

    public override void OnNewGameModel(GameModel gameModel)
    {
        if (!Enabled) return;

        var engineer = gameModel.GetParagonTower(TowerType.EngineerMonkey);
        var sentry = engineer.FindDescendant<TowerModel>(TowerType.SentryParagonGreen);
        var attackModel = sentry.GetAttackModel();

        TacticalTweaksMod.UpdatePointer(attackModel);
        TacticalTweaksMod.AddAllTargets(attackModel);
        
        sentry.UpdateTargetProviders();
    }
}