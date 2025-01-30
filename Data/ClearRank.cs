namespace Rubicon.Core.Data;

/// <summary>
/// Defines what type of clear a player can get.
/// </summary>
public enum ClearRank
{
    /// <summary>
    /// Used when the player has failed a song.
    /// </summary>
    Failure,
    
    /// <summary>
    /// Used when the player has cleared a song with broken combos.
    /// </summary>
    Clear,
    
    /// <summary>
    /// Used when the player clears a song with a full combo with a good hit or lower.
    /// </summary>
    FullCombo,
    
    /// <summary>
    /// Used when the player clears a song with a full combo with some great hits.
    /// </summary>
    GreatFullCombo,
    
    /// <summary>
    /// Used when the player clears a song with all perfects.
    /// </summary>
    Perfect
}