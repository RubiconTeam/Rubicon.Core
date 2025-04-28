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
	/// The index pointing to the element currently being used in <see cref="TimeChanges"/>.
	/// </summary>
	[Export] public int TimeChangeIndex = 0;

	/// <summary>
	/// Corrects the time, in seconds.
	/// </summary>
	[Export] public float Offset = 0f;

	/// <summary>
	/// How fast the Conductor goes.
	/// </summary>
	[Export] public float Speed = 1f;

	/// <summary>
	/// Is true when the Conductor has been started with Start() or Play(), false when either Pause() or Stop() is called.
	/// </summary>
	[Export] public bool Playing = false;

	/// <summary>
	/// The current timestamp from when the time was last set. Only updated every frame.
	/// </summary>
	[Export] public float Time { get => _visualTime; set => SetTime(value); }
	
	/// <summary>
	/// The current timestamp from when the time was last set. Will get the EXACT timestamp when this is called.
	/// </summary>
	[Export] public float ExactTime { get => GetTime(); set => SetTime(value); }

	/// <summary>
	/// The current audio timestamp.
	/// </summary>
	[Export] public float AudioTime { get => GetAudioTime(); set => SetAudioTime(value); }

	/// <summary>
	/// The current step according to the time.
	/// </summary>
	public float CurrentStep => GetCurrentStep();

	/// <summary>
	/// The current beat according to the time.
	/// </summary>
	public float CurrentBeat => GetCurrentBeat();

	/// <summary>
	/// The current measure according to the time.
	/// </summary>
	public float CurrentMeasure => GetCurrentMeasure();
	
	/// <summary>
	/// The time changes currently set to follow.
	/// </summary>
	[ExportGroup("Info"), Export] public TimeChange[] TimeChanges { get => _timeChanges; set => SetTimeChanges(value); }

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
	[Signal] public delegate void BpmChangedEventHandler(TimeChange currentBpm);

	#region Private Fields
	private double _relativeStartTime = 0;
	private double _relativeTimeOffset = 0;
	
	private float _lastTime = float.NegativeInfinity;
	private double _time = 0d;
	private float _visualTime = 0f;
		
	private float _cachedStep;
	private float _cachedStepTime;
	
	private float _cachedBeat;
	private float _cachedBeatTime;
	
	private float _cachedMeasure;
	private float _cachedMeasureTime;
		
	private TimeChange[] _timeChanges = [new() { Bpm = 100 }];

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
			return;

		_visualTime = ExactTime;

		base._Process(delta);
			
		while (TimeChangeIndex < TimeChanges.Length - 1 && TimeChanges[TimeChangeIndex + 1].MsTime / 1000f <= Time)
		{
			TimeChangeIndex++;
			EmitSignalBpmChanged(TimeChanges[TimeChangeIndex]);
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
		SetTime(time);
	}

	/// <summary>
	/// Resumes the Conductor at the last time it was paused at.
	/// </summary>
	public void Resume()
	{
		SetAudioTime((float)_time);
		Playing = true;
	}

	/// <summary>
	/// Stops the Conductor from ticking, but keeps the current time.
	/// </summary>
	public void Pause()
	{
		_time = GetAudioTime();
		_visualTime = (float)_time;
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
	/// Get the current time change.
	/// </summary>
	/// <returns>The current time change.</returns>
	public TimeChange GetCurrentTimeChange()
	{
		return _timeChanges[TimeChangeIndex];
	}
		
	/// <summary>
	/// Resets the Conductor, wiping all its fields with its default values.
	/// </summary>
	public void Reset()
	{
		TimeChanges = [new TimeChange { Bpm = 100 }];
		TimeChangeIndex = 0;
		Offset = 0;
		Speed = 1f;
		Stop();
	}

	/// <summary>
	/// Gets the audio timestamp of this Conductor.
	/// </summary>
	/// <returns>The time, in seconds</returns>
	public float GetAudioTime()
	{
		return (Offset + (Playing ? (float)(Godot.Time.GetUnixTimeFromSystem() - _relativeStartTime + _relativeTimeOffset) :
			_time != 0d ? (float)_time : 0f));
	}
	
	/// <summary>
	/// Sets the audio time of this Conductor.
	/// </summary>
	/// <param name="time">The time to set it to, in seconds.</param>
	public void SetAudioTime(float time)
	{
		_time = time;
		_visualTime = time;
		_relativeStartTime = Godot.Time.GetUnixTimeFromSystem();
		_relativeTimeOffset = time;
	}
		
	/// <summary>
	/// Gets the timestamp of this Conductor.
	/// </summary>
	/// <returns>The time, in seconds</returns>
	public float GetTime()
	{
		return GetAudioTime() * Speed;
	}

	/// <summary>
	/// Sets the time of this Conductor.
	/// </summary>
	/// <param name="time">The time to set it to, in seconds.</param>
	public void SetTime(float time)
	{
		SetAudioTime(time / Speed);
	}

	/// <summary>
	/// Gets the current step of this Conductor. Is not a whole number.
	/// </summary>
	/// <returns>The current step</returns>
	public float GetCurrentStep()
	{
		if (_cachedStepTime == ExactTime)
			return _cachedStep;

		TimeChange curTimeChange = GetCurrentTimeChange();
		if (TimeChanges.Length <= 1)
			return Time / (60f / (curTimeChange.Bpm * curTimeChange.TimeSignatureDenominator));

		_cachedStepTime = ExactTime;
		_cachedStep = (ExactTime - curTimeChange.MsTime / 1000f) / (60f / (curTimeChange.Bpm * curTimeChange.TimeSignatureDenominator)) + (curTimeChange.Time * curTimeChange.TimeSignatureNumerator * curTimeChange.TimeSignatureDenominator);
		return _cachedStep;
	}

	/// <summary>
	/// Gets the current beat of this Conductor. Is not a whole number.
	/// </summary>
	/// <returns>The current beat</returns>
	public float GetCurrentBeat()
	{
		if (_cachedBeatTime == ExactTime)
			return _cachedBeat;

		TimeChange curTimeChange = GetCurrentTimeChange();
		if (TimeChanges.Length <= 1)
			return Time / (60f / curTimeChange.Bpm);

		_cachedBeatTime = ExactTime;
		_cachedBeat = (ExactTime - curTimeChange.MsTime / 1000f) / (60f / curTimeChange.Bpm) + curTimeChange.Time * curTimeChange.TimeSignatureNumerator;
		return _cachedBeat;
	}

	/// <summary>
	/// Gets the current measure of this Conductor. Is not a whole number.
	/// </summary>
	/// <returns>The current measure</returns>
	public float GetCurrentMeasure()
	{
		if (_cachedMeasureTime == ExactTime)
			return _cachedMeasure;

		TimeChange curTimeChange = GetCurrentTimeChange();
		if (TimeChanges.Length <= 1)
			return Time / (60f / (curTimeChange.Bpm / curTimeChange.TimeSignatureNumerator));

		_cachedMeasureTime = ExactTime;
		_cachedMeasure = (ExactTime - curTimeChange.MsTime / 1000f) / (60f / (curTimeChange.Bpm / curTimeChange.TimeSignatureNumerator)) + curTimeChange.Time;
		return _cachedMeasure;
	}
	
	/// <summary>
	/// Set the time changes,
	/// </summary>
	/// <param name="data">An array of BpmInfos</param>
	public void SetTimeChanges(TimeChange[] data)
	{
		_timeChanges = data;
		TimeChangeIndex = 0;
	}
}
