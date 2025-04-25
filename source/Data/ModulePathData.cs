namespace Rubicon.Core.Data;

[GlobalClass] public partial class ModulePathData : Resource
{
    [Export(PropertyHint.File, "*.tscn,*.scn")] public string Path;

    public Node LoadAndInstantiate()
    {
        PackedScene scene = ResourceLoader.Load<PackedScene>(Path);
        if (!scene.CanInstantiate())
            return null;

        return scene.Instantiate();
    }

    public T LoadAndInstantiate<T>() where T : Node
    {
        Node scene = LoadAndInstantiate();
        if (scene is not T typedScene)
            return null;
        
        return typedScene;
    }
}