using System.Linq;
using Rubicon.Core;
using Rubicon.Core.Chart;
using Rubicon.Core.Data;
using Array = System.Array;

namespace Rubicon.Core.Rulesets.Mania;

/// <summary>
/// A bar line class for Mania gameplay. Also referred to as a "strum" by some.
/// </summary>
[GlobalClass] public partial class ManiaNoteController : NoteController
{
	/// <summary>
	/// The direction of this note manager.
	/// </summary>
	[Export] public string Direction = "";
	
	/// <inheritdoc/>
	[Export] public override float ScrollSpeed
	{
		get => base.ScrollSpeed;
		set
		{
			base.ScrollSpeed = value;
			for (int i = 0; i < HitObjects.Length; i++)
			{
				Note hitObject = HitObjects[i];
				if (hitObject is not ManiaNote maniaNote)
					continue;
				
				maniaNote.AdjustInitialTailSize();
			}
		}
	}

	/// <summary>
	/// The note that is currently being held.
	/// </summary>
	[Export] public NoteData NoteHeld;
	
	/// <summary>
	/// The angle the notes come from in radians.
	/// </summary>
	[Export] public float DirectionAngle = Mathf.Pi / 2f;

	/// <summary>
	/// The note skin for this manager. Please change via <see cref="ChangeNoteSkin"/>!
	/// </summary>
	[Export] public ManiaNoteSkin NoteSkin;

	/// <summary>
	/// The lane graphic for this manager.
	/// </summary>
	[Export] public AnimatedSprite2D LaneObject;

	/// <summary>
	/// Sets up this manager for Mania gameplay.
	/// </summary>
	/// <param name="parent">The parent <see cref="ManiaBarLine"/>></param>
	/// <param name="lane">The lane index</param>
	/// <param name="noteSkin">The note skin provided</param>
	public void Setup(ManiaBarLine parent, int lane, ManiaNoteSkin noteSkin)
	{
		ParentBarLine = parent;
		Lane = lane;
		Direction = noteSkin.GetDirection(lane, parent.Chart.Lanes);
		ChangeNoteSkin(noteSkin);
		
		Notes = parent.Chart.Notes.Where(x => x.Lane == Lane).ToArray();
		Array.Sort(Notes, (a, b) =>
		{
			if (a.Time < b.Time)
				return -1;
			if (a.Time > b.Time)
				return 1;
			
			return 0;
		});
	}

	public override void _Process(double delta)
	{
		if (NoteHeld != null && NoteHeld.MsTime + NoteHeld.MsLength < Conductor.Time * 1000f)
			ProcessQueue.Add(GetInputElement(noteIndex: HoldingIndex, distance: 0f, holding: false));
		
		base._Process(delta);
	}

	protected override NoteInputElement GetInputElement(int noteIndex, float distance, bool holding)
	{
		NoteInputElement inputElement = base.GetInputElement(noteIndex, distance, holding);
		inputElement.Direction = Direction;
		return inputElement;
	}

	/// <summary>
	/// Changes the note skin for this manager. Does not change the notes on-screen automatically!
	/// </summary>
	/// <param name="noteSkin">The note skin</param>
	public void ChangeNoteSkin(ManiaNoteSkin noteSkin)
	{
		NoteSkin = noteSkin;

		LaneObject = new AnimatedSprite2D();
		LaneObject.Name = "Lane Graphic";
		LaneObject.Scale = Vector2.One * NoteSkin.Scale;
		LaneObject.SpriteFrames = NoteSkin.LaneAtlas;
		LaneObject.Play($"{Direction}LaneNeutral", 1f, true);
		LaneObject.AnimationFinished += OnAnimationFinish;
		AddChild(LaneObject);
		MoveChild(LaneObject, 0);
	}
	
	/*
	/// <inheritdoc/>
	protected override Note CreateNote() => new ManiaNote();

	/// <inheritdoc/>
	protected override void SetupNote(Note note, NoteData data)
	{
		if (note is not ManiaNote maniaNote)
			return;
		
		maniaNote.Setup(data, this, NoteSkin);
	}*/

	protected override void AssignData(Note note, NoteData noteData)
	{
		if (note is not ManiaNote maniaNote)
			return;
		
		maniaNote.Assign(noteData, this);
	}

	/// <inheritdoc/>
	protected override void OnNoteHit(NoteInputElement inputElement)
	{
		if (inputElement.Hit != HitType.Miss)
		{
			if (!inputElement.Holding)
			{
				if (NoteHeld == null || NoteHeld != null && (Autoplay || !Autoplay && Input.IsActionPressed($"play_mania_{ParentBarLine.Managers.Length}k_{Lane}")))
					LaneObject.Animation = $"{Direction}LaneConfirm";
				
				NoteHeld = null;
				HoldingIndex = -1;
				LaneObject.Play();
				
				RemoveChild(HitObjects[inputElement.Index]);
				HitObjects[inputElement.Index].PrepareRecycle();
			}
			else
			{
				NoteHeld = inputElement.Note;
				HoldingIndex = inputElement.Index;
				LaneObject.Animation = $"{Direction}LaneConfirm";
				LaneObject.Pause();   
			}	
		}
		else
		{
			if (inputElement.Note == NoteHeld)
			{
				if (HitObjects[inputElement.Index] is ManiaNote maniaNote)
					maniaNote.UnsetHold();
			
				NoteHeld = null;
			}

			if (inputElement.Note.MsLength <= 0f)
			{
				RemoveChild(HitObjects[inputElement.Index]);
				HitObjects[inputElement.Index].PrepareRecycle();
			}
		}

		inputElement.Note.WasHit = true;
		ParentBarLine.OnNoteHit(inputElement);
	}
	
	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		string actionName = $"play_mania_{ParentBarLine.Managers.Length}k_{Lane}";
		if (Autoplay || !InputsEnabled || !InputMap.HasAction(actionName) || !@event.IsAction(actionName) || @event.IsEcho())
			return;

		if (@event.IsPressed())
		{
			NoteData[] notes = Notes;
			if (NoteHitIndex >= notes.Length)
			{
				if (LaneObject.Animation != $"{Direction}LanePress")
					LaneObject.Play($"{Direction}LanePress");
				
				return;
			}

			float songPos = Conductor.Time * 1000f;
			while (notes[NoteHitIndex].MsTime - songPos <= -ProjectSettings.GetSetting("rubicon/judgments/bad_hit_window").AsSingle())
			{
				// Miss every note thats too late first
				ProcessQueue.Add(GetInputElement(noteIndex: NoteHitIndex, distance: -ProjectSettings.GetSetting("rubicon/judgments/bad_hit_window").AsSingle() - 1f, holding: false));
				NoteHitIndex++;
			}

			float hitTime = notes[NoteHitIndex].MsTime - songPos;
			if (Mathf.Abs(hitTime) <= ProjectSettings.GetSetting("rubicon/judgments/bad_hit_window").AsSingle()) // Literally any other rating
			{
				ProcessQueue.Add(GetInputElement(noteIndex: NoteHitIndex, distance: hitTime, holding: notes[NoteHitIndex].Length > 0));
				NoteHitIndex++;
			}
			else
			{
				if (LaneObject.Animation != $"{Direction}LanePress")
					LaneObject.Play($"{Direction}LanePress");
			}
		}
		else if (@event.IsReleased())
		{
			if (NoteHeld != null)
			{
				float length = NoteHeld.MsTime + NoteHeld.MsLength - (Conductor.Time * 1000f);
				bool holding = length <= ProjectSettings.GetSetting("rubicon/judgments/bad_hit_window").AsSingle();
				ProcessQueue.Add(GetInputElement(noteIndex: HoldingIndex, distance: length, holding: !holding));
			}

			if (LaneObject.Animation != $"{Direction}LaneNeutral")
				LaneObject.Play($"{Direction}LaneNeutral", 1f, true);
		}
	}

	/// <summary>
	/// Mainly for when the autoplay finishes hitting a note.
	/// </summary>
	private void OnAnimationFinish()
	{
		if (!Autoplay || LaneObject.Animation != $"{Direction}LaneConfirm")
			return;

		if (LaneObject.Animation != $"{Direction}LaneNeutral")
			LaneObject.Play($"{Direction}LaneNeutral");
	}
}
