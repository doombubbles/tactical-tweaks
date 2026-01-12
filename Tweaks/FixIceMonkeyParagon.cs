using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;

namespace TacticalTweaks.Tweaks;

public class FixIceMonkeyParagon : TacticalTweak
{
    public override void OnNewGameModel(GameModel gameModel)
    {
        var iceMonkeyParagon = gameModel.GetParagonTower(TowerType.IceMonkey);
        iceMonkeyParagon.GetAbility().GetDescendant<AgeModel>().rounds = 999999;
    }
}