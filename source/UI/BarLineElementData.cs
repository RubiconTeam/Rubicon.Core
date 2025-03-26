using Rubicon.Core.Rulesets;

namespace Rubicon.Core.UI;

[GlobalClass] public partial class BarLineElementData : Resource
{
    /// <summary>
    /// The scene to instantiate and add onto the <see cref="BarLine"/>.
    /// </summary>
    [Export] public PackedScene Element;

    /// <summary>
    /// How much to offset the node from the bar line's position.
    /// </summary>
    [Export] public Vector2 Offset;
}