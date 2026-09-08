using System.Collections;
using System.Collections.Generic;
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

    bool backgroundMusicOn = true;

    float timer = 0;
    int currentTrack = 0;
    int totalTracks;

    // Start is called before the first frame update
    void Start()
    {        
        totalTracks = backgroundMusicClips.Length;
        if (totalTracks > 0)
        {
            StartCoroutine(JukeBox(backgroundMusic));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator JukeBox(AudioSource audioSource)
    {
        while (backgroundMusicOn)
        {
            //Get the new song and play
            backgroundMusic.clip = backgroundMusicClips[currentTrack];
            float currentTrackLength = 5;
            backgroundMusic.Play();

            onScreenText.text = backgroundMusic.clip.name + " " + currentTrackLength / 60f;
            //Debug.LogWarning("Track: " + backgroundMusic.clip.name + " started. Length: " + currentTrackLength / 60f + " Min");

            //Wait for the song to end
            while (currentTrackLength > timer)
            {
                Debug.Log("Time: " + timer);
                timer += Time.deltaTime;
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
}
