using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;

namespace TacticalTweaks.Tweaks;

public class BetterTrojan : ToggleableTweak
{
    protected override bool DefaultEnabled => false;

    public override string Description =>
        "Makes Benjamin's Bloon Trojan use the real internal layer count for Bloons so that it no longer loses money.";

    protected override string Icon => VanillaSprites.BenjaminMatrixPlacementIcon;

    [HarmonyPatch(typeof(IncreaseBloonWorthWithTierModel.BloonWorthMutator),
        nameof(IncreaseBloonWorthWithTierModel.BloonWorthMutator.Mutate))]
    internal static class IncreaseBloonWorthWithTierModel_BloonWorthMutator_Mutate
    {
        [HarmonyPrefix]
        internal static void Prefix(Model model, Model baseModel, ref int __state)
        {
            if (!GetInstance<BetterTrojan>().Enabled || !model.Is(out BloonModel bloonModel) ||
                !baseModel.Is(out BloonModel baseBloonModel)) return;

            __state = bloonModel.layerNumber;
            bloonModel.layerNumber = (int) baseBloonModel.GetTotalLayers(InGame.Bridge.Model);
        }

        [HarmonyPostfix]
        internal static void Postfix(Model model, ref int __state)
        {
            if (!GetInstance<BetterTrojan>().Enabled || !model.Is(out BloonModel bloonModel)) return;
            bloonModel.layerNumber = __state;
        }
    }
}