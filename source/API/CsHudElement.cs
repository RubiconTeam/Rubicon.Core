using Rubicon.Core.Rulesets;

namespace Rubicon.Core.API;

/// <summary>
/// A C# variant HUD element for use in-game in <see cref="PlayField"/>.
/// </summary>
[GlobalClass] public abstract partial class CsHudElement : Control, IPlayElement
{
    /// <inheritdoc/>
    public PlayField PlayField { get; set; }

    /// <inheritdoc />
    public abstract void Initialize();

    /// <summary>
    /// Triggers to make any adjustments for options that may have been changed.
    /// </summary>
    public abstract void OptionsUpdated();
}