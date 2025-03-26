using Rubicon.Core.Data;

namespace Rubicon.Core.API;

/// <summary>
/// A template for a health bar in C#. Must be inherited.
/// </summary>
[GlobalClass] public abstract partial class CsHealthBar : CsCustomBar
{
    /// <inheritdoc />
    public override void Initialize()
    {
        if (PlayField is null)
            return;

        Direction = PlayField.TargetIndex == 0 ? BarDirection.LeftToRight : BarDirection.RightToLeft;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (PlayField != null)
            ProgressRatio = (float)PlayField.Health / PlayField.MaxHealth;
        
        base._Process(delta);
    }
}