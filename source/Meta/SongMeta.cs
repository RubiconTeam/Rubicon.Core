using System.Linq;
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

    [Export] public SongDifficulty[] Difficulties = [];
    
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

    /// <summary>
    /// Gets a difficulty by direct index.
    /// </summary>
    /// <param name="index">The index to search for</param>
    /// <returns>A difficulty if found, null if not.</returns>
    public SongDifficulty GetDifficultyByIndex(int index)
    {
        if (index >= Difficulties.Length)
            return null;
        
        return Difficulties[index];
    }

    /// <summary>
    /// Gets a difficulty by name and ruleset.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="ruleSet">The ruleset the difficulty is associated with.</param>
    /// <returns>A difficulty if found, null if not.</returns>
    public SongDifficulty GetDifficultyByName(string name, string ruleSet)
    {
        int index = FindDifficulty(name, ruleSet);
        if (index == -1)
            return null;

        return GetDifficultyByIndex(index);
    }

    /// <summary>
    /// Gets the first difficulty for the ruleset provided.
    /// </summary>
    /// <param name="ruleSet">A ruleset name.</param>
    /// <returns>A difficulty if found, null if not.</returns>
    public SongDifficulty GetFirstDifficultyOfRuleSet(string ruleSet)
    {
        return Difficulties.FirstOrDefault(x => x.RuleSet == ruleSet);
    }

    /// <summary>
    /// Finds the index of a difficulty with the name provided.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="ruleSet">The ruleset the difficulty is associated with.</param>
    /// <returns>Its index if found, -1 if not.</returns>
    public int FindDifficulty(string name, string ruleSet)
    {
        return Array.FindIndex(Difficulties, x => x.Name == name && x.RuleSet == ruleSet);
    }

    /// <summary>
    /// Checks if this song meta has any difficulty with the name provided.
    /// </summary>
    /// <param name="name">The name to search for</param>
    /// <param name="ruleSet">The reulset the difficulty is associated with.</param>
    /// <returns>True if found, false if not.</returns>
    public bool HasDifficulty(string name, string ruleSet)
    {
        return Difficulties.Any(x => x.Name == name && x.RuleSet == ruleSet);
    }
}