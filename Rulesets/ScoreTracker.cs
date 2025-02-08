using System.Collections.Generic;
using System.Linq;
using Rubicon.Core.Chart;
using Rubicon.Core.Data;
using Range = System.Range;

namespace Rubicon.Core.Rulesets;

[GlobalClass] public partial class ScoreTracker : RefCounted
{
    [Export] public int Score = 0;

    [Export] public ScoreRank Rank = ScoreRank.P;

    [Export] public ClearRank Clear = ClearRank.Perfect;

    [Export] public float Accuracy = 100f;

    [Export] public int PerfectHits = 0;
    
    [Export] public int GreatHits = 0;
    
    [Export] public int GoodHits = 0;

    [Export] public int OkayHits = 0;
    
    [Export] public int BadHits = 0;
    
    [Export] public int Misses = 0;

    [Export] public int MissStreak = 0;

    [Export] public int Combo = 0;

    [Export] public int ComboBreaks = 0;

    [Export] public int HighestCombo = 0;

    [Export] public int NoteCount = 0;

    [Export] public int MaxCombo = 0;

    [Export] public int TapsHit = 0;

    [Export] public int TotalHit = 0;

    [ExportGroup("References"), Export] public RubiChart Chart;
    
    public void Initialize(RubiChart chart, StringName target)
    {
        Chart = chart;
        
        IndividualChart playerChart = chart.Charts.FirstOrDefault(x => x.Name == target);
        if (playerChart == null)
            return;
        
        float startTime = 0;
        List<TargetSwitch> switches = [..playerChart.Switches];
        switches.Insert(0, new TargetSwitch{ Time = 0f, MsTime = 0f, Name = target });
        for (int i = 0; i < switches.Count; i++)
        {
            IndividualChart curChart = chart.Charts.FirstOrDefault(x => x.Name == switches[i].Name);
            if (curChart == null)
                continue;
            
            if (i < switches.Count - 1)
            {
                int tapNoteCount = GetNoteCountInRange(curChart.Notes, startTime, switches[i + 1].MsTime);
                MaxCombo += tapNoteCount;
                NoteCount += tapNoteCount + GetHoldNoteCountInRange(curChart.Notes, startTime, switches[i + 1].MsTime);
                startTime = switches[i + 1].Time;
                continue;
            }

            int tapNotes = GetNoteCountInRange(curChart.Notes, startTime);
            MaxCombo += tapNotes;
            NoteCount += tapNotes + GetHoldNoteCountInRange(curChart.Notes, startTime);
        }
    }

    private int GetNoteCountInRange(NoteData[] notes, double start)
    {
        return notes.Count(x => x.Time >= start && x.CountsTowardScore);
    }
    
    private int GetNoteCountInRange(NoteData[] notes, double start, double end)
    {
        return notes.Count(x => x.Time >= start && x.Time <= end && x.CountsTowardScore);
    }
    
    private int GetHoldNoteCountInRange(NoteData[] notes, double start)
    {
        return notes.Count(x => x.Time >= start && x.CountsTowardScore && x.Length > 0);
    }
    
    private int GetHoldNoteCountInRange(NoteData[] notes, double start, double end)
    {
        return notes.Count(x => x.Time >= start && x.Time <= end && x.CountsTowardScore && x.Length > 0);
    }
}