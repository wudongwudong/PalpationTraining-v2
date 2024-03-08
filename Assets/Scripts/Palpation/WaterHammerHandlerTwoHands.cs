using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class WaterHammerHandlerTwoHands : MonoBehaviour
{
    public WaterHammerGetHand radialPulseHand;
    public WaterHammerGetHand brachialPulseHand;
    public TMP_Text logText;
    public WaterHammerPulseTwoHands waterHammerPulseScript;

    private Color normalPulseHighPeakColor,
        normalPulseHighValleyColor,
        normalPulseLowPeakColor,
        normalPulseLowValleyColor,
        weakPulseHighPeakColor,
        weakPulseHighValleyColor,
        weakPulseLowPeakColor,
        weakPulseLowValleyColor;

    enum PulseStrength : byte
    {
        weak = 0,
        normal = 1,
        strong = 2
    }
    enum PulseStatus : byte
    {
        healthy = 0,
        unhealthy = 1
    }

    void Start()
    {
        normalPulseHighPeakColor = new Color(171f / 255f, 27f / 255f, 27f / 255f, 1);
        normalPulseHighValleyColor = new Color(171f / 255f, 27f / 255f, 27f / 255f, 100/255f);
        normalPulseLowPeakColor = new Color(171f / 255f, 27f / 255f, 27f / 255f, 200/255f);
        normalPulseLowValleyColor = new Color(171f / 255f, 27f / 255f, 27f / 255f, 20f / 255f);

        weakPulseHighPeakColor = new Color(171f / 255f, 27f / 255f, 27f / 255f, 100/255f);
        weakPulseHighValleyColor = new Color(171f / 255f, 27f / 255f, 27f / 255f, 50 / 255f);
        weakPulseLowPeakColor = new Color(171f / 255f, 27f / 255f, 27f / 255f, 60 / 255f);
        weakPulseLowValleyColor = new Color(171f / 255f, 27f / 255f, 27f / 255f, 20f / 255f);

        pulseStrengthes = new[] { (byte)PulseStrength.normal, (byte)PulseStrength.normal, (byte)PulseStrength.normal, (byte)PulseStrength.normal, (byte)PulseStrength.normal };
    }

    public bool[] pulseAppliedToFingers = new bool[5];
    public byte[] pulseStrengthes = new byte[5];

    void Update()
    {
        if (radialPulseHand.pulseReadyToApply != waterHammerPulseScript.radialBeatOn)
        {
            waterHammerPulseScript.UpdateRadialPulse(radialPulseHand.interactingHand, 1, pulseStrengthes[1], radialPulseHand.pulseReadyToApply);
        }

        if (brachialPulseHand.pulseReadyToApply != waterHammerPulseScript.brachialBeatOn)
        {
            waterHammerPulseScript.UpdateBrachialPulse(brachialPulseHand.interactingHand, 1, pulseStrengthes[1], brachialPulseHand.pulseReadyToApply);
        }


        if (radialPulseHand.pulseReadyToApply | brachialPulseHand.pulseReadyToApply)
        {
            if (waterHammerPulseScript.beatOn == false)
            {
                //visual cue start
                waterHammerPulseScript.UpdateRadialPulseColor(normalPulseHighPeakColor, normalPulseHighValleyColor,
                    normalPulseLowPeakColor, normalPulseLowValleyColor);
                waterHammerPulseScript.UpdateBrachialPulseColor(normalPulseHighPeakColor, normalPulseHighValleyColor,
                    normalPulseLowPeakColor, normalPulseLowValleyColor);
                waterHammerPulseScript.beatOn = true;
            }
            
            //both hand grasped the arm and pulse applied, measure the angle and change the pulse accordingly
            if (radialPulseHand.pulseReadyToApply & brachialPulseHand.pulseReadyToApply)
            {
                float angle = 90 - Vector3.Angle(brachialPulseHand.transform.up, Vector3.up);
                //Debug.Log(angle);
                if ((angle > 70) & !weakPulseIsApplied)
                {
                    //weak pulse for ? times, then change back to normal
                    if (waterHammerPulseScript.GetCurrentState() == 1)
                    {
                        coroutine = WaterHammerPulse(noWeakPulse);
                        StartCoroutine(coroutine);
                        weakPulseIsApplied = true;
                    }
                }

                if ((angle < 30))
                {
                    if (weakPulseIsApplied)
                    {
                        weakPulseIsApplied = false;
                        SetNormalPulse("Radial");
                    }
                }
            }
            else
            {
                if (weakPulseIsApplied)
                {
                    weakPulseIsApplied = false;
                    SetNormalPulse("Radial");
                }
            }
        }
        else
        {
            if (waterHammerPulseScript.beatOn == true)
            {
                //visual cue ends
                waterHammerPulseScript.beatOn = false;
                waterHammerPulseScript.EndPulse();

                SetNormalPulse("Radial");
                //need to reset the visual
                ////////       ////////////
            }
        }

    }

    private bool weakPulseIsApplied = false;
    private int noWeakPulse = 0;
    private IEnumerator coroutine;
    IEnumerator WaterHammerPulse(int noWeakPulse)
    {
        pulseStrengthes = new[] { (byte)PulseStrength.weak, (byte)PulseStrength.weak, (byte)PulseStrength.weak, (byte)PulseStrength.weak, (byte)PulseStrength.weak };

        waterHammerPulseScript.UpdateRadialPulse(radialPulseHand.interactingHand, 1, pulseStrengthes[1], radialPulseHand.pulseReadyToApply);
        waterHammerPulseScript.UpdateRadialPulseColor(weakPulseHighPeakColor, weakPulseHighValleyColor, weakPulseLowPeakColor, weakPulseLowValleyColor);

        Debug.Log("weak pulse");

        if (noWeakPulse != 0)
        {
            float weakPulseDuration = noWeakPulse / waterHammerPulseScript.GetPulseFrequency();

            yield return new WaitForSecondsRealtime(weakPulseDuration);

            SetNormalPulse("Radial");
        }
        
    }

    private void SetNormalPulse(String pulseName)
    {
        pulseStrengthes = new[] { (byte)PulseStrength.normal, (byte)PulseStrength.normal, (byte)PulseStrength.normal, (byte)PulseStrength.normal, (byte)PulseStrength.normal };

        if (pulseName == "Radial")
        {
            waterHammerPulseScript.UpdateRadialPulse(radialPulseHand.interactingHand, 1, pulseStrengthes[1], radialPulseHand.pulseReadyToApply);
            waterHammerPulseScript.UpdateRadialPulseColor(normalPulseHighPeakColor, normalPulseHighValleyColor,
                normalPulseLowPeakColor, normalPulseLowValleyColor);
        }
        else if(pulseName == "Brachial")
        {
            waterHammerPulseScript.UpdateBrachialPulse(brachialPulseHand.interactingHand, 1, pulseStrengthes[1], brachialPulseHand.pulseReadyToApply);
            waterHammerPulseScript.UpdateBrachialPulseColor(normalPulseHighPeakColor, normalPulseHighValleyColor,
                normalPulseLowPeakColor, normalPulseLowValleyColor);
        }
    }

}
