using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;

namespace TacticalTweaks.Tweaks;

public class AutoCollectBanks : ToggleableTweak
{
    protected override bool DefaultEnabled => false;
    public override string Description => "All Banks will auto collect when full like the XX2 Banks do.";

    protected override string Icon => VanillaSprites.MonkeyBankUpgradeIcon;

    public override void OnNewGameModel(GameModel gameModel)
    {
        foreach (var towerModel in gameModel.GetTowersWithBaseId(TowerType.BananaFarm).AsIEnumerable())
        {
            if (towerModel.HasBehavior(out BankModel bankModel))
            {
                bankModel.autoCollect = true;
            }
        }
    }
}