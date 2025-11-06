using System.Linq;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppSystem.Linq;

namespace TacticalTweaks.Tweaks;

public class BetterLaserBlasts : ToggleableTweak
{
    protected override bool DefaultEnabled => false;

    public override string Description =>
        """
        Makes the Laser Blasts upgrade give a minor Attack Speed buff for Super Monkeys. Plasma Blasts unaffected.
        Rates for 0/0/0, 1/0/0, 2/0/0: .045, 0.45, .03 -> .045, .04, .03
        """;

    protected override string Icon => VanillaSprites.LaserBlastUpgradeIcon;

    public override void OnNewGameModel(GameModel gameModel)
    {
        if (!Enabled) return;

        foreach (var towerModel in gameModel.GetTowersWithBaseId(TowerType.SuperMonkey).ToArray()
                     .Where(model => model.tiers[0] == 1))
        {
            var baseTower = gameModel.GetTower(TowerType.SuperMonkey, 0, towerModel.tiers[1], towerModel.tiers[2]);
            var plasmaTower = gameModel.GetTower(TowerType.SuperMonkey, 2, towerModel.tiers[1], towerModel.tiers[2]);

            var baseSpeed = baseTower.GetAttackModel().weapons[0]!.Rate;
            var plasmaSpeed = plasmaTower.GetAttackModel().weapons[0]!.Rate;

            towerModel.GetAttackModel().weapons[0]!.Rate = (baseSpeed * 2 + plasmaSpeed) / 3;
        }
    }
}