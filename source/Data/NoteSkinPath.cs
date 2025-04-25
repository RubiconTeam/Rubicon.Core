namespace Rubicon.Core.Data;

[GlobalClass] public partial class NoteSkinPath : Resource
{
    [Export(PropertyHint.File, "*.tres,*.res")] public string Path;
}