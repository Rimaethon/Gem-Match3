using System.Collections.Generic;
using System.Linq;
using Rimaethon.Scripts.Managers;
using Rimaethon.Scripts.Utility;
using UnityEngine;

//Another Singleton? I think we have enough of those.  
public class AudioManager : PersistentSingleton<AudioManager>
{
    [SerializeField] private AudioLibrary audioLibrary;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private GameObject audioSourcePrefab;
    private List<AudioSource> _sfxSources; 
    private AudioClip[] _musicClips;
    private AudioClip[] _sfxClips;


    private void OnEnable()
    {
        EventManager.Instance.AddHandler(GameEvents.OnMusicToggle, HandleMusicToggle);
    }

    private void OnDisable()
    {
        if(EventManager.Instance == null) return;
        EventManager.Instance.RemoveHandler(GameEvents.OnMusicToggle, HandleMusicToggle);
    }


    protected override void Awake()
    {
        base.Awake();
        _sfxSources = new List<AudioSource>();
        _musicClips = audioLibrary.MusicClips;
        _sfxClips = audioLibrary.SFXClips;
        PlayMusic(MusicClips.BackgroundMusic);
    }

    private void HandleMusicToggle()
    {
        if (SaveManager.Instance.IsMusicOn())
        {
            PlayMusic(MusicClips.BackgroundMusic);
        }
        else
        {
            musicSource.Stop();
        }
    }
    private void PlayMusic(MusicClips clipEnum)
    {
        if (!SaveManager.Instance.IsMusicOn()) return;
        if (musicSource.isPlaying) musicSource.Stop();
        musicSource.clip = _musicClips[(int)clipEnum];
        musicSource.Play();
    }

    public AudioSource PlaySFX(SFXClips clipEnum, bool isLooping = false)
    {
        if (!SaveManager.Instance.IsSfxOn()) return null;
        
        AudioSource availableSource = _sfxSources.FirstOrDefault(source => !source.isPlaying);

        // If there is no available AudioSource, create a new one
        if (availableSource == null)
        {
            GameObject newAudioSourceObject = Instantiate(audioSourcePrefab, transform);
            availableSource = newAudioSourceObject.GetComponent<AudioSource>();
            _sfxSources.Add(availableSource);
        }
        availableSource.clip = _sfxClips[(int)clipEnum];
        availableSource.loop = isLooping;
        if(isLooping)
        {
            availableSource.Play();
        }
        else
        {
            availableSource.PlayOneShot(availableSource.clip);
        }
        return availableSource;
    }

}