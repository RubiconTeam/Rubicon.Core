using Godot.Collections;

namespace Rubicon.Core.Data;

[GlobalClass] public partial class NoteSkinDatabase : Resource
{
    [Export] public Dictionary<StringName, NoteSkinMap> Skins = [];
}