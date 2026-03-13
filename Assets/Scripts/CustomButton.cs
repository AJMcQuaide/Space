using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CustomButton : MonoBehaviour
{
    [SerializeField]
    Texture Unselected; //Button is "OFF"
    [SerializeField]
    Texture Selected;   //Button is "ON"
    [SerializeField]
    Texture Hover;  //Mouse is hovering over button
    [SerializeField]
    Texture Pressed;    //"Mouse press held"

    Image image;

    RectTransform rt;

    [SerializeField]
    bool selected = false;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.material.mainTexture = Unselected;

        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        ButtonPress();
    }

    bool MouseHover()
    {
        //Check the image location in rect transform, compared to mouse position
        Vector2 mouse = Mouse.current.position.ReadValue();
        Vector2 rectPos = rt.anchoredPosition;
        Vector2 sizeHalf = rt.sizeDelta * 0.5f;

        bool x = mouse.x > (rt.position.x - sizeHalf.x) && mouse.x < (rt.position.x + sizeHalf.x);
        bool y = mouse.y > (rt.position.y - sizeHalf.y) && mouse.y < (rt.position.y + sizeHalf.y);

        if (x && y)
        {
            Debug.Log("Hovering over button " + gameObject.name);
        }

        return x && y;
    }

    void ButtonPress()
    {
        //If hovering over button, and pressed
        if (MouseHover())
        {
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                selected = !selected;
            }
            else
            {
                image.material.mainTexture = Hover;
            }
        }
        else
        {
            image.material.mainTexture = selected ? Selected : Unselected;
        }
        image.SetMaterialDirty();
    }
}
