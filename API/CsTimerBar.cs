namespace Rubicon.Core.API;

[GlobalClass] public abstract partial class CsTimerBar : CsHudElement
{
    private float _length = 0f;

    public override void Initialize()
    {
        base._Ready();
        
        _length = (float)PlayField.Music.Stream.GetLength();
    }

    public override void _Process(double delta)
    {
        UpdateTimer(Conductor.Time, _length);
    }

    public abstract void UpdateTimer(float currentTime, float length);
}