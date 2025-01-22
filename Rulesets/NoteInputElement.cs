using Rubicon.Core.Chart;
using Rubicon.Core.Data;

namespace Rubicon.Core.Rulesets;

/// <summary>
/// An element for notes to be placed in a queue to be processed next frame. 
/// </summary>
[GlobalClass] public partial class NoteInputElement : GodotObject
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
    /// Indicates whether the NoteManager was holding this note or not, if this note is a hold note.
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
}