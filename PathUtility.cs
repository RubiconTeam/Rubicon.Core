using System.Collections.Generic;

namespace Rubicon.Core;

public static class PathUtility
{
    public static string[] GetAbsoluteFilePathsAt(string path, bool recursive = false)
    {
        if (!DirAccess.DirExistsAbsolute(path))
            return [];
        
        List<string> pathList = [];
        if (recursive)
        {
            string[] directories = DirAccess.GetDirectoriesAt(path);
            for (int i = 0; i < directories.Length; i++)
                pathList.AddRange(GetAbsoluteFilePathsAt(directories[i], true));
        }

        string[] files = DirAccess.GetFilesAt(path);
        for (int i = 0; i < files.Length; i++)
            pathList.Add(path + files[i]);
        return pathList.ToArray();
    }
    
    public static string GetResourcePath(string path) => GetPathWithExt(path, "res");
    
    public static string GetScenePath(string path) => GetPathWithExt(path, "scn");
    
    public static string GetAudioPath(string path)
    {
        bool oggExists = ResourceLoader.Exists(path + ".ogg");
        bool wavExists = ResourceLoader.Exists(path + ".wav");
        bool mp3Exists = ResourceLoader.Exists(path + ".mp3");

        if (!oggExists && !wavExists && !mp3Exists)
            return null;
        
        if (oggExists)
            return path + ".ogg";
        
        if (wavExists)
            return path + ".wav";
        
        return path + ".mp3";
    }

    public static bool ResourceExists(string path) => GetResourcePath(path) != null;
    
    public static bool SceneExists(string path) => GetScenePath(path) != null;
    
    public static bool AudioExists(string path) => GetAudioPath(path) != null;
    
    private static string GetPathWithExt(string path, string extension)
    {
        bool tExtExists = ResourceLoader.Exists(path + ".t" + extension);
        bool extExists = ResourceLoader.Exists(path + "." + extension);
        if (!tExtExists && !extExists)
            return null;
				
        if (tExtExists)
            return path + ".t" + extension;
        
        return path + "." + extension;
    }
}