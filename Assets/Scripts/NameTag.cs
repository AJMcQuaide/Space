using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;

public class NameTag : MonoBehaviour
{
    Canvas canvas;

    TextMeshProUGUI tm;

    CelestialBody cb;

    static float distModifier = 2f;
    static float fontSize = 12f;

    private void Start()
    {
        //Set the world space camera
        canvas = transform.parent.GetComponent<Canvas>();
        canvas.worldCamera = FindAnyObjectByType<Camera>();
        //Get the celestial body that this name tag is resonsible for
        cb = GetComponentInParent<CelestialBody>();
        //Get the text box
        tm = GetComponent<TextMeshProUGUI>();
        tm.text = cb.ThisCelestialBody.ToString();
        tm.fontSize = fontSize;
        //Placement
        float scaledRadius = cb.Radius * distModifier * (float)CelestialBody.SD;
        //canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(scaledRadius, scaledRadius);

        Vector3 pos = new(transform.localPosition.x, scaledRadius, transform.localPosition.z);
        transform.parent.parent.localPosition = pos;
    }
}
