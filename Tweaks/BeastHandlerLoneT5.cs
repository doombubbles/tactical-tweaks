using System.Collections.Generic;
using System.Linq;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppSystem.IO;

namespace TacticalTweaks.Tweaks;

public class BeastHandlerLoneT5s : ToggleableTweak
{
    public override string DisplayName => "Beast Handler Lone T5s";

    protected override bool DefaultEnabled => false;

    public override string Description =>
        """
        Allows Tier 5 Beast Handlers to be purchased even without any other Beast Handlers contributing to them.
        Megalodon still requires 84 Beast Power to insta kill BAD Bloons.
        """;

    protected override string Icon => VanillaSprites.BeastHandlerIcon;

    public override void OnNewGameModel(GameModel gameModel)
    {
        if (!Enabled) return;

        foreach (var beast in gameModel.GetTowersWithBaseId(TowerType.BeastHandler).ToArray()
                     .SelectMany(model => model.GetDescendants<TowerModel>().ToArray())
                     .SelectMany(model => model.GetDescendants<BeastHandlerPetModel>().ToArray()))
        {
            beast.requiredCount = 0;
        }
    }

    [HarmonyPatch(typeof(BeastHandlerLeashModel.ContributionMutator),
        nameof(BeastHandlerLeashModel.ContributionMutator.Mutate))]
    internal static class ContributionMutator_Mutate
    {
        [HarmonyPostfix]
        internal static void Postfix(BeastHandlerLeashModel.ContributionMutator __instance, Model model)
        {
            if (GetInstance<BeastHandlerLoneT5s>().Enabled && __instance.towerBaseId == TowerType.Piranha &&
                model.Is(out TowerModel towerModel) &&
                towerModel.tier >= 5 && __instance.power < 84)
            {
                model.GetDescendant<TowerModel>().GetDescendant<CreateGreatWhiteEffectModel>().maxGrabBloonName =
                    BloonType.Zomg;
            }
        }
    }

    private class T5Description : ModTextOverride
    {
        private string Upgrade { get; init; } = null!;

        public override string Name => Upgrade;

        public override string LocalizationKey => Upgrade + " Description";

        public override bool Active => GetInstance<BeastHandlerLoneT5s>().Enabled;

        public override IEnumerable<ModContent> Load()
        {
            yield return new T5Description {Upgrade = UpgradeType.Megalodon};
            yield return new T5Description {Upgrade = UpgradeType.Giganotosaurus};
            yield return new T5Description {Upgrade = UpgradeType.Pouakai};
        }

        public override string TextValue => OriginalText.RegexReplace(" Requires.*$", "");
    }
}