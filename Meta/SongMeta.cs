using Rubicon.Core.Chart;
using Rubicon.Core.Data;

namespace Rubicon.Core.Meta;

/// <summary>
/// Used to hold important information about a song.
/// </summary>
[GlobalClass] public partial class SongMeta : Resource
{
    /// <summary>
    /// The icon that's associated with this song.
    /// </summary>
    [Export] public Texture2D Icon;
    
    /// <summary>
    /// The name of the song.
    /// </summary>
    [Export] public string Name = "Test";

    /// <summary>
    /// The raw name of the song used to load it.
    /// Should be PascalCased and not contain any spaces or symbols.
    /// </summary>
    [Export] public string InternalName = "Test";

    /// <summary>
    /// The artist who made the song.
    /// </summary>
    [Export] public string Artist = "Hideo Kojima";

    /// <summary>
    /// The instrumental for this song.
    /// </summary>
    [Export] public AudioStream Instrumental;
    
    /// <summary>
    /// The vocals for this song.
    /// </summary>
    [Export] public AudioStream Vocals;
    
    /// <summary>
    /// A list of BPM changes.
    /// </summary>
    [Export] public BpmInfo[] BpmInfo = [];
    
    /// <summary>
    /// The UI style to use for this song.
    /// </summary>
    [ExportGroup("Style"), Export] public string UiStyle = ProjectSettings.GetSetting("rubicon/general/default_ui_style").AsString();

    /// <summary>
    /// The Note Skin to use for this song.
    /// </summary>
    [Export] public string NoteSkin = ProjectSettings.GetSetting("rubicon/rulesets/mania/default_note_skin").AsString();
    
    /// <summary>
    /// Determines what type of backend the engine will use when loading into a song.
    /// </summary>
    [ExportGroup("Environment"), Export] public GameEnvironment Environment = GameEnvironment.None;
    
    /// <summary>
    /// The stage to spawn in for this song.
    /// </summary>
    [Export] public string Stage = "stage";
    
    /// <summary>
    /// The characters to spawn in the song.
    /// </summary>
    [Export] public CharacterMeta[] Characters = [];

    /// <summary>
    /// Offsets the position of the notes, in milliseconds.s
    /// </summary>
    [ExportGroup("Options"), Export] public float Offset = 0f;
    
    /// <summary>
    /// The default ruleset for this chart.
    /// </summary>
    [Export] public string DefaultRuleset = ProjectSettings.GetSetting("rubicon/rulesets/default_ruleset").AsString();

    /// <summary>
    /// Marks the playable charts in this song.
    /// </summary>
    [Export] public StringName[] PlayableCharts = ["Player"];

    /// <summary>
    /// Converts everything in this chart to millisecond format.
    /// </summary>
    /// <returns>Itself</returns>
    public SongMeta ConvertData()
    {
        for (int i = 1; i < BpmInfo.Length; i++)
            BpmInfo[i].MsTime = BpmInfo[i - 1].MsTime + ConductorUtility.MeasureToMs(BpmInfo[i].Time - BpmInfo[i - 1].Time, BpmInfo[i - 1].Bpm, BpmInfo[i].TimeSignatureNumerator);

        return this;
    }
}