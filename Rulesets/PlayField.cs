using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rubicon.Core.API;
using Rubicon.Core.Audio;
using Rubicon.Core.Chart;
using Rubicon.Core.Data;
using Rubicon.Core.Events;
using Rubicon.Core.Meta;
using Rubicon.Core.Settings;
using Rubicon.Core.UI;

namespace Rubicon.Core.Rulesets;

/// <summary>
/// A control node with all general ruleset gameplay-related functions.
/// </summary>
[GlobalClass] public abstract partial class PlayField : Control
{
    /// <summary>
    /// The current health the player has.
    /// </summary>
    [Export] public int Health = 50;

    /// <summary>
    /// The max health the player can have.
    /// </summary>
    [Export] public int MaxHealth = 100;

    /// <summary>
    /// Keeps track of if the play field is paused or not.
    /// </summary>
    [Export] public bool Paused = false;

    /// <summary>
    /// Keeps track of the player's combos and score.
    /// </summary>
    [Export] public ScoreTracker ScoreTracker = new();

    /// <summary>
    /// The Chart for this PlayField.
    /// </summary>
    [Export] public RubiChart Chart;

    /// <summary>
    /// The Song meta for this PlayField.
    /// </summary>
    [Export] public SongMeta Metadata;

    /// <summary>
    /// The events for this song.
    /// </summary>
    [Export] public EventMeta Events;

    /// <summary>
    /// The UiStyle currently being used
    /// </summary>
    [Export] public UiStyle UiStyle;

    /// <summary>
    /// A control node that displays cool things + potentially important statistics for the player.
    /// </summary>
    [Export] public PlayHud Hud;
    
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
    /// The music player for this play field.
    /// </summary>
    [Export] public AudioStreamPlayer Music;

    /// <summary>
    /// Controls events that can happen throughout the song.
    /// </summary>
    [Export] public SongEventController EventController;
    
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
    /// A signal that is emitted in case other note types need to modify the note result.
    /// </summary>
    [Signal] public delegate void ModifyResultEventHandler(StringName barLineName, NoteResult element);
    
    /// <summary>
    /// Emitted after every note type processes through the result.
    /// </summary>
    [Signal] public delegate void NoteHitEventHandler(StringName barLineName, NoteResult element);

    /// <summary>
    /// Readies the PlayField for gameplay!
    /// </summary>
    /// <param name="meta">The song meta</param>
    /// <param name="chart">The chart loaded</param>
    /// <param name="targetIndex">The index to play in <see cref="SongMeta.PlayableCharts"/>.</param>
    public virtual void Setup(SongMeta meta, RubiChart chart, int targetIndex, EventMeta events = null)
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        
        Name = "Base PlayField";
        Metadata = meta;
        Chart = chart;
        Events = events;
        
        Input.UseAccumulatedInput = false;

        Chart.ConvertData(meta.BpmInfo).Format();
        
        // Handle UI Style
        string uiStylePath = $"res://Resources/UI/Styles/{Metadata.UiStyle}/Style";
        if (!PathUtility.ResourceExists(uiStylePath))
        {
            string defaultUi = ProjectSettings.GetSetting("rubicon/general/default_ui_style").AsString();
            string defaultUiPath = $"res://Resources/UI/Styles/{defaultUi}/Style";
            GD.PrintErr($"[PlayField] UI Style {Metadata.UiStyle} does not exist. Defaulting to {defaultUi}");
            uiStylePath = defaultUiPath;
        }
        
        UiStyle = ResourceLoader.LoadThreadedGet(PathUtility.GetResourcePath(uiStylePath)) as UiStyle;
        if (UiStyle.HitDistance != null && UiStyle.HitDistance.CanInstantiate())
        {
            Node hitDistance = UiStyle.HitDistance.Instantiate();
            InitializeGodotScript(hitDistance);
            AddChild(hitDistance);
        }

        if (UiStyle.Judgment != null && UiStyle.Judgment.CanInstantiate())
        {
            Node judgment = UiStyle.Judgment.Instantiate();
            InitializeGodotScript(judgment);
            AddChild(judgment);
        }

        if (UiStyle.Combo != null && UiStyle.Combo.CanInstantiate())
        {
            Node combo = UiStyle.Combo.Instantiate();
            InitializeGodotScript(combo);
            AddChild(combo);   
        }
        
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
                curBarLine.SetAutoPlay(UserSettings.Gameplay.Autoplay);   
            }
            
            AddChild(curBarLine);
            BarLines[i] = curBarLine;
            curBarLine.NoteHit += BarLineHit;
        }
        
        UpdateOptions();
        
        Conductor.Reset();
        Conductor.ChartOffset = Metadata.Offset;
        Conductor.BpmList = Metadata.BpmInfo;

        Music = AudioManager.Music.Player;
        AudioManager.Music.Player.Stream = Metadata.Instrumental;
        
        // TODO: LOAD AUTOLOADS AND NOTE TYPES!!!!
        
        if (Events != null)
        {
            for (int i = 0; i < Events.Events.Length; i++)
                Events.Events[i].ConvertData(Metadata.BpmInfo);
            
            EventController = new SongEventController();
            EventController.Setup(events, this);
            AddChild(EventController);
        }
        
        if (UiStyle.PlayHud != null && UiStyle.PlayHud.CanInstantiate())
        {
            Hud = UiStyle.PlayHud.Instantiate<PlayHud>();
            AddChild(Hud);
            
            Hud.Setup(this);
            Hud.UpdatePosition(UserSettings.Gameplay.DownScroll);
        }
        
        foreach (var pair in noteTypeMap)
        {
            EmitSignalInitializeNote(pair.Value.ToArray(), pair.Key);
            pair.Value.Clear();
        }
        
        noteTypeMap.Clear();
        ScoreTracker.Initialize(chart, TargetBarLine);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        if (GetFailCondition())
            Fail();
    }

    public void Start()
    {
        Conductor.Play();
        Music.Play();
    }

    public void Pause()
    {
        ProcessMode = ProcessModeEnum.Disabled;
        Conductor.Pause();
        Music.Stop();
    }

    public void Resume()
    {
        ProcessMode = ProcessModeEnum.Inherit;
        Conductor.Resume();
        
        float time = Conductor.RawTime;
        Music.Play(time);
    }

    /// <summary>
    /// Instantly kills the player and emits the signal.
    /// </summary>
    public void Fail()
    {
        ScoreTracker.Rank = ScoreRank.F;
        ScoreTracker.Clear = ClearRank.Failure;
        
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
    /// Triggers every time the player hits a note to update their health.
    /// </summary>
    public abstract void UpdateHealth(HitType hit);
    
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
        EmitSignalModifyResult(name, result);

        bool isPlayer = TargetBarLine == name;
        if (isPlayer)
        {
            if (!result.Flags.HasFlag(NoteResultFlags.Health))
                UpdateHealth(result.Hit);
        
            if (!result.Flags.HasFlag(NoteResultFlags.Score))
            {
                HitType hit = result.Hit;
                bool isInitialTap = result.Note.Length == 0f || result.Note.Length > 0f && result.Holding && result.Tapped;
                if (isInitialTap) // Tap note or initial tap of hold note
                {
                    ScoreTracker.TapsHit++;
                    
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
                            if (result.Note.Length > 0)
                                ScoreTracker.Misses++;
                            break;
                    }

                    if (hit >= HitType.Okay)
                    {
                        ScoreTracker.ComboBreaks++;
                        ScoreTracker.Combo = 0;
                    }
                    else
                    {
                        ScoreTracker.Combo++;
                    }
            
                    if (ScoreTracker.Combo > ScoreTracker.HighestCombo)
                        ScoreTracker.HighestCombo = ScoreTracker.Combo;   
                }
                else // Hold note end
                {
                    switch (hit)
                    {
                        case HitType.Perfect:
                            ScoreTracker.PerfectHits++;
                            break;
                        case HitType.Miss:
                            ScoreTracker.Misses++;
                            ScoreTracker.Combo = 0;
                            ScoreTracker.ComboBreaks++;
                            break;
                    }
                }

                if (hit == HitType.Miss)
                    ScoreTracker.MissStreak++;
                else
                    ScoreTracker.MissStreak = 0;

                ScoreTracker.TotalHit++;
            
                UpdateStatistics();
                
                if (isInitialTap)
                    EmitSignalStatisticsUpdated(ScoreTracker.Combo, result.Hit, result.Distance);
            }
        }
        
        EmitSignalNoteHit(name, result);
    }

    public void HandleGhostTap(StringName barLineName, int index)
    {
        ScoreTracker.Combo = 0;
        ScoreTracker.ComboBreaks++;
        UpdateHealth(HitType.Miss);
        
        UpdateStatistics();
        EmitSignalStatisticsUpdated(ScoreTracker.Combo, HitType.None, ProjectSettings.GetSetting("rubicon/judgments/bad_hit_window").AsSingle() + 1f);
    }

    public void InitializeGodotScript(Node node)
    {
        if (node is IPlayElement element)
        {
            element.PlayField = this;
            element.Initialize();
            return;
        }
        
        Script currentScript = node.GetScript().As<Script>();
        bool isValidScript = false;
        while (currentScript is not null)
        {
            string[] validScriptNames = ApiConstants.ValidGodotScriptNames;
            for (int i = 0; i < validScriptNames.Length; i++)
            {
                if (currentScript.GetGlobalName() == validScriptNames[i])
                {
                    isValidScript = true;
                    break;
                }
            }

            if (isValidScript)
                break;

            currentScript = currentScript.GetBaseScript();
        }

        if (!isValidScript)
            return;
        
        node.Set("play_field", this);
        node.Call("initialize");
    }
}