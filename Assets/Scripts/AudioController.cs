using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class AudioController : MonoBehaviour
{
    static AudioController instance;
    public static AudioController Instance
    {
        get
        {
            return instance;
        }
        set
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<AudioController>();
                if (instance == null )
                {
                    Debug.LogError("Cannot find Audio Controller");
                }
            }
        }
    }

    [SerializeField]
    AudioSource backgroundMusic;

    List<AudioSource> soundEffects;

    [SerializeField]
    AudioClip[] backgroundMusicClips;

    [SerializeField]
    AudioClip escapeMenuSound;
    public AudioClip EscapeMenuSound { get { return escapeMenuSound; } }

    [SerializeField]
    AudioClip planetFocusSound;
    public AudioClip PlanetFocusSound { get { return planetFocusSound; } }

    [SerializeField]
    TextMeshProUGUI onScreenText;

    [SerializeField]
    Slider volumeSlider;

    bool backgroundMusicOn = true;

    float timer = 0;
    float trackLengthSeconds = 0;
    int currentTrack = 0;
    int totalTracks;

    private void Awake()
    {
        Instance = this;
        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //Initialize sound effects array
        soundEffects = new List<AudioSource>(1);

        //Get the number of tracks
        totalTracks = backgroundMusicClips.Length;

        //Set volume initially to half
        volumeSlider.value = 0.5f;

        //Set a random starting track
        currentTrack = Random.Range(0, totalTracks);

        //Start JukeBox coroutine
        StartCoroutine(JukeBox(backgroundMusic));
    }

    IEnumerator JukeBox(AudioSource audioSource)
    {
        while (backgroundMusicOn)
        {
            float secondTimer = 0;
            //Get the new song and play
            backgroundMusic.clip = backgroundMusicClips[currentTrack];
            trackLengthSeconds = backgroundMusic.clip.length;
            backgroundMusic.Play();

            onScreenText.text = backgroundMusic.clip.name;
            float sec = 0f;
            float min = 0f;

            //Logic for changing to the next track, as well as the on screen timer
            while (trackLengthSeconds >= timer)
            {
                secondTimer += Time.deltaTime;
                timer += Time.deltaTime;
                if (secondTimer > 1f)
                {
                    sec++;
                    if (sec >= 60f)
                    {
                        sec = 0f;
                        min++;
                    }

                    //Show the title of the track and current time on screen
                    if (sec < 10f)
                    {
                        onScreenText.text = backgroundMusic.clip.name + " " + min + ":0" + sec;
                    }
                    else
                    {
                        onScreenText.text = backgroundMusic.clip.name + " " + min + ":" + sec;
                    }
                    secondTimer = 0;
                }
                yield return null;
            }

            //Stop and get a new song
            timer = 0;
            currentTrack++;
            if (currentTrack + 1 > totalTracks)
            {
                currentTrack = 0;
            }
            yield return null;
        }
    }

    public void UpdateVolume()
    {
        backgroundMusic.volume = volumeSlider.value;
    }

    public void NextSong()
    {
        timer = trackLengthSeconds;
    }

    public IEnumerator PlaySoundEffect(AudioClip clip, float volume)
    {
        AudioSource _as = gameObject.AddComponent<AudioSource>();
        float timer = 0;
        _as.clip = clip;
        _as.volume = volume;
        _as.Play();
        yield return null;
        while (clip.length > timer)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        _as.Stop();
        Destroy(_as);
        yield return null;
    }
}
