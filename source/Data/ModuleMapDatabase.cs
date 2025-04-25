using Godot.Collections;

namespace Rubicon.Core.Data;

[GlobalClass] public partial class ModuleMapDatabase : Resource
{
    [Export] public Dictionary<StringName, ModulePathData> Paths = [];
}