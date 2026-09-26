using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;

public class NameTagController : MonoBehaviour
{
    static NameTagController instance;
    public static NameTagController Instance
    {
        get
        {
            if (instance == null) { instance = FindAnyObjectByType<NameTagController>(); }
            return instance;
        }
    }

    Camera cam;

    SpaceController sc;

    [SerializeField]
    GameObject textPrefab;

    List<TextMeshProUGUI> nameTagList = new();

    [SerializeField]
    float multiplier = 70f;
    [SerializeField]
    float multiplier2 = 2300f;

    private void Awake()
    {
        //Singleton
        if (Instance != this) { Destroy(gameObject); }
        else
        {
            cam = FindAnyObjectByType<Camera>();
            sc = SpaceController.Instance;
        }
    }

    void LateUpdate()
    {
        //In Late Update because the name tags depend on the camera, which is in Update
        PositionNameTags();
    }

    /// <summary>
    /// Position the nametags with respect to the celestial bodies
    /// </summary>
    void PositionNameTags()
    {
        for (int i = 0; i < nameTagList.Count; i++)
        {
            CelestialBody cb = sc.CelestialBodiesInScene[i];

            Vector3 screenPoint = cam.WorldToScreenPoint(cb.transform.position);

            //Object is ON screen
            if (screenPoint.x > 0f && screenPoint.y > 0f && screenPoint.z > 0f)
            {
                //Show if hidden
                if (nameTagList[i].gameObject.activeSelf == false)
                {
                    nameTagList[i].gameObject.SetActive(true);
                }

                float radius = (cb.Radius * (float)CelestialBody.SD * multiplier2) / screenPoint.z;
                //Debug.LogWarning("nameTagPos: " + radius + " Rad: " + cb.Radius * (float)CelestialBody.SD + " Distance: " + screenPoint.z);
                screenPoint.z = 0;
                screenPoint.x += radius + multiplier;
                screenPoint.y += radius + multiplier;
                //Debug.LogWarning("Radius: " + radius);
                nameTagList[i].rectTransform.position = screenPoint;
            }
            else
            {
                //Hide if shown
                if (nameTagList[i].gameObject.activeSelf == true)
                {
                    nameTagList[i].gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Add the name tag
    /// </summary>
    /// <param name="cb"></param>
    public void Register(CelestialBody cb)
    {
        int index = sc.CelestialBodiesInScene.IndexOf(cb);
        if (index >= 0)
        {
            //Match the index of the Cb list and the nametag list
            TextMeshProUGUI text = Instantiate(textPrefab.GetComponent<TextMeshProUGUI>());
            text.transform.SetParent(transform, false);
            text.alignment = TextAlignmentOptions.Left;
            text.text = cb.name + "<br>" + cb.Speed + "m/s";
            nameTagList.Add(text);
            StartCoroutine(UpdateSpeedText(cb, text));
        }
    }

    /// <summary>
    /// Remove the name tag
    /// </summary>
    /// <param name="cb"></param>
    public void DeRegister(CelestialBody cb)
    {
        if (sc != null)
        {
            int index = sc.CelestialBodiesInScene.IndexOf(cb);
            if (index >= 0)
            {
                Destroy(nameTagList[index].gameObject);
                nameTagList.RemoveAt(index);
            }
        }
    }

    IEnumerator UpdateSpeedText(CelestialBody cb, TextMeshProUGUI text)
    {
        WaitForSeconds delay = new(1);
        while (cb != null)
        {
            text.text = text.text = cb.name + "<br>" + (int)cb.Speed + "m/s";
            yield return delay;
        }
    }
}
