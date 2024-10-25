using BTD_Mod_Helper.Api.ModOptions;

namespace TacticalTweaks;

public abstract class ToggleableTweak : TacticalTweak
{
    private ModSettingBool setting = null!;
    protected abstract bool DefaultEnabled { get; }

    public bool Enabled => setting;

    public abstract override string Description { get; }

    protected virtual ModSettingCategory Category => null!;

    public override void OnLoad()
    {
        mod.ModSettings[Name] = setting = new ModSettingBool(DefaultEnabled)
        {
            displayName = DisplayName,
            icon = Icon,
            category = Category,
            description = Description,
            source = this
        };
    }
}