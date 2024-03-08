using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HaptGlove;

public class TumorSpecs : MonoBehaviour
{
    public bool showVisualCue = true;

    private HaptGloveHandler gloveHandler;
    //private Grasping graspingScript;

    private HaptGloveHandler.HandType whichHand;
    private bool[] fingerTouchedTumor = new bool[5];
    public bool tumorTouched = false;
    private string fingerName;
    private byte fingerID;
    private byte[] clutchState;

    public byte tumorTargetPressure = 50;
    public TMP_Text logText;
    public MeshRenderer tumorVisual;

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "RealFingertip")
        {
            fingerID = GetFinger(col, "Enter");
            if ((fingerTouchedTumor[fingerID] == false) & (fingerID != 4))
            {
                fingerTouchedTumor[fingerID] = true;
                //Haptics.ApplyHaptics(clutchState, tumorTargetPressure, whichHand, true);
                byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchState, tumorTargetPressure, true);
                gloveHandler.BTSend(btData);

                tumorTouched = true;

                logText.text = "Tumor found";

                if (showVisualCue)
                {
                    tumorVisual.material = Resources.Load<Material>("Materials/Red");
                }
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        //if (col.tag == "RealFingertip")
        //{
        //    fingerID = GetFinger(col, "Exit");

        //    if (graspingScript.normalizedMicrotubeData[fingerID] > graspingScript.hapticMaxPosition[fingerID])
        //    {
        //        return;
        //    }

        //    Haptics.ApplyHaptics(clutchState, tumorTargetPressure, whichHand, true);
        //    fingerTouchedTumor[fingerID] = false;
        //}
    }

    private byte GetFinger(Collider col, string bufState)
    {
        //graspingScript = col.GetComponentInParent<Grasping>();
        gloveHandler = col.GetComponentInParent<HaptGloveHandler>();
        whichHand = gloveHandler.whichHand;
        foreach (Transform child in col.transform)
        {
            if (child.tag == "GhostHand")
            {
                fingerName = child.name;
                break;
            }
        }

        clutchState = Haptics.SetClutchState(fingerName, bufState);
        return clutchState[0];
    }

    public void ResetTumorSpecs()
    {
        for (byte i = 0; i < 4; i++)
        {
            if (fingerTouchedTumor[i])
            {
                byte[] clutchState = new byte[] { i, 2 };
                //Haptics.ApplyHaptics(clutchState, tumorTargetPressure, whichHand, true);
                byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchState, tumorTargetPressure, true);
                gloveHandler.BTSend(btData);
            }
        }

        fingerTouchedTumor = new bool[5];
        tumorTouched = false;

        logText.text = "Training";

        if (showVisualCue)
        {
            tumorVisual.material = Resources.Load<Material>("Materials/Transparent");
        }
        
    }
}
