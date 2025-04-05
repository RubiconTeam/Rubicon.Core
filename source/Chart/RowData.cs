using System.Collections.Generic;
using System.Linq;

namespace Rubicon.Core.Chart;

[GlobalClass] public partial class RowData : Resource
{
    [Export] public SectionData Section;

    [Export] public byte LanePriority = 0;

    [Export] public byte Offset = 0;

    [Export] public byte Quant = RubiChartConstants.Quants[0];

    [Export] public NoteData[] StartNotes = [];

    [Export] public NoteData[] EndNotes = [];

    public void ConvertData(BpmInfo[] bpmInfo, SvChange[] svChanges)
    {
        for (int s = 0; s < StartNotes.Length; s++)
        {
            NoteData noteData = StartNotes[s];
            noteData.StartingRow = this;
            noteData.ConvertData(bpmInfo, svChanges);
        }

        for (int e = 0; e < EndNotes.Length; e++)
        {
            NoteData noteData = EndNotes[e];
            noteData.EndingRow = this;
            noteData.ConvertData(bpmInfo, svChanges);
        }
    }

    public NoteData[] GetNotes(bool includeEnds = false)
    {
        if (!includeEnds)
            return StartNotes;
        
        List<NoteData> noteList = new List<NoteData>();
        noteList.AddRange(StartNotes);
        noteList.AddRange(EndNotes);
        noteList.Sort((x, y) => x.Lane.CompareTo(y.Lane));
        return noteList.ToArray();
    }

    public NoteData GetNoteAtLane(byte lane, bool includeEnds = false)
    {
        NoteData note = StartNotes.FirstOrDefault(x => x.Lane == lane);
        if (note == null && includeEnds)
            note = EndNotes.FirstOrDefault(x => x.Lane == lane);

        return note;
    }
    
    public NoteData[] GetNotesOfType(StringName noteType, bool includeEnds = false)
    {
        List<NoteData> noteList = new List<NoteData>();
        noteList.AddRange(StartNotes.Where(x => x.Type == noteType));

        if (includeEnds)
        {
            noteList.AddRange(EndNotes.Where(x => x.Type == noteType));
            noteList.Sort((x, y) => x.Lane.CompareTo(y.Lane));
        }
        
        return noteList.ToArray();
    }

    public string[] GetNoteTypes()
    {
        List<string> noteTypes = new List<string>();
        for (int s = 0; s < StartNotes.Length; s++)
        {
            NoteData curNote = StartNotes[s];
            string typeString = curNote.Type.ToString();
            bool hasType = typeString.ToLower() != "normal";
            if (!hasType || noteTypes.Contains(typeString))
                continue;
                        
            noteTypes.Add(typeString);
        }
        
        for (int e = 0; e < EndNotes.Length; e++)
        {
            NoteData curNote = EndNotes[e];
            string typeString = curNote.Type.ToString();
            bool hasType = typeString.ToLower() != "normal";
            if (!hasType || noteTypes.Contains(typeString))
                continue;
                        
            noteTypes.Add(typeString);
        }
        
        return noteTypes.ToArray();
    }
    
    public void AddStartNote(NoteData note)
    {
        if (HasNoteAtLane(note.Lane))
            return;
        
        List<NoteData> noteList = StartNotes.ToList();
        noteList.Add(note);
        StartNotes = noteList.ToArray();
        
        SortNotesByLane();
    }
    
    public void AddEndNote(NoteData note)
    {
        if (HasNoteAtLane(note.Lane))
            return;
        
        List<NoteData> noteList = EndNotes.ToList();
        noteList.Add(note);
        EndNotes = noteList.ToArray();
        
        SortNotesByLane();
    }

    public void RemoveNote(NoteData note)
    {
        if (StartNotes.Contains(note))
        {
            List<NoteData> startNoteList = StartNotes.ToList();
            startNoteList.Remove(note);
            StartNotes = startNoteList.ToArray();
        }

        if (EndNotes.Contains(note))
        {
            List<NoteData> endNoteList = EndNotes.ToList();
            endNoteList.Remove(note);
            EndNotes = endNoteList.ToArray();
        }
        
        SortNotesByLane();
    }

    public void RemoveNoteAtLane(int lane)
    {
        NoteData note = StartNotes.FirstOrDefault(n => n.Lane == lane);
        if (note == null)
            note = EndNotes.FirstOrDefault(n => n.Lane == lane);
        
        RemoveNote(note);
    }

    public bool HasNoteAtLane(int lane)
    {
        return StartNotes.Any(n => n.Lane == lane) || EndNotes.Any(n => n.Lane == lane);
    }
    
    /// <summary>
    /// Sorts the notes by lane order.
    /// </summary>
    public void SortNotesByLane()
    {
        Array.Sort(StartNotes, (a, b) => a.Lane.CompareTo(b.Lane));
        Array.Sort(EndNotes, (a, b) => a.Lane.CompareTo(b.Lane));
    }
}