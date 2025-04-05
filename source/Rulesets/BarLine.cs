using Rubicon.Core;
using Rubicon.Core.Chart;

namespace Rubicon.Core.Rulesets;

/// <summary>
/// A base bar line for Rubicon rulesets
/// </summary>
[GlobalClass] public abstract partial class BarLine : Control
{
	/// <summary>
	/// The individual chart for this bar line. Contains notes and scroll velocity changes.
	/// </summary>
	[Export] public ChartData Chart;
	
	/// <summary>
	/// Contains all the nodes used to manage notes.
	/// </summary>
	[Export] public NoteController[] Managers;
	
	/// <summary>
	/// The PlayField this instance is associated with.
	/// </summary>
	[Export] public PlayField PlayField;

	/// <summary>
	/// The distance to offset notes by position-wise.
	/// </summary>
	[Export] public float DistanceOffset = 0;
	
	/// <summary>
	/// The index of the current scroll velocity.
	/// </summary>
	[Export] public int ScrollVelocityIndex = 0;
	
	/// <summary>
	/// A signal that is emitted every time a manager in this bar line hits a note. Can be a miss.
	/// </summary>
	[Signal] public delegate void NoteHitEventHandler(StringName name, NoteResult result);

	/// <summary>
	/// A signal that is emitted every time a manager in this bar line either presses or lets go of a bind.
	/// </summary>
	[Signal] public delegate void BindPressedEventHandler(BarLine barLine);
	
	public override void _Process(double delta)
	{
		base._Process(delta);
		
		// Handle SV changes
		if (Chart?.SvChanges == null)
			return;
		
		float time = Conductor.Time * 1000f;
		SvChange[] svChangeList = Chart.SvChanges;
		while (ScrollVelocityIndex + 1 < svChangeList.Length && svChangeList[ScrollVelocityIndex + 1].MsTime - time <= 0)
			ScrollVelocityIndex++;
		
		SvChange currentScrollVel = Chart.SvChanges[ScrollVelocityIndex];
		DistanceOffset = -(float)(currentScrollVel.Position + (time - currentScrollVel.MsTime) * currentScrollVel.Multiplier);
	}

	/// <summary>
	/// Triggers upon one of its note managers hitting a note in any way, even if it's a miss.
	/// </summary>
	/// <param name="result">The input element received</param>
	public abstract void OnNoteHit(NoteResult result);

	/// <summary>
	/// Determines if the player should miss when hitting a blank space with a specified lane.
	/// </summary>
	/// <param name="index">The index of the lane that should ignore Ghost Tapping.</param>
	public void InvokeGhostTap(int index)
	{
		PlayField.HandleGhostTap(Name, index);
	}
	
	/// <summary>
	/// Determines if every <see cref="NoteController"/> in the BarLine should be played automatically.
	/// </summary>
	/// <param name="autoplay">Bool that decides if the <see cref="BarLine"/> should be auto-played or not.</param>
	public void SetAutoPlay(bool autoplay)
	{
		foreach (NoteController noteManager in Managers)
			noteManager.Autoplay = autoplay;
	}

	/// <summary>
	/// Sets a scroll speed for every <see cref="NoteController"/> in the BarLine
	/// </summary>
	/// <param name="scrollSpeed">The new speed to be applied.</param>
	public void SetScrollSpeed(float scrollSpeed)
	{
		foreach (NoteController noteManager in Managers)
			noteManager.ScrollSpeed = scrollSpeed;
	}
}
