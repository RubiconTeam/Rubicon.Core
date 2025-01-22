using System.Linq;
using Godot.Collections;
using Rubicon.Core;
using Rubicon.Core.Chart;
using Rubicon.Core.Data;

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
	[Export] public Array<NoteInputElement> ProcessQueue = new();

	private NoteData[] _notes = [];
	private NoteInputElement _inputElement = new();

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
						ProcessQueue.Add(GetInputElement(noteIndex: NoteHitIndex, distance: 0f, holding: curNoteData.Length > 0f));
				
					NoteHitIndex++;
					if (NoteHitIndex >= Notes.Length)
						break;
				
					curNoteData = Notes[NoteHitIndex];
				}
			}

			if (curNoteData.MsTime - time <= -ProjectSettings.GetSetting("rubicon/judgments/bad_hit_window").AsDouble())
			{
				ProcessQueue.Add(GetInputElement(noteIndex: NoteHitIndex, distance: -ProjectSettings.GetSetting("rubicon/judgments/bad_hit_window").AsSingle() - 1f, holding: false));
				NoteHitIndex++;
			}	
		}

		for (int i = 0; i < ProcessQueue.Count; i++)
			OnNoteHit(ProcessQueue[i]);
			
		ProcessQueue.Clear();
	}
	
	protected abstract void AssignData(Note note, NoteData noteData);

	protected virtual NoteInputElement GetInputElement(int noteIndex, float distance, bool holding)
	{
		NoteInputElement element = _inputElement;
		element.Note = Notes[noteIndex];
		element.Distance = distance;
		element.Holding = holding;
		element.Index = noteIndex;
		
		// Auto detect hit based on distance
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
			if (Mathf.Abs(element.Distance) <= hitWindows[i])
			{
				hit = i;
				break;
			}
		}

		element.Hit = (HitType)hit;
		
		return element;
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		/*
		
		string actionName = $"MANIA_{ParentBarLine.Managers.Length}K_{Lane}";
		if (Autoplay || !InputsEnabled || !InputMap.HasAction(actionName) || !@event.IsAction(actionName) || @event.IsEcho())
			return;

		if (!@event.IsEcho())
			ParentBarLine.EmitSignal("BindPressed", ParentBarLine);*/
	}

	/// <summary>
	/// Triggers upon this note manager hitting/missing a note.
	/// </summary>
	/// <param name="element">Contains information about a note and its hits</param>
	protected abstract void OnNoteHit(NoteInputElement element);
}
