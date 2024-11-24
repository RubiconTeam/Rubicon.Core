using Rubicon.Core.Chart;
using Rubicon.Core.Data;

namespace Rubicon.Core.Meta;

/// <summary>
/// Used to hold important information about a song.
/// </summary>
[GlobalClass]
public partial class SongMeta : Resource
{
    /// <summary>
    /// The name of the song.
    /// </summary>
    [Export] public string Name = "Test";

    /// <summary>
    /// The raw name of the song used to load it.
    /// Should be PascalCased and not contain any spaces or symbols.
    /// </summary>
    [Export] public string RawName = "Test";

    /// <summary>
    /// The artist who made the song.
    /// </summary>
    [Export] public string Artist = "Hideo Kojima";

    /// <summary>
    /// The chart offset.
    /// </summary>
    [Export] public double Offset = 0d;
    
    /// <summary>
    /// The default ruleset for this chart.
    /// </summary>
    [Export] public string DefaultRuleset = ProjectSettings.GetSetting("rubicon/rulesets/default_ruleset").AsString();

    /// <summary>
    /// The icon that's associated with this song.
    /// </summary>
    [Export] public Texture2D Icon;

    /// <summary>
    /// The UI style to use for this song.
    /// </summary>
    [Export] public string UiStyle = ProjectSettings.GetSetting("rubicon/general/default_ui_style").AsString();

    /// <summary>
    /// The Note Skin to use for this song.
    /// </summary>
    [Export] public string NoteSkin = ProjectSettings.GetSetting("rubicon/rulesets/mania/default_note_skin").AsString();

    /// <summary>
    /// Marks the playable charts in this song.
    /// </summary>
    [Export] public StringName[] PlayableCharts = ["Player"];

    /// <summary>
    /// The characters to spawn in the song.
    /// </summary>
    [Export] public CharacterMeta[] Characters = [];
    
    /// <summary>
    /// A list of BPM changes.
    /// </summary>
    [Export] public BpmInfo[] BpmInfo = [];

    /// <summary>
    /// Determines what type of backend the engine will use when loading into a song.
    /// </summary>
    [Export] public GameEnvironment Environment = GameEnvironment.CanvasItem;

    /// <summary>
    /// The stage to spawn in for this song.
    /// </summary>
    [Export] public string Stage = "stage";

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