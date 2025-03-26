using System.Linq;

namespace Rubicon.Core.API;

public static class ApiUtility
{
    public static bool InheritsFrom(this Node node, string globalName)
    {
        Script curScript = node.GetScript().As<Script>();
        while (curScript != null)
        {
            if (curScript.GetGlobalName() == globalName)
                return true;
                
            curScript = curScript.GetBaseScript();
        }

        return false;
    }

    public static bool InheritsFrom(this Node node, string[] globalNames)
    {
        Script curScript = node.GetScript().As<Script>();
        while (curScript != null)
        {
            if (globalNames.Any(x => x == curScript.GetGlobalName()))
                return true;
                
            curScript = curScript.GetBaseScript();
        }

        return false;
    }
}