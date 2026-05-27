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

    //The direction of a mouse right click and drag, returns zero if not
    Vector2 rightMouseDragDir;
    public Vector2 RightMouseDragDir
    {
        get { return rightMouseDragDir; }
        set { 
            if ( rightMouseDragDir != value )
            {
                rightMouseDragDir = value; } }
    }

    //The direction of a mouse left click and drag, returns zero if not
    Vector2 leftMouseDragDir;
    public Vector2 LeftMouseDragDir
    {
        get { return leftMouseDragDir; }
        set {
            if (leftMouseDragDir != value)
            {
                leftMouseDragDir = value; } }
    }

    //The direction of a mouse middle click and drag, returns zero if not
    Vector2 middleMouseDragDir;
    public Vector2 MiddleMouseDragDir
    {
        get { return middleMouseDragDir; }
        set {
            if (middleMouseDragDir != value)
            {
                middleMouseDragDir = value; } }
    }

    private void Awake()
    {
        //Singleton
        if (Instance != this) { Destroy(gameObject); }
        mousePos = Mouse.current.position.ReadValue();
        prevMousePos = mousePos;
    }

    private void Update()
    {
        RightMouseDragDir = MouseClickDrag(Mouse.current.rightButton.isPressed);
        LeftMouseDragDir = MouseClickDrag(Mouse.current.leftButton.isPressed);
        MiddleMouseDragDir = MouseClickDrag(Mouse.current.middleButton.isPressed);
    }

    /// <summary>
    /// Output the mouse delta each frame, if the left button is held down.
    /// </summary>
    /// <param name="dragSensativity"></param>
    /// <returns></returns>
    public Vector2 MouseClickDrag(bool mousePress)
    {
        if (mousePress)
        {
            prevMousePos = mousePos;
            mousePos = Mouse.current.position.ReadValue();
            Vector2 normalizedDelta = (mousePos - prevMousePos).normalized;
            return normalizedDelta;
        }
        else
        {
            return Vector2.zero;
        }
    }
}
