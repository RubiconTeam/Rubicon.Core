using Rubicon.Core.Chart;
using Rubicon.Core.Data;

namespace Rubicon.Core.Rulesets;

/// <summary>
/// An element for notes to be placed in a queue to be processed next frame. 
/// </summary>
[GlobalClass] public partial class NoteResult : RefCounted
{
    /// <summary>
    /// The note data linked with this object.
    /// </summary>
    [Export] public NoteData Note;
    
    /// <summary>
    /// The index of the note hit.
    /// </summary>
    [Export] public int Index = 0;

    /// <summary>
    /// The distance from the note's actual hit time.
    /// </summary>
    [Export] public float Distance;

    /// <summary>
    /// Whether the note was actually hit once before.
    /// </summary>
    [Export] public bool Tapped = false;
    
    /// <summary>
    /// Whether this note is being held by the <see cref="NoteController"/> controlling it.
    /// </summary>
    [Export] public bool Holding = false;

    /// <summary>
    /// The type of hit that was retrieved.
    /// </summary>
    [Export] public HitType Hit;

    /// <summary>
    /// The direction to call upon singing.
    /// </summary>
    [Export] public string Direction = "";
    
    /// <summary>
    /// Flags that prevent one action or multiple actions from being executed, if raised.
    /// </summary>
    [Export] public NoteResultFlags Flags = NoteResultFlags.None;
}