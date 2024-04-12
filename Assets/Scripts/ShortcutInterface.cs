using System.Collections;
using System.Collections.Generic;
using HaptGlove;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShortcutInterface : MonoBehaviour
{
    //public Button ButtonLastScene, ButtonResetScene, ButtonNextScene;

    public Interactable toggleLeft, toggleRight;
    public List<string> controlledHandsList = new List<string>();
    public Interactable buttonBT, buttonPS, buttonEx, buttonDropObject;
    //public Interactable toggleRecord;

    public GameObject handLeft;
    public GameObject handRight;

    void Start()
    {
        toggleLeft.OnClick.AddListener(delegate{
            if (toggleLeft.IsToggled)
                controlledHandsList.Add("Left");
            else
                controlledHandsList.Remove("Left");
        });

        toggleRight.OnClick.AddListener(delegate {
            if (toggleRight.IsToggled)
                controlledHandsList.Add("Right");
            else
                controlledHandsList.Remove("Right");
        });

        //toggleRecord.OnClick.AddListener(delegate {
        //    if (toggleRecord.IsToggled)
        //        gameObject.GetComponent<JointDataLogger>().StartRecord();
        //    else
        //        gameObject.GetComponent<JointDataLogger>().EndRecord();
            
        //});

        buttonBT.OnClick.AddListener(BTButtonOnClick);
        buttonPS.OnClick.AddListener(PSButtonOnClick);
        buttonEx.OnClick.AddListener(ExButtonOnClick);
        buttonDropObject.OnClick.AddListener(DropObjectButtonOnClick);
    }


    private void BTButtonOnClick()
    {
        foreach (var hand in controlledHandsList)
        {
            switch (hand)
            {
                case "Left":
                    handLeft.GetComponent<HaptGloveHandler>().BTConnection();
                    break;
                case "Right":
                    handRight.GetComponent<HaptGloveHandler>().BTConnection();
                    break;
            }
        }
    }

    private void PSButtonOnClick()
    {
        foreach (var hand in controlledHandsList)
        {
            switch (hand)
            {
                case "Left":
                    handLeft.GetComponent<HaptGloveHandler>().AirPressureSourceControl();
                    break;
                case "Right":
                    handRight.GetComponent<HaptGloveHandler>().AirPressureSourceControl();
                    break;
            }
        }
    }

    private void ExButtonOnClick()
    {
        byte[][] clutchStates =
            {new byte[] {0, 2}, new byte[] {1, 2}, new byte[] {2, 2}, new byte[] {3, 2}, new byte[] {4, 2}};
        byte[] btData;

        foreach (var hand in controlledHandsList)
        {
            switch (hand)
            {
                case "Left":
                    //Haptics.ApplyHaptics(clutchStates, 60, hand, false);
                    btData = handLeft.GetComponent<HaptGloveHandler>().haptics.ApplyHaptics(clutchStates, 60, false);
                    handLeft.GetComponent<HaptGloveHandler>().BTSend(btData);
                    break;
                case "Right":
                    //Haptics.ApplyHaptics(clutchStates, 60, hand, false);
                    btData = handRight.GetComponent<HaptGloveHandler>().haptics.ApplyHaptics(clutchStates, 60, false);
                    handRight.GetComponent<HaptGloveHandler>().BTSend(btData);
                    break;
            }
        }
    }

    private void DropObjectButtonOnClick()
    {
        foreach (var hand in controlledHandsList)
        {
            switch (hand)
            {
                case "Left":
                    handLeft.GetComponent<Grasping>().DropObject();
                    break;
                case "Right":
                    handRight.GetComponent<Grasping>().DropObject();
                    break;
            }
        }
    }


}
