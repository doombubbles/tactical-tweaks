using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;

namespace TacticalTweaks.Tweaks;

public class ObynPriceTweak : ToggleableTweak
{
    protected override bool DefaultEnabled => false;

    public override string Description =>
        "Changes Obyn's base price so that he's able to be used as a CHIMPs start like he used to be.";

    protected override string Icon => VanillaSprites.ObynGreenFootIcon;

    public override void OnNewGameModel(GameModel gameModel)
    {
        var sauda = gameModel.GetTowerWithName(TowerType.Sauda);
        foreach (var towerModel in gameModel.GetTowersWithBaseId(TowerType.ObynGreenfoot).AsIEnumerable())
        {
            towerModel.cost = sauda.cost;
        }
    }
}