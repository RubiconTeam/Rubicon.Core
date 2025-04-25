namespace Rubicon.Core.Chart;

/// <summary>
/// Keeps track of a BPM / time signature change.
/// </summary>
[GlobalClass]
public partial class TimeChange : Resource
{
	/// <summary>
	/// The exact time this change happens in milliseconds.
	/// </summary>
	public float MsTime = 0f;

	/// <summary>
	/// The measure this change happens in.
	/// </summary>
	[Export] public float Time;
			
	/// <summary>
	/// The BPM to set with this change.
	/// </summary>
	[Export] public float Bpm;
		
	/// <summary>
	/// The number of beats in a measure with this change.
	/// </summary>
	[Export] public int TimeSignatureNumerator = 4;
		
	/// <summary>
	/// The type of note which equals one beat with this change.
	/// </summary>
	[Export] public int TimeSignatureDenominator = 4;

	/// <summary>
	/// Gets the value of a measure in milliseconds.
	/// </summary>
	/// <returns>The value of a measure in milliseconds.</returns>
	public float GetMeasureValue() => ConductorUtility.MeasureToMs(1f, Bpm, TimeSignatureNumerator);
	
	/// <summary>
	/// Gets the value of a beat in seconds.
	/// </summary>
	/// <returns>The value of a beat in milliseconds.</returns>
	public float GetBeatValue() => ConductorUtility.BeatsToMs(1f, Bpm);
	
	/// <summary>
	/// Gets the value of a step in seconds.
	/// </summary>
	/// <returns>The value of a step in milliseconds.</returns>
	public float GetStepValue() => ConductorUtility.StepsToMs(1f, Bpm, TimeSignatureDenominator);
}
