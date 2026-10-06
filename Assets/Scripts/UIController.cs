using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

    SpaceController sc;

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

    //RectTransform rt;
    bool playPauseSelected = true;
    public bool PlayPauseSelected { get { return playPauseButton; } }

    SpriteState playPause;

    [SerializeField]
    GameObject escMenu;
    public GameObject EscMenu { get { return escMenu; } set { escMenu = value; } }

    [SerializeField]
    TMP_Dropdown dropdownAddCB;

    [SerializeField]
    TMP_InputField addCBInputSpeed;

    private void Awake()
    {
        if (Instance != this) { Destroy(gameObject); }
        playPause = new();
        SetSpriteState();
        escMenu.SetActive(false);
        sc = SpaceController.Instance;
    }

    public void PlayPauseButton()
    {
        playPauseSelected = !playPauseSelected;
        sc.InPlayMode = !playPauseSelected;

        //Set the input field to only accept numbers
        addCBInputSpeed.contentType = TMP_InputField.ContentType.DecimalNumber;

        SetSpriteState();
    }

    /// <summary>
    /// Speed up the time multiplier of the simulation
    /// </summary>
    public void FastForward()
    {
        sc.TimeScale *= 2f;
    }

    /// <summary>
    /// Slow down the time multiplier of the simulation
    /// </summary>
    public void SlowForward()
    {
        sc.TimeScale *= 0.5f;
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

    public void QuitApplication()
    {
        Application.Quit();
    }

    public void AddCelestialBody()
    {
        CelestialBody newCB = Instantiate(sc.GetCelestialBodyPrefab(dropdownAddCB.value));
        newCB.name = sc.GetCelestialBodyPrefab(dropdownAddCB.value).name;
        if (float.TryParse(addCBInputSpeed.text, out float result))
        {
            newCB.ResetVelocity(result);
        }
        else
        {
            Debug.LogError("Input field failed to parse");
        }
    }

    public void OnValueChangedDropdown()
    {
        addCBInputSpeed = null;
    }
}
