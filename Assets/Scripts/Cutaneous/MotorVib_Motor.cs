using System.Collections;
using System.Collections.Generic;
using HaptGlove;
using UnityEngine;

public class MotorVib_Motor : MonoBehaviour
{
    public byte frequency_Hz = 20;
    public byte valveOnTiming = 15;
    public GameObject eccentric;
    public float rotateSpeed;
    public bool visual = false;
    public bool vibration = false;
    public bool multimode = true;
    public string controlPanelName = "";

    private GameObject controlPanel;
    private float time = 0;

    private float beatHitInterval = 0;
    private float beatHitInterval_half = 0;

    private bool beatOn = false;
    private bool beatHapticsIsApplied = false;

    private HaptGloveHandler gloveHandler;

    void Start()
    {
        beatHitInterval = (float)1 / frequency_Hz;
        beatHitInterval_half = (float)beatHitInterval / 2;
        Debug.Log("beathit interval" + beatHitInterval);
        Debug.Log("beathit Interval_half" + beatHitInterval_half);

        controlPanel = GameObject.Find(controlPanelName);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.name.Contains("_PalmCollider"))
        {
            gloveHandler = collider.GetComponentInParent<HaptGloveHandler>();
            beatOn = true;
            byte[] clutchState = { 1, 0 };
            byte[] valveTiming = { valveOnTiming, 0 };


            if ((500 / frequency_Hz) > 255)
            {
                valveTiming[1] = 250;
            }
            else
            {
                valveTiming[1] = (byte)(500 / frequency_Hz);
            }

            if (visual) { }
            else if (vibration)
            {
                //Haptics.ApplyHaptics(0xff, clutchState, 6, whichHand, false);
                byte[] btData = gloveHandler.haptics.ApplyHaptics(0xff, clutchState, 6, false);
                gloveHandler.BTSend(btData);
            }
            else
            {
                //Haptics.ApplyHapticsWithTiming(frequency_Hz, clutchState, valveTiming, whichHand, false);
                byte[] btData = gloveHandler.haptics.ApplyHapticsWithTiming(frequency_Hz, clutchState, valveTiming, false);
                gloveHandler.BTSend(btData);
            }

            beatHapticsIsApplied = true;

            ToggleControlPanel(false);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.name.Contains("_PalmCollider"))
        {
            beatOn = false;
            if (beatHapticsIsApplied)
            {
                if (visual) { }
                else if (vibration) { }
                else
                {
                    byte[] clutchState = { 1, 2 };
                    //Haptics.ApplyHapticsWithTiming(frequency_Hz, clutchState, new byte[] { 0, 255 }, whichHand, false);
                    byte[] btData = gloveHandler.haptics.ApplyHapticsWithTiming(frequency_Hz, clutchState, new byte[] { 0, 255 }, false);
                    gloveHandler.BTSend(btData);
                }
                    
                beatHapticsIsApplied = false;
                beatHitInterval = (float)1 / frequency_Hz;
            }

            ToggleControlPanel(true);
        }
    }

    private void ToggleControlPanel(bool state)
    {
        if (controlPanel != null)
        {
            foreach (Transform child in controlPanel.transform)
            {
                child.GetComponent<SphereCollider>().enabled = state;
            }
        }
    }

    float MotorVibTimer = 0;
    void Update()
    {
        if (beatOn)
        {
            time += Time.deltaTime;
            eccentric.transform.RotateAround(eccentric.transform.position, eccentric.transform.up, rotateSpeed * Time.deltaTime);

            if (vibration)
            {
                MotorVibTimer += Time.deltaTime;

                if (MotorVibTimer >= 0.1f)
                {
                    byte[] clutchState = { 1, 0 };
                    //Haptics.ApplyHaptics(0xff, clutchState, 6, whichHand, false);
                    byte[] btData = gloveHandler.haptics.ApplyHaptics(0xff, clutchState, 6, false);
                    gloveHandler.BTSend(btData);
                    MotorVibTimer = 0;
                }

            }
        }
        
    }


    /// <summary>
    /// mode name: visual, vibration, multimode
    /// </summary>
    /// <param name="mode"></param>
    public void SetMode(string mode)
    {
        switch (mode)
        {
            case "visual":
                vibration = false;
                multimode = false;
                visual = true;
                break;
            case "vibration":
                visual = false;
                multimode = false;
                vibration = true;
                break;
            case "multimode":
                visual = false;
                vibration = false;
                multimode = true;
                break;
            default:
                break;
        }
    }
}
