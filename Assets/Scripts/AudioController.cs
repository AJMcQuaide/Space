using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField]
    AudioSource backgroundMusic;

    [SerializeField]
    AudioClip[] backgroundMusicClips;

    [SerializeField]
    TextMeshProUGUI onScreenText;

    [SerializeField]
    Slider volumeSlider;

    bool backgroundMusicOn = true;

    float timer = 0;
    float trackLengthSeconds = 0;
    int currentTrack = 0;
    int totalTracks;

    void Start()
    {        
        totalTracks = backgroundMusicClips.Length;
        if (totalTracks > 0)
        {
            StartCoroutine(JukeBox(backgroundMusic));
        }

        //Set volume initially to half
        volumeSlider.value = 0.5f;
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
}
