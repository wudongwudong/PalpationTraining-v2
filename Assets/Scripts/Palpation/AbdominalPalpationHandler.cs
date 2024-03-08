using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Newtonsoft.Json.Bson;
using UnityEngine;
using HaptGlove;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine.Events;

public class AbdominalPalpationHandler : MonoBehaviour
{
    [SerializeField] private GameObject graspedObject;
    [SerializeField] private List<string> fingerLeftList = new List<string>();
    [SerializeField] private List<string> fingerRightList = new List<string>();
    [SerializeField] private GameObject targetHand;
    private HaptGloveHandler haptGloveHandler;
    private bool leftGrasped = false;
    [SerializeField] private bool rightGrasped = false;
    public FixedJoint fixedJoint;

    public UnityAction<HaptGloveHandler.HandType, HaptGloveHandler> onGrasped;
    public UnityAction<HaptGloveHandler.HandType, HaptGloveHandler> onReleased;

    private GameObject[] realFingertip;
    //private byte[] clutchState = new byte[2];
    [SerializeField] private byte tarPres = 30;
    private List<string> hapticsAppliedLeftList = new List<string>();
    [SerializeField] private List<string> hapticsAppliedRightList = new List<string>();
    [SerializeField] private List<string> trackerList = new List<string>();

    //Delay
    private int delayColEnterTime = 1000; //ms
    private int delayColExitTime = 1000; //ms
    private int[] hapticsApplyTick_L = new int[6];
    private int[] hapticsRemoveTick_L = new int[6];
    private bool[] hapticsDelayToApply_L = new bool[6];
    private bool[] hapticsDelayToRemove_L = new bool[65];
    [SerializeField] private int[] hapticsApplyTick_R = new int[6];
    [SerializeField] private int[] hapticsRemoveTick_R = new int[6];
    [SerializeField] private bool[] hapticsDelayToApply_R = new bool[6];
    [SerializeField] private bool[] hapticsDelayToRemove_R = new bool[6];

    [SerializeField] private TMP_Text pressure_Text;


    public void OnSliderChanged(SliderEventData eventData)
    {
        byte pres1Min = 10;
        byte pres1Max = 50;
        //byte pres2Min = 10;
        //byte pres2Max = 50;
        //byte hrMin = 40;
        //byte hrMax = 120;

        string sliderName = eventData.Slider.name;

        switch (sliderName)
        {
            case "Slider_I1":
                tarPres = (byte)(eventData.NewValue * (pres1Max - pres1Min) + pres1Min);
                pressure_Text.text = tarPres.ToString();
                break;
            //case "Slider_I2":
            //    pres2 = (byte)(eventData.NewValue * (pres2Max - pres2Min) + pres2Min);
            //    lowerThreshold_Text.text = pres2.ToString();
            //    break;
            //case "Slider_I3":
            //    heartBeat_Hz = (eventData.NewValue * (hrMax - hrMin) + hrMin) / 60;
            //    upperThreshold_Text.text = (heartBeat_Hz * 60).ToString();
            //    break;
            //case "Slider_I4":
            //    i4 = (byte)eventData.NewValue;
            //    i4_Text.text = i4.ToString();
            //    break;
        }
    }

    private void FixedUpdate()
    {
        if (haptGloveHandler != null)
        {
            if (haptGloveHandler.whichHand == HaptGloveHandler.HandType.Left)
            {
                
            }
            else if (haptGloveHandler.whichHand == HaptGloveHandler.HandType.Right)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (hapticsDelayToRemove_R[i])
                    {
                        if (Environment.TickCount > hapticsApplyTick_R[i] + delayColExitTime)
                        {
                            hapticsDelayToRemove_R[i] = false;

                            if (!fingerRightList.Contains(GetColliderName(i)))
                            {
                                byte[] clutchState = new byte[] { (byte)i, 0x02 };
                                byte[] btData = haptGloveHandler.haptics.ApplyHaptics(clutchState, tarPres, false);
                                haptGloveHandler.BTSend(btData);
                                Debug.Log("Delay Haptics removed to: " + clutchState[0] + " at " + tarPres);

                                hapticsAppliedRightList.Remove(GetColliderName(i));

                                hapticsRemoveTick_R[i] = Environment.TickCount;
                                hapticsDelayToApply_R[i] = true;
                            }
                        }
                    }

                    if (hapticsDelayToApply_R[i])
                    {
                        if (Environment.TickCount > hapticsRemoveTick_R[i] + delayColEnterTime)
                        {
                            hapticsDelayToApply_R[i] = false;

                            if (fingerRightList.Contains(GetColliderName(i)))
                            {
                                byte[] clutchState = new byte[] { (byte)i, 0x01 };
                                byte[] btData = haptGloveHandler.haptics.ApplyHaptics(clutchState, tarPres, false);
                                haptGloveHandler.BTSend(btData);
                                Debug.Log("Delay Haptics applied to: " + clutchState[0] + " at " + tarPres);
                                hapticsAppliedRightList.Add(GetColliderName(i));

                                hapticsApplyTick_R[i] = Environment.TickCount;
                                hapticsDelayToRemove_R[i] = true;
                            }
                        }
                    }

                }
            }
            

        }

        
    }


    void OnTriggerEnter(Collider col)
    {
        HaptGloveHandler gloveHandler = col.GetComponentInParent<HaptGloveHandler>();
        List<string> fingerList = new List<string>();
        List<string> hapticsAppliedList = new List<string>();
        bool isGrasped = false;
        byte[] clutchState = new byte[2];

        int[] hapticsApplyTick = new int[6];
        int[] hapticsRemoveTick = new int[6];
        bool[] hapticsDelayToApply = new bool[6];
        bool[] hapticsDelayToRemove = new bool[6];

        if (gloveHandler != null)
        {
            if (gloveHandler.whichHand == HaptGloveHandler.HandType.Left)
            {
                fingerList = fingerLeftList;
                hapticsAppliedList = hapticsAppliedLeftList;
                isGrasped = leftGrasped;

                hapticsApplyTick = hapticsApplyTick_L;
                hapticsRemoveTick = hapticsRemoveTick_L;
                hapticsDelayToApply = hapticsDelayToApply_L;
                hapticsDelayToRemove = hapticsDelayToRemove_L;
            }
            else if (gloveHandler.whichHand == HaptGloveHandler.HandType.Right)
            {
                fingerList = fingerRightList;
                hapticsAppliedList = hapticsAppliedRightList;
                isGrasped = rightGrasped;

                hapticsApplyTick = hapticsApplyTick_R;
                hapticsRemoveTick = hapticsRemoveTick_R;
                hapticsDelayToApply = hapticsDelayToApply_R;
                hapticsDelayToRemove = hapticsDelayToRemove_R;
            }

            targetHand = gloveHandler.gameObject;
            haptGloveHandler = gloveHandler;
            //Debug.Log(targetHand.name);
        }
        else
        {
            return;
        }


        switch (col.name)
        {
            case "GhostThumb":
                clutchState = new byte[2] { 0x00, 0x00 };
                break;
            case "GhostIndex":
                clutchState = new byte[2] { 0x01, 0x00 };
                break;
            case "GhostMiddle":
                clutchState = new byte[2] { 0x02, 0x00 };
                break;
            case "GhostRing":
                clutchState = new byte[2] { 0x03, 0x00 };
                break;
            //case "GhostPinky":
            //    clutchState = new byte[2] { 0x04, 0x00 };
            //    break;
            case "GhostPalm":
                clutchState = new byte[2] { 0x05, 0x00 };
                break;
            //case "R_PalmCollider":
            //    clutchState = new byte[2] { 0x05, 0x00 };
            //    break;

            case "L_PalmTracker":
                trackerList.Add(col.name);
                return;
            case "R_PalmTracker":
                trackerList.Add(col.name);
                return;
            default:
                return;
        }


        if (!fingerList.Contains(col.name))
        {
            if (!isGrasped | (!hapticsAppliedList.Contains("GhostThumb") & (col.name == "GhostThumb")))
            {
                //Delay haptics
                int? fingerID = GetFingerID(col.name);

                if (fingerID != null)
                {
                    if (!hapticsDelayToApply[(int)fingerID] & !hapticsAppliedList.Contains(col.name))
                    {
                        byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchState, tarPres, false);
                        gloveHandler.BTSend(btData);
                        Debug.Log("Haptics applied to: " + clutchState[0] + " at " + tarPres + ", Collider: " + col.name);
                        hapticsAppliedList.Add(col.name);

                        hapticsApplyTick[(int)fingerID] = Environment.TickCount;
                        hapticsDelayToRemove[(int)fingerID] = true;
                        
                    }
                }

            }
            
        }

        fingerList.Add(col.name);
        Debug.Log("FingerList add: " + col.name);

        //Create fixed joint
        if (fingerList.Contains("GhostIndex") & fingerList.Contains("GhostMiddle") & fingerList.Contains("GhostRing") & fingerList.Contains("GhostPalm"))
        {
            if (gloveHandler.whichHand == HaptGloveHandler.HandType.Left)
            {
                if (leftGrasped == false)
                {
                    leftGrasped = true;

                    if (onGrasped != null)
                    {
                        onGrasped(HaptGloveHandler.HandType.Left, gloveHandler);
                    }

                    if (targetHand.GetComponent<FixedJoint>() == null)
                    {
                        fixedJoint = targetHand.AddComponent<FixedJoint>();
                        fixedJoint.connectedBody = graspedObject.GetComponent<Rigidbody>();
                        fixedJoint.massScale = 1e-05f;
                        Debug.Log("Create fixed joint on left hand");

                        realFingertip = targetHand.GetComponent<Grasping>().realFingertip;
                        Debug.Log("Realfingertip: " + realFingertip.Length);
                        for (int i = 0; i < realFingertip.Length; i++)
                        {
                            realFingertip[i].GetComponent<Collider>().isTrigger = true;
                        }
                    }
                }
            }
            else if (gloveHandler.whichHand == HaptGloveHandler.HandType.Right)
            {
                if (rightGrasped == false)
                {
                    rightGrasped = true;

                    if (onGrasped != null)
                    {
                        onGrasped(HaptGloveHandler.HandType.Right, gloveHandler);
                    }

                    if (targetHand.GetComponent<FixedJoint>() == null)
                    {
                        fixedJoint = targetHand.AddComponent<FixedJoint>();
                        fixedJoint.connectedBody = graspedObject.GetComponent<Rigidbody>();
                        fixedJoint.massScale = 1e-05f;
                        Debug.Log("Create fixed joint on right hand");

                        realFingertip = targetHand.GetComponent<Grasping>().realFingertip;
                        Debug.Log("Realfingertip: " + realFingertip.Length);
                        for (int i = 0; i < realFingertip.Length; i++)
                        {
                            realFingertip[i].GetComponent<Collider>().isTrigger = true;
                        }
                    }
                }
            }
            
        }

    }

    void OnTriggerExit(Collider col)
    {
        HaptGloveHandler gloveHandler = col.GetComponentInParent<HaptGloveHandler>();
        List<string> fingerList = new List<string>();
        List<string> hapticsAppliedList = new List<string>();
        bool isGrasped = false;

        int[] hapticsApplyTick = new int[6];
        int[] hapticsRemoveTick = new int[6];
        bool[] hapticsDelayToApply = new bool[6];
        bool[] hapticsDelayToRemove = new bool[6];
        byte[] clutchState = new byte[2];

        if (gloveHandler != null)
        {
            if (gloveHandler.whichHand == HaptGloveHandler.HandType.Left)
            {
                fingerList = fingerLeftList;
                hapticsAppliedList = hapticsAppliedLeftList;
                isGrasped = leftGrasped;

                hapticsApplyTick = hapticsApplyTick_L;
                hapticsRemoveTick = hapticsRemoveTick_L;
                hapticsDelayToApply = hapticsDelayToApply_L;
                hapticsDelayToRemove = hapticsDelayToRemove_L;
            }
            else if (gloveHandler.whichHand == HaptGloveHandler.HandType.Right)
            {
                fingerList = fingerRightList;
                hapticsAppliedList = hapticsAppliedRightList;
                isGrasped = rightGrasped;

                hapticsApplyTick = hapticsApplyTick_R;
                hapticsRemoveTick = hapticsRemoveTick_R;
                hapticsDelayToApply = hapticsDelayToApply_R;
                hapticsDelayToRemove = hapticsDelayToRemove_R;
            }

            targetHand = gloveHandler.gameObject;
        }
        else
        {
            return;
        }


        switch (col.name)
        {
            case "GhostThumb":
                clutchState = new byte[2] { 0x00, 0x02 };
                break;
            case "GhostIndex":
                clutchState = new byte[2] { 0x01, 0x02 };
                break;
            case "GhostMiddle":
                clutchState = new byte[2] { 0x02, 0x02 };
                break;
            case "GhostRing":
                clutchState = new byte[2] { 0x03, 0x02 };
                break;
            //case "GhostPinky":
            //    clutchState = new byte[2] { 0x04, 0x02 };
            //    break;
            case "GhostPalm":
                clutchState = new byte[2] { 0x05, 0x02 };
                break;
            //case "R_PalmCollider":
            //    clutchState = new byte[2] { 0x05, 0x02 };
            //    break;

            case "L_PalmTracker":
                trackerList.Remove(col.name);
                break;
            case "R_PalmTracker":
                trackerList.Remove(col.name);
                break;

            default:
                return;
        }

        

        if (fingerList.Contains(col.name))
        {
            fingerList.Remove(col.name);
            Debug.Log("FingerList remove: " + col.name);

            if (!fingerList.Contains(col.name))
            {
                //Remove individual channel of haptics only when not grapsed
                if (!isGrasped)
                {
                    //Delay haptics
                    int? fingerID = GetFingerID(col.name);

                    if (fingerID != null)
                    {
                        if (!hapticsDelayToRemove[(int)fingerID] & hapticsAppliedList.Contains(col.name))
                        {
                            byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchState, tarPres, false);
                            gloveHandler.BTSend(btData);
                            Debug.Log("Haptics removed to: " + clutchState[0] + " at " + tarPres);

                            hapticsAppliedList.Remove(col.name);

                            hapticsRemoveTick[(int)fingerID] = Environment.TickCount;
                            hapticsDelayToApply[(int)fingerID] = true;
                        }
                    }
                }
            }
        }


        if (isGrasped)
        {
            if ((col.name == "L_PalmTracker") & (!trackerList.Contains(col.name)))
            {
                if (leftGrasped == true)
                {
                    //leftGrasped = false;

                    if (onReleased != null)
                    {
                        onReleased(HaptGloveHandler.HandType.Left, gloveHandler);
                    }

                    if (targetHand.GetComponent<FixedJoint>() != null)
                    {
                        Destroy(targetHand.GetComponent<FixedJoint>());

                        realFingertip = targetHand.GetComponent<Grasping>().realFingertip;
                        Debug.Log("Realfingertip: " + realFingertip.Length);
                        for (int i = 0; i < realFingertip.Length; i++)
                        {
                            realFingertip[i].GetComponent<Collider>().isTrigger = false;
                        }
                    }

                    Debug.Log("Destroy fixed joint on left hand");
                }
            }

            if ((col.name == "R_PalmTracker") & (!trackerList.Contains(col.name)))
            {
                if (rightGrasped == true)
                {
                    //rightGrasped = false;

                    if (onReleased != null)
                    {
                        onReleased(HaptGloveHandler.HandType.Right, gloveHandler);
                    }

                    if (targetHand.GetComponent<FixedJoint>() != null)
                    {
                        Destroy(targetHand.GetComponent<FixedJoint>());

                        realFingertip = targetHand.GetComponent<Grasping>().realFingertip;
                        Debug.Log("Realfingertip: " + realFingertip.Length);
                        for (int i = 0; i < realFingertip.Length; i++)
                        {
                            realFingertip[i].GetComponent<Collider>().isTrigger = false;
                        }
                    }

                    Debug.Log("Destroy fixed joint on right hand");
                }
            }

            //remove all haptics
            if (fingerList.Count == 0)
            {
                int len = hapticsAppliedList.Count;
                Debug.Log(len.ToString());
                int index = 0;
                byte[][] clutchStates = new byte[len][];

                foreach (var channel in hapticsAppliedList)
                {
                    switch (channel)
                    {
                        case "GhostThumb":
                            clutchStates[index] = new byte[2] { 0x00, 0x02 };
                            break;
                        case "GhostIndex":
                            clutchStates[index] = new byte[2] { 0x01, 0x02 };
                            break;
                        case "GhostMiddle":
                            clutchStates[index] = new byte[2] { 0x02, 0x02 };
                            break;
                        case "GhostRing":
                            clutchStates[index] = new byte[2] { 0x03, 0x02 };
                            break;
                        //case "GhostPinky":
                        //    clutchState = new byte[2] { 0x04, 0x00 };
                        //    break;
                        case "GhostPalm":
                            clutchStates[index] = new byte[2] { 0x05, 0x02 };
                            break;
                        //case "R_PalmCollider":
                        //    clutchStates[index] = new byte[2] { 0x05, 0x02 };
                        //    break;
                    }

                    index++;

                    hapticsDelayToRemove[(int)GetFingerID(channel)] = false;
                    hapticsDelayToApply[(int)GetFingerID(channel)] = true;
                    hapticsRemoveTick[(int)GetFingerID(channel)] = Environment.TickCount;
                }

                byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchStates, tarPres, false);
                gloveHandler.BTSend(btData);
                Debug.Log("Hand remove from patient after grasping");

                //Clear
                hapticsAppliedLeftList = new List<string>();
                hapticsAppliedRightList = new List<string>();

                if (gloveHandler.whichHand == HaptGloveHandler.HandType.Left)
                {
                    leftGrasped = false;
                }
                else if (gloveHandler.whichHand == HaptGloveHandler.HandType.Right)
                {
                    rightGrasped = false;
                }
                
            }
        }
        
    }

    private int? GetFingerID(string name)
    {
        int? fingerID = 0;

        switch (name)
        {
            case "GhostThumb":
                fingerID = 0;
                break;
            case "GhostIndex":
                fingerID = 1;
                break;
            case "GhostMiddle":
                fingerID = 2;
                break;
            case "GhostRing":
                fingerID = 3;
                break;
            //case "GhostPinky":
            //    fingerID = 4;
            //    break;
            case "GhostPalm":
                fingerID = 5;
                break;
            default:
                fingerID = null;
                Debug.Log("Invalid collider name: " + name);
                break;
        }

        return fingerID;
    }

    private string GetColliderName(int fingerID)
    {
        string name = "";

        switch (fingerID)
        {
            case 0:
                name = "GhostThumb";
                break;
            case 1:
                name = "GhostIndex";
                break;
            case 2:
                name = "GhostMiddle";
                break;
            case 3:
                name = "GhostRing";
                break;
            //case 4:
            //    name = "GhostPinky";
            //    break;
            case 5:
                name = "GhostPalm";
                break;
            default:
                name = null;
                Debug.Log("Invalid fingerID: " + fingerID);
                break;
        }

        return name;
    }


    //IEnumerator Delay(HaptGloveHandler gloveHandler, byte[] clutchState, List<string> fingerList, Collider col)
    //{
    //    yield return new WaitForSeconds(0.01f);

    //    fingerList.Remove(col.name);

    //    if (!fingerList.Contains(col.name))
    //    {
    //        byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchState, tarPres, false);
    //        gloveHandler.BTSend(btData);
    //        Debug.Log("Haptics removed to: " + clutchState[0] + " at " + tarPres);
    //    }
    //}

    //void Update()
    //{
    //    if (fingerLeftList.Contains("L_PalmCollider") & fingerLeftList.Contains("GhostIndex") & fingerLeftList.Contains("GhostMiddle") & fingerLeftList.Contains("GhostRing"))
    //    {
    //        if (leftGrasped == false)
    //        {
    //            leftGrasped = true;

    //            if (onGrasped != null)
    //            {
    //                onGrasped(HaptGloveHandler.HandType.Left);
    //            }

    //            if (targetHand.GetComponent<FixedJoint>() == null)
    //            {
    //                fixedJoint = targetHand.AddComponent<FixedJoint>();
    //                fixedJoint.connectedBody = graspedObject.GetComponent<Rigidbody>();
    //                fixedJoint.massScale = 1e-05f;
    //            }

    //        }
    //    }
    //    else
    //    {
    //        if (leftGrasped == true)
    //        {
    //            leftGrasped = false;

    //            if (onReleased != null)
    //            {
    //                onReleased(HaptGloveHandler.HandType.Left);
    //            }

    //            //Destroy(targetHand.GetComponent<FixedJoint>());

    //            if (targetHand.GetComponent<FixedJoint>() != null)
    //            {
    //                Destroy(targetHand.GetComponent<FixedJoint>());
    //            }

    //        }
    //    }

    //    if (fingerRightList.Contains("R_PalmCollider") & fingerRightList.Contains("GhostIndex") & fingerRightList.Contains("GhostMiddle") & fingerRightList.Contains("GhostRing"))
    //    {
    //        if (rightGrasped == false)
    //        {
    //            rightGrasped = true;

    //            if (onGrasped != null)
    //            {
    //                onGrasped(HaptGloveHandler.HandType.Right);
    //            }

    //            if (targetHand.GetComponent<FixedJoint>() == null)
    //            {
    //                fixedJoint = targetHand.AddComponent<FixedJoint>();
    //                fixedJoint.connectedBody = graspedObject.GetComponent<Rigidbody>();
    //                fixedJoint.massScale = 1e-05f;
    //                //fixedJoint = gameObject.AddComponent<FixedJoint>();
    //                //fixedJoint.connectedBody = targetHand.GetComponent<Rigidbody>();

    //                Debug.Log("Create fixed joint on right hand");
    //            }


    //        }
    //    }
    //    else
    //    {
    //        if (rightGrasped == true)
    //        {
    //            rightGrasped = false;
    //            //Destroy(targetHand.GetComponent<FixedJoint>());

    //            if (onReleased != null)
    //            {
    //                onReleased(HaptGloveHandler.HandType.Right);
    //            }

    //            if (targetHand.GetComponent<FixedJoint>() != null)
    //            {
    //                Destroy(targetHand.GetComponent<FixedJoint>());
    //            }

    //            Debug.Log("Destroy fixed joint on right hand");
    //        }
    //    }

    //}


    ////public TumorSpecs tumorSpecs;
    //private Grasping graspLeftScript;
    //private HaptGloveHandler gloveHandler;



    //private void OnTriggerEnter(Collider col)
    //{


    //    Debug.Log("Trigger enter" + col.name);
    //    if (col.gameObject.layer == LayerMask.NameToLayer("Tracker"))
    //    {
    //        switch (col.transform.parent.name)
    //        {
    //            case "ViveTracker_Left":
    //                graspLeftScript = GameObject.Find("Hands_Large").transform.Find("Generic Hand_Left").GetComponent<Grasping>();
    //                gloveHandler = GameObject.Find("Hands_Large").transform.Find("Generic Hand_Left").GetComponent<HaptGloveHandler>();
    //                Debug.Log("hahah");
    //                break;
    //            case "ViveTracker_Right":
    //                graspLeftScript = GameObject.Find("Hands_Large").transform.Find("Generic Hand_Right").GetComponent<Grasping>();
    //                gloveHandler = GameObject.Find("Hands_Large").transform.Find("Generic Hand_Right").GetComponent<HaptGloveHandler>();
    //                break;
    //            case "ViveTracker_Left_Medium":
    //                graspLeftScript = GameObject.Find("Hands_Medium").transform.Find("Generic Hand_Left").GetComponent<Grasping>();
    //                gloveHandler = GameObject.Find("Hands_Medium").transform.Find("Generic Hand_Left").GetComponent<HaptGloveHandler>();
    //                break;
    //            case "ViveTracker_Right_Medium":
    //                graspLeftScript = GameObject.Find("Hands_Medium").transform.Find("Generic Hand_Right").GetComponent<Grasping>();
    //                gloveHandler = GameObject.Find("Hands_Medium").transform.Find("Generic Hand_Right").GetComponent<HaptGloveHandler>();
    //                break;
    //            case "ViveTracker":
    //                graspLeftScript = GameObject.Find("Hand Tutor").GetComponent<Grasping>();
    //                gloveHandler = GameObject.Find("Hand Tutor").GetComponent<HaptGloveHandler>();
    //                break;
    //        }
    //    }
    //}

    //private void OnTriggerExit(Collider col)
    //{
    //    if (col.gameObject.layer == LayerMask.NameToLayer("Tracker"))
    //    {
    //        graspLeftScript.DropObject();

    //        byte[] clutchState = new byte[] { 0x00, 0x02 };
    //        //Haptics.ApplyHaptics(clutchState, 30, graspLeftScript.whichHand, true);
    //        byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchState, 30, true);
    //        gloveHandler.BTSend(btData);

    //        //tumorSpecs.ResetTumorSpecs();
    //    }
    //}
}
