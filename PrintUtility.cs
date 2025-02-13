namespace Rubicon.Core;

/// <summary>
/// Print utilities.
/// Lets you add an identifier to each print and determine
/// if its a verbose print or not.
/// </summary>
#if TOOLS
[Tool]
#endif
public static class PrintUtility
{
    private static readonly bool Verbose = ProjectSettings.GetSetting("rubicon/general/verbose").AsBool();

    public static void Print(string message, bool onlyIfVerbose = false)
    {   
        if(!(Verbose == onlyIfVerbose || !onlyIfVerbose))
            return;
        
        GD.Print(message);
    }
    
    public static void Print(string identifier, string message, bool onlyIfVerbose = false)
    {
        if(!(Verbose == onlyIfVerbose || !onlyIfVerbose))
            return;
        
        GD.Print($"[{identifier}] {message}");
    }
    
    public static void PrintError(string message, bool onlyIfVerbose = false)
    {   
        if(!(Verbose == onlyIfVerbose || !onlyIfVerbose))
            return;
        
        GD.PrintErr(message);
    }
    
    public static void PrintError(string identifier, string message, bool onlyIfVerbose = false)
    {
        if(!(Verbose == onlyIfVerbose || !onlyIfVerbose))
            return;
        
        GD.PrintErr($"[{identifier}] {message}");
    }
}