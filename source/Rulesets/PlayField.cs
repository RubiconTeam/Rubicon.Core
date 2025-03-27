using System.Linq;
using Godot.Collections;
using Rubicon.Core.API;
using Rubicon.Core.Chart;
using Rubicon.Core.Data;
using Rubicon.Core.Events;
using Rubicon.Core.Meta;
using PukiTools.GodotSharp;
using PukiTools.GodotSharp.Audio;
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
    /// The ruleset data for this PlayField.
    /// </summary>
    [Export] public RuleSet RuleSet;

    /// <summary>
    /// The events for this song.
    /// </summary>
    [Export] public EventMeta Events;

    /// <summary>
    /// The UiStyle currently being used
    /// </summary>
    [Export] public UiStyle UiStyle;

    /// <summary>
    /// A control node that's constantly on the screen.
    /// </summary>
    [Export] public GameHud GameHud;

    /// <summary>
    /// A control node that's attached to the player's <see cref="BarLine"/>.
    /// </summary>
    [Export] public BarLineHud PlayerHud;
    
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
    [Signal] public delegate void StatisticsUpdatedEventHandler(long combo, Judgment hit, float distance);
    
    /// <summary>
    /// A signal that is emitted upon failure.
    /// </summary>
    [Signal] public delegate void FailedEventHandler();
    
    /// <summary>
    /// Emitted for every note type to set everything up initially.
    /// </summary>
    [Signal] public delegate void InitializeNoteEventHandler(Array<NoteData> notes, StringName noteType);
    
    /// <summary>
    /// A signal that is emitted in case other note types need to modify the note result.
    /// </summary>
    [Signal] public delegate void ModifyResultEventHandler(StringName barLineName, NoteResult element);
    
    /// <summary>
    /// Emitted after every note type processes through the result.
    /// </summary>
    [Signal] public delegate void NoteHitEventHandler(StringName barLineName, NoteResult element);
    
    /// <summary>
    /// Emitted by <see cref="PlayField.UpdateOptions"/>, in case anything needs changing after options were changed.
    /// </summary>
    [Signal] public delegate void OptionsUpdatedEventHandler();

    /// <summary>
    /// Readies the PlayField for gameplay!
    /// </summary>
    /// <param name="meta">The song meta</param>
    /// <param name="chart">The chart loaded</param>
    /// <param name="targetIndex">The index to play in <see cref="SongMeta.PlayableCharts"/>.</param>
    public virtual void Setup(RuleSet ruleSetData, SongMeta meta, RubiChart chart, int targetIndex, EventMeta events = null)
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        
        Name = "Base PlayField";
        RuleSet = ruleSetData;
        Metadata = meta;
        Chart = chart;
        Events = events;
        
        Input.UseAccumulatedInput = false;

        Metadata.ConvertData();
        Chart.ConvertData(meta.BpmInfo).Format();
        
        // Handle UI Style
        string uiStylePath = $"res://resources/ui/styles/{Metadata.UiStyle}/Style";
        if (!PathUtility.ResourceExists(uiStylePath))
        {
            string defaultUi = ProjectSettings.GetSetting("rubicon/general/default_ui_style").AsString();
            string defaultUiPath = $"res://resources/ui/styles/{defaultUi}/Style";
            GD.PrintErr($"[PlayField] UI Style {Metadata.UiStyle} does not exist. Defaulting to {defaultUi}");
            uiStylePath = defaultUiPath;
        }
        
        UiStyle = ResourceLoader.LoadThreadedGet(PathUtility.GetResourcePath(uiStylePath)) as UiStyle;
        
        BarLines = new BarLine[chart.Charts.Length];
        TargetBarLine = meta.PlayableCharts[targetIndex];
        Dictionary<StringName, Array<NoteData>> noteTypeMap = new Dictionary<StringName, Array<NoteData>>();
        for (int i = 0; i < chart.Charts.Length; i++)
        {
            IndividualChart indChart = chart.Charts[i];
            for (int n = 0; n < indChart.Notes.Length; n++)
            {
                NoteData curNote = indChart.Notes[n];
                StringName noteType = curNote.Type;
                
                if (!noteTypeMap.ContainsKey(noteType))
                    noteTypeMap[noteType] = [];
                
                noteTypeMap[noteType].Add(curNote);
            }
            
            BarLine curBarLine = CreateBarLine(indChart, i);
            curBarLine.Name = indChart.Name;
            curBarLine.PlayField = this;
            if (indChart.Name == TargetBarLine)
            {
                TargetIndex = i;
                curBarLine.SetAutoPlay(UserSettings.Rubicon.Autoplay);   
            }
            
            AddChild(curBarLine);
            BarLines[i] = curBarLine;
            curBarLine.NoteHit += BarLineHit;
        }
        
        Conductor.Reset();
        Conductor.ChartOffset = Metadata.Offset;
        Conductor.BpmList = Metadata.BpmInfo;

        Music = AudioManager.GetGroup("Music").Play(Metadata.Instrumental, false);
        PrintUtility.Print("PlayField", "Instrumental loaded", true);
        
        // TODO: LOAD AUTOLOADS AND NOTE TYPES!!!!
        
        if (Events != null)
        {
            for (int i = 0; i < Events.Events.Length; i++)
                Events.Events[i].ConvertData(Metadata.BpmInfo);
            
            EventController = new SongEventController();
            EventController.Setup(events, this);
            AddChild(EventController);
        }

        if (UiStyle.MainHud != null && UiStyle.MainHud.CanInstantiate())
        {
            GameHud = UiStyle.MainHud.Instantiate<GameHud>();
            AddChild(GameHud);
            
            GameHud.Setup(this);
        }
        
        if (UiStyle.BarLineHud != null && UiStyle.BarLineHud.CanInstantiate())
        {
            PlayerHud = UiStyle.BarLineHud.Instantiate<BarLineHud>();
            PlayerHud.Setup(BarLines[TargetIndex], this);
        }
        
        UpdateOptions();

        // TODO: BAD CODE, CHANGE LATER
        foreach (StringName noteType in noteTypeMap.Keys)
        {
            if (!RubiconCore.NoteTypePaths.ContainsKey(noteType))
                continue;
            
            string noteTypePath = RubiconCore.NoteTypePaths[noteType];
            Resource noteTypeResource = ResourceLoader.LoadThreadedGet(noteTypePath);
            if (noteTypeResource is PackedScene packedScene)
            {
                Node node = packedScene.Instantiate();
                InitializeGodotScript(node);
                AddChild(node);
            }

            if (noteTypeResource is GDScript gdScript)
            {
                Node node = gdScript.New().As<Node>();
                InitializeGodotScript(node);
                AddChild(node);
            }

            if (noteTypeResource is CSharpScript csScript)
            {
                Node node = csScript.New().As<Node>();
                InitializeGodotScript(node);
                AddChild(node);
            }
        }
        
        foreach (var pair in noteTypeMap)
        {
            EmitSignalInitializeNote(pair.Value, pair.Key);
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

    public void Start(float time = 0f)
    {
        Conductor.Play(time);
        Music.Play(time);
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
    /// Get the judgment UI component for the currently running PlayField.
    /// </summary>
    /// <returns>The Judgment UI component if it exists, or null if it doesn't</returns>
    public Node GetJudgment()
    {
        if (PlayerHud != null && PlayerHud.Judgment != null)
            return PlayerHud.Judgment;
        
        if (GameHud != null && GameHud.Judgment != null)
            return GameHud.Judgment;
        
        return null;
    }
    
    /// <summary>
    /// Get the combo display UI component for the currently running PlayField.
    /// </summary>
    /// <returns>The combo display UI component if it exists, or null if it doesn't</returns>
    public Node GetComboDisplay()
    {
        if (PlayerHud != null && PlayerHud.Combo != null)
            return PlayerHud.Combo;
        
        if (GameHud != null && GameHud.Combo != null)
            return GameHud.Combo;
        
        return null;
    }
    
    /// <summary>
    /// Get the hit distance UI component for the currently running PlayField.
    /// </summary>
    /// <returns>The hit distance UI component if it exists, or null if it doesn't</returns>
    public Node GetHitDistance()
    {
        if (PlayerHud != null && PlayerHud.HitDistance != null)
            return PlayerHud.HitDistance;
        
        if (GameHud != null && GameHud.HitDistance != null)
            return GameHud.HitDistance;
        
        return null;
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
    public abstract void UpdateHealth(Judgment hit);
    
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
                UpdateHealth(result.Rating);
        
            if (!result.Flags.HasFlag(NoteResultFlags.Score))
            {
                Judgment rating = result.Rating;
                if (result.Note.CountsTowardScore)
                {
                    if (result.Hit == Hit.Tap || result.Hit == Hit.Hold) // Tap note or initial tap of hold note
                    {
                        ScoreTracker.NotesHit++;
                        
                        switch (rating)
                        {
                            case Judgment.Perfect:
                                ScoreTracker.PerfectHits++;
                                ScoreTracker.Combo++;
                                break;
                            case Judgment.Great:
                                ScoreTracker.GreatHits++;
                                ScoreTracker.Combo++;
                                break;
                            case Judgment.Good:
                                ScoreTracker.GoodHits++;
                                ScoreTracker.Combo++;
                                break;
                            case Judgment.Okay:
                                ScoreTracker.OkayHits++;
                                ScoreTracker.ComboBreaks++;
                                ScoreTracker.Combo = 0;
                                break;
                            case Judgment.Bad:
                                ScoreTracker.BadHits++;
                                ScoreTracker.ComboBreaks++;
                                ScoreTracker.Combo = 0;
                                break;
                            case Judgment.Miss:
                                ScoreTracker.Misses++;
                                if (result.Note.Length > 0)
                                    ScoreTracker.Misses++;
                                break;
                        }
                    }
                    else // Hold note end
                    {
                        ScoreTracker.TailsHit++;
                        
                        switch (rating)
                        {
                            case Judgment.Perfect:
                                ScoreTracker.PerfectHits++;
                                break;
                            case Judgment.Miss:
                                ScoreTracker.Misses++;
                                break;
                        }
                    }   
                }

                if (rating == Judgment.Miss)
                {
                    ScoreTracker.MissStreak++;
                    ScoreTracker.Combo = 0;
                    ScoreTracker.ComboBreaks++;
                }
                else if (rating != Judgment.None)
                {
                    ScoreTracker.MissStreak = 0;   
                }
                
                if (ScoreTracker.Combo > ScoreTracker.HighestCombo)
                    ScoreTracker.HighestCombo = ScoreTracker.Combo;   
            
                UpdateStatistics();
                
                if (result.Rating != Judgment.None && result.Hit != Hit.Tail)
                    EmitSignalStatisticsUpdated(ScoreTracker.Combo, result.Rating, result.Distance);
            }
        }
        
        EmitSignalNoteHit(name, result);
    }

    public void HandleGhostTap(StringName barLineName, int index)
    {
        ScoreTracker.Combo = 0;
        ScoreTracker.ComboBreaks++;
        UpdateHealth(Judgment.Miss);
        
        UpdateStatistics();
        EmitSignalStatisticsUpdated(ScoreTracker.Combo, Judgment.None, ProjectSettings.GetSetting("rubicon/judgments/bad_hit_window").AsSingle() + 1f);
    }

    public void InitializeGodotScript(Node node)
    {
        // Handle CSharp elements first.
        if (node is IPlayElement element)
        {
            element.PlayField = this;
            element.Initialize();

            if (node is CsHudElement hudElement)
                OptionsUpdated += hudElement.OptionsUpdated;
            
            return;
        }
        
        // Handle any other programming language
        if (!node.InheritsFrom(ApiConstants.ValidGodotScriptNames))
            return;
        
        node.Set("play_field", this);
        node.Call("initialize");

        if (node.InheritsFrom(ApiConstants.GdScriptHudElement))
            Connect(SignalName.OptionsUpdated, new Callable(node, "options_updated"));
    }
}