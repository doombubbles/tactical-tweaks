using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.TowerFilters;

namespace TacticalTweaks.Tweaks;

public class BrickellMermonkeys : ToggleableTweak
{
    protected override bool DefaultEnabled => true;

    public override string Description => "Makes Brickell's effects apply to non-exclusively water based towers again.";

    protected override string Icon => VanillaSprites.NavalTacticsAA;

    public override void OnNewGameModel(GameModel gameModel)
    {
        if (!Enabled) return;

        foreach (var tower in gameModel.towers)
        {
            if (tower.baseId != TowerType.AdmiralBrickell) continue;

            tower.GetDescendants<ActivateRateSupportZoneModel>()
                .ForEach(model => model.AddChildDependants(model.filters));
            tower.GetDescendants<ActivatePierceSupportZoneModel>()
                .ForEach(model => model.AddChildDependants(model.filters));
            tower.GetDescendants<ActivateTowerDamageSupportZoneModel>()
                .ForEach(model => model.AddChildDependants(model.filters));
            tower.GetDescendants<ActivateVisibilitySupportZoneModel>()
                .ForEach(model => model.AddChildDependants(model.filters));
            tower.GetDescendants<PierceSupportModel>()
                .ForEach(model => model.AddChildDependants(model.filters));

            tower.GetDescendants<FilterTowerByPlaceableAreaModel>().ForEach(model => model.exclusive = false);
        }
    }
}