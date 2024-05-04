using UnityEngine;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Audio/Audio Library", order = 1)]
public class AudioLibrary : ScriptableObject
{
    public AudioClip[] SFXClips;
    public AudioClip[] MusicClips;
}