using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using HaptGlove;
using Microsoft.MixedReality.Toolkit.UI;

public class HeartbeatHandler : MonoBehaviour
{
    private HaptGloveHandler leftGloveHandler;
    private HaptGloveHandler rightGloveHandler;

    [SerializeField] private TMP_Text i1_Text;
    [SerializeField] private TMP_Text i2_Text;
    [SerializeField] private TMP_Text heartRate_Text;
    [SerializeField] private TMP_Text i4_Text;

    public List<byte>[] fingerListLeft = new List<byte>[6];
    public List<byte>[] fingerListRight = new List<byte>[6];

    //private byte[] activatedFingerLeft = new byte[5];
    //private byte[] activatedFingerRight = new byte[5];

    private List<byte>[] activatedFingerLeft = new List<byte>[2];   //[0] weak pulse, [1] strong pulse
    private List<byte>[] activatedFingerRight = new List<byte>[2];  //[0] weak pulse, [1] strong pulse

    private int fingerNo;

    public bool showVisualCue = true;

    public Animate heartbeatScanAnimation;
    public Animation heartbeatAnimation;
    public MeshRenderer leftAtriumIndicatorMaterial;
    public MeshRenderer rightAtriumIndicatorMaterial;
    public MeshRenderer leftVentricleIndicatorMaterial;
    public MeshRenderer rightVentricleIndicatorMaterial;
    public float heartBeat_Hz = 0.9f;//0.89f;
    private int oneCycle;

    public bool isInteractable = true;
    public bool beatOn = false;
    private Color beatColor_Valley1;
    private Color beatColor_Peak1;
    private Color beatColor_Valley2;
    private Color beatColor_Peak2;

    public bool[] radialFingerIDs = new bool[5];
    public bool[] brachialFingerIDs = new bool[5];

    private bool beatHapticsIsApplied = false;

    public bool isHeartScan = false;

    public string controlPanelName = "";

    private GameObject controlPanel;
    private bool controlPanelIsActive = true;

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

    public void OnSliderChanged(SliderEventData eventData)
    {
        byte pres1Min = 10;
        byte pres1Max = 50;
        byte pres2Min = 10;
        byte pres2Max = 50;
        byte hrMin = 40;
        byte hrMax = 120;

        string sliderName = eventData.Slider.name;

        switch (sliderName)
        {
            case "Slider_I1":
                pres1 = (byte)(eventData.NewValue * (pres1Max - pres1Min) + pres1Min);
                i1_Text.text = pres1.ToString();
                break;
            case "Slider_I2":
                pres2 = (byte)(eventData.NewValue * (pres2Max - pres2Min) + pres2Min);
                i2_Text.text = pres2.ToString();
                break;
            case "Slider_I3":
                heartBeat_Hz = (eventData.NewValue * (hrMax - hrMin) + hrMin) / 60;
                heartRate_Text.text = (heartBeat_Hz * 60).ToString();
                break;
            case "Slider_I4":
                i4 = (byte)eventData.NewValue;
                i4_Text.text = i4.ToString();
                break;
        }
    }

    void Start()
    {
        leftGloveHandler = GameObject.Find("Left Hand Physics").GetComponent<HaptGloveHandler>();
        rightGloveHandler = GameObject.Find("Right Hand Physics").GetComponent<HaptGloveHandler>();
        oneCycle = (int)(1 / heartBeat_Hz * 1000);
        Debug.Log("Pulse interval: " + oneCycle);

        UpdatePulsePara(0);//Healthy

        beatColor_Valley1 = new Color(171f / 255f, 27f / 255f, 27f / 255f, 20f / 255f);
        beatColor_Peak1 = new Color(171f / 255f, 27f / 255f, 27f / 255f, 1);
        beatColor_Valley2 = new Color(171f / 255f, 27f / 255f, 27f / 255f, 100f / 255f);
        beatColor_Peak2 = new Color(171f / 255f, 27f / 255f, 27f / 255f, 100f / 255f);

        for (int i = 0; i < fingerListLeft.Length; i++)
        {
            fingerListLeft[i] = new List<byte>();
        }
        for (int i = 0; i < fingerListRight.Length; i++)
        {
            fingerListRight[i] = new List<byte>();
        }

        for (int i = 0; i < activatedFingerLeft.Length; i++)
        {
            activatedFingerLeft[i] = new List<byte>();
        }
        for (int i = 0; i < activatedFingerRight.Length; i++)
        {
            activatedFingerRight[i] = new List<byte>();
        }

        if (heartbeatAnimation != null)
        {
            heartbeatAnimation.Stop();
        }

        if (leftAtriumIndicatorMaterial == null)
        {
            showVisualCue = false;
        }

        controlPanel = GameObject.Find(controlPanelName);
    }

    private void OrganizeFingerHaptics()
    {
        for (int i = 0; i < activatedFingerLeft.Length; i++)
        {
            activatedFingerLeft[i] = new List<byte>();
        }
        for (int i = 0; i < activatedFingerRight.Length; i++)
        {
            activatedFingerRight[i] = new List<byte>();
        }

        for (int i = 0; i < fingerListLeft.Length; i++)
        {
            if (fingerListLeft[i].Count != 0)
            {
                byte heartField = fingerListLeft[i].Last();

                if ((heartField == 1) | (heartField == 2))
                {
                    activatedFingerLeft[0].Add((byte)i);
                }
                else
                {
                    activatedFingerLeft[1].Add((byte)i);
                }
            }
        }

        for (int i = 0; i < fingerListRight.Length; i++)
        {
            if (fingerListRight[i].Count != 0)
            {
                byte heartField = fingerListRight[i].Last();

                if ((heartField == 1) | (heartField == 2))
                {
                    activatedFingerRight[0].Add((byte)i);
                }
                else
                {
                    activatedFingerRight[1].Add((byte)i);
                }
            }
        }

        //Debug.Log("Right[0]: " + BitConverter.ToString(activatedFingerRight[0].ToArray()));
        //Debug.Log("Right[1]: " + BitConverter.ToString(activatedFingerRight[1].ToArray()));
    }

    private void HeartbeatHaptics(bool isLeft, byte[] fingerIDs, bool isOn, byte[] valveTiming)
    {
        byte[][] clutchStates = new byte[fingerIDs.Length][];
        byte[][] valveTimings = new byte[fingerIDs.Length][];

        if (isOn)
        {
            clutchStates = new byte[fingerIDs.Length][];
            for (int i = 0; i < fingerIDs.Length; i++)
            {
                clutchStates[i] = new byte[] { fingerIDs[i], 0x00 };
                valveTimings[i] = valveTiming;
            }
        }
        else
        {
            clutchStates = new byte[fingerIDs.Length][];
            for (int i = 0; i < fingerIDs.Length; i++)
            {
                clutchStates[i] = new byte[] { fingerIDs[i], 0x02 };
                valveTimings[i] = valveTiming;
            }
        }

        if (isLeft)
        {
            //Haptics.ApplyHapticsWithTiming(clutchStates, valveTimings, "LargeLeft", false);
            byte[] btData = leftGloveHandler.haptics.ApplyHapticsWithTiming(clutchStates, valveTimings, false);
            leftGloveHandler.BTSend(btData);
        }
        else
        {
            //Haptics.ApplyHapticsWithTiming(clutchStates, valveTimings, "LargeRight", false);
            byte[] btData = rightGloveHandler.haptics.ApplyHapticsWithTiming(clutchStates, valveTimings, false);
            rightGloveHandler.BTSend(btData);
        }
    }

    private void HeartbeatHaptics(bool isLeft, byte[] fingerIDs, bool isOn, byte tarPres)
    {
        byte[][] clutchStates;

        if (isOn)
        {
            clutchStates = new byte[fingerIDs.Length][];
            for (int i = 0; i < fingerIDs.Length; i++)
            {
                clutchStates[i] = new byte[] { fingerIDs[i], 0x00 };
            }
        }
        else
        {
            clutchStates = new byte[fingerIDs.Length][];
            for (int i = 0; i < fingerIDs.Length; i++)
            {
                clutchStates[i] = new byte[] { fingerIDs[i], 0x02 };
            }
        }

        if (isLeft)
        {
            //Haptics.ApplyHaptics(clutchStates, tarPres, "LargeLeft", false);
            byte[] btData = leftGloveHandler.haptics.ApplyHaptics(clutchStates, tarPres, false);
            leftGloveHandler.BTSend(btData);
        }
        else
        {
            //Haptics.ApplyHaptics(clutchStates, tarPres, "LargeRight", false);
            byte[] btData = rightGloveHandler.haptics.ApplyHaptics(clutchStates, tarPres, false);
            rightGloveHandler.BTSend(btData);
        }
    }

    public void StopBeating()
    {
        Debug.Log("Stop beating");
        cycleStart = false;

        if (beatHapticsIsApplied)
        {
            beatHapticsIsApplied = false;
        }

        if (leftAtriumIndicatorMaterial != null)
        {
            leftAtriumIndicatorMaterial.material.color = beatColor_Valley1;
            rightAtriumIndicatorMaterial.material.color = beatColor_Valley1;
            leftVentricleIndicatorMaterial.material.color = beatColor_Valley1;
            rightVentricleIndicatorMaterial.material.color = beatColor_Valley1;
        }

        curStage = 1;

        if (activatedFingerLeft[0].Count > 0)
        {
            HeartbeatHaptics(true, activatedFingerLeft[0].ToArray(), false, pres1);
        }
        if (activatedFingerRight[0].Count > 0)
        {
            HeartbeatHaptics(false, activatedFingerRight[0].ToArray(), false, pres1);
        }
        if (activatedFingerLeft[1].Count > 0)
        {
            HeartbeatHaptics(true, activatedFingerLeft[1].ToArray(), false, pres2);
        }
        if (activatedFingerRight[1].Count > 0)
        {
            HeartbeatHaptics(false, activatedFingerRight[1].ToArray(), false, pres2);
        }

        if (heartbeatAnimation != null)
        {
            heartbeatAnimation.Stop();
        }

        if (heartbeatScanAnimation != null)
        {
            heartbeatScanAnimation.Stop();
        }
    }

    public void EndPulse()
    {
        Debug.Log("End pulse");
        beatOn = false;
        cycleStart = false;

        //radialFingerIDs = new bool[5];
        //brachialFingerIDs = new bool[5];

        if (beatHapticsIsApplied)
        {
            beatHapticsIsApplied = false;
        }

        if (leftAtriumIndicatorMaterial != null)
        {
            leftAtriumIndicatorMaterial.material.color = beatColor_Valley1;
            rightAtriumIndicatorMaterial.material.color = beatColor_Valley1;
            leftVentricleIndicatorMaterial.material.color = beatColor_Valley1;
            rightVentricleIndicatorMaterial.material.color = beatColor_Valley1;
        }

        curStage = 1;

        //if (activatedFingerLeft[0].Count > 0)
        //{
        //    HeartbeatHaptics(true, activatedFingerLeft[0].ToArray(), false, new byte[] { i1, i2 });
        //}
        //if (activatedFingerRight[0].Count > 0)
        //{
        //    HeartbeatHaptics(false, activatedFingerRight[0].ToArray(), false, new byte[] { i1, i2 });
        //}
        //if (activatedFingerLeft[1].Count > 0)
        //{
        //    HeartbeatHaptics(true, activatedFingerLeft[1].ToArray(), false, new byte[] { i3, i4 });
        //}
        //if (activatedFingerRight[1].Count > 0)
        //{
        //    HeartbeatHaptics(false, activatedFingerRight[1].ToArray(), false, new byte[] { i3, i4 });
        //}

        if (activatedFingerLeft[0].Count > 0)
        {
            HeartbeatHaptics(true, activatedFingerLeft[0].ToArray(), false, pres1);
        }
        if (activatedFingerRight[0].Count > 0)
        {
            HeartbeatHaptics(false, activatedFingerRight[0].ToArray(), false, pres1);
        }
        if (activatedFingerLeft[1].Count > 0)
        {
            HeartbeatHaptics(true, activatedFingerLeft[1].ToArray(), false, pres2);
        }
        if (activatedFingerRight[1].Count > 0)
        {
            HeartbeatHaptics(false, activatedFingerRight[1].ToArray(), false, pres2);
        }

        if (heartbeatAnimation != null)
        {
            heartbeatAnimation.Stop();
        }

        if (heartbeatScanAnimation != null)
        {
            heartbeatScanAnimation.Stop();
        }
    }


    public void UpdatePulsePara(byte status)
    {
        // 0 - t1, approaching, no haptics - t2, pressing, increasing pressure - t3. holding, keeping pressure
        // - t4, releasing, decreasing pressure - t5, released, no haptics - one cycle
        switch (status)
        {
            case 0:                         //healthy
                if (isHeartScan)
                {
                    norT1 = 50f / 1000f;
                    norT2 = 400f / 1000f + norT1;
                    norT3 = 50f / 1000f + norT2;
                    norT4 = 500f / 1000f + norT3;
                    norT5 = 1; //0
                }
                else
                {
                    norT1 = 100f / 1000f;
                    norT2 = 150f / 1000f + norT1;
                    norT3 = 150f / 1000f + norT2;
                    norT4 = 150f / 1000f + norT3;
                    norT5 = 1; //450
                }
                break;
            case 1:                         //unhealthy
                norT1 = 200f / 1000f;
                norT2 = 30f / 1000f;
                norT3 = 70f / 1000f;
                norT4 = 200f / 1000f;
                norT5 = 500f / 1000f;
                break;
        }
    }


    byte[] DecodePulseStrength(byte pulseStrength)
    {
        byte[] valveTiming = new byte[2];
        switch (pulseStrength)
        {
            case 0:
                valveTiming = new byte[] { 4, 5 };//weak
                break;
            case 1:
                valveTiming = new byte[] { 9, 10 };//normal
                break;
            case 2:
                valveTiming = new byte[] { 12, 13 };//strong
                break;
        }

        return valveTiming;
    }

    /// 0 - t1, approaching, no haptics - t2, pressing, increasing pressure - t3. holding, keeping pressure
    /// - t4, releasing, decreasing pressure - t5, released, no haptics - one cycle
    private float norT1;
    private float norT2;
    private float norT3;
    private float norT4;
    private float norT5;
    private int curStage = 1;
    private float timeFrame = 0;

    //pulse intensity
    private byte i1 = 4;
    private byte i2 = 50;
    private byte i3 = 8;
    private byte i4 = 50;

    [SerializeField] public byte pres1 = 20;
    [SerializeField] public byte pres2 = 50;

    private bool cycleStart = false;

    void FixedUpdate()
    {
        oneCycle = (int)(1 / heartBeat_Hz * 1000);

        fingerNo = fingerListLeft[0].Count + fingerListLeft[1].Count + fingerListLeft[2].Count +
                   fingerListLeft[3].Count + fingerListLeft[4].Count + fingerListLeft[5].Count + 
                   fingerListRight[0].Count + fingerListRight[1].Count + fingerListRight[2].Count + 
                   fingerListRight[3].Count + fingerListRight[4].Count + fingerListRight[5].Count;
        //Debug.Log(fingerNo);


        if (fingerNo > 0)
        {
            if (controlPanelIsActive)
            {
                ToggleControlPanel(false);
                controlPanelIsActive = false;
            }
            beatOn = true;

            if (!isInteractable)
            {
                return;
            }

            if (heartbeatAnimation != null)
            {
                if (!heartbeatAnimation.isPlaying)
                {
                    heartbeatAnimation.Play();
                }
            }

            if (heartbeatScanAnimation != null)
            {
                if (heartbeatScanAnimation.animationStart == false)
                {
                    heartbeatScanAnimation.animationStart = true;
                }
            }

            if (!cycleStart)
            {
                OrganizeFingerHaptics();
                cycleStart = true;
            }

            switch (curStage)
            {
                case 1:
                    if (showVisualCue)
                    {
                        leftAtriumIndicatorMaterial.material.color = Color.Lerp(beatColor_Valley1, beatColor_Peak2,
                            timeFrame / (norT1 * oneCycle));
                        rightAtriumIndicatorMaterial.material.color = Color.Lerp(beatColor_Valley1, beatColor_Peak2,
                            timeFrame / (norT1 * oneCycle));

                    }

                    if (timeFrame >= (norT1 * oneCycle))
                    {
                        curStage = 2;
                        //timeFrame = 0;

                        //if (activatedFingerLeft[0].Count > 0)
                        //{
                        //    HeartbeatHaptics(true, activatedFingerLeft[0].ToArray(), true, new byte[] { i1, i2 });
                        //}
                        //if (activatedFingerRight[0].Count > 0)
                        //{
                        //    HeartbeatHaptics(false, activatedFingerRight[0].ToArray(), true, new byte[] { i1, i2 });
                        //}

                        if (activatedFingerLeft[0].Count > 0)
                        {
                            HeartbeatHaptics(true, activatedFingerLeft[0].ToArray(), true, pres1);
                        }
                        if (activatedFingerRight[0].Count > 0)
                        {
                            HeartbeatHaptics(false, activatedFingerRight[0].ToArray(), true, pres1);
                        }

                        //byte[] clutchState = new byte[2] { 0x01, 0x00 };
                        //byte[] valveTiming = new byte[2] { norI1, 0 };
                        //Haptics.ApplyHapticsWithTiming(clutchState, valveTiming, "LargeRight", false);
                        beatHapticsIsApplied = true;
                    }

                    break;
                case 2:
                    if (showVisualCue)
                    {
                        leftAtriumIndicatorMaterial.material.color = Color.Lerp(beatColor_Peak2, beatColor_Valley1,
                            (timeFrame - norT1 * oneCycle) / (norT2 * oneCycle - norT1 * oneCycle));
                        rightAtriumIndicatorMaterial.material.color = Color.Lerp(beatColor_Peak2, beatColor_Valley1,
                            (timeFrame - norT1 * oneCycle) / (norT2 * oneCycle - norT1 * oneCycle));

                    }

                    if (timeFrame >= (norT2 * oneCycle))
                    {
                        curStage = 3;
                        //timeFrame = 0;

                        //if (activatedFingerLeft[0].Count > 0)
                        //{
                        //    HeartbeatHaptics(true, activatedFingerLeft[0].ToArray(), false, new byte[] { i1, i2 });
                        //}
                        //if (activatedFingerRight[0].Count > 0)
                        //{
                        //    HeartbeatHaptics(false, activatedFingerRight[0].ToArray(), false, new byte[] { i1, i2 });
                        //}

                        if (activatedFingerLeft[0].Count > 0)
                        {
                            HeartbeatHaptics(true, activatedFingerLeft[0].ToArray(), false, pres1);
                        }
                        if (activatedFingerRight[0].Count > 0)
                        {
                            HeartbeatHaptics(false, activatedFingerRight[0].ToArray(), false, pres1);
                        }

                        //byte[] clutchState = new byte[2] { 0x01, 0x02 };
                        //byte[] valveTiming = new byte[2] { 0, norI2 };
                        //Haptics.ApplyHapticsWithTiming(clutchState, valveTiming, "LargeRight", false);
                    }

                    break;
                case 3:
                    if (showVisualCue)
                    {
                        leftVentricleIndicatorMaterial.material.color = Color.Lerp(beatColor_Valley1, beatColor_Peak1,
                            (timeFrame - norT2 * oneCycle) / (norT3 * oneCycle - norT2 * oneCycle));
                        rightVentricleIndicatorMaterial.material.color = Color.Lerp(beatColor_Valley1, beatColor_Peak1,
                            (timeFrame - norT2 * oneCycle) / (norT3 * oneCycle - norT2 * oneCycle));
                    }

                    if (timeFrame >= (norT3 * oneCycle))
                    {
                        curStage = 4;
                        //timeFrame = 0;

                        //if (activatedFingerLeft[1].Count > 0)
                        //{
                        //    HeartbeatHaptics(true, activatedFingerLeft[1].ToArray(), true, new byte[] { i3, i4 });
                        //}
                        //if (activatedFingerRight[1].Count > 0)
                        //{
                        //    HeartbeatHaptics(false, activatedFingerRight[1].ToArray(), true, new byte[] { i3, i4 });
                        //}

                        if (activatedFingerLeft[1].Count > 0)
                        {
                            HeartbeatHaptics(true, activatedFingerLeft[1].ToArray(), true, pres2);
                        }
                        if (activatedFingerRight[1].Count > 0)
                        {
                            HeartbeatHaptics(false, activatedFingerRight[1].ToArray(), true, pres2);
                        }

                        //byte[] clutchState = new byte[2] { 0x01, 0x00 };
                        //byte[] valveTiming = new byte[2] { norI3, 0 };
                        //Haptics.ApplyHapticsWithTiming(clutchState, valveTiming, "LargeRight", false);
                    }

                    break;
                case 4:
                    if (showVisualCue)
                    {
                        leftVentricleIndicatorMaterial.material.color = Color.Lerp(beatColor_Peak1, beatColor_Valley1,
                            (timeFrame - norT3 * oneCycle) / (norT4 * oneCycle - norT3 * oneCycle));
                        rightVentricleIndicatorMaterial.material.color = Color.Lerp(beatColor_Peak1, beatColor_Valley1,
                            (timeFrame - norT3 * oneCycle) / (norT4 * oneCycle - norT3 * oneCycle));
                    }

                    if (timeFrame >= (norT4 * oneCycle))
                    {
                        curStage = 5;
                        //timeFrame = 0;

                        //if (activatedFingerLeft[1].Count > 0)
                        //{
                        //    HeartbeatHaptics(true, activatedFingerLeft[1].ToArray(), false, new byte[] { i3, i4 });
                        //}
                        //if (activatedFingerRight[1].Count > 0)
                        //{
                        //    HeartbeatHaptics(false, activatedFingerRight[1].ToArray(), false, new byte[] { i3, i4 });
                        //}

                        if (activatedFingerLeft[1].Count > 0)
                        {
                            HeartbeatHaptics(true, activatedFingerLeft[1].ToArray(), false, pres2);
                        }
                        if (activatedFingerRight[1].Count > 0)
                        {
                            HeartbeatHaptics(false, activatedFingerRight[1].ToArray(), false, pres2);
                        }

                        //byte[] clutchState = new byte[2] { 0x01, 0x02 };
                        //byte[] valveTiming = new byte[2] { 0, norI4 };
                        //Haptics.ApplyHapticsWithTiming(clutchState, valveTiming, "LargeRight", false);
                        beatHapticsIsApplied = false;
                    }

                    break;
                case 5:
                    if (timeFrame >= (norT5 * oneCycle))
                    {
                        curStage = 1;
                        timeFrame = 0;

                        cycleStart = false;
                    }

                    break;
            }

            timeFrame += Time.fixedDeltaTime * 1000;
        }
        else
        {
            if (cycleStart)
            {
                EndPulse();
            }

            if (!controlPanelIsActive)
            {
                ToggleControlPanel(true);
                controlPanelIsActive = true;
            }
            
        }

    }

}
