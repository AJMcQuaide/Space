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
    InputAction mouseMoveInput;
    public InputAction MouseMoveInput { get { return mouseMoveInput; } }

    private void Awake()
    {
        //Singleton
        if (Instance != this) { Destroy(gameObject); }
    }

    private void Start()
    {
        mouseMoveInput = playerInput.actions.FindAction("MouseDelta");
        if (mouseMoveInput == null)
        {
            Debug.LogError("Cannot find Input Action");
        }
    }

    private void OnEnable()
    {
        mouseMoveInput.Enable();
    }

    private void OnDisable()
    {
        mouseMoveInput.Disable();
    }
}
