using System.Linq;
using Godot.Collections;
using Rubicon.Core;
using Rubicon.Core.Chart;
using Rubicon.Core.Data;
using Rubicon.Core.Settings;

namespace Rubicon.Core.Rulesets;

/// <summary>
/// A base note manager for Rubicon rulesets.
/// </summary>
[GlobalClass] public abstract partial class NoteController : Control
{
	/// <summary>
	/// The lane index of this note manager.
	/// </summary>
	[Export] public int Lane = 0;

	/// <summary>
	/// Contains the individual notes for this manager.
	/// </summary>
	[Export] public NoteData[] Notes
	{
		get => _notes;
		set
		{
			_notes = value;
			HitObjects = new Note[_notes.Length];
		}
	}

	/// <summary>
	/// Hit objects whose index is linked to the note data's index in <see cref="Notes"/>.
	/// </summary>
	[Export] public Note[] HitObjects;

	/// <summary>
	/// If true, the computer will hit the notes that come by.
	/// </summary>
	[Export] public bool Autoplay = true;

	/// <summary>
	/// If false, nothing can be input through this note manager. Not even the computer.
	/// </summary>
	[Export] public bool InputsEnabled = true;

	/// <summary>
	/// The constant rate at which the notes go down. This is different from scroll velocities.
	/// </summary>
	[Export] public virtual float ScrollSpeed { get; set; } = 1f;

	/// <summary>
	/// This note manager's parent bar line.
	/// </summary>
	[Export] public BarLine ParentBarLine;
	
	/// <summary>
	/// Is true when the manager has gone through all notes present in <see cref="Chart">Chart</see>.
	/// </summary>
	public bool IsComplete => NoteHitIndex >= Notes.Length;

	/// <summary>
	/// Is true when the manager has no notes to hit for at least a measure.
	/// </summary>
	public bool OnBreak => !IsComplete && Notes[NoteHitIndex].MsTime - Conductor.Time * 1000f >
		ConductorUtility.MeasureToMs(Conductor.CurrentMeasure, Conductor.Bpm, Conductor.TimeSigNumerator);
	
	/// <summary>
	/// The next note's index to be hit.
	/// </summary>
	[ExportGroup("Advanced"), Export] public int NoteHitIndex = 0; 
	
	/// <summary>
	/// The next hit object's index to be spawned in.
	/// </summary>
	[Export] public int NoteSpawnIndex = 0;

	/// <summary>
	/// The index of the note that is currently being held.
	/// </summary>
	[Export] public int HoldingIndex = -1;

	/// <summary>
	/// The queue list for notes to be processed next frame.
	/// </summary>
	[Export] public Array<NoteResult> ProcessQueue = new();

	/// <summary>
	/// The <see cref="InputEvent"/> name for this controller.
	/// </summary>
	[Export] public string Action;

	private NoteData[] _notes = [];
	private NoteResult _result = new();

	public override void _Process(double delta)
	{
		base._Process(delta);
		
		// Handle note spawning
		float time = Conductor.Time * 1000f;
		if (NoteSpawnIndex < Notes.Length && Visible)
		{
			while (NoteSpawnIndex < Notes.Length && Notes[NoteSpawnIndex].MsTime - time <= 2000f)
			{
				if (Notes[NoteSpawnIndex].MsTime - time < 0f || Notes[NoteSpawnIndex].WasSpawned)
				{
					NoteSpawnIndex++;
					continue;
				}

				Note note = ParentBarLine.PlayField.Factory.GetNote(Notes[NoteSpawnIndex].Type);
				AddChild(note);
				note.MoveToFront();

				HitObjects[NoteSpawnIndex] = note;
				note.Name = $"Note {NoteSpawnIndex}";
				AssignData(note, Notes[NoteSpawnIndex]);
				Notes[NoteSpawnIndex].WasSpawned = true;
				NoteSpawnIndex++;
			}
		}
		
		if (!IsComplete)
		{
			NoteData curNoteData = Notes[NoteHitIndex];
			if (Autoplay && InputsEnabled)
			{
				while (curNoteData.MsTime - time <= 0f)
				{
					if (!curNoteData.ShouldMiss)
						ProcessQueue.Add(GetResult(noteIndex: NoteHitIndex, distance: 0f, holding: curNoteData.Length > 0f));
				
					NoteHitIndex++;
					if (NoteHitIndex >= Notes.Length)
						break;
				
					curNoteData = Notes[NoteHitIndex];
				}
			}

			if (curNoteData.MsTime - time <= -ProjectSettings.GetSetting("rubicon/judgments/bad_hit_window").AsDouble())
			{
				ProcessQueue.Add(GetResult(noteIndex: NoteHitIndex, distance: -ProjectSettings.GetSetting("rubicon/judgments/bad_hit_window").AsSingle() - 1f, holding: false));
				NoteHitIndex++;
			}	
		}

		for (int i = 0; i < ProcessQueue.Count; i++)
			OnNoteHit(ProcessQueue[i]);
			
		ProcessQueue.Clear();
	}

	public void InvokeGhostTap()
	{
		ParentBarLine.InvokeGhostTap(Lane);
	}
	
	/// <summary>
	/// Gets the current note's distance from <see cref="Conductor.Time"/>.
	/// </summary>
	/// <param name="includeOffset">Whether to include the offset set by the player in <see cref="UserSettings"/>.</param>
	/// <returns>The current note's distance, 0 if there is none.</returns>
	protected float GetCurrentNoteDistance(bool includeOffset = false)
	{
		if (NoteHitIndex >= Notes.Length)
			return 0;
		
		float distance = Notes[NoteHitIndex].MsTime - Conductor.Time * 1000f;
		if (includeOffset)
			distance -= (float)UserSettings.Gameplay.Offset;

		return distance;
	}
	
	protected abstract void AssignData(Note note, NoteData noteData);

	protected virtual NoteResult GetResult(int noteIndex, float distance, bool holding)
	{
		NoteResult result = _result;
		result.Note = Notes[noteIndex];
		result.Distance = distance;
		result.Holding = holding;
		result.Index = noteIndex;
		
		// Auto detect hit based on distance
		if (result.Note.Length > 0 && !holding) // If hold note was let go
		{
			result.Hit = Mathf.Abs(Conductor.CurrentMeasure - result.Note.Time - result.Note.Length) < 1f ? HitType.Perfect : HitType.Miss;
			result.Tapped = true;
		}
		else
		{
			float[] hitWindows = [ 
				ProjectSettings.GetSetting("rubicon/judgments/perfect_hit_window").AsSingle(),
				ProjectSettings.GetSetting("rubicon/judgments/great_hit_window").AsSingle(),
				ProjectSettings.GetSetting("rubicon/judgments/good_hit_window").AsSingle(),
				ProjectSettings.GetSetting("rubicon/judgments/okay_hit_window").AsSingle(),
				ProjectSettings.GetSetting("rubicon/judgments/bad_hit_window").AsSingle()
			]; 
			int hit = hitWindows.Length;
			for (int i = 0; i < hitWindows.Length; i++)
			{
				if (Mathf.Abs(result.Distance) <= hitWindows[i])
				{
					hit = i;
					break;
				}
			}

			result.Hit = (HitType)hit;
			result.Tapped = result.Hit != HitType.Miss;
		}
		
		return result;
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		
		if (Autoplay || !InputsEnabled || !InputMap.HasAction(Action) || !@event.IsAction(Action) || @event.IsEcho())
			return;
		
		if (@event.IsPressed())
			PressedEvent();
		
		if (@event.IsReleased())
			ReleasedEvent();
	}

	protected abstract void PressedEvent();
	
	protected abstract void ReleasedEvent();

	/// <summary>
	/// Triggers upon this note manager hitting/missing a note.
	/// </summary>
	/// <param name="element">Contains information about a note and its hits</param>
	protected abstract void OnNoteHit(NoteResult element);
}
