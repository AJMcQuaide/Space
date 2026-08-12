using System.Collections.Generic;
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

    List<TextMeshProUGUI> nameTagList = new List<TextMeshProUGUI>();

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
        PositionNameTags();
    }

    void PositionNameTags()
    {
        for (int i = 0; i < nameTagList.Count; i++)
        {
            CelestialBody cb = sc.CelestialBodiesInScene[i];

            Vector3 screenPoint = cam.WorldToScreenPoint(cb.transform.position);

            //Debug.Log("Screen Pos: " + sc.CelestialBodiesInScene[i].name + screenPoint);



            //Object is ON screen
            if (screenPoint.x > 0f && screenPoint.y > 0f && screenPoint.z > 0f)
            {
                //Show if hidden
                if (nameTagList[i].gameObject.activeSelf == false)
                {
                    Debug.LogWarning("Show");
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
                    Debug.LogWarning("Hidden");
                    nameTagList[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void Register(CelestialBody cb)
    {
        int index = sc.CelestialBodiesInScene.IndexOf(cb);
        //Debug.LogWarning("Index: " + index + " cb index of: " + sc.CelestialBodiesInScene.IndexOf(cb) + " Total CBs: " + sc.CelestialBodiesInScene.Count);
        if (index >= 0)
        {
            //Match the index of the Cb list and the nametag list
            TextMeshProUGUI text = Instantiate(textPrefab.GetComponent<TextMeshProUGUI>());
            text.transform.SetParent(transform, false);
            text.text = cb.name;
            nameTagList.Add(text);
            //Debug.LogWarning("Added to nametag list " + nameTagList[index].text + " at index " + index);
        }
    }

    public void DeRegister(CelestialBody cb)
    {
        if (sc != null)
        {
            int index = sc.CelestialBodiesInScene.IndexOf(cb);
            if (index >= 0)
            {
                nameTagList.RemoveAt(index);
            }
        }
    }
}
