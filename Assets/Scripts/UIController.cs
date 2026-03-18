using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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

    [SerializeField]
    Sprite Play;
    [SerializeField]
    Sprite Play_Hover;
    [SerializeField]
    Sprite Play_Pressed;

    [SerializeField]
    Sprite Pause;
    [SerializeField]
    Sprite Pause_Hover;
    [SerializeField]
    Sprite Pause_Pressed;

    [SerializeField]
    Button playPauseButton;

    RectTransform rt;
    bool selected = true;

    private void Awake()
    {
        if (Instance != this) { Destroy(gameObject); }
    }

    //bool MouseHover()
    //{
    //    //Check the image location in rect transform, compared to mouse position
    //    Vector2 mouse = Mouse.current.position.ReadValue();
    //    //Vector2 rectPos = rt.anchoredPosition;
    //    Vector2 sizeHalf = rt.sizeDelta * 0.5f;

    //    bool x = mouse.x > (rt.position.x - sizeHalf.x) && mouse.x < (rt.position.x + sizeHalf.x);
    //    bool y = mouse.y > (rt.position.y - sizeHalf.y) && mouse.y < (rt.position.y + sizeHalf.y);

    //    return x && y;
    //}

    ////Not using anymore
    //void CustomButton()
    //{
    //    //If hovering over button, and pressed
    //    if (MouseHover())
    //    {
    //        if (Mouse.current.leftButton.isPressed)
    //        {
    //            image.material.mainTexture = Pressed;
    //            Debug.Log("Pressed");
    //        }
    //        else if (Mouse.current.leftButton.wasReleasedThisFrame)
    //        {
    //            selected = !selected;
    //            //PlayPauseButton(!selected);
    //        }
    //        else
    //        {
    //            image.material.mainTexture = Hover;
    //        }
    //    }
    //    else
    //    {
    //        image.material.mainTexture = selected ? Pause : Play;
    //    }
    //    image.SetMaterialDirty();
    //}

    public void PlayPauseButton()
    {
        selected = !selected;
        SpaceController.Instance.Play = !selected;

        SpriteState ss = playPauseButton.spriteState;

        playPauseButton.image.sprite = selected? Play : Pause;
        ss.highlightedSprite = selected? Play_Hover : Pause_Hover;
        ss.pressedSprite = selected? Play_Pressed : Pause_Pressed;

        playPauseButton.spriteState = ss;
        playPauseButton.image.SetMaterialDirty();
    }
}
