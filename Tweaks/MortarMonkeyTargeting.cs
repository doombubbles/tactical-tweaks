using System.Linq;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;

namespace TacticalTweaks.Tweaks;

public class MortarMonkeyTargeting : ToggleableTweak
{
    protected override bool DefaultEnabled => true;

    public override string Description => "Gives all Mortar Monkeys First/Last/Close/Strong targeting";

    protected override ModSettingCategory Category => TacticalTweaksMod.Targeting;

    protected override string Icon => VanillaSprites.MortarMonkeyIcon;

    protected override string DisableIfModPresent => "MegaKnowledge";

    public override void OnNewGameModel(GameModel gameModel)
    {
        if (!Enabled) return;

        foreach (var model in gameModel.towers.Where(model => model.baseId == TowerType.MortarMonkey))
        {
            var attackModel = model.GetAttackModel();
            
            TacticalTweaksMod.AddAllTargets(attackModel);
            
            model.towerSelectionMenuThemeId = "ActionButton";
            model.UpdateTargetProviders();
        }
    }
}