using System.Collections.Generic;
using System.Linq;
using Rubicon.Core.Chart;
using Rubicon.Core.Data;
using Rubicon.Core.Meta;

namespace Rubicon.Core.Rulesets;

/// <summary>
/// A control node with all general ruleset gameplay-related functions.
/// </summary>
[GlobalClass] public abstract partial class PlayField : Control
{
    /// <summary>
    /// The current health the player has.
    /// </summary>
    [Export] public uint Health = 0;

    /// <summary>
    /// The max health the player can have.
    /// </summary>
    [Export] public uint MaxHealth = 1000;

    /// <summary>
    /// Keeps track of the player's combos and score.
    /// </summary>
    [Export] public ScoreTracker ScoreTracker = new();

    /// <summary>
    /// The Chart for this PlayField.
    /// </summary>
    [Export] public RubiChart Chart;

    /// <summary>
    /// The Song meta for this PlayField
    /// </summary>
    [Export] public SongMeta Metadata;

    /// <summary>
    /// The UiStyle currently being used
    /// </summary>
    [Export] public UiStyle UiStyle;
    
    /// <summary>
    /// The bar lines associated with this play field.
    /// </summary>
    [Export] public BarLine[] BarLines;
    
    /// <summary>
    /// The Target Bar Line's name for the player to control
    /// </summary>
    [Export] public StringName TargetBarLine = "Player";
    
    /// <summary>
    /// The Target Bar Line's index for the player to control
    /// </summary>
    [Export] public int TargetIndex = 0;

    /// <summary>
    /// Creates notes for bar lines to use.
    /// </summary>
    [Export] public NoteFactory Factory;

    /// <summary>
    /// An event that is invoked when a note is hit.
    /// </summary>
    [Export] public RubiconEvent GetNoteResults = new();
    
    /// <summary>
    /// Triggers upon the statistics updating.
    /// </summary>
    [Signal] public delegate void StatisticsUpdatedEventHandler(long combo, HitType hit, float distance);
    
    /// <summary>
    /// A signal that is emitted upon failure.
    /// </summary>
    [Signal] public delegate void FailedEventHandler();
    
    /// <summary>
    /// Emitted for every note type to set everything up initially.
    /// </summary>
    [Signal] public delegate void InitializeNoteEventHandler(NoteData[] notes, StringName noteType);
    
    /// <summary>
    /// A signal that is emitted in case other note types need to modify the note result. Always is called.
    /// </summary>
    [Signal] public delegate void NoteHitEventHandler(StringName barLineName, NoteResult element);
    
    /// <summary>
    /// A signal that is emitted when calling for a sing animation.
    /// </summary>
    [Signal] public delegate void SingCalledEventHandler(StringName barLineName, NoteResult element);

    /// <summary>
    /// Readies the PlayField for gameplay!
    /// </summary>
    /// <param name="meta">The song meta</param>
    /// <param name="chart">The chart loaded</param>
    /// <param name="targetIndex">The index to play in <see cref="SongMeta.PlayableCharts"/>.</param>
    public virtual void Setup(SongMeta meta, RubiChart chart, int targetIndex)
    {
        Name = "Base PlayField";
        Metadata = meta;
        Chart = chart;
        SetAnchorsPreset(LayoutPreset.FullRect);
        Input.UseAccumulatedInput = false;
        
        // Handle UI Style
        string uiStylePath = $"res://Resources/UI/{Metadata.UiStyle}/Style.tres";
        if (!ResourceLoader.Exists(uiStylePath))
        {
            string defaultUiPath = $"res://Resources/UI/{ProjectSettings.GetSetting("rubicon/general/default_ui_style")}/style.tres";
            GD.PrintErr($"UI Style Path: {uiStylePath} does not exist. Defaulting to {defaultUiPath}");
            uiStylePath = defaultUiPath;
        }
        UiStyle = GD.Load<UiStyle>(uiStylePath);
        if (UiStyle.HitDistance != null && UiStyle.HitDistance.CanInstantiate())
            AddChild(UiStyle.HitDistance.Instantiate());
        if (UiStyle.Judgment != null && UiStyle.Judgment.CanInstantiate())
            AddChild(UiStyle.Judgment.Instantiate());
        if (UiStyle.Combo != null && UiStyle.Combo.CanInstantiate())
            AddChild(UiStyle.Combo.Instantiate());
        
        BarLines = new BarLine[chart.Charts.Length];
        TargetBarLine = meta.PlayableCharts[targetIndex];
        Dictionary<StringName, List<NoteData>> noteTypeMap = new Dictionary<StringName, List<NoteData>>();
        for (int i = 0; i < chart.Charts.Length; i++)
        {
            IndividualChart indChart = chart.Charts[i];
            for (int n = 0; n < indChart.Notes.Length; n++)
            {
                NoteData curNote = indChart.Notes[n];
                StringName noteType = curNote.Type;
                
                if (!noteTypeMap.ContainsKey(noteType))
                    noteTypeMap[noteType] = new List<NoteData>();
                
                noteTypeMap[noteType].Add(curNote);
            }
            
            BarLine curBarLine = CreateBarLine(indChart, i);
            curBarLine.Name = indChart.Name;
            curBarLine.PlayField = this;
            if (indChart.Name == TargetBarLine)
            {
                TargetIndex = i;
                curBarLine.SetAutoPlay(false);   
            }
            
            AddChild(curBarLine);
            BarLines[i] = curBarLine;
            curBarLine.NoteHit += BarLineHit;
        }

        foreach (var pair in noteTypeMap)
        {
            EmitSignalInitializeNote(pair.Value.ToArray(), pair.Key);
            pair.Value.Clear();
        }
        noteTypeMap.Clear();
        
        ScoreTracker.Initialize(chart, TargetBarLine);
        UpdateOptions();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        if (GetFailCondition())
            Fail();
    }

    /// <summary>
    /// Instantly kills the player and emits the signal.
    /// </summary>
    public void Fail()
    {
        EmitSignalFailed();
    }

    /// <summary>
    /// This function is triggered upon an update to the settings.
    /// </summary>
    public abstract void UpdateOptions();

    /// <summary>
    /// Triggers every time the player hits a note to update the in-game statistics
    /// </summary>
    public abstract void UpdateStatistics();
    
    /// <summary>
    /// The fail condition for this play field.
    /// </summary>
    /// <returns>Whether the player has failed</returns>
    public virtual bool GetFailCondition() => false;

    /// <summary>
    /// Creates a new bar line and sets it up along with it.
    /// </summary>
    /// <param name="chart">The chart to assign</param>
    /// <param name="index">The assigned index of the bar line</param>
    /// <returns>A new <see cref="BarLine"/></returns>
    public abstract BarLine CreateBarLine(IndividualChart chart, int index);

    /// <summary>
    /// The function that is connected to the bar lines when a note is hit. Can be overriden if needed for a specific ruleset.
    /// </summary>
    /// <param name="name">The bar line's name</param>
    /// <param name="result">Info about the input received</param>
    private void BarLineHit(StringName name, NoteResult result)
    {
        EmitSignalNoteHit(name, result);
        
        if (TargetBarLine == name && !result.Flags.HasFlag(NoteResultFlags.Score))
        {
            HitType hit = result.Hit;
            ScoreTracker.Combo = hit != HitType.Miss ? ScoreTracker.Combo + 1 : 0;
            if (ScoreTracker.Combo > ScoreTracker.HighestCombo)
                ScoreTracker.HighestCombo = ScoreTracker.Combo;

            switch (hit)
            {
                case HitType.Perfect:
                    ScoreTracker.PerfectHits++;
                    break;
                case HitType.Great:
                    ScoreTracker.GreatHits++;
                    break;
                case HitType.Good:
                    ScoreTracker.GoodHits++;
                    break;
                case HitType.Okay:
                    ScoreTracker.OkayHits++;
                    break;
                case HitType.Bad:
                    ScoreTracker.BadHits++;
                    break;
                case HitType.Miss:
                    ScoreTracker.Misses++;
                    break;
            }
            
            UpdateStatistics();
            EmitSignalStatisticsUpdated(ScoreTracker.Combo, result.Hit, result.Distance);
        }
        
        if (!result.Flags.HasFlag(NoteResultFlags.Animation))
            EmitSignalSingCalled(name, result);
        
        result.Free();
    }
}