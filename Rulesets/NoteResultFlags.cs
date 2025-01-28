namespace Rubicon.Core.Rulesets;

/// <summary>
/// Flags for NoteResult. Will prevent the action from being activated.
/// </summary>
[Flags] public enum NoteResultFlags : uint
{
    /// <summary>
    /// Will let every action be triggered.
    /// </summary>
    None = 0b00000000,
    
    /// <summary>
    /// Will prevent health from being updated if raised.
    /// </summary>
    Health = 0b00000001,
    
    /// <summary>
    /// Will prevent the score from being updated if raised.
    /// </summary>
    Score = 0b00000010,
    
    /// <summary>
    /// Will prevent the splash animation from being played if raised.
    /// </summary>
    Splash = 0b00000100,
    
    /// <summary>
    /// Will prevent the sing animation from being played if raised.
    /// </summary>
    Animation = 0b00001000
}