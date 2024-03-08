using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HaptGlove;
using TMPro;

public class PulsePalpation : MonoBehaviour
{
    public MeshRenderer pulseIndicatorMaterial;

    private float heartBeat_Hz = 1;
    private int oneCycle;

    //private float beatHitInterval = 0;
    //private float beatStayInterval = 0;
    //private float beatStayInterval_buf = 0;
    private System.Random rdm = new System.Random();
    private bool beatOn = false;
    private Color beatColorTransparent;
    private Color beatColor;

    private string whichHand;
    private HaptGloveHandler gloveHandler;

    public TMP_Text pulseRateText;
    public TMP_Text pulseIntensityText;
    private byte pulseIntensity;

    void Start()
    {
        oneCycle = (int)(1 / heartBeat_Hz * 1000);
        //beatHitInterval = (float)0.5 / heartBeat_Hz;
        //beatStayInterval = (float)300 / 1000; //s
        //beatStayInterval_buf = beatStayInterval;
        Debug.Log("Pulse rate: " + heartBeat_Hz);
    }


    private void OnTriggerEnter(Collider col)
    {
        if ((col.gameObject.layer == LayerMask.NameToLayer("GhostHand")) & (col.name.Contains("Index")))
        {
            gloveHandler = col.GetComponentInParent<HaptGloveHandler>();

            beatColorTransparent = new Color(171f/255f, 27f/255f, 27f/255f, 20f/255f);
            beatColor = new Color(171f/255f, 27f/255f, 27f/255f, 1);

            beatOn = true;
        }
    }
    private void OnTriggerExit(Collider col)
    {
        if ((col.gameObject.layer == LayerMask.NameToLayer("GhostHand")) & (col.name.Contains("Index")))
        {
            beatOn = false;
            //if (beatHapticsIsApplied)
            //{
                //byte[] clutchState = new byte[] { fingerID, 2 };
                //Haptics.ApplyHaptics(clutchState, targetPres);
                //Debug.Log("Beat Off: " + fingerID + "\t" + targetPres);

                //byte[][] clutchStates =
                //    {new byte[] {0, 2}, new byte[] {1, 2}, new byte[] {2, 2}, new byte[] {3, 2}, new byte[] {4, 2}};
                byte[][] clutchStates = {new byte[] {1, 2}, new byte[] { 2, 2 } };
                byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchStates, pulseIntensity, false);
                gloveHandler.BTSend(btData);

                beatHapticsIsApplied = false;
                //beatStayInterval_buf = beatStayInterval;

                pulseIndicatorMaterial.material.color = beatColorTransparent;
                curStage = 1;
            //}
        }
    }

    private bool beatHapticsIsApplied = false;
    private byte fingerID = 0;
    private byte targetPres = 20;

    /// 0 - t1, approaching, no haptics - t2, pressing, increasing pressure - t3. holding, keeping pressure
    /// - t4, releasing, decreasing pressure - t5, released, no haptics - one cycle
    private float norT1 = 200f / 1000f;
    private float norT2 = 30f / 1000f;
    private float norT3 = 200f / 1000f;
    private float norT4 = 200f / 1000f;
    private float norT5 = 370f / 1000f;
    private int curStage = 1;
    private float timeFrame = 0;

    private IEnumerator coroutine;

    void Update()
    {
        heartBeat_Hz = Convert.ToSingle(pulseRateText.text) / 60;
        oneCycle = (int)(1 / heartBeat_Hz * 1000);
        pulseIntensity = Convert.ToByte(pulseIntensityText.text);

        if (beatOn)
        {
            switch (curStage)
            {
                case 1:
                    pulseIndicatorMaterial.material.color = Color.Lerp(beatColorTransparent, beatColor, timeFrame/(norT1 * oneCycle));

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
                    if (timeFrame >= (norT2 * oneCycle))
                    {
                        curStage = 3;
                        timeFrame = 0;

                        //Debug.Log("curStage = 3");
                    }
                    break;
                case 3:
                    pulseIndicatorMaterial.material.color = Color.Lerp(beatColor, beatColorTransparent, 0.5f*timeFrame / (norT3 * oneCycle));

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
                    pulseIndicatorMaterial.material.color = Color.Lerp(beatColor, beatColorTransparent, 0.5f+timeFrame / (norT3 * oneCycle));

                    if (timeFrame >= (norT4 * oneCycle))
                    {
                        curStage = 5;
                        timeFrame = 0;

                        //Debug.Log("curStage = 5");
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

        IEnumerator LiverEdgeApplyHaptics(int milliseconds, bool isPressing)
        {
            //int bufTiming = 150;

            //byte integer = (byte)(milliseconds / bufTiming);
            //byte remainder = (byte)(milliseconds % bufTiming);

            byte[][] clutchStates;
            byte[] valveTiming = new byte[] { 255, 255 };

            if (isPressing)
            {
                clutchStates = new byte[][]{new byte[] {1, 0}, new byte[] {2, 0}};
                byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchStates, pulseIntensity, false);
                gloveHandler.BTSend(btData);
            }
            else
            {
                clutchStates = new byte[][]{new byte[] {1, 2}, new byte[] {2, 2}};
                byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchStates, pulseIntensity, false);
                gloveHandler.BTSend(btData);
            }

            yield return null;
            //for (int i = 0; i < integer; i++)
            //{
            //    Haptics.ApplyHapticsWithTiming(clutchStates, valveTiming, whichHand, false);
            //    yield return new WaitForSeconds((float)bufTiming / 1000);
            //}

            //if (remainder != 0)
            //{
            //    valveTiming = new byte[] { remainder, remainder };
            //    Haptics.ApplyHapticsWithTiming(clutchState, valveTiming, whichHand, false);
            //}
        }


    }
}
