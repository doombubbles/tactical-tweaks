using System.Collections.Generic;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Filters;
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

            var filters = new List<TowerFilterModel>();

            tower.GetDescendants<Model>().ForEach(model =>
            {
                if (model.Is(out ActivateRateSupportZoneModel activateRateSupportZoneModel))
                    filters.AddRange(activateRateSupportZoneModel.filters);
                if (model.Is(out ActivatePierceSupportZoneModel activatePierceSupportZoneModel))
                    filters.AddRange(activatePierceSupportZoneModel.filters);
                if (model.Is(out ActivateTowerDamageSupportZoneModel activateTowerDamageSupportZoneModel))
                    filters.AddRange(activateTowerDamageSupportZoneModel.filters);
                if (model.Is(out ActivateVisibilitySupportZoneModel activateVisibilitySupportZoneModel))
                    filters.AddRange(activateVisibilitySupportZoneModel.filters);
                if (model.Is(out PierceSupportModel pierceSupportModel))
                    filters.AddRange(pierceSupportModel.filters);
            });

            foreach (var towerFilterModel in filters)
            {
                if (towerFilterModel.Is(out FilterTowerByPlaceableAreaModel filterTowerByPlaceableAreaModel))
                {
                    filterTowerByPlaceableAreaModel.exclusive = false;
                }
            }
        }
    }
}