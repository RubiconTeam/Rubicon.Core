using Rubicon.Core.Data;

namespace Rubicon.Core.API;

/// <summary>
/// A template for a health bar in C#. Must be inherited.
/// </summary>
[GlobalClass] public abstract partial class CsHealthBar : CsHudElement
{
    /// <summary>
    /// What direction the bar filling goes.
    /// </summary>
    [Export] public BarDirection Direction = BarDirection.LeftToRight;
    
    /// <summary>
    /// The bar's color on the left side.
    /// </summary>
    [Export] public Color LeftColor
    {
        get => _leftColor;
        set
        {
            _leftColor = value;
            ChangeLeftColor(value);
        }
    }

    /// <summary>
    /// The bar's color on the right side.
    /// </summary>
    [Export] public Color RightColor
    {
        get => _rightColor;
        set
        {
            _rightColor = value;
            ChangeRightColor(value);
        }
    }

    private Color _leftColor = Colors.Red;
    private Color _rightColor = Colors.Green;
    
    protected int PreviousHealth = 0;
    protected BarDirection PreviousDirection = BarDirection.RightToLeft;

    public override void Initialize()
    {
        if (PlayField is null)
            return;

        Direction = PlayField.TargetIndex == 0 ? BarDirection.LeftToRight : BarDirection.RightToLeft;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        if (PlayField == null || PreviousHealth == PlayField.Health && PreviousDirection == Direction)
            return;
        
        UpdateBar((float)PlayField.Health / PlayField.MaxHealth, Direction);
        PreviousDirection = Direction;
        PreviousHealth = PlayField.Health;
    }
    
    protected abstract void UpdateBar(float progress, BarDirection direction);

    protected abstract void ChangeLeftColor(Color leftColor);
    
    protected abstract void ChangeRightColor(Color rightColor);
}