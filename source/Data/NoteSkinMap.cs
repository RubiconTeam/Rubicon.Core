using Godot.Collections;

namespace Rubicon.Core.Data;

[GlobalClass] public partial class NoteSkinMap : Resource
{
    [Export] public Dictionary<StringName, NoteSkinPath> Rulesets = [];
}