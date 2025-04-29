using System.Linq;
using Godot.Collections;
using Rubicon.Core.API;
using Rubicon.Core.Chart;
using Rubicon.Core.Data;
using Rubicon.Core.Meta;
using PukiTools.GodotSharp;
using PukiTools.GodotSharp.Audio;
using Rubicon.Core.UI;
using ApiConstants = Rubicon.Core.API.ApiConstants;

namespace Rubicon.Core.Rulesets;

/// <summary>
/// A control node with all general ruleset gameplay-related functions.
/// </summary>
[GlobalClass] public abstract partial class PlayField : Control
{
    /// <summary>
    /// The range in which the audio is allowed to be delayed from the Conductor.
    /// </summary>
    public const double LatencyThreshold = 0.15;
    
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
    [Export] public ScoreManager ScoreManager;

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
    /// The UiStyle currently being used
    /// </summary>
    [Export] public UiStyle UiStyle;

    /// <summary>
    /// A control node that's constantly on the screen.
    /// </summary>
    [Export] public GameHud GlobalHud;
    
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
    /// A signal that is emitted upon failure.
    /// </summary>
    [Signal] public delegate void FailedEventHandler();
    
    /// <summary>
    /// Emitted when the PlayField resynchronizes with the Conductor, in case of any delays.
    /// </summary>
    [Signal] public delegate void ResynchronizedEventHandler();
    
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
    /// Emitted when a new bar line is added.
    /// </summary>
    [Signal] public delegate void BarLineAddedEventHandler(StringName barLineName);
    
    /// <summary>
    /// Emitted when a bar line is removed.
    /// </summary>
    [Signal] public delegate void BarLineRemovedEventHandler(StringName barLineName);

    /// <summary>
    /// Readies the PlayField for gameplay!
    /// </summary>
    /// <param name="ruleSetData">The ruleset data.</param>
    /// <param name="meta">The song meta</param>
    /// <param name="chart">The chart loaded</param>
    /// <param name="targetIndex">The index to play in <see cref="SongMeta.PlayableCharts"/>.</param>
    public virtual void Setup(RuleSet ruleSetData, SongMeta meta, RubiChart chart, int targetIndex)
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        
        Name = "Base PlayField";
        RuleSet = ruleSetData;
        Metadata = meta;
        Chart = chart;
        
        Input.UseAccumulatedInput = false;

        Metadata.ConvertData();
        Chart.ConvertData(meta.TimeChanges);
        
        // Handle UI Style
        Factory = CreateNoteFactory();
        UiStyle = GD.Load<UiStyle>(Metadata.UiStyle);
        ScoreManager = CreateScoreManager();
        
        BarLines = new BarLine[chart.Charts.Length];
        TargetBarLine = meta.PlayableCharts[targetIndex];
        for (int i = 0; i < chart.Charts.Length; i++)
        {
            ChartData indChart = chart.Charts[i];
            BarLine curBarLine = CreateBarLine();
            curBarLine.Name = indChart.Name;
            curBarLine.PlayField = this;
            curBarLine.Setup(indChart, this);
            
            if (indChart.Name == TargetBarLine)
            {
                TargetIndex = i;
                curBarLine.SetAutoPlay(UserSettings.Rubicon.Autoplay);   
            }
            
            AddChild(curBarLine);
            BarLines[i] = curBarLine;
            curBarLine.NoteHit += BarLineHit;
            
            AfterBarLineSetup(curBarLine);
        }
        
        if (UiStyle.RuleSets.TryGetValue(RuleSet.UniqueId, out RuleSetUiData ruleSetUiStyle))
        {
            PackedScene mainHud = ruleSetUiStyle.GlobalHud;
            if (mainHud != null && mainHud.CanInstantiate())
                AssignGlobalHud(mainHud.InstantiateOrNull<GameHud>());

            PackedScene barLineHud = ruleSetUiStyle.LocalHud;
            if (barLineHud != null && barLineHud.CanInstantiate())
            {
                BarLine targetBarLine = BarLines[TargetIndex];
                targetBarLine.AssignLocalHud(barLineHud.InstantiateOrNull<GameHud>());
            }
        }
        
        Conductor.Reset();
        Conductor.Offset = Metadata.Offset;
        Conductor.TimeChanges = Metadata.TimeChanges;

        Music = AudioManager.GetGroup("Music").Play(Metadata.Instrumental, false);
        PrintUtility.Print("PlayField", "Instrumental loaded", true);

        // TODO: BAD CODE, CHANGE LATER
        Dictionary<StringName, Array<NoteData>> noteTypeMap = new Dictionary<StringName, Array<NoteData>>();
        string[] noteTypeList = Chart.GetAllNoteTypes();
        for (int t = 0; t < noteTypeList.Length; t++)
        {
            string curNoteType = noteTypeList[t];
            
            Array<NoteData> notes = new Array<NoteData>();
            for (int c = 0; c < Chart.Charts.Length; c++)
                notes.AddRange(Chart.Charts[c].GetNotesOfType(curNoteType));
            
            noteTypeMap[curNoteType] = notes;
        }
        
        foreach (StringName noteType in noteTypeMap.Keys)
        {
            if (!RubiconCore.NoteTypes.Paths.ContainsKey(noteType))
                continue;
            
            Node noteTypeScene = RubiconCore.NoteTypes.Paths[noteType].LoadAndInstantiate();
            InitializeGodotScript(noteTypeScene);
            AddChild(noteTypeScene);
        }
        
        foreach (var pair in noteTypeMap)
        {
            EmitSignalInitializeNote(pair.Value, pair.Key);
            pair.Value.Clear();
        }
        
        noteTypeMap.Clear();
        ScoreManager.Initialize(chart, TargetBarLine);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        if (HasFailed())
            Fail();

        if (Music == null || !Music.IsPlaying())
            return;

        // Synchronize with Conductor
        double currentMusicPosition = AudioServer.GetTimeSinceLastMix() + Music.GetPlaybackPosition();
        if (Mathf.Abs(currentMusicPosition - Conductor.AudioTime) <= LatencyThreshold)
            return;
        
        Music.Seek(Conductor.AudioTime);
        EmitSignalResynchronized();
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

        float time = Conductor.AudioTime;
        Music.Play(time);
    }

    /// <summary>
    /// Instantly kills the player and emits the signal.
    /// </summary>
    public void Fail()
    {
        ScoreManager.Rank = ScoreRank.F;
        ScoreManager.Clear = ClearRank.Failure;
        
        EmitSignalFailed();
    }

    /// <summary>
    /// Triggers every time the player hits a note to update their health.
    /// </summary>
    public abstract void UpdateHealth(Judgment hit);
    
    /// <summary>
    /// The fail condition for this play field.
    /// </summary>
    /// <returns>Whether the player has failed</returns>
    public virtual bool HasFailed() => false;

    public abstract NoteFactory CreateNoteFactory();

    /// <summary>
    /// Creates a new bar line.
    /// </summary>
    /// <returns>A new <see cref="BarLine"/></returns>
    public abstract BarLine CreateBarLine();
    
    public abstract ScoreManager CreateScoreManager();

    /// <summary>
    /// Invoked right after <see cref="BarLine.Setup"/> gets called.
    /// </summary>
    /// <param name="barLine">The bar line that just got setup.</param>
    public abstract void AfterBarLineSetup(BarLine barLine);

    public void AddBarLine(BarLine barLine)
    {
        BarLines = [..BarLines, barLine];
        EmitSignalBarLineAdded(barLine.Name);
    }

    public void RemoveBarLine(BarLine barLine)
    {
        StringName name = barLine.Name;
        
        Array<BarLine> barLines = new Array<BarLine>(BarLines);
        barLines.Remove(barLine);
        BarLines = barLines.ToArray();
        
        EmitSignalBarLineRemoved(name);
    }

    public virtual void AssignGlobalHud(GameHud hud, bool queueFree = true, bool keepTransform = false)
    {
        if (hud == null || GlobalHud == hud)
            return;
        
        if (queueFree)
            GlobalHud?.QueueFree();
        
        GlobalHud = hud;
        GlobalHud.Name = "HUD";
        
        if (GlobalHud.GetParent() == null)
            AddChild(GlobalHud);
        else
            GlobalHud.Reparent(this, keepTransform);
        
        GlobalHud.Setup(this);
    }

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
                ScoreManager.JudgeNoteResult(result);
        }
        
        EmitSignalNoteHit(name, result);
    }

    public void InitializeGodotScript(Node node)
    {
        // Handle CSharp elements first.
        if (node is IPlayElement element)
        {
            element.PlayField = this;
            element.Initialize();

            if (node is CsHudElement hudElement)
                UserSettings.SettingsChanged += hudElement.OptionsUpdated;
            
            return;
        }
        
        // Handle any other programming language
        if (!node.InheritsFrom(ApiConstants.ValidGodotScriptNames))
            return;
        
        node.Set("play_field", this);
        node.Call("initialize");

        if (node.InheritsFrom(ApiConstants.GdScriptHudElement))
            Connect(UserSettingsInstance.SignalName.SettingsChanged, new Callable(node, "options_updated"));
    }
}