using PukiTools.GodotSharp;
using Rubicon.Core.Chart;

namespace Rubicon.Core;

/// <summary>
/// A Node that keeps track of musical timing and what not.
/// </summary>
[GlobalClass, Autoload("Conductor")]
public partial class ConductorInstance : Node
{
	/// <summary>
	/// The current BPM at the moment.
	/// </summary>
	[Export] public float Bpm = 100;

	/// <summary>
	/// The index pointing to which BPM is currently set based on the bpms array.
	/// </summary>
	[Export] public int BpmIndex = 0;

	/// <summary>
	/// Corrects the time so the chart can be accurate.
	/// </summary>
	[Export] public float ChartOffset = 0f;

	/// <summary>
	/// How fast the Conductor goes.
	/// </summary>
	[Export] public float Speed = 1f;

	/// <summary>
	/// Is true when the Conductor has been started with Start() or Play(), false when either Pause() or Stop() is called.
	/// </summary>
	[Export] public bool Playing = false;
	
	/// <summary>
	/// The raw timestamp of this Conductor, without any corrections made to it.
	/// </summary>
	public float RawTime => GetRawTime();
		
	/// <summary>
	/// The raw timestamp of this Conductor + the chart offset.
	/// </summary>
	public float UncorrectedTime => GetUncorrectedTime();

	/// <summary>
	/// The current timestamp from when the time was last set.
	/// Equivalent to Conductor.songPosition in most other FNF projects.
	/// </summary>
	[Export] public float Time { get => GetTime(); set => SetTime(value); }

	/// <summary>
	/// The current step according to the time, which also keeps BPM changes in mind.
	/// </summary>
	public float CurrentStep => GetCurrentStep();

	/// <summary>
	/// The current beat according to the time, which also keeps BPM changes in mind.
	/// </summary>
	public float CurrentBeat => GetCurrentBeat();

	/// <summary>
	/// The current measure according to the time, which also keeps BPM changes in mind.
	/// </summary>
	public float CurrentMeasure => GetCurrentMeasure();
	
	/// <summary>
	/// Get all BPMs listed in the Conductor currently.
	/// </summary>
	[ExportGroup("Info"), Export] public BpmInfo[] BpmList { get => GetBpmList(); set => SetBpmList(value); }
		
	/// <summary>
	/// A whole number that indicates the number of beats in each measure.
	/// </summary>
	[Export] public int TimeSigNumerator = 4;
		
	/// <summary>
	/// A whole number representing how many steps are in a beat.
	/// </summary>
	[Export] public int TimeSigDenominator = 4;

	/// <summary>
	/// How long a measure is in milliseconds.
	/// </summary>
	[Export] public float MeasureValue;
	
	/// <summary>
	/// How long a beat is in milliseconds. Equivalent to "Conductor.crochet".
	/// </summary>
	[Export] public float BeatValue;
	
	/// <summary>
	/// How long a step is in milliseconds. Equivalent to "Conductor.stepCrochet".
	/// </summary>
	[Export] public float StepValue;

	/// <summary>
	/// Event triggered when the next beat is hit.
	/// </summary>
	[Signal] public delegate void BeatHitEventHandler(int beat);

	/// <summary>
	/// Event triggered when the next step is hit.
	/// </summary>
	[Signal] public delegate void StepHitEventHandler(int step);
	
	/// <summary>
	/// Event triggered when the next measure is hit.
	/// </summary>
	[Signal] public delegate void MeasureHitEventHandler(int measure);
	
	/// <summary>
	/// Event triggered when the next bpm change is done.
	/// </summary>
	[Signal] public delegate void BpmChangedEventHandler(BpmInfo currentBpm);

	#region Private Fields
	private double _relativeStartTime = 0;
	private double _relativeTimeOffset = 0;
	private float _lastTime = float.NegativeInfinity;
	private float _delta = 0f;
	private double _time = 0d;
		
	private float _cachedStep;
	private float _cachedStepTime;

	private float _cachedBeat;
	private float _cachedBeatTime;

	private float _cachedMeasure;
	private float _cachedMeasureTime;
		
	private BpmInfo[] _bpmList = [new() { Bpm = 100 }];

	private int _lastBeat = -int.MaxValue;
	private int _lastStep = -int.MaxValue;
	private int _lastMeasure = -int.MaxValue;
	#endregion

	public override void _Ready()
	{
		base._Ready();

		ProcessMode = ProcessModeEnum.Always;
	}

	public override void _Process(double delta)
	{
		if (!Playing)
		{
			_relativeStartTime = Godot.Time.GetUnixTimeFromSystem();
			_relativeTimeOffset = _time;
		}

		base._Process(delta);
			
		// Handles bpm changing
		if (BpmIndex < BpmList.Length - 1 && BpmList[BpmIndex + 1].MsTime / 1000f <= Time)
		{
			BpmIndex++;
			
			Bpm = BpmList[BpmIndex].Bpm;
			TimeSigNumerator = BpmList[BpmIndex].TimeSignatureNumerator;
			TimeSigDenominator = BpmList[BpmIndex].TimeSignatureDenominator;
			
			MeasureValue = ConductorUtility.MeasureToMs(1f, Bpm, TimeSigNumerator);
			BeatValue = ConductorUtility.BeatsToMs(1f, Bpm);
			StepValue = ConductorUtility.StepsToMs(1f, Bpm, TimeSigDenominator);
			
			EmitSignalBpmChanged(BpmList[BpmIndex]);
		}
		
		int curMeasure = Mathf.FloorToInt(CurrentMeasure);
		int curBeat = Mathf.FloorToInt(CurrentBeat);
		int curStep = Mathf.FloorToInt(CurrentStep);

		if (curMeasure != _lastMeasure)
			EmitSignal(SignalName.MeasureHit, curMeasure);

		if (curBeat != _lastBeat)
			EmitSignal(SignalName.BeatHit, curBeat);

		if (curStep != _lastStep)
			EmitSignal(SignalName.StepHit, curStep);

		_lastMeasure = curMeasure;
		_lastBeat = curBeat;
		_lastStep = curStep;
	}
		
	/// <summary>
	/// Starts the Conductor at the time provided.
	/// </summary>
	/// <param name="time">The time the Conductor starts at. Default is 0</param>
	public void Play(float time = 0f)
	{
		Resume();
		Time = time;
	}

	/// <summary>
	/// Resumes the Conductor at the last time it was paused at.
	/// </summary>
	public void Resume()
	{
		Playing = true;
	}

	/// <summary>
	/// Stops the Conductor from ticking, but keeps the current time.
	/// </summary>
	public void Pause()
	{
		_time = Time;
		Playing = false;
	}

	/// <summary>
	/// Stops the Conductor entirely, resetting its time to 0.
	/// </summary>
	public void Stop()
	{
		Time = 0;
		Pause();
	}
		
	/// <summary>
	/// Resets the Conductor, wiping all its fields with its default values.
	/// </summary>
	public void Reset()
	{
		TimeSigNumerator = TimeSigDenominator = 4;
		BpmList = [new BpmInfo { Bpm = 100 }];
		Bpm = BpmList[0].Bpm;
		BpmIndex = 0;
		ChartOffset = 0;
		Speed = 1f;
		Stop();
	}
		
	#region Getters and Setters
	/// <summary>
	/// Gets the raw time of this Conductor, without any corrections made to it.
	/// </summary>
	/// <returns>The raw time, in seconds</returns>
	public float GetRawTime()
	{
		return Playing ? (float)(Godot.Time.GetUnixTimeFromSystem() - _relativeStartTime + _relativeTimeOffset) :
			_time != 0d ? (float)_time : 0f;
	}

	/// <summary>
	/// Gets the raw time of this Conductor + the chart offset.
	/// </summary>
	/// <returns>The raw time + the chart offset, in seconds</returns>
	public float GetUncorrectedTime()
	{
		return RawTime + ChartOffset;
	}

	/// <summary>
	/// Gets the calculated time of this Conductor.
	/// </summary>
	/// <returns>The raw time + the chart offset multiplied by the speed, in seconds</returns>
	public float GetTime()
	{
		return UncorrectedTime * Speed;
	}

	/// <summary>
	/// Sets the time of this Conductor.
	/// </summary>
	/// <param name="time">The time to set it to, in seconds.</param>
	public void SetTime(float time)
	{
		_time = time;
		_relativeStartTime = Godot.Time.GetUnixTimeFromSystem();
		_relativeTimeOffset = time;
	}

	/// <summary>
	/// Gets the current step of this Conductor, with decimals.
	/// </summary>
	/// <returns>The current step</returns>
	public float GetCurrentStep()
	{
		if (_cachedStepTime == Time)
			return _cachedStep;

		if (BpmList.Length <= 1)
			return Time / (60f / (Bpm * TimeSigDenominator));

		_cachedStepTime = Time;
		_cachedStep = (Time - BpmList[BpmIndex].MsTime / 1000f) / (60f / (Bpm * TimeSigDenominator)) + (BpmList[BpmIndex].Time * TimeSigNumerator * TimeSigDenominator);
		return _cachedStep;
	}

	/// <summary>
	/// Gets the current beat of this Conductor, with decimals.
	/// </summary>
	/// <returns>The current beat</returns>
	public float GetCurrentBeat()
	{
		if (_cachedBeatTime == Time)
			return _cachedBeat;

		if (BpmList.Length <= 1)
			return Time / (60f / Bpm);

		_cachedBeatTime = Time;
		_cachedBeat = (Time - BpmList[BpmIndex].MsTime / 1000f) / (60f / Bpm) + BpmList[BpmIndex].Time * TimeSigNumerator;
		return _cachedBeat;
	}

	/// <summary>
	/// Gets the current measure of this Conductor, with decimals.
	/// </summary>
	/// <returns>The current measure</returns>
	public float GetCurrentMeasure()
	{
		if (_cachedMeasureTime == Time)
			return _cachedMeasure;

		if (BpmList.Length <= 1)
			return Time / (60f / (Bpm / TimeSigNumerator));

		_cachedMeasureTime = Time;
		_cachedMeasure = (Time - BpmList[BpmIndex].MsTime / 1000f) / (60f / (Bpm / TimeSigNumerator)) + BpmList[BpmIndex].Time;
		return _cachedMeasure;
	}

	/// <summary>
	/// Get all BPMs listed in the Conductor currently.
	/// </summary>
	/// <returns>All BPMs listed in the Conductor</returns>
	public BpmInfo[] GetBpmList() => _bpmList;
		
	/// <summary>
	/// Set the BPM list in the Conductor.
	/// </summary>
	/// <param name="data">An array of BpmInfos</param>
	public void SetBpmList(BpmInfo[] data)
	{
		_bpmList = data;

		BpmIndex = 0;
		
		Bpm = BpmList[BpmIndex].Bpm;
		TimeSigNumerator = BpmList[BpmIndex].TimeSignatureNumerator;
		TimeSigDenominator = BpmList[BpmIndex].TimeSignatureDenominator;
			
		MeasureValue = ConductorUtility.MeasureToMs(1f, Bpm, TimeSigNumerator);
		BeatValue = ConductorUtility.BeatsToMs(1f, Bpm);
		StepValue = ConductorUtility.StepsToMs(1f, Bpm, TimeSigDenominator);
	}
	#endregion
}
