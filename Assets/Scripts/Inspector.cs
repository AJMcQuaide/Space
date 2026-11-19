using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

public class Inspector : MonoBehaviour
{
    SpaceController sc;

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

    [SerializeField]
    TMP_InputField xRot;

    [SerializeField]
    TMP_InputField yRot;

    [SerializeField]
    TMP_InputField zRot;

    [SerializeField]
    TMP_InputField radius;

    bool inspectorInputFieldSelected = false;

    private void Awake()
    {
        sc = SpaceController.Instance;
    }

    /// <summary>
    /// Update the inspector in the UI periodically
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpdateInspector()
    {
        Debug.LogWarning("Start Inspector pull coroutine");
        WaitForSeconds delay = new WaitForSeconds(0.2f);
        CelestialBody cb = sc.Cc.CameraTrackedObject;

        while (inspectorInputFieldSelected == false && cb != null)
        {
            PullInspectorValues();
            yield return delay;
        }
        Debug.LogWarning("End Inspector pull coroutine");
    }

    /// <summary>
    /// Update the inspector in the UI by sending information to it, linked to the inspector 'Update' button
    /// </summary>
    public void PushInspectorValues()
    {
        CelestialBody cb = sc.Cc.CameraTrackedObject;
        //Only modify the object if the game is paused, and if there is a Celestial Body to modify
        if (cb != null && sc.InPlayMode == false)
        {
            Debug.Log("Push inspector data");
            sc.Cc.CameraTrackedObject.name = celestialBodyName.text;

            //Set position from input text
            Vector3 pos = new Vector3(SpaceController.StringToFloat(xPos.text), SpaceController.StringToFloat(yPos.text), SpaceController.StringToFloat(zPos.text));
            cb.transform.position = pos;

            //Set rotation
            Vector3 eulerAnglesInput = new Vector3(SpaceController.StringToFloat(xRot.text), SpaceController.StringToFloat(yRot.text), SpaceController.StringToFloat(zRot.text));
            Quaternion rot = Quaternion.LookRotation(eulerAnglesInput);

            //Set the manipulation tool to the input rotation
            sc.ObjectManipulation.RotationTool.transform.rotation = rot;

            //Now that the rotation tool is pointed in the right direction, set the Celestial Body to match it
            cb.SetVelocityToManipulationForward(cb.Speed);

            //Consider the field deselected
            OnInputFieldDeSelected();
        }
        else
        {
            //The data was not pushed
            AudioController.PlaySound(AudioController.Instance.ErrorSound, 1f);
        }
    }

    /// <summary>
    /// Update the inspector with selected Celestial Body information
    /// </summary>
    public void PullInspectorValues()
    {
        CelestialBody cb = sc.Cc.CameraTrackedObject;
        if (cb != null)
        {
            celestialBodyName.text = cb.name;
            radius.text = cb.Radius.ToString();

            //Set position
            xPos.text = cb.transform.position.x.ToString("F2");
            yPos.text = cb.transform.position.y.ToString("F2");
            zPos.text = cb.transform.position.z.ToString("F2");

            //Set rotation
            Vector3 dir = sc.ObjectManipulation.RotationTool.transform.forward;
            Quaternion rot = Quaternion.LookRotation(dir);
            Vector3 euler = new Vector3(rot.eulerAngles.x, rot.eulerAngles.y, rot.eulerAngles.z);
            xRot.text = euler.x.ToString("F0");
            yRot.text = euler.y.ToString("F0");
            zRot.text = euler.z.ToString("F0");
        }
    }

    /// <summary>
    /// Called when you click on an inspector input field
    /// </summary>
    /// <param name="text"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void OnInputFieldSelected(string text, int start, int end)
    {
        if (sc.InPlayMode == false)
        {
            inspectorInputFieldSelected = true;
            Debug.LogWarning("Input field SELECTED: " + text + " start: " + start + " end: " + end);
        }
    }

    /// <summary>
    /// Called when you click off of an inspector input field
    /// </summary>
    /// <param name="text"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void OnInputFieldDeSelected()
    {
        inspectorInputFieldSelected = false;

        //Start the coroutine which pulls inspector data once you de select
        //If you clicked inside of a inspector field, but didnt click 'Update' new pulled data overwrites your input
        StartCoroutine(UpdateInspector());
        Debug.LogWarning("Input field DE-SELECFTED: ");
    }

    private void OnEnable()
    {
        //Position
        xPos.onTextSelection.AddListener(OnInputFieldSelected);
        //xPos.onDeselect.AddListener(OnInputFieldDeSelected);

        yPos.onTextSelection.AddListener(OnInputFieldSelected);
        //yPos.onDeselect.AddListener(OnInputFieldDeSelected);

        zPos.onTextSelection.AddListener(OnInputFieldSelected);
        //zPos.onDeselect.AddListener(OnInputFieldDeSelected);

        //Rotate
        xRot.onTextSelection.AddListener(OnInputFieldSelected);
        //xRot.onDeselect.AddListener(OnInputFieldDeSelected);

        yRot.onTextSelection.AddListener(OnInputFieldSelected);
        //yRot.onDeselect.AddListener(OnInputFieldDeSelected);

        zRot.onTextSelection.AddListener(OnInputFieldSelected);
        //zRot.onDeselect.AddListener(OnInputFieldDeSelected);
    }

    private void OnDisable()
    {
        //Position
        xPos.onTextSelection.RemoveListener(OnInputFieldSelected);
        //xPos.onDeselect.RemoveListener(OnInputFieldDeSelected);

        yPos.onTextSelection.RemoveListener(OnInputFieldSelected);
        //yPos.onDeselect.RemoveListener(OnInputFieldDeSelected);

        zPos.onTextSelection.RemoveListener(OnInputFieldSelected);
        //zPos.onDeselect.RemoveListener(OnInputFieldDeSelected);

        //Rotate
        xRot.onTextSelection.RemoveListener(OnInputFieldSelected);
        //xRot.onDeselect.RemoveListener(OnInputFieldDeSelected);

        yRot.onTextSelection.RemoveListener(OnInputFieldSelected);
        //yRot.onDeselect.RemoveListener(OnInputFieldDeSelected);

        zRot.onTextSelection.RemoveListener(OnInputFieldSelected);
        //zRot.onDeselect.RemoveListener(OnInputFieldDeSelected);
    }
}
