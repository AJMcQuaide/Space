using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    ////Check for a mouse hover using polling
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

    ////Check for click on a button and set to selected or unselected
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
}
