using System.Collections.Generic;
using System.Linq;
using Rubicon.Core.Chart;
using Rubicon.Core.Data;
using Range = System.Range;

namespace Rubicon.Core.Rulesets;

/// <summary>
/// A class that tracks the results of a song,
/// such as Score, Rank, Accuracy and Judgments.
/// </summary>
[GlobalClass] public partial class ScoreTracker : RefCounted
{
    /// <summary>
    /// Current score of the song.
    /// On perfect circumstances, it should reach the maximum score set by the <see cref="RuleSet"/>
    /// </summary>
    [Export] public int Score = 0;

    /// <summary>
    /// Current rank of the song.
    /// Depends on the amount of <see cref="Score"/>.
    /// </summary>
    [Export] public ScoreRank Rank = ScoreRank.P;

    /// <summary>
    /// Current clear rank of the song.
    /// Depends on the amount of misses and the accuracy of the hit notes.
    /// </summary>
    [Export] public ClearRank Clear = ClearRank.Perfect;

    /// <summary>
    /// Current accuracy of the hit/missed notes.
    /// </summary>
    [Export] public float Accuracy = 100f;

    /// <summary>
    /// The amount of perfect hits.
    /// </summary>
    [Export] public int PerfectHits = 0;
    
    /// <summary>
    /// The amount of great hits.
    /// </summary>
    [Export] public int GreatHits = 0;
    
    /// <summary>
    /// The amount of good hits.
    /// </summary>
    [Export] public int GoodHits = 0;

    /// <summary>
    /// The amount of okay hits.
    /// </summary>
    [Export] public int OkayHits = 0;
    
    /// <summary>
    /// The amount of bad hits.
    /// </summary>
    [Export] public int BadHits = 0;
    
    /// <summary>
    /// The amount of misses.
    /// </summary>
    [Export] public int Misses = 0;

    /// <summary>
    /// The current miss streak.
    /// The damage taken from missing a note will be multiplied by it.
    /// </summary>
    [Export] public int MissStreak = 0;

    /// <summary>
    /// The amount of notes hit in a row.
    /// </summary>
    [Export] public int Combo = 0;

    /// <summary>
    /// The amount of times a combo has been broken.
    /// Not necessarily the same as <see cref="Misses"/>.
    /// </summary>
    [Export] public int ComboBreaks = 0;

    /// <summary>
    /// The highest amount of notes hit in a row.
    /// </summary>
    [Export] public int HighestCombo = 0;

    /// <summary>
    /// How many notes there is in the chart.
    /// </summary>
    [Export] public int NoteCount = 0;

    /// <summary>
    /// The highest combo that can be achieved in a song.
    /// </summary>
    [Export] public int MaxCombo = 0;

    /// <summary>
    /// The amount of notes hit, counting the start of a hold note.
    /// Only takes account of notes that count towards score.
    /// </summary>
    [Export] public int NotesHit = 0;

    /// <summary>
    /// The amount of notes hit, counting the start and end of a hold note.
    /// Only takes account of notes that count towards score.
    /// </summary>
    [Export] public int TailsHit = 0;
    
    /// /// <summary>
    /// The chart of the current song and difficulty.
    /// </summary>
    [ExportGroup("References"), Export] public RubiChart Chart;
    
    /// <summary>
    /// Sets the <see cref="NoteCount"/> and <see cref="MaxCombo"/> from the chart.
    /// </summary>
    /// <param name="chart">Which chart should be accounted.</param>
    /// <param name="target">Which character's <see cref="IndividualChart"/> should be targeted.</param>
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