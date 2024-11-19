namespace Rubicon.Core.Meta;

[GlobalClass]
public partial class CharacterMeta : Resource
{
    /// <summary>
    /// The character to get. Used for pathing.
    /// </summary>
    [Export] public string Character = "";

    /// <summary>
    /// The nickname this character will be given.
    /// </summary>
    [Export] public StringName Nickname = "";
    
    /// <summary>
    /// The name of the bar line (strum line) to link this character to.
    /// </summary>
    [Export] public StringName BarLine = "";

    /// <summary>
    /// The name of the spawn point to link this character to.
    /// </summary>
    [Export] public StringName SpawnPoint = "";
}