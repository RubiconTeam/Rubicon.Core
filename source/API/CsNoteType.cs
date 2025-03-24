using Godot.Collections;
using Rubicon.Core.Chart;
using Rubicon.Core.Rulesets;

namespace Rubicon.Core.API;

/// <summary>
/// A template for a note type in C#. Must be inherited.
/// </summary>
[GlobalClass] public abstract partial class CsNoteType : Node, IPlayElement
{
    /// <summary>
    /// Whether the autoplay should miss this note type or not.
    /// </summary>
    [Export] public bool ShouldMiss = false;

    /// <summary>
    /// Whether this note type should count towards the score.
    /// </summary>
    [Export] public bool CountsTowardScore = false;
    
    /// <inheritdoc/>
    public PlayField PlayField { get; set; }
    
    private bool _initialized = false;
    
    /// <summary>
    /// If it hasn't been initialized already, link itself to the play field.
    /// </summary>
    public void Initialize()
    {
        if (_initialized)
            return;

        PlayField.Factory.SpawnNote += SpawnNote;
        PlayField.InitializeNote += InitializeNote;
        PlayField.ModifyResult += NoteHit;

        _initialized = true;
    }

    /// <summary>
    /// Used to set up note data initially for every note type.
    /// </summary>
    /// <param name="notes">An array of notes</param>
    /// <param name="noteType">The note type</param>
    protected virtual void InitializeNote(Array<NoteData> notes, StringName noteType)
    {
        if (noteType != Name)
            return;

        for (int i = 0; i < notes.Count; i++)
        {
            NoteData note = notes[i];
            note.ShouldMiss = ShouldMiss;
            note.CountsTowardScore = CountsTowardScore;
        }
    }

    /// <summary>
    /// Triggers when the factory spawns a note of this type. Use this to set up your note.
    /// </summary>
    /// <param name="note">The note node graphic</param>
    /// <param name="noteType">The note's result</param>
    protected abstract void SpawnNote(Note note, StringName noteType);
    
    /// <summary>
    /// Triggers every time a note of this type is hit. Use this to modify the result, if need be.
    /// </summary>
    /// <param name="barLineName">The bar line's name</param>
    /// <param name="result">The result passed through</param>
    protected abstract void NoteHit(StringName barLineName, NoteResult result);
}