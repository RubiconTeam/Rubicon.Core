using Rubicon.Core.Data;

namespace Rubicon.Core.API;

/// <summary>
/// A template for a visual indicator that tracks a value.
/// </summary>
[GlobalClass] public abstract partial class CsCustomBar : CsHudElement
{
    /// <summary>
    /// How much progress this bar has made from 0.0 to 1.0.
    /// </summary>
    [Export] public float ProgressRatio = 0f;
    
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
            
            if (AffectLeftColor)
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
            
            if (AffectRightColor)
                ChangeRightColor(value);
        }
    }
    
    /// <summary>
    /// Whether the left side would be affected by the color.
    /// </summary>
    [Export] public bool AffectLeftColor = true;
    
    /// <summary>
    /// Whether the right side would be affected by the color.
    /// </summary>
    [Export] public bool AffectRightColor = true;

    private Color _leftColor = Colors.Red;
    private Color _rightColor = Colors.Green;

    public override void _Process(double delta)
    {
        base._Process(delta);
        UpdateBar();
    }
    
    /// <summary>
    /// Invoked every frame to update the progress bar.
    /// </summary>
    protected abstract void UpdateBar();

    /// <summary>
    /// Changes the left side's color to the one provided.
    /// </summary>
    /// <param name="leftColor">The new left color</param>
    protected abstract void ChangeLeftColor(Color leftColor);
    
    /// <summary>
    /// Changes the right side's color to the one provided.
    /// </summary>
    /// <param name="rightColor">The new right color</param>
    protected abstract void ChangeRightColor(Color rightColor);
}