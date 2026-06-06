using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;

namespace TacticalTweaks.Tweaks;

public class EtienneTargeting : ToggleableTweak
{
    protected override bool DefaultEnabled => false;

    public override string Description =>
        "Replaces Etienne's targeting options with the standard Fist, Last, Close Strong.";

    protected override string Icon => VanillaSprites.EtienneIcon;

    protected override ModSettingCategory Category => TacticalTweaksMod.Targeting;

    public override void OnNewGameModel(GameModel gameModel)
    {
        if (!Enabled) return;

        foreach (var towerModel in gameModel.GetTowersWithBaseId(TowerType.Etienne).AsIEnumerable())
        {
            var attack = towerModel.GetAttackModel();
            attack.RemoveBehaviors<TargetDivideAndConquerModel>();
            attack.RemoveBehaviors<TargetZoneDefenceModel>();
            attack.AddBehavior(TargetLastModel.Create(new() { isSelectable = true }));
            attack.AddBehavior(TargetCloseModel.Create(new() { isSelectable = true }));
            attack.AddBehavior(TargetStrongModel.Create(new() { isSelectable = true }));

            towerModel.UpdateTargetProviders();
        }
    }
}