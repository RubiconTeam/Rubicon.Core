using GodotSharp.Utilities;

namespace Rubicon.Core.Audio;

[GlobalClass, Autoload("AudioManager")] public partial class AudioManagerInstance : Node
{
    [Export] public TrackList Music;

    [Export] public TrackList SoundEffects;
    
    public override void _Ready()
    {
        base._Ready();

        Music = new TrackList();
        AddChild(Music);
        Music.Setup("Music");
        
        SoundEffects = new TrackList();
        AddChild(SoundEffects);
        SoundEffects.Setup("SoundEffects");
    }

    public AudioStreamPlayer PlayMusic(AudioStream stream) => Music.Play(stream);
    
    public AudioStreamPlayer PlaySoundEffect(AudioStream stream) => SoundEffects.Play(stream);
}