using System;
using System.Collections;
using System.Collections.Generic;
using HaptGlove;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LiverEdgeHaptics : MonoBehaviour
{
    [SerializeField] private AbdominalPalpationHandler abdominalPalpationHandler;

    public bool showVisualCue = true;
    public TMP_Text logText;
    public GameObject liver;
    private Vector3 liverIniPosition;
    public Vector3 liverDisplacement = new Vector3(0f, -0.02f, 0f);

    public float breath_Hz = 0.2f;
    public int oneCycle;

    private bool beatOn = false;

    private HaptGloveHandler gloveHandler;

    public int[] palpateStartFingerPos = new int[5];
    public int[] palpateCurFingerPos = new int[5];
    public int liverEdgePalpateThreshold = 100;  /////////////
    public int curForce = 0;
    private bool isInLiverRegion = false;
    private bool isGrasped = false;

    [SerializeField] private TMP_Text lowerThreshold_Text;
    [SerializeField] private TMP_Text upperThreshold_Text;
    [SerializeField] private TMP_Text breathRate_Text;

    public enum PressingState
    {
        None,
        Light,
        Normal,
        Hard
    }

    public PressingState pressingState = PressingState.None;

    private int[] pressingThreshold = new int[3] {10, 100, 200};

    //GPT
    private HoloLensClient hololensClient;

    void Start()
    {
        oneCycle = (int)(1 / breath_Hz * 1000);

        liverIniPosition = liver.transform.localPosition;

        GameObject gptObject = GameObject.Find("GPTHandler");
        if (gptObject != null)
        {
            hololensClient = gptObject.GetComponent<HoloLensClient>();
        }

        abdominalPalpationHandler.onGrasped += OnGraspedPatient;
        abdominalPalpationHandler.onReleased += OnReleasedPatient;
    }

    private void OnGraspedPatient(HaptGloveHandler.HandType hand, HaptGloveHandler handler)
    {
        isGrasped = true;

        gloveHandler = handler;

        if (gloveHandler != null)
        {
            //palpateStartFingerPos = gloveHandler.GetFingerPosition();
            gloveHandler.GetFingerPosition().CopyTo(palpateStartFingerPos, 0);
        }
        
    }
    private void OnReleasedPatient(HaptGloveHandler.HandType hand, HaptGloveHandler handler)
    {
        isGrasped = false;
    }

    public void OnSliderChanged(SliderEventData eventData)
    {
        int lowerMin = 1000;
        int lowerMax = 2000;
        int UpperMin = 1600;
        int upperMax = 2600;
        int breathMin = 10;
        int breathMax = 20;

        string sliderName = eventData.Slider.name;

        switch (sliderName)
        {
            //case "Slider_I1":
            //    tarPres = (byte)(eventData.NewValue * (pres1Max - pres1Min) + pres1Min);
            //    pressure_Text.text = tarPres.ToString();
            //    break;
            case "Slider_I2":
                pressingThreshold[1] = (int)(eventData.NewValue * (lowerMax - lowerMin) + lowerMin);
                lowerThreshold_Text.text = pressingThreshold[1].ToString();
                break;
            case "Slider_I3":
                pressingThreshold[2] = (int)(eventData.NewValue * (upperMax - UpperMin) + UpperMin);
                upperThreshold_Text.text = pressingThreshold[2].ToString();
                break;
            case "Slider_I4":
                breath_Hz = (eventData.NewValue * (breathMax - breathMin) + breathMin) / 60;
                oneCycle = (int)(1 / breath_Hz * 1000);
                breathRate_Text.text = (breath_Hz * 60).ToString();
                break;
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "R_IndexDistal")
        {
            gloveHandler = collider.GetComponentInParent<HaptGloveHandler>();
            //palpateStartFingerPos = gloveHandler.GetFingerPosition();
            gloveHandler.GetFingerPosition().CopyTo(palpateStartFingerPos, 0);

            isInLiverRegion = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.name == "R_IndexDistal")
        {
            StartCoroutine("DelayFunc");

            isInLiverRegion = false;

            if ((curStage>=2) | (curStage <=4)) //pressure applied
            {
                coroutine = LiverEdgeApplyHaptics((int)(norT4 * oneCycle), false);
                StartCoroutine(coroutine);
            }
            curForce = 0;
            curStage = 1;
            timeFrame = 0;
            liver.transform.localPosition = liverIniPosition;

            logText.text = "Training";
        }
    }

    /// 0 - t1, approaching, no haptics - t2, pressing, increasing pressure - t3. holding, keeping pressure
    /// - t4, releasing, decreasing pressure - t5, released, no haptics - one cycle - t6, rest, no haptics
    private float norT1 = (float)1/6;
    private float norT2 = (float)1/6;
    private float norT3 = (float)1/6;
    private float norT4 = (float)1/6;
    private float norT5 = (float)1/6;
    private float norT6 = (float)1/6;
    private int curStage = 1;
    private float timeFrame = 0;

    private IEnumerator coroutine;

    [SerializeField] TMP_Text debugTMP;

    void Update()
    {
        if (isGrasped)
        {
            int[] airPressure = gloveHandler.GetAirPressure();
            debugTMP.text = airPressure[1] + ", " + airPressure[2] + ", " + airPressure[3] + ",\n" + palpateCurFingerPos[1] + "\n" + palpateCurFingerPos[2] + "\n" + palpateCurFingerPos[3] + "\n\n" + curForce;


            palpateCurFingerPos = gloveHandler.GetFingerPosition();

            curForce = (palpateCurFingerPos[1] + palpateCurFingerPos[2] + palpateCurFingerPos[3]) - (palpateStartFingerPos[1] + palpateStartFingerPos[2] + palpateStartFingerPos[3]);

            if (curForce <= pressingThreshold[0])
            {
                if (pressingState != PressingState.None)
                {
                    pressingState = PressingState.None;
                    
                    if (isInLiverRegion)
                        beatOn = false;

                    logText.text = "PressingState: None";
                }
            }
            else if (curForce <= pressingThreshold[1])
            {
                if (pressingState != PressingState.Light)
                {
                    pressingState = PressingState.Light;

                    if (isInLiverRegion)
                        beatOn = false;

                    logText.text = "PressingState: Light";
#if !UNITY_EDITOR
                    hololensClient.SendForceDetectedMessage(HoloLensClient.forceLevel.small);
#endif
                }
            }
            else if (curForce <= pressingThreshold[2])
            {
                if (pressingState != PressingState.Normal)
                {
                    pressingState = PressingState.Normal;

                    if (isInLiverRegion) 
                        beatOn = true;

                    logText.text = "PressingState: Normal";
#if !UNITY_EDITOR
                    hololensClient.SendForceDetectedMessage(HoloLensClient.forceLevel.medium);
#endif
                }
            }
            else if (curForce > pressingThreshold[2])
            {
                if (pressingState != PressingState.Hard)
                {
                    pressingState = PressingState.Hard;

                    if (isInLiverRegion) 
                        beatOn = true;

                    logText.text = "PressingState: Hard";
#if !UNITY_EDITOR
                    hololensClient.SendForceDetectedMessage(HoloLensClient.forceLevel.large);
#endif
                }
            }


            //if (curForce >= liverEdgePalpateThreshold)
            //{
            //    beatOn = true;
            //    logText.text = "Liver palpation correct";
            //}
            //else { beatOn = false; logText.text = "Press harder to feel the liver edge"; }

            //beatOn = true;



            if (beatOn)
            {
                if (showVisualCue)
                {
                    liver.SetActive(true);
                    //liver.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Models/Liver/Materials/Liver");
                }
                else
                {
                    liver.SetActive(false);
                    //liver.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Transparent");
                }
                switch (curStage)
                {
                    case 1:
                        liver.transform.localPosition = Vector3.Lerp(liverIniPosition, liverIniPosition + liverDisplacement,
                            timeFrame / ((norT1 + norT2) * oneCycle));

                        if (timeFrame >= (norT1 * oneCycle))
                        {
                            curStage = 2;
                            timeFrame = 0;

                            //Debug.Log("curStage = 2");
                            coroutine = LiverEdgeApplyHaptics((int)(norT2 * oneCycle), true);
                            StartCoroutine(coroutine);
                        }
                        break;
                    case 2:
                        liver.transform.localPosition = Vector3.Lerp(liverIniPosition, liverIniPosition + liverDisplacement,
                            norT1/(norT1 + norT2) + timeFrame / ((norT1 + norT2) * oneCycle));

                        if (timeFrame >= (norT2 * oneCycle))
                        {
                            curStage = 3;
                            timeFrame = 0;

                            //Debug.Log("curStage = 3");
                        }
                        break;
                    case 3:
                        if (timeFrame >= (norT3 * oneCycle))
                        {
                            curStage = 4;
                            timeFrame = 0;

                            //Debug.Log("curStage = 4");
                            coroutine = LiverEdgeApplyHaptics((int)(norT4 * oneCycle), false);
                            StartCoroutine(coroutine);
                        }
                        break;
                    case 4:
                        liver.transform.localPosition = Vector3.Lerp(liverIniPosition + liverDisplacement, liverIniPosition,
                            timeFrame / ((norT4 + norT5) * oneCycle));

                        if (timeFrame >= (norT4 * oneCycle))
                        {
                            curStage = 5;
                            timeFrame = 0;

                            //Debug.Log("curStage = 5");
                        }
                        break;
                    case 5:
                        liver.transform.localPosition = Vector3.Lerp(liverIniPosition + liverDisplacement, liverIniPosition,
                            norT4/(norT4 + norT5) + timeFrame / ((norT4 + norT5) * oneCycle));

                        if (timeFrame >= (norT5 * oneCycle))
                        {
                            curStage = 6;
                            timeFrame = 0;

                            //Debug.Log("curStage = 6");
                        }
                        break;
                    case 6:
                        if (timeFrame >= (norT6 * oneCycle))
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
        
    }

    IEnumerator LiverEdgeApplyHaptics(int milliseconds, bool isPressing)
    {
        int bufTiming = 150;

        byte integer = (byte)(milliseconds / bufTiming);
        byte remainder = (byte)(milliseconds % bufTiming);

        byte[] clutchState;
        byte[] valveTiming = new byte[] { 255, 255 };

        if (isPressing)
        {
            clutchState = new byte[] { 4, 0 };
        }
        else
        {
            clutchState = new byte[] { 4, 2 };
        }

        for (int i = 0; i < integer; i++)
        {
            //Haptics.ApplyHapticsWithTiming(clutchState, valveTiming, whichHand, false);
            byte[] btData = gloveHandler.haptics.ApplyHapticsWithTiming(clutchState, valveTiming, false);
            gloveHandler.BTSend(btData);
            yield return new WaitForSeconds((float)bufTiming / 1000);
        }

        if (remainder != 0)
        {
            valveTiming = new byte[] { remainder, remainder };
            //Haptics.ApplyHapticsWithTiming(clutchState, valveTiming, whichHand, false);
            byte[] btData = gloveHandler.haptics.ApplyHapticsWithTiming(clutchState, valveTiming, false);
            gloveHandler.BTSend(btData);
        }
    }

    IEnumerator DelayFunc()
    {
        gameObject.GetComponent<MeshCollider>().enabled = false;
        yield return new WaitForSeconds(1f);
        gameObject.GetComponent<MeshCollider>().enabled = true;
    }
}
