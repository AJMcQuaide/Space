using System.Collections;
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

    [SerializeField]
    Button addCBButton;

    /// <summary>
    /// The Celestial Body that is inserted into the scene
    /// </summary>
    CelestialBody insertedCB;

    private void Awake()
    {
        if (Instance != this) { Destroy(gameObject); }
        playPause = new();
        SetSpriteState();
        escMenu.SetActive(false);
        sc = SpaceController.Instance;
        addCBButton.onClick.AddListener(AddCelestialBody);
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

    /// <summary>
    /// Insert a new Celestial Body into the scene, but in a 'ghosted' mode to position it, without physics or contacts
    /// </summary>
    public void AddCelestialBody()
    {
        Debug.LogWarning("Add");

        //Instantiate
        insertedCB = Instantiate(sc.GetCelestialBodyPrefab(dropdownAddCB.value));
        insertedCB.GhostMode = true;
        addCBButton.GetComponentInChildren<TextMeshProUGUI>().text = "CONFIRM";

        //If the new CB is initially interfering with something, move it until it is not
        while (sc.GlobalCBInteference(insertedCB))
        {
            insertedCB.transform.position += new Vector3(0f, 0f, 1f);
        }

        //Set name and initial speed
        insertedCB.name = sc.GetCelestialBodyPrefab(dropdownAddCB.value).name;
        if (float.TryParse(addCBInputSpeed.text, out float result))
        {
            insertedCB.ResetVelocity(result);
        }
        else
        {
            Debug.LogError("Input field failed to parse");
        }

        //Swap useage of button
        addCBButton.onClick.RemoveAllListeners();
        addCBButton.onClick.AddListener(ConfirmCelestialBody);

        //Start checking for interference coroutine
        StartCoroutine(PeriodicallyCheckInterference(0.2f));
    }

    /// <summary>
    /// Finalize the insertion of a Celestial Body in ghost mode, and make it into a real object with physics and contacts
    /// </summary>
    public void ConfirmCelestialBody()
    {
        //Check if the new CB is interfering again based on user movement
        if (sc.GlobalCBInteference(insertedCB))
        {
            insertedCB.GetComponentInChildren<MeshRenderer>().material.color = sc.GhostMaterialFail;
            AudioController.PlaySound(AudioController.Instance.ErrorSound, 1f);
        }
        else
        {
            insertedCB.GetComponentInChildren<MeshRenderer>().material.color = sc.GhostMaterialPass;
            insertedCB.GhostMode = false;
            addCBButton.GetComponentInChildren<TextMeshProUGUI>().text = "ADD";
            addCBButton.onClick.RemoveAllListeners();
            addCBButton.onClick.AddListener(AddCelestialBody);
            insertedCB = null;
        }
    }

    /// <summary>
    /// Check for interference of spheres every X time period
    /// </summary>
    /// <returns></returns>
    IEnumerator PeriodicallyCheckInterference(float t)
    {
        Debug.LogWarning("Started inserted CB checking interference");
        WaitForSeconds delay = new WaitForSeconds(t);
        MeshRenderer mr = insertedCB.GetComponentInChildren<MeshRenderer>();
        bool interfering = false;
        bool check;
        while (insertedCB != null)
        {
            check = sc.GlobalCBInteference(insertedCB);
            if (interfering != check)
            {
                interfering = check;
                mr.material.color = interfering ? sc.GhostMaterialFail : sc.GhostMaterialPass;
                Debug.LogWarning("Changed ghost material color for new CB");

            }
            yield return delay;
        }
        Debug.LogWarning("End coroutine because insertedCB was null");
    }

    public void SetInitialSpeedToZero()
    {
        addCBInputSpeed.text = "0";
    }
}
