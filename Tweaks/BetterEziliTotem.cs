using System.Linq;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;

namespace TacticalTweaks.Tweaks;

public class BetterEziliTotem : ToggleableTweak
{
    protected override bool DefaultEnabled => false;
    public override string Description => "Makes Ezili's Sacrificial Totem not cost any lives";

    protected override string Icon => VanillaSprites.SacrificialTotemAA;

    public override void OnNewGameModel(GameModel gameModel)
    {
        for (var i = 7; i <= 20; i++)
        {
            var towerModel = gameModel.GetHeroWithNameAndLevel(TowerType.Ezili, i);
            var ability = towerModel.GetAbilities().FirstOrDefault(b => b.name.Contains("Totem"));

            if (ability != null)
            {
                ability.livesCost = 0;
            }
        }
    }
}