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

    private void Awake()
    {
        //Singleton
        if (Instance != this) { Destroy(gameObject); }
        mousePos = Mouse.current.position.ReadValue();
        prevMousePos = mousePos;
    }

    private void Update()
    {
        //MousePosition();
    }

    /// <summary>
    /// Output the mouse delta each frame, if the left button is held down.
    /// </summary>
    /// <param name="dragSensativity"></param>
    /// <returns></returns>
    public Vector2 MouseClickDrag()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            prevMousePos = mousePos;
            mousePos = Mouse.current.position.ReadValue();
            Vector2 normalizedDelta = (prevMousePos - mousePos).normalized;
            return normalizedDelta;
        }
        else
        {
            return Vector2.zero;
        }
    }
    
    //public Vector2 MousePosition()
    //{
    //    //Mouse position on screen
    //    Vector2 ss = UIController.Instance.ScreenSize;
    //    prevMousePos = MousePos;
    //    MousePos = Mouse.current.position.ReadValue();
    //    if (ss != null)
    //    {
    //        MousePos -= ss * 0.5f;
    //    }
    //}
}
