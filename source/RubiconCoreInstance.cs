using System.Collections.Generic;
using System.IO;
using PukiTools.GodotSharp;
using Rubicon.Core.Data;

namespace Rubicon.Core;

[GlobalClass, Autoload("RubiconCore")]
public partial class RubiconCoreInstance : Node
{
    [Export] public NoteSkinDatabase NoteSkins;

    [Export] public ModuleMapDatabase NoteTypes;

    public override void _Ready()
    {
        NoteSkins = ResourceLoader.Load<NoteSkinDatabase>(ProjectSettings.GetSetting("rubicon_core/paths/note_skin_database").AsString());
        NoteTypes = ResourceLoader.Load<ModuleMapDatabase>(ProjectSettings.GetSetting("rubicon_core/paths/note_type_database").AsString());
    }
}