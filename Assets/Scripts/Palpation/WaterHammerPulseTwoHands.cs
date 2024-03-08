using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HaptGlove;
public class WaterHammerPulseTwoHands : MonoBehaviour
{
    public bool showVisualCue = true;
    public MeshRenderer radialIndicatorMaterial;
    public MeshRenderer brachialIndicatorMaterial;

    private float heartBeat_Hz = 1f;
    private int oneCycle;

    public bool beatOn = false;
    public bool radialBeatOn = false;
    public bool brachialBeatOn = false;
    private Color radialPeakHighColor;
    private Color radialValleyHighColor;
    private Color radialPeakLowColor;
    private Color radialValleyLowColor;

    private Color brachialPeakHighColor;
    private Color brachialValleyHighColor;
    private Color brachialPeakLowColor;
    private Color brachialValleyLowColor;

    private HaptGloveHandler radialPulseHand;
    private HaptGloveHandler brachialPulseHand;
    public bool[] radialFingerIDs = new bool[5];
    public bool[] brachialFingerIDs = new bool[5];

    //private byte[][] clutchStates = new byte[5][];
    public byte[][] radialValveTimings = new byte[5][];
    public byte[][] brachialValveTimings = new byte[5][];

    private int noFingersRadial = 0;
    private int noFingersBrachial = 0;
    private bool beatHapticsIsApplied = false;


    void Start()
    {
        oneCycle = (int)(1 / heartBeat_Hz * 1000);
        Debug.Log("Pulse interval: " + oneCycle);

        UpdatePulsePara(0);//Healthy

        //radialValleyLowColor = new Color(171f / 255f, 27f / 255f, 27f / 255f, 20f / 255f);
        //radialPeakHighColor = new Color(171f / 255f, 27f / 255f, 27f / 255f, 1);
        //brachialValleyLowColor = new Color(171f / 255f, 27f / 255f, 27f / 255f, 20f / 255f);
        //brachialPeakHighColor = new Color(171f / 255f, 27f / 255f, 27f / 255f, 1);
    }

    //public void StartPulse(string whitchHandBuf, byte fingerID, byte pulseStrength)
    //{
    //    whichHand = whitchHandBuf;
    //    fingerIDs[fingerID] = true;

    //    valveTimings[fingerID] = DecodePulseStrength(pulseStrength);
    //    //clutchStates[fingerID] = new byte[] { fingerID, 0 };

    //    beatOn = true;
    //}

    public void UpdateRadialPulse(HaptGloveHandler gloveHandler, byte fingerID, byte pulseStrength, bool isApplyPulse)
    {
        radialBeatOn = isApplyPulse;
        if (isApplyPulse)
        {
            radialPulseHand = gloveHandler;
            radialFingerIDs[fingerID] = true;
            radialValveTimings[fingerID] = DecodePulseStrength(pulseStrength);
        }
        else
        {
            radialPulseHand = null;
            radialFingerIDs[fingerID] = false;
            radialValveTimings[fingerID] = new byte[] { 0, 0 };

            if (beatHapticsIsApplied)
            {
                byte[][] clutchStates = { new byte[] { 1, 2 }, new byte[] { 2, 2 } };
                //Haptics.ApplyHaptics(clutchStates, 30, radialPulseHand, false);
                byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchStates, 30, false);
                gloveHandler.BTSend(btData);
            }
        }
    }

    public void UpdateBrachialPulse(HaptGloveHandler gloveHandler, byte fingerID, byte pulseStrength, bool isApplyPulse)
    {
        brachialBeatOn = isApplyPulse;
        if (isApplyPulse)
        {
            brachialPulseHand = gloveHandler;
            brachialFingerIDs[fingerID] = true;
            brachialValveTimings[fingerID] = DecodePulseStrength(pulseStrength);
        }
        else
        {
            brachialPulseHand = null;
            brachialFingerIDs[fingerID] = false;
            brachialValveTimings[fingerID] = new byte[] { 0, 0 };

            if (beatHapticsIsApplied)
            {
                byte[][] clutchStates = { new byte[] { 1, 2 }, new byte[] { 2, 2 } };
                //Haptics.ApplyHaptics(clutchStates, 30, brachialPulseHand, false);
                byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchStates, 30, false);
                gloveHandler.BTSend(btData);
            }
        }

        //Debug.Log(BitConverter.ToString(brachialValveTimings[1]));
    }

    public void UpdateRadialPulseColor(Color peakHighColor, Color valleyHighCollor, Color peakLowColor, Color valleyLowCollor)
    {
        radialPeakHighColor = peakHighColor;
        radialValleyHighColor = valleyHighCollor;
        radialPeakLowColor = peakLowColor;
        radialValleyLowColor = valleyLowCollor;
    }
    public void UpdateBrachialPulseColor(Color peakHighColor, Color valleyHighCollor, Color peakLowColor, Color valleyLowCollor)
    {
        brachialPeakHighColor = peakHighColor;
        brachialValleyHighColor = valleyHighCollor;
        brachialPeakLowColor = peakLowColor;
        brachialValleyLowColor = valleyLowCollor;
    }

    //public void UpdatePulse(byte fingerID, byte pulseStrength, bool isApplyPulse)
    //{
    //    if (isApplyPulse)
    //    {
    //        fingerIDs[fingerID] = true;
    //        valveTimings[fingerID] = DecodePulseStrength(pulseStrength);
    //        //clutchStates[fingerID] = new byte[] { fingerID, 0 };
    //    }
    //    else
    //    {
    //        fingerIDs[fingerID] = false;
    //        valveTimings[fingerID] = new byte[] { 0, 0 };
    //    }
    //}

    public void EndPulse()
    {
        Debug.Log("End pulse");
        beatOn = false;
        radialFingerIDs = new bool[5];
        brachialFingerIDs = new bool[5];

        if (beatHapticsIsApplied)
        {
            beatHapticsIsApplied = false;
        }

        radialIndicatorMaterial.material.color = radialValleyLowColor; 
        brachialIndicatorMaterial.material.color = brachialValleyLowColor;
        
        curStage = 1;
    }

    public void UpdatePulseFrequency(float frequency)
    {
        heartBeat_Hz = frequency;
        oneCycle = (int)(1 / heartBeat_Hz * 1000);
        Debug.Log("Pulse interval updated: " + oneCycle);
    }

    public float GetPulseFrequency()
    {
        return heartBeat_Hz;
    }

    public int GetCurrentState()
    {
        return curStage;
    }

    public void UpdatePulsePara(byte status)
    {
        // 0 - t1, approaching, no haptics - t2, pressing, increasing pressure - t3. holding, keeping pressure
        // - t4, releasing, decreasing pressure - t5, released, no haptics - one cycle
        switch (status)
        {
            case 0:                         //healthy
                norT1 = 200f / 1000f;
                norT2 = 150f / 1000f;
                norT3 = 100f / 1000f;
                norT4 = 100f / 1000f;
                norT5 = 450f / 1000f;
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
                valveTiming = new byte[] { 3, 3 };//weak
                break;
            case 1:
                valveTiming = new byte[] { 8, 5 };//normal
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

    private byte norI1 = 8;
    private byte norI2 = 4;
    private byte norI3 = 2;
    private byte norI4 = 4;

    private IEnumerator coroutine;

    void Update()
    {
        if (beatOn)
        {
            //int bufRadial = 0;
            //int bufBrachial = 0;
            //for (int i = 0; i < 5; i++)
            //{
            //    if (radialFingerIDs[i] == true)
            //    {
            //        bufRadial++;
            //    }

            //    if (brachialFingerIDs[i] == true)
            //    {
            //        bufBrachial++;
            //    }
            //}
            //noFingersRadial = bufRadial;
            //noFingersBrachial = bufBrachial;


            //if ((noFingersRadial <= 0)&(noFingersBrachial <= 0)) { return; }

            switch (curStage)
            {
                case 1:
                    if (showVisualCue)
                    {
                        radialIndicatorMaterial.material.color = Color.Lerp(radialValleyLowColor, radialPeakHighColor, timeFrame / (norT1 * oneCycle));
                        brachialIndicatorMaterial.material.color = Color.Lerp(brachialValleyLowColor, brachialPeakHighColor, timeFrame / (norT1 * oneCycle));
                    }

                    if (timeFrame >= (norT1 * oneCycle))
                    {
                        curStage = 2;
                        timeFrame = 0;

                        //Debug.Log("curStage = 2");
                        //ApplyPulseHaptics(norI1, true);
                        ApplyPulseHaptics(radialValveTimings[1], brachialValveTimings[1], true);
                        beatHapticsIsApplied = true;
                    }
                    break;
                case 2:
                    //if (showVisualCue)
                    //{
                    //    radialIndicatorMaterial.material.color = Color.Lerp(radialPeakHighColor, radialValleyHighColor, timeFrame / (norT2 * oneCycle));
                    //    brachialIndicatorMaterial.material.color = Color.Lerp(brachialPeakHighColor, brachialValleyHighColor, timeFrame / (norT2 * oneCycle));
                    //}

                    if (timeFrame >= (norT2 * oneCycle))
                    {
                        curStage = 3;
                        timeFrame = 0;

                        //Debug.Log("curStage = 3");
                        //ApplyPulseHaptics(norI2, false);
                    }
                    break;
                case 3:
                    //if (showVisualCue)
                    //{
                    //    radialIndicatorMaterial.material.color = Color.Lerp(radialValleyHighColor, radialPeakLowColor, timeFrame / (norT3 * oneCycle));
                    //    brachialIndicatorMaterial.material.color = Color.Lerp(brachialValleyHighColor, brachialPeakLowColor, timeFrame / (norT3 * oneCycle));
                    //}

                    if (timeFrame >= (norT3 * oneCycle))
                    {
                        curStage = 4;
                        timeFrame = 0;

                        //Debug.Log("curStage = 4");
                        //ApplyPulseHaptics(norI3, true);
                    }
                    break;
                case 4:
                    if (showVisualCue)
                    {
                        radialIndicatorMaterial.material.color = Color.Lerp(radialPeakHighColor, radialValleyLowColor, timeFrame / (norT4 * oneCycle));
                        brachialIndicatorMaterial.material.color = Color.Lerp(brachialPeakHighColor, brachialValleyLowColor, timeFrame / (norT4 * oneCycle));
                    }

                    if (timeFrame >= (norT4 * oneCycle))
                    {
                        curStage = 5;
                        timeFrame = 0;

                        //Debug.Log("curStage = 5");
                        //ApplyPulseHaptics(norI4, false);
                        ApplyPulseHaptics(radialValveTimings[1],brachialValveTimings[1], false);
                        beatHapticsIsApplied = false;
                    }
                    break;
                case 5:
                    if (timeFrame >= (norT5 * oneCycle))
                    {
                        curStage = 1;
                        timeFrame = 0;

                        //Debug.Log("curStage = 1");
                    }
                    break;
            }

            timeFrame += Time.deltaTime * 1000;

        }

    }

    private void ApplyPulseHaptics(byte[] valveTimingRadial, byte[] valveTimingBrachial, bool isApply)
    {
        byte[] clutchState;
        if (isApply)
        {
            clutchState = new byte[2] { 0x01, 0x00 };
        }
        else
        {
            clutchState = new byte[2] { 0x01, 0x02 };
        }


        if (radialBeatOn)
        {
            //Haptics.ApplyHapticsWithTiming(clutchState, valveTimingRadial, radialPulseHand, false);
            byte[] btData = radialPulseHand.haptics.ApplyHapticsWithTiming(clutchState, valveTimingRadial, false);
            radialPulseHand.BTSend(btData);
        }
        if (brachialBeatOn)
        {
            //Haptics.ApplyHapticsWithTiming(clutchState, valveTimingBrachial, brachialPulseHand, false);
            byte[] btData = brachialPulseHand.haptics.ApplyHapticsWithTiming(clutchState, valveTimingBrachial, false);
            brachialPulseHand.BTSend(btData);
        }
    }

    private void ApplyPulseHaptics(byte intensity, bool isApply)
    {
        byte[] clutchState;
        byte[] valveTiming;
        if (isApply)
        {
            clutchState = new byte[2] { 0x01, 0x00 };
            valveTiming = new byte[2] { intensity, 0 };
        }
        else
        {
            clutchState = new byte[2] { 0x01, 0x02 };
            valveTiming = new byte[2] { 0, intensity };
        }
        

        if (radialBeatOn)
        {
            //Haptics.ApplyHapticsWithTiming(clutchState, valveTiming, radialPulseHand, false);
            byte[] btData = radialPulseHand.haptics.ApplyHapticsWithTiming(clutchState, valveTiming, false);
            radialPulseHand.BTSend(btData);
        }
        if (brachialBeatOn)
        {
            //Haptics.ApplyHapticsWithTiming(clutchState, valveTiming, brachialPulseHand, false);
            byte[] btData = brachialPulseHand.haptics.ApplyHapticsWithTiming(clutchState, valveTiming, false);
            brachialPulseHand.BTSend(btData);
        }
    }

    //IEnumerator ApplyPulseHaptics(bool isPressing)
    //{
    //    byte[][] radialClutchStatesBuf = new byte[noFingersRadial][];
    //    byte[][] brachialClutchStatesBuf = new byte[noFingersBrachial][];
    //    byte[][] radialValveTimingsBuf = new byte[noFingersRadial][];
    //    byte[][] brachialValveTimingsBuf = new byte[noFingersBrachial][];

    //    byte radialIndex = 0;
    //    byte brachialIndex = 0;

    //    if (isPressing)
    //    {
    //        for (byte i = 0; i < 5; i++)
    //        {
    //            if (radialFingerIDs[i] == true)
    //            {
    //                radialClutchStatesBuf[radialIndex] = new byte[] { i, 0 };
    //                radialValveTimingsBuf[radialIndex] = radialValveTimings[i];
    //                radialIndex++;
    //            }

    //            if (brachialFingerIDs[i] == true)
    //            {
    //                brachialClutchStatesBuf[brachialIndex] = new byte[] { i, 0 };
    //                brachialValveTimingsBuf[brachialIndex] = brachialValveTimings[i];
    //                brachialIndex++;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        for (byte i = 0; i < 5; i++)
    //        {
    //            if (radialFingerIDs[i] == true)
    //            {
    //                radialClutchStatesBuf[radialIndex] = new byte[] { i, 2 };
    //                radialValveTimingsBuf[radialIndex] = radialValveTimings[i];
    //                radialIndex++;
    //            }

    //            if (brachialFingerIDs[i] == true)
    //            {
    //                brachialClutchStatesBuf[brachialIndex] = new byte[] { i, 2 };
    //                brachialValveTimingsBuf[brachialIndex] = brachialValveTimings[i];
    //                brachialIndex++;
    //            }
    //        }
    //    }

    //    if (radialBeatOn)
    //    {
    //        Haptics.ApplyHapticsWithTiming(radialClutchStatesBuf, radialValveTimingsBuf, radialPulseHand, false);
    //    }

    //    if (brachialBeatOn)
    //    {
    //        Haptics.ApplyHapticsWithTiming(brachialClutchStatesBuf, brachialValveTimingsBuf, brachialPulseHand, false);
    //    }

    //    yield return null;

    //}
}
