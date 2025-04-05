using System.Collections.Generic;
using System.Linq;

namespace Rubicon.Core.Chart;

[GlobalClass] public partial class RowData : Resource
{
    [Export] public SectionData Section;

    [Export] public byte LanePriority = 0;

    [Export] public byte Offset = 0;

    [Export] public byte Quant = RubiChartConstants.Quants[0];

    [Export] public NoteData[] Notes = [];

    public void AddNote(NoteData note)
    {
        if (HasNoteAtLane(note.Lane))
            return;

        List<NoteData> noteList = Notes.ToList();
        noteList.Add(note);
        Notes = noteList.ToArray();
        
        SortNotesByLane();
    }

    public void RemoveNote(NoteData note)
    {
        List<NoteData> noteList = Notes.ToList();
        noteList.Remove(note);
        Notes = noteList.ToArray();
        
        SortNotesByLane();
    }

    public void RemoveNoteAtLane(int lane)
    {
        NoteData note = Notes.FirstOrDefault(n => n.Lane == lane);
        RemoveNote(note);
    }

    public bool HasNoteAtLane(int lane)
    {
        return Notes.Any(n => n.Lane == lane);
    }
    
    /// <summary>
    /// Sorts the notes by lane order.
    /// </summary>
    public void SortNotesByLane()
    {
        Array.Sort(Notes, (a, b) => a.Lane.CompareTo(b.Lane));
    }
}