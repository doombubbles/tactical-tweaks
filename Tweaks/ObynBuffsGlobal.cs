using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;

namespace TacticalTweaks.Tweaks;

public class ObynBuffsGlobal : ToggleableTweak
{
    protected override bool DefaultEnabled => false;

    public override string Description =>
        "Makes Obyn's buff apply to all Druids, not just ones within range.";

    protected override string Icon => VanillaSprites.BuffIconObynLevel11;

    public override void OnNewGameModel(GameModel gameModel)
    {
        foreach (var towerModel in gameModel.GetTowersWithBaseId(TowerType.ObynGreenfoot).AsIEnumerable())
        {
            foreach (var supportModel in towerModel.GetBehaviors<SupportModel>())
            {
                supportModel.isGlobal = true;
            }
        }
    }
}