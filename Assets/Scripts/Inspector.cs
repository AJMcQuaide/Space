using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Inspector : MonoBehaviour
{
    SpaceController sc;
    UIController ui;
    /// <summary>
    /// The name of the celestial body that the user has selected, as shown in the inspector
    /// </summary>
    [SerializeField]
    TMP_InputField celestialBodyName;

    //bool updateInspector = true;

    [SerializeField]
    TMP_InputField xPos;

    [SerializeField]
    TMP_InputField yPos;

    [SerializeField]
    TMP_InputField zPos;

    private void Awake()
    {
        sc = SpaceController.Instance;
        ui = UIController.Instance;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Update the inspector in the UI periodically
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpdateInspector()
    {
        WaitForSeconds delay = new WaitForSeconds(0.1f);
        CelestialBody cc = sc.Cc.CameraTrackedObject;
        while (cc != null)
        {
            //Update position in inspector
            xPos.text = cc.transform.position.x.ToString();
            yPos.text = cc.transform.position.y.ToString();
            zPos.text = cc.transform.position.z.ToString();
            yield return delay;
        }
    }

    /// <summary>
    /// Update the inspector in the UI by sending information to it
    /// </summary>
    public void PushInspectorValues()
    {
        if (sc.Cc.CameraTrackedObject != null)
        {
            Debug.Log("Push inspector data");
            sc.Cc.CameraTrackedObject.name = celestialBodyName.text;
        }
    }

    /// <summary>
    /// Update the inspector in the UI by pulling information to it
    /// </summary>
    public void PullInspectorValues()
    {
        if (sc.Cc.CameraTrackedObject != null)
        {
            Debug.Log("Pull inspector data");
            celestialBodyName.text = sc.Cc.CameraTrackedObject.name;
        }
    }
}
