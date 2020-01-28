using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    public AudioClip[] musicClips;
    public AudioClip[] winClips;
    public AudioClip[] loseClips;
    public AudioClip[] bonusClips;

    [Range(0,1)]
    public float musicVolume = 0.5f;

    [Range(0,1)]
    public float fxVolume = 1f;

    public float lowPitch = 0.95f;
    public float highPitch = 1.05f;

    // Start is called before the first frame update
    void Start()
    {
        PlayMusic();
    }

    public AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1f, bool playAtRandomPitch = false)
    {
        if (clip != null)
        {
            GameObject gameObject = new GameObject("SoundFX" + clip.name);
            gameObject.transform.position = position;

            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;

            if (playAtRandomPitch)
            {
                float randomPitch = Random.Range(lowPitch, highPitch);
                audioSource.pitch = randomPitch;
            }
            audioSource.volume = volume;
            audioSource.Play();

            Destroy(gameObject, clip.length);

            return audioSource;
        }

        return null;
    }

    public AudioSource PlayRandom(AudioClip[] clips, Vector3 position, float volume = 1f, bool playAtRandomPitch = false)
    {
        if (clips != null)
        {
            if (clips.Length > 0)
            {
                int randomIndex = Random.Range(0, clips.Length);

                if (clips[randomIndex] != null)
                {
                    AudioSource audioSource = PlayClipAtPoint(clips[randomIndex], position, volume, playAtRandomPitch);
                    return audioSource;
                }
            }
        }

        return null;
    }

    public void PlayMusic(float volumeMultiplier = 1f)
    {
        PlayRandom(musicClips, Vector3.zero, musicVolume * volumeMultiplier);
    }

    public void PlayWinSound(float volumeMultiplier = 1f)
    {
        PlayRandom(winClips, Vector3.zero, fxVolume * volumeMultiplier);
    }

    public void PlayLoseSound(float volumeMultiplier = 1f)
    {
        PlayRandom(loseClips, Vector3.zero, fxVolume * volumeMultiplier);
    }

    public void PlayBonusSound(float volumeMultiplier = 1f)
    {
        PlayRandom(bonusClips, Vector3.zero, fxVolume * volumeMultiplier);
    }
}
