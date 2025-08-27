using System.Linq;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2Cpp;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;

namespace TacticalTweaks.Tweaks;

public class BetterArmorPiercingDarts : ToggleableTweak
{
    protected override bool DefaultEnabled => false;

    public override string Description =>
        "Makes Monkey Subs with the Armor Piercing Darts upgrade to be able to pop Lead Bloons";

    protected override string Icon => VanillaSprites.ArmorPiercingDartsUpgradeIcon;

    public override void OnNewGameModel(GameModel gameModel)
    {
        foreach (var towerModel in gameModel.GetTowersWithBaseId(TowerType.MonkeySub)
                     .AsIEnumerable()
                     .Where(model => model.appliedUpgrades.Contains(UpgradeType.ArmorPiercingDarts)))
        {
            var damageModel = towerModel.GetWeapon()!.projectile.GetDamageModel()!;
            damageModel.immuneBloonProperties &= ~BloonProperties.Lead;
            damageModel.immuneBloonPropertiesOriginal &= ~BloonProperties.Lead;
        }
    }
}