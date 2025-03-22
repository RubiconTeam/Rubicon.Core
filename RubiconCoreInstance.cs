using System.Collections.Generic;
using System.IO;
using PukiTools.GodotSharp;

namespace Rubicon.Core;

[GlobalClass, Autoload("RubiconCore")]
public partial class RubiconCoreInstance : Node
{
    [Export] public Godot.Collections.Dictionary<StringName, string> NoteTypePaths = new();

    public override void _Ready()
    {
        List<string> noteTypePaths = [];
        noteTypePaths.AddRange(PathUtility.GetAbsoluteFilePathsAt("res://resources/game/notetypes/", true));
        for (int i = 0; i < noteTypePaths.Count; i++)
        {
            string path = noteTypePaths[i];
            string noteType = Path.GetFileNameWithoutExtension(path.GetFile());
            string ext = path.GetExtension().ToLower();
            if (NoteTypePaths.ContainsKey(noteType) || noteType.ToLower() == "normal" || (ext != "tscn" && ext != "scn" && ext != "cs" && ext != "gd"))
                continue;
			
            NoteTypePaths.Add(noteType, path);
        }
    }
}