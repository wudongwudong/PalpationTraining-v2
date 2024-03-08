using System.Collections;
using System.Collections.Generic;
using DigitalRuby.RainMaker;
using HaptGlove;
using UnityEngine;

public class RainDrops_rain : MonoBehaviour
{
    public int rainDrop_Hz = 5;
    public GameObject rain;
    public bool visual = false;
    public bool vibration = false;
    public bool multimode = true;
    public string controlPanelName = "";

    private GameObject controlPanel;

    private float rainHitInterval = 0;
    private float rainStayInterval = 0;
    private float rainStayInterval_buf = 0;
    private System.Random rdm = new System.Random();
    private bool rainOn = false;

    private HaptGloveHandler gloveHandler;

    void Start()
    {
        rainHitInterval = (float)1 / rainDrop_Hz;
        rainStayInterval = (float)20 / 1000; //s
        rainStayInterval_buf = rainStayInterval;
        Debug.Log("Rain Hit Interval: " + rainHitInterval);

        //emissionModule = rainParticle.emission;
        //emissionModule.enabled = false;
        rain.GetComponent<RainScript>().RainIntensity = 0;

        controlPanel = GameObject.Find(controlPanelName);
    }


    private void OnTriggerEnter(Collider collider)
    {
        if (collider.name.Contains("_PalmCollider"))
        {
            gloveHandler = collider.GetComponentInParent<HaptGloveHandler>();

            rainOn = true;
            rain.GetComponent<RainScript>().RainIntensity = 0.011f;

            ToggleControlPanel(false);
        }
    }
    private void OnTriggerExit(Collider collider)
    {
        if (collider.name.Contains("_PalmCollider"))
        {
            rainOn = false;
            if (rainHapticsIsApplied)
            {
                if (visual) { }
                else if (vibration) { }
                else
                {
                    byte[] clutchState = new byte[] { fingerID, 2 };
                    //Haptics.ApplyHaptics(clutchState, targetPres, whichHand, false);
                    byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchState, targetPres, false);
                    gloveHandler.BTSend(btData);
                }
                
                //Debug.Log("Rain Off: " + fingerID + "\t" + targetPres);
                rainHapticsIsApplied = false;
                rainStayInterval_buf = rainStayInterval;
            }
            rain.GetComponent<RainScript>().RainIntensity = 0;

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

    private bool rainHapticsIsApplied = false;
    private byte fingerID = 0;
    private byte targetPres = 0;
    void Update()
    {
        if (rainOn)
        {
            rainHitInterval -= Time.deltaTime;
            if (rainHitInterval <= 0)
            {
                fingerID = (byte)rdm.Next(5);
                targetPres = (byte)rdm.Next(20, 30);

                if (visual) { }
                else if (vibration)
                {
                    byte[] clutchState = new byte[] { fingerID, 0 };
                    //Haptics.ApplyHaptics(0xff, clutchState, 5, whichHand, false);
                    byte[] btData = gloveHandler.haptics.ApplyHaptics(0xff, clutchState, 5, false);
                    gloveHandler.BTSend(btData);
                }
                else
                {
                    byte[] clutchState = new byte[] { fingerID, 0 };
                    //Haptics.ApplyHaptics(clutchState, targetPres, whichHand, false);
                    byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchState, targetPres, false);
                    gloveHandler.BTSend(btData);
                }
                
                //Debug.Log("Rain On: " + fingerID + "\t" + targetPres);
                rainHapticsIsApplied = true;
                rainHitInterval = (float)1 / rainDrop_Hz;
            }

            if (rainHapticsIsApplied)
            {
                rainStayInterval_buf -= Time.deltaTime;
                if (rainStayInterval_buf < 0)
                {
                    if (visual) { }
                    else if (vibration) {}
                    else
                    {
                        byte[] clutchState = new byte[] { fingerID, 2 };
                        //Haptics.ApplyHaptics(clutchState, targetPres, whichHand, false);
                        byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchState, targetPres, false);
                        gloveHandler.BTSend(btData);
                    }
                        
                    //Debug.Log("Rain Off: " + fingerID + "\t" + targetPres);
                    rainHapticsIsApplied = false;
                    rainStayInterval_buf = rainStayInterval;
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
