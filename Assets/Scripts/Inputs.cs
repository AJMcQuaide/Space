using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Read for input actions
/// </summary>
public class Inputs : MonoBehaviour
{
    static Inputs instance;
    public static Inputs Instance
    {
        get
        {
            if (instance == null) { instance = FindAnyObjectByType<Inputs>(); }
            return instance;
        }
    }

    [SerializeField]
    PlayerInput playerInput;

    /// <summary>
    /// Mouse drag / movement delta
    /// </summary>
    InputAction mouseMoveInput;
    public InputAction MouseMoveInput { get { return mouseMoveInput; } }

    /// <summary>
    /// Mouse wheel scroll delta
    /// </summary>
    InputAction mouseScrollInput;
    public InputAction MouseScrollInput { get { return mouseScrollInput; } }

    /// <summary>
    /// Mouse wheel scroll delta
    /// </summary>
    InputAction escapeKey;
    public InputAction EscapeKey { get { return escapeKey; } }

    //Refereneces
    UIController ui;
    AudioController ac;
    SpaceController sc;

    private void Awake()
    {
        //Singleton
        if (Instance != this) { Destroy(gameObject); }
    }

    private void Start()
    {
        ui = UIController.Instance;
        ac = AudioController.Instance;
        sc = SpaceController.Instance;
    }

    public void OnEscapeKey(InputAction.CallbackContext context)
    {
        ui.EscMenu.SetActive(!ui.EscMenu.activeSelf);
        if (sc.InPlayMode)
        {
            ui.PlayPauseButton();
        }
        if (ui.EscMenu.activeSelf == true)
        {
            StartCoroutine(ac.PlaySoundEffect(ac.EscapeMenuSound, 0.5f));
        }
    }

    private void OnEnable()
    {
        //Find input actions
        mouseMoveInput = playerInput.actions.FindAction("MouseDelta");
        mouseScrollInput = playerInput.actions.FindAction("MouseWheelDelta");
        escapeKey = playerInput.actions.FindAction("Escape");

        //Enable input action callbacks
        mouseMoveInput.Enable();
        mouseScrollInput.Enable();
        escapeKey.performed += OnEscapeKey;
        escapeKey.Enable();
    }

    private void OnDisable()
    {
        mouseMoveInput.Disable();
        mouseScrollInput.Disable();
        escapeKey.Disable();
        escapeKey.performed -= OnEscapeKey;
    }
}
