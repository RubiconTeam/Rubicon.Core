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
[GlobalClass] public abstract partial class ScoreManager : RefCounted
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
    /// </summary>
    [Export] public int ComboBreaks = 0;

    /// <summary>
    /// The highest amount of notes hit in a row.
    /// </summary>
    [Export] public int HighestCombo = 0;
    
    /// /// <summary>
    /// A reference to the player's chart.
    /// </summary>
    [ExportGroup("References"), Export] public ChartData Chart;
    
    /// <summary>
    /// Triggers upon the statistics updating.
    /// </summary>
    [Signal] public delegate void StatisticsUpdatedEventHandler(long combo, Judgment hit, float distance);
    
    public virtual void Initialize(RubiChart chart, StringName target)
    {
        Chart = chart.Charts.FirstOrDefault(x => x.Name == target);
    }
    
    public abstract void JudgeNoteResult(NoteResult result);
}