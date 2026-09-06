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

    private void Awake()
    {
        //Singleton
        if (Instance != this) { Destroy(gameObject); }
    }

    private void OnEnable()
    {
        mouseMoveInput = playerInput.actions.FindAction("MouseDelta");
        mouseScrollInput = playerInput.actions.FindAction("MouseWheelDelta");
        if (mouseMoveInput == null || mouseScrollInput == null)
        {
            Debug.LogError("Cannot find Input Action(s)");
        }
        mouseMoveInput.Enable();
        mouseScrollInput.Enable();
    }

    private void OnDisable()
    {
        mouseMoveInput.Disable();
        mouseScrollInput.Disable();
    }
}
