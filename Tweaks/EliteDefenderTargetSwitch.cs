using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Abilities;

namespace TacticalTweaks.Tweaks;

public class EliteDefenderTargetSwitch : ToggleableTweak
{
    protected override bool DefaultEnabled => false;

    public override string Description =>
        "An Elite Defender will switch to First targeting after a Bloon leak activates its ability.";

    protected override string Icon => VanillaSprites.EliteDefenderUpgradeIcon;

    protected override ModSettingCategory Category => TacticalTweaksMod.Targeting;

    [HarmonyPatch(typeof(Ability), nameof(Ability.Activate))]
    internal static class Ability_Activate
    {
        [HarmonyPostfix]
        internal static void Postfix(Ability __instance)
        {
            if (GetInstance<EliteDefenderTargetSwitch>().Enabled &&
                __instance.abilityModel?.displayName == "Retaliation")
            {
                __instance.tower.SetTargetType(TargetType.first);
            }
        }
    }
}