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

    ////The direction of a mouse right click and drag, returns zero if not
    //Vector2 rightMouseDragDir;
    //public Vector2 RightMouseDragDir
    //{
    //    get { return rightMouseDragDir; }
    //    set
    //    { 
    //        if ( rightMouseDragDir != value )
    //        {
    //            rightMouseDragDir = value;
    //        }
    //    }
    //}

    ////The direction of a mouse left click and drag, returns zero if not
    //Vector2 leftMouseDragDir;
    //public Vector2 LeftMouseDragDir
    //{
    //    get { return leftMouseDragDir; }
    //    set
    //    {
    //        if (leftMouseDragDir != value)
    //        {
    //            leftMouseDragDir = value;
    //        }
    //    }
    //}

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

    private void Awake()
    {
        //Singleton
        if (Instance != this) { Destroy(gameObject); }
        mousePos = Mouse.current.position.ReadValue();
        prevMousePos = mousePos;
    }

    private void Update()
    {
        MouseDrag = MouseClickDrag();
    }

    /// <summary>
    /// Output the mouse delta each frame, if the left button is held down.
    /// </summary>
    /// <param name="dragSensativity"></param>
    /// <returns></returns>
    public Vector2 MouseClickDrag()
    {
        prevMousePos = mousePos;
        mousePos = Mouse.current.position.ReadValue();
        Vector2 delta = mousePos - prevMousePos;
        return delta;
    }
}
