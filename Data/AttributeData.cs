using Array = Godot.Collections.Array;

namespace Rubicon.Core.Data;

[GlobalClass] public partial class AttributeData : RefCounted
{
    [Export] public string Name;

    [Export] public Array Parameters = [];

    public AttributeData(string name, params Variant[] parameters)
    {
        Name = name;
        for (int i = 0; i < parameters.Length; i++)
            Parameters.Add(parameters[i]);
    }
}