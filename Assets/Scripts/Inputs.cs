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

    private void Awake()
    {
        //Singleton
        if (Instance != this) { Destroy(gameObject); }
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
        escapeKey.performed += UIController.Instance.OnEscapeKey;
        escapeKey.Enable();
    }

    private void OnDisable()
    {
        mouseMoveInput.Disable();
        mouseScrollInput.Disable();
        escapeKey.Disable();
        escapeKey.performed -= UIController.Instance.OnEscapeKey;
    }
}
