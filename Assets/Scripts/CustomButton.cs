using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CustomButton : MonoBehaviour
{
    Texture Button;     //Button is "OFF"
    Texture Hover;      //Mouse is hovering over button
    Texture Pressed;    //"Mouse press held"

    [SerializeField]
    Texture Button_One;
    [SerializeField]
    Texture Hover_One;
    [SerializeField]
    Texture Pressed_One;
    [SerializeField]
    Texture Button_Two;
    [SerializeField]
    Texture Hover_Two;
    [SerializeField]
    Texture Pressed_Two;

    Image image;
    RectTransform rt;
    bool selected = false;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.material.mainTexture = Button;

        rt = GetComponent<RectTransform>();

        SwitchButton(true);
    }

    void Update()
    {
        ButtonPress();
    }

    bool MouseHover()
    {
        //Check the image location in rect transform, compared to mouse position
        Vector2 mouse = Mouse.current.position.ReadValue();
        //Vector2 rectPos = rt.anchoredPosition;
        Vector2 sizeHalf = rt.sizeDelta * 0.5f;

        bool x = mouse.x > (rt.position.x - sizeHalf.x) && mouse.x < (rt.position.x + sizeHalf.x);
        bool y = mouse.y > (rt.position.y - sizeHalf.y) && mouse.y < (rt.position.y + sizeHalf.y);

        return x && y;
    }

    void ButtonPress()
    {
        //If hovering over button, and pressed
        if (MouseHover())
        {
            if (Mouse.current.leftButton.isPressed)
            {
                image.material.mainTexture = Pressed;
                Debug.Log("Pressed");
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                selected = !selected;
                SwitchButton(!selected);
            }
            else
            {
                image.material.mainTexture = Hover;
            }
        }
        else
        {
            image.material.mainTexture = selected ? Button_Two : Button_One;
        }
        image.SetMaterialDirty();
    }

    void SwitchButton(bool UseButtonOne)
    {
        Button = UseButtonOne ? Button_One : Button_Two;
        Hover = UseButtonOne ? Hover_One : Hover_Two;
        Pressed = UseButtonOne ? Pressed_One : Pressed_Two;
    }
}
