using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using TMPro;


public class PhysicsHandTracking : MonoBehaviour
{
    public enum HandType
    {
        Left,
        Right
    };
    public HandType handType;
    private string hand;
    private string hand_Short;

    private Transform[] followingJoints = new Transform[26];
    private Transform[] bufJoints = new Transform[26];

    private Vector3 targePosition = new Vector3();
    private Quaternion targeRotation = new Quaternion();
    private Rigidbody rb;
    MixedRealityPose pose;

    private Handedness handedness;
    //public bool leftHand;
    [HideInInspector] public Vector3 rotOffsetPalm = Vector3.zero;//new Vector3(0, -90, 180);
    [HideInInspector] public Vector3 rotOffsetFinger = Vector3.zero; //new Vector3(180, 90, 0);

    private Transform handTracker;

    //public TMP_Text logText, logText2;

    void Start()
    {
        if (handType == HandType.Left)
        {
            handedness = Handedness.Left;
            hand = "Left";
            hand_Short = "L";
        }
        else
        {
            handedness = Handedness.Right;
            hand = "Right";
            hand_Short = "R";

            rotOffsetPalm = -rotOffsetPalm;
            rotOffsetFinger = -rotOffsetFinger;
        }
        rb = GetComponent<Rigidbody>();

        followingJoints[0] = transform.Find(hand_Short + "_Wrist/" + hand_Short + "_ThumbMetacarpal");
        followingJoints[1] = followingJoints[0].GetChild(0);
        followingJoints[2] = followingJoints[1].GetChild(0);
        followingJoints[3] = followingJoints[2].GetChild(0);

        followingJoints[4] = transform.Find(hand_Short + "_Wrist/" + hand_Short + "_IndexMetacarpal");
        followingJoints[5] = followingJoints[4].GetChild(0);
        followingJoints[6] = followingJoints[5].GetChild(0);
        followingJoints[7] = followingJoints[6].GetChild(0);
        followingJoints[8] = followingJoints[7].GetChild(0);

        followingJoints[9] = transform.Find(hand_Short + "_Wrist/" + hand_Short + "_MiddleMetacarpal");
        followingJoints[10] = followingJoints[9].GetChild(0);
        followingJoints[11] = followingJoints[10].GetChild(0);
        followingJoints[12] = followingJoints[11].GetChild(0);
        followingJoints[13] = followingJoints[12].GetChild(0);

        followingJoints[14] = transform.Find(hand_Short + "_Wrist/" + hand_Short + "_RingMetacarpal");
        followingJoints[15] = followingJoints[14].GetChild(0);
        followingJoints[16] = followingJoints[15].GetChild(0);
        followingJoints[17] = followingJoints[16].GetChild(0);
        followingJoints[18] = followingJoints[17].GetChild(0);

        followingJoints[19] = transform.Find(hand_Short + "_Wrist/" + hand_Short + "_LittleMetacarpal");
        followingJoints[20] = followingJoints[19].GetChild(0);
        followingJoints[21] = followingJoints[20].GetChild(0);
        followingJoints[22] = followingJoints[21].GetChild(0);
        followingJoints[23] = followingJoints[22].GetChild(0);

        followingJoints[24] = transform.Find(hand_Short + "_Wrist/" + hand_Short + "_Palm");
        followingJoints[25] = transform.Find(hand_Short + "_Wrist/");

        ////////////////////////////////
        bufJoints[0] = transform.Find(hand_Short + "_Wrist_Ghost/" + hand_Short + "_ThumbMetacarpal");
        bufJoints[1] = bufJoints[0].GetChild(0);
        bufJoints[2] = bufJoints[1].GetChild(0);
        bufJoints[3] = bufJoints[2].GetChild(0);

        bufJoints[4] = transform.Find(hand_Short + "_Wrist_Ghost/" + hand_Short + "_IndexMetacarpal");
        bufJoints[5] = bufJoints[4].GetChild(0);
        bufJoints[6] = bufJoints[5].GetChild(0);
        bufJoints[7] = bufJoints[6].GetChild(0);
        bufJoints[8] = bufJoints[7].GetChild(0);

        bufJoints[9] = transform.Find(hand_Short + "_Wrist_Ghost/" + hand_Short + "_MiddleMetacarpal");
        bufJoints[10] = bufJoints[9].GetChild(0);
        bufJoints[11] = bufJoints[10].GetChild(0);
        bufJoints[12] = bufJoints[11].GetChild(0);
        bufJoints[13] = bufJoints[12].GetChild(0);

        bufJoints[14] = transform.Find(hand_Short + "_Wrist_Ghost/" + hand_Short + "_RingMetacarpal");
        bufJoints[15] = bufJoints[14].GetChild(0);
        bufJoints[16] = bufJoints[15].GetChild(0);
        bufJoints[17] = bufJoints[16].GetChild(0);
        bufJoints[18] = bufJoints[17].GetChild(0);

        bufJoints[19] = transform.Find(hand_Short + "_Wrist_Ghost/" + hand_Short + "_LittleMetacarpal");
        bufJoints[20] = bufJoints[19].GetChild(0);
        bufJoints[21] = bufJoints[20].GetChild(0);
        bufJoints[22] = bufJoints[21].GetChild(0);
        bufJoints[23] = bufJoints[22].GetChild(0);

        bufJoints[24] = transform.Find(hand_Short + "_Wrist_Ghost/" + hand_Short + "_Palm");
        bufJoints[25] = transform.Find(hand_Short + "_Wrist_Ghost/");

        handTracker = GameObject.Find(hand_Short + "_HandTracker").transform;
    }


    void FixedUpdate()
    {
        try
        {
            // position
            rb.velocity = (targePosition - transform.position) / Time.fixedDeltaTime;

            // rotation
            Quaternion deltaRotation = targeRotation * Quaternion.Inverse(rb.rotation);
            deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
            //if (float.IsNaN(axis.x)| float.IsNaN(axis.y)| float.IsNaN(axis.z)) { return; }
            //if (float.IsInfinity(axis.x) | float.IsInfinity(axis.y) | float.IsInfinity(axis.z)) { return; }
            if (angle > 180f) { angle -= 360f; }
            Vector3 angularVelocity = angle * axis * Mathf.Deg2Rad / Time.fixedDeltaTime;
            if (float.IsNaN(angularVelocity.x) | float.IsNaN(angularVelocity.y) | float.IsNaN(angularVelocity.z)) { return; }
            rb.angularVelocity = angle * axis * Mathf.Deg2Rad / Time.fixedDeltaTime;
        }
        catch (Exception e)
        {
            //logText2.text += "\n" + e.ToString();
            //Debug.Log("a" + e.ToString());
        }

    }


    void Update()
    {
        try
        {
            //Wrist
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, handedness, out pose))
            {
                targePosition = pose.Position;
                targeRotation = pose.Rotation * Quaternion.Euler(rotOffsetPalm);

                bufJoints[25].position = pose.Position;
                bufJoints[25].rotation = pose.Rotation * Quaternion.Euler(rotOffsetPalm);

                if (handTracker != null)
                {
                    handTracker.position = pose.Position;
                    handTracker.rotation = pose.Rotation;
                }
            }

            for (int i = 3; i < 27; i++)
            {
                if (HandJointUtils.TryGetJointPose((TrackedHandJoint)i, handedness, out pose))
                {
                    bufJoints[(int)i - 3].position = pose.Position;
                    bufJoints[(int)i - 3].rotation = pose.Rotation * Quaternion.Euler(rotOffsetFinger);

                    followingJoints[(int)i - 3].localPosition = bufJoints[(int)i - 3].localPosition;
                    followingJoints[(int)i - 3].localRotation = bufJoints[(int)i - 3].localRotation;
                }
            }


            


            //for (int i = 3; i < 26; i++)
            //{
            //    if (((i - 6) % 5 != 0) & ((i - 7) % 5 != 0))
            //    {
            //        if (HandJointUtils.TryGetJointPose((TrackedHandJoint)i, handedness, out pose))
            //        {
            //            followingJoints[(int)i - 3].position = pose.Position;
            //            followingJoints[(int)i - 3].rotation = pose.Rotation * Quaternion.Euler(rotOffsetFinger);

            //            //Joints_left[(int)i - 3].localPosition =
            //            //    bufJoints[(int)i - 3].localPosition;// + new Vector3(0.01f, 0, 0);
            //            //Joints_left[(int)i - 3].localRotation = bufJoints[(int)i - 3].localRotation;
            //        }
            //    }

            //}
        }
        catch (Exception e)
        {
            //logText2.text += "\n" + e.ToString();\
            //Debug.Log("b" + e.ToString());
        }


    }



    //void Start()
    //{
    //    if (leftHand)
    //    {
    //        handedness = Handedness.Left;
    //    }
    //    else
    //    {
    //        handedness = Handedness.Right;
    //    }
    //    rb = GetComponent<Rigidbody>();

    //    Joints_left[0] = transform.Find("WristL_JNT/ThumbL_JNT1");
    //    Joints_left[1] = Joints_left[0].GetChild(0);
    //    Joints_left[2] = Joints_left[1].GetChild(0);
    //    Joints_left[3] = Joints_left[2].GetChild(0);

    //    Joints_left[4] = transform.Find("WristL_JNT/PointL_JNT");
    //    Joints_left[5] = Joints_left[4].GetChild(0);
    //    Joints_left[6] = Joints_left[5].GetChild(0);
    //    Joints_left[7] = Joints_left[6].GetChild(0);
    //    Joints_left[8] = Joints_left[7].GetChild(0);

    //    Joints_left[9] = transform.Find("WristL_JNT/MiddleL_JNT");
    //    Joints_left[10] = Joints_left[9].GetChild(0);
    //    Joints_left[11] = Joints_left[10].GetChild(0);
    //    Joints_left[12] = Joints_left[11].GetChild(0);
    //    Joints_left[13] = Joints_left[12].GetChild(0);

    //    Joints_left[14] = transform.Find("WristL_JNT/RingL_JNT");
    //    Joints_left[15] = Joints_left[14].GetChild(0);
    //    Joints_left[16] = Joints_left[15].GetChild(0);
    //    Joints_left[17] = Joints_left[16].GetChild(0);
    //    Joints_left[18] = Joints_left[17].GetChild(0);

    //    Joints_left[19] = transform.Find("WristL_JNT/PinkyL_JNT");
    //    Joints_left[20] = Joints_left[19].GetChild(0);
    //    Joints_left[21] = Joints_left[20].GetChild(0);
    //    Joints_left[22] = Joints_left[21].GetChild(0);
    //    Joints_left[23] = Joints_left[22].GetChild(0);

    //    Joints_left[24] = transform.Find("WristL_JNT");
    //    Joints_left[25] = transform.Find("WristL_JNT");

    //    //////////////////////////////
    //    bufJoints[0] = transform.Find("GhostWristL_JNT/ThumbL_JNT1");
    //    bufJoints[1] = bufJoints[0].GetChild(0);
    //    bufJoints[2] = bufJoints[1].GetChild(0);
    //    bufJoints[3] = bufJoints[2].GetChild(0);

    //    bufJoints[4] = transform.Find("GhostWristL_JNT/PointL_JNT");
    //    bufJoints[5] = bufJoints[4].GetChild(0);
    //    bufJoints[6] = bufJoints[5].GetChild(0);
    //    bufJoints[7] = bufJoints[6].GetChild(0);
    //    bufJoints[8] = bufJoints[7].GetChild(0);

    //    bufJoints[9] = transform.Find("GhostWristL_JNT/MiddleL_JNT");
    //    bufJoints[10] = bufJoints[9].GetChild(0);
    //    bufJoints[11] = bufJoints[10].GetChild(0);
    //    bufJoints[12] = bufJoints[11].GetChild(0);
    //    bufJoints[13] = bufJoints[12].GetChild(0);

    //    bufJoints[14] = transform.Find("GhostWristL_JNT/RingL_JNT");
    //    bufJoints[15] = bufJoints[14].GetChild(0);
    //    bufJoints[16] = bufJoints[15].GetChild(0);
    //    bufJoints[17] = bufJoints[16].GetChild(0);
    //    bufJoints[18] = bufJoints[17].GetChild(0);

    //    bufJoints[19] = transform.Find("GhostWristL_JNT/PinkyL_JNT");
    //    bufJoints[20] = bufJoints[19].GetChild(0);
    //    bufJoints[21] = bufJoints[20].GetChild(0);
    //    bufJoints[22] = bufJoints[21].GetChild(0);
    //    bufJoints[23] = bufJoints[22].GetChild(0);

    //    bufJoints[24] = transform.Find("GhostWristL_JNT");
    //    bufJoints[25] = transform.Find("GhostWristL_JNT");
    //}
}
