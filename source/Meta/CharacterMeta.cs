namespace Rubicon.Core.Meta;

[GlobalClass]
public partial class CharacterMeta : Resource
{
    /// <summary>
    /// The path to the character to instantiate.
    /// </summary>
    [Export(PropertyHint.File, "*.tscn,*.scn")] public string Character = "";

    /// <summary>
    /// The nickname this character will be given. This also determines where it will spawn on-stage.
    /// </summary>
    [Export] public StringName Nickname = "";
    
    /// <summary>
    /// The name of the bar line (strum line) to link this character to.
    /// </summary>
    [Export] public StringName BarLine = "";
}