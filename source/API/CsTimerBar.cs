namespace Rubicon.Core.API;

/// <summary>
/// A template for a bar that tracks the time left in-game.
/// </summary>
[GlobalClass] public abstract partial class CsTimerBar : CsCustomBar
{
    /// <summary>
    /// The length of the instrumental in seconds. Is modifiable just in case you want to screw around with it.
    /// </summary>
    [Export] public float Length = 0f;
    
    /// <inheritdoc />
    public override void Initialize()
    {
        if (PlayField != null)
            Length = (float)PlayField.Music.Stream.GetLength();
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (PlayField != null)
            ProgressRatio = Conductor.RawTime / Length;
        
        base._Process(delta);
    }
}