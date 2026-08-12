using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    static UIController instance;
    public static UIController Instance
    {
        get
        {
            if (instance == null) { instance = FindAnyObjectByType<UIController>(); }
            return instance;
        }
    }

    [SerializeField]
    Sprite Play;

    [SerializeField]
    Sprite play_Hover;
    public Sprite Play_Hover { get { return play_Hover; } }

    [SerializeField]
    Sprite Play_Pressed;

    [SerializeField]
    Sprite Pause;

    [SerializeField]
    Sprite pause_Hover;

    public Sprite Pause_Hover { get { return pause_Hover; } }

    [SerializeField]
    Sprite Pause_Pressed;

    [SerializeField]
    Button playPauseButton;

    RectTransform rt;
    bool playPauseSelected = true;
    public bool PlayPauseSelected { get { return playPauseButton; } }

    SpriteState playPause;

    private void Awake()
    {
        if (Instance != this) { Destroy(gameObject); }
        playPause = new();

        SetSpriteState();
    }

    public void PlayPauseButton()
    {
        playPauseSelected = !playPauseSelected;
        SpaceController.Instance.InPlayMode = !playPauseSelected;

        SetSpriteState();
    }

    /// <summary>
    /// Speed up the time multiplier of the simulation
    /// </summary>
    public void FastForward()
    {
        SpaceController.Instance.TimeScale *= 2f;
    }

    /// <summary>
    /// Slow down the time multiplier of the simulation
    /// </summary>
    public void SlowForward()
    {
        SpaceController.Instance.TimeScale *= 0.5f;
    }

    //Set state of the game mode (Play/Pause) control buttons in the UI
    public void SetSpriteState()
    {
        //Standard sprite
        playPauseButton.image.sprite = playPauseSelected ? Play : Pause;
        //Highlighted/Hover sprite
        playPause.highlightedSprite = playPauseSelected ? Play_Hover : Pause_Hover;
        //Selected/Pressed sprite
        playPause.pressedSprite = playPauseSelected ? Play_Pressed : Pause_Pressed;

        //Set the button sprite state
        playPauseButton.spriteState = playPause;
    }
}
