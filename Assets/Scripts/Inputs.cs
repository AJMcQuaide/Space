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

    Vector2 prevMousePos;
    Vector2 mousePos;

    //The direction of a mouse middle click and drag, returns zero if not
    Vector2 mouseDrag;
    public Vector2 MouseDrag
    {
        get { return mouseDrag; }
        set
        {
            if (mouseDrag != value)
            {
                mouseDrag = value; 
            }
        }
    }

    [SerializeField]
    PlayerInput playerInput;
    InputAction mouseMoveInput;

    private void Awake()
    {
        //Singleton
        if (Instance != this) { Destroy(gameObject); }

        mouseMoveInput = playerInput.actions.FindAction("MouseDelta");

        //mousePos = Mouse.current.position.ReadValue();
        //prevMousePos = mousePos;
    }

    private void Update()
    {
        //Debug.LogWarning("Mouse Drag: " + mouseDrag);
        //mouseDrag = MouseDelta();
    }

    /// <summary>
    /// Output the mouse delta each frame, if the left button is held down.
    /// </summary>
    /// <param name="dragSensativity"></param>
    /// <returns></returns>
    private void OnMouseDelta(InputAction.CallbackContext context)
    {
        //Previous code
        //prevMousePos = mousePos;
        //mousePos = Mouse.current.position.ReadValue(); //There is also Mouse.current.delta.ReadValue();
        //Vector2 delta = mousePos - prevMousePos;
        //return delta;

        //New code
        mouseDrag = context.ReadValue<Vector2>();
    }
    private void OnEnable()
    {
        mouseMoveInput.Enable();
        mouseMoveInput.performed += OnMouseDelta;
    }

    private void OnDisable()
    {
        mouseMoveInput.performed -= OnMouseDelta;
        mouseMoveInput.Disable();
    }
}
