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

    private void Awake()
    {
        //Singleton
        if (Instance != this) { Destroy(gameObject); }
        mousePos = Mouse.current.position.ReadValue();
        prevMousePos = mousePos;
    }

    private void Update()
    {
        mouseDrag = MouseClickDrag();
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
        Debug.LogWarning("Mouse Drag: " + delta * Time.deltaTime);
        return delta;
    }
}
