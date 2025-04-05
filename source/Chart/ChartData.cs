using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rubicon.Core.Meta;

namespace Rubicon.Core.Chart;

/// <summary>
/// A class to store data for individual charts (referred to as "strum lines" for some).
/// </summary>
[GlobalClass]
public partial class ChartData : Resource
{
    /// <summary>
    /// What to name this bar-line. Used primarily for <see cref="SongMeta"/>.
    /// </summary>
    [Export] public StringName Name = "Player";
    
    /// <summary>
    /// How many lanes this specific chart will have.
    /// </summary>
    [Export] public int Lanes = 4;

    /// <summary>
    /// Stores data about scroll velocity changes.
    /// </summary>
    [Export] public SvChange[] SvChanges = [ new SvChange() ];

    /// <summary>
    /// If this chart is the target bar line, these target bar line switches will activate.
    /// </summary>
    [Export] public TargetSwitch[] Switches = [];
    
    [Export] public SectionData[] Sections = [];

    [Export] public NoteData[] Strays = [];

    public NoteData[] GetNotes()
    {
        List<NoteData> notes = new List<NoteData>();
        for (int s = 0; s < Sections.Length; s++)
        {
            SectionData section = Sections[s];
            for (int r = 0; r < section.Rows.Length; r++)
            {
                RowData row = section.Rows[r];
                notes.AddRange(row.Notes.Where(x => x.EndingRow != row));
            }
        }
        
        notes.AddRange(Strays);
        notes.Sort((x, y) => x.MeasureTime.CompareTo(y.MeasureTime));
        
        return notes.ToArray();
    }
    
    public NoteData[] GetNotesAtLane(byte lane)
    {
        List<NoteData> notes = new List<NoteData>();
        for (int s = 0; s < Sections.Length; s++)
        {
            SectionData section = Sections[s];
            for (int r = 0; r < section.Rows.Length; r++)
            {
                RowData row = section.Rows[r];
                notes.AddRange(row.Notes.Where(x => x.Lane == lane && x.EndingRow != row));
            }
        }
        
        notes.AddRange(Strays.Where(x => x.Lane == lane));
        notes.Sort((x, y) => x.MeasureTime.CompareTo(y.MeasureTime));
        
        return notes.ToArray();
    }

    public NoteData[] GetNotesOfType(StringName noteType)
    {
        List<NoteData> notes = new List<NoteData>();
        for (int s = 0; s < Sections.Length; s++)
        {
            SectionData section = Sections[s];
            for (int r = 0; r < section.Rows.Length; r++)
            {
                RowData row = section.Rows[r];
                notes.AddRange(row.Notes.Where(x => x.Type == noteType && x.EndingRow != row));
            }
        }
        
        notes.AddRange(Strays.Where(x => x.Type == noteType));
        notes.Sort((x, y) => x.MeasureTime.CompareTo(y.MeasureTime));
        
        return notes.ToArray();
    }

    public void AddNoteStart(NoteData note, int measure, byte offset, byte quant)
    {
        SectionData section = AddSection(measure);
        RowData row = section.AddRow(offset, quant);
        row.AddNote(note);

        note.StartingRow = row;
    }

    public void AddNoteEnd(NoteData note, int measure, byte offset, byte quant)
    {
        SectionData section = AddSection(measure);
        RowData row = section.AddRow(offset, quant);
        row.AddNote(note);

        note.EndingRow = row;
    }

    /// <summary>
    /// Adds a note at the measure time and length provided. This is not accurate.
    /// </summary>
    /// <param name="note">Note data</param>
    /// <param name="measureTime">Measure time</param>
    /// <param name="length">Length</param>
    public void AddNoteAtMeasureTime(NoteData note, float measureTime, float length)
    {
        int baseMeasure = Mathf.FloorToInt(measureTime);
        float measureOffset = measureTime - baseMeasure;
        
        byte[] quants = RubiChartConstants.Quants;
        byte offset = (byte)Math.Clamp(Mathf.RoundToInt(measureOffset * quants[^1]), 0, quants[^1] - 1);
        byte quant = quants[^1];
        for (int q = 0; q < quants.Length; q++)
        {
            byte curQuant = quants[q];
            float result = measureOffset * curQuant;
            bool isSnapped = result % 1 == 0;
            if (!isSnapped)
            {
                int roundedResult = Mathf.RoundToInt(result);
                if (!Mathf.IsEqualApprox(result, roundedResult, 0.1f))
                    continue;

                offset = (byte)roundedResult;
                quant = curQuant;
                break;
            }

            offset = (byte)result;
            quant = curQuant;
            break;
        }

        AddNoteStart(note, baseMeasure, offset, quant);
        if (length <= 0)
            return;

        int baseEndMeasure = Mathf.FloorToInt(measureTime + length);
        float endMeasureOffset = measureTime + length - baseEndMeasure;
        offset = (byte)Math.Clamp(Mathf.RoundToInt(endMeasureOffset * quants[^1]), 0, quants[^1] - 1);
        quant = quants[^1];
        for (int q = 0; q < quants.Length; q++)
        {
            byte curQuant = quants[q];
            float result = endMeasureOffset * curQuant;
            bool isSnapped = result % 1 == 0;
            if (!isSnapped)
            {
                int roundedResult = Mathf.RoundToInt(result);
                if (!Mathf.IsEqualApprox(result, roundedResult, 0.1f))
                    continue;

                offset = (byte)roundedResult;
                quant = curQuant;
                break;
            }

            offset = (byte)result;
            quant = curQuant;
            break;
        }
        
        AddNoteEnd(note, baseEndMeasure, offset, quant);
    }

    /// <summary>
    /// Adds a note that isn't connected to any row. Less functionality will be given if done so.
    /// </summary>
    /// <param name="note">The note</param>
    public void AddStrayNote(NoteData note)
    {
        List<NoteData> notes = Strays.ToList();
        notes.Add(note);
        Strays = notes.ToArray();
        
        SortStraysByTime();
    }

    public void MoveNoteStart(NoteData note, int measure, byte offset, byte quant)
    {
        int distance = 0;
        byte distanceQuant = 0;
        bool isHold = note.EndingRow != null;
        if (isHold)
        {
            int measureDistance = note.EndingRow.Section.Measure - note.StartingRow.Section.Measure;
            
            bool endingQuantHigher = note.EndingRow.Quant > note.StartingRow.Quant;
            bool startingQuantHigher = note.StartingRow.Quant > note.EndingRow.Quant;
            distanceQuant = Math.Max(note.StartingRow.Quant, note.EndingRow.Quant);
            
            int startingOffset = note.StartingRow.Offset;
            if (endingQuantHigher)
                startingOffset *= note.EndingRow.Quant / note.StartingRow.Quant;
            
            int endingOffset = note.EndingRow.Offset;
            if (startingQuantHigher)
                endingOffset *= note.StartingRow.Quant / note.EndingRow.Quant;

            distance = ((measureDistance * distanceQuant) + endingOffset) - startingOffset;
        }
        
        RemoveNoteStart(note);
        AddNoteStart(note, measure, offset, quant);
        if (!isHold)
            return;

        bool distanceQuantHigher = distanceQuant > quant;
        bool paramQuantHigher = quant > distanceQuant;
        byte endingQuant = Math.Max(quant, distanceQuant);

        int offsetInt = offset;
        if (distanceQuantHigher)
            offsetInt *= distanceQuant / quant;
        
        int distanceQuantInt = distance;
        if (paramQuantHigher)
            distanceQuantInt *= quant / distanceQuant;
        
        int totalDistance = offsetInt + distanceQuantInt;
        int endingMeasure = totalDistance / endingQuant;
        byte endOffset = (byte)(totalDistance % endingQuant);
        
        AddNoteEnd(note, endingMeasure, endOffset, endingQuant);
    }

    public void MoveNoteEnd(NoteData note, int measure, byte offset, byte quant)
    {
        RemoveNoteEnd(note);
        AddNoteEnd(note, measure, offset, quant);
    }

    public void RemoveNoteStart(NoteData note)
    {
        RowData startingRow = note.StartingRow;
        RowData endingRow = note.EndingRow;
        
        startingRow.RemoveNote(note);
        CleanupSections();
        
        if (endingRow == null)
            return;
        
        RemoveNoteEnd(note);
    }

    public void RemoveNoteEnd(NoteData note)
    {
        RowData endingRow = note.EndingRow;
        
        endingRow.RemoveNote(note);
        CleanupSections();
    }

    public void RemoveStrayNote(NoteData note)
    {
        List<NoteData> notes = Strays.ToList();
        notes.Remove(note);
        Strays = notes.ToArray();
    }

    public SectionData AddSection(int measure)
    {
        SectionData section = GetSectionAtMeasure(measure);
        if (section != null)
            return section;

        section = new SectionData();
        section.Measure = measure;
        
        List<SectionData> sectionList = Sections.ToList();
        sectionList.Add(section);
        Sections = sectionList.ToArray();
        
        SortSectionsByMeasure();
        return section;
    }

    public void RemoveSection(int measure)
    {
        SectionData section = GetSectionAtMeasure(measure);
        if (section == null)
            return;
        
        List<SectionData> sectionList = Sections.ToList();
        sectionList.Remove(section);
        Sections = sectionList.ToArray();
        
        SortSectionsByMeasure();
    }
    
    public SectionData GetSectionAtMeasure(int measure)
    {
        return Sections.FirstOrDefault(x => x.Measure == measure);
    }

    public void CleanupSections()
    {
        List<SectionData> sectionList = Sections.ToList();
        for (int i = 0; i < Sections.Length; i++)
        {
            SectionData section = Sections[i];
            section.CleanupRows();
            if (section.Rows.Length > 0)
                continue;

            sectionList.Remove(section);
        }
        
        Sections = sectionList.ToArray();
        SortSectionsByMeasure();
    }

    public void SortSectionsByMeasure()
    {
        Array.Sort(Sections, (x, y) => x.Measure.CompareTo(y.Measure));
    }

    public void SortStraysByTime()
    {
        Array.Sort(Strays, (x, y) => x.MeasureTime.CompareTo(y.MeasureTime));
    }

    public void ConvertData(BpmInfo[] bpmInfo)
    {
        for (int s = 0; s < Sections.Length; s++)
            for (int r = 0; r < Sections[s].Rows.Length; r++)
                for (int n = 0 ; n < Sections[s].Rows[r].Notes.Length; n++)
                    Sections[s].Rows[r].Notes[n].ConvertData(bpmInfo, SvChanges);
        
        for (int i = 0; i < Strays.Length; i++)
            Strays[i].ConvertData(bpmInfo, SvChanges);
    }
}