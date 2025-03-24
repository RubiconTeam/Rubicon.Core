using Rubicon.Core.Data;
using Rubicon.Core.UI;

namespace Rubicon.Core.API;

/// <summary>
/// A template for a UI statistics (i.e. combo, judgment) element in C#. Must be inherited.
/// </summary>
[GlobalClass] public abstract partial class CsStatDisplay : CsHudElement
{
	/// <summary>
	/// A reference to the UI Style.
	/// </summary>
	public UiStyle UiStyle => PlayField.UiStyle;

    private bool _initialized = false;

    /// <summary>
    /// If it hasn't been initialized already, add itself to the play field.
    /// </summary>
    public override void Initialize()
    {
        base._Ready();

        if (_initialized)
	        return;
        
        PlayField.StatisticsUpdated += UpdateStats;
        _initialized = true;
    }

    /// <summary>
    /// Triggers when the player either hits or misses a note. Must be inherited!
    /// </summary>
    /// <param name="combo">The current combo</param>
    /// <param name="hit">The hit type</param>
    /// <param name="distance">The hit distance from the note</param>
    public abstract void UpdateStats(long combo, Judgment hit, float distance);

    /// <summary>
    /// A helper method to get the material based on the hit type.
    /// </summary>
    /// <param name="hit">The hit type / judgment</param>
    /// <returns>The corresponding material</returns>
    protected Material GetHitMaterial(Judgment hit)
    {
        switch (hit)
        {
            case Judgment.Perfect:
                return UiStyle.PerfectMaterial;
            case Judgment.Great:
                return UiStyle.GreatMaterial;
            case Judgment.Good:
                return UiStyle.GoodMaterial;
            case Judgment.Okay:
                return UiStyle.OkayMaterial;
            case Judgment.Bad:
                return UiStyle.BadMaterial;
            case Judgment.Miss:
                return UiStyle.MissMaterial;
        }

        return null;
    }

    /// <summary>
    /// A helper method to get the name based on the hit type.
    /// </summary>
    /// <param name="hit">The hit type / judgment</param>
    /// <returns>The corresponding name</returns>
    protected StringName GetHitName(Judgment hit)
    {
        switch (hit)
        {
            case Judgment.Perfect:
                return new StringName("Perfect");
            case Judgment.Great:
                return new StringName("Great");
            case Judgment.Good:
                return new StringName("Good");
            case Judgment.Okay:
                return new StringName("Okay");
            case Judgment.Bad:
                return new StringName("Bad");
            case Judgment.Miss:
                return new StringName("Miss");
        }

        return "";
    }
}