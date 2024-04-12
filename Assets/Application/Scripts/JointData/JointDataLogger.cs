using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using OfficeOpenXml;
using System.Linq;

using Microsoft;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine.UI;
using TMPro;

#if WINDOWS_UWP
using Windows.Storage;
using Windows.System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
#endif


public class JointDataLogger : MonoBehaviour
{
    //public Vector3 rotOffsetPalmLeft = new Vector3(0, -90, 180);
    //public Vector3 rotOffsetFingerLeft = new Vector3(180, 90, 0);

    //private float _delay = 0.05f;
    private float savingFrequency = 20;

    private float _timer = 0.0f;

    private bool fileCreate = false;

    private Transform[] trackingJoints_L = new Transform[26];
    private Transform[] trackingJoints_R = new Transform[26];
    public Transform leftHand;
    public Transform rightHand; 
    //public GameObject dialog;
    private Transform refTransform;
    //string dialogText;
    string lineData;

    MixedRealityPose pose;


    void Start()
    {
        string hand_Short = "L";
        trackingJoints_L[0] = leftHand.Find(hand_Short + "_Wrist/"); 
        trackingJoints_L[1] = leftHand.Find(hand_Short + "_Wrist/" + hand_Short + "_Palm");

        trackingJoints_L[2] = leftHand.Find(hand_Short + "_Wrist/" + hand_Short + "_ThumbMetacarpal");
        trackingJoints_L[3] = trackingJoints_L[2].GetChild(0);
        trackingJoints_L[4] = trackingJoints_L[3].GetChild(0);
        trackingJoints_L[5] = trackingJoints_L[4].GetChild(0);

        trackingJoints_L[6] = leftHand.Find(hand_Short + "_Wrist/" + hand_Short + "_IndexMetacarpal");
        trackingJoints_L[7] = trackingJoints_L[6].GetChild(0);
        trackingJoints_L[8] = trackingJoints_L[7].GetChild(0);
        trackingJoints_L[9] = trackingJoints_L[8].GetChild(0);
        trackingJoints_L[10] = trackingJoints_L[9].GetChild(0);

        trackingJoints_L[11] = leftHand.Find(hand_Short + "_Wrist/" + hand_Short + "_MiddleMetacarpal");
        trackingJoints_L[12] = trackingJoints_L[11].GetChild(0);
        trackingJoints_L[13] = trackingJoints_L[12].GetChild(0);
        trackingJoints_L[14] = trackingJoints_L[13].GetChild(0);
        trackingJoints_L[15] = trackingJoints_L[14].GetChild(0);

        trackingJoints_L[16] = leftHand.Find(hand_Short + "_Wrist/" + hand_Short + "_RingMetacarpal");
        trackingJoints_L[17] = trackingJoints_L[16].GetChild(0);
        trackingJoints_L[18] = trackingJoints_L[17].GetChild(0);
        trackingJoints_L[19] = trackingJoints_L[18].GetChild(0);
        trackingJoints_L[20] = trackingJoints_L[19].GetChild(0);

        trackingJoints_L[21] = leftHand.Find(hand_Short + "_Wrist/" + hand_Short + "_LittleMetacarpal");
        trackingJoints_L[22] = trackingJoints_L[21].GetChild(0);
        trackingJoints_L[23] = trackingJoints_L[22].GetChild(0);
        trackingJoints_L[24] = trackingJoints_L[23].GetChild(0);
        trackingJoints_L[25] = trackingJoints_L[24].GetChild(0);

        hand_Short = "R";
        trackingJoints_R[0] = rightHand.Find(hand_Short + "_Wrist/");
        trackingJoints_R[1] = rightHand.Find(hand_Short + "_Wrist/" + hand_Short + "_Palm");
        
        trackingJoints_R[2] = rightHand.Find(hand_Short + "_Wrist/" + hand_Short + "_ThumbMetacarpal");
        trackingJoints_R[3] = trackingJoints_R[2].GetChild(0);
        trackingJoints_R[4] = trackingJoints_R[3].GetChild(0);
        trackingJoints_R[5] = trackingJoints_R[4].GetChild(0);

        trackingJoints_R[6] = rightHand.Find(hand_Short + "_Wrist/" + hand_Short + "_IndexMetacarpal");
        trackingJoints_R[7] = trackingJoints_R[6].GetChild(0);
        trackingJoints_R[8] = trackingJoints_R[7].GetChild(0);
        trackingJoints_R[9] = trackingJoints_R[8].GetChild(0);
        trackingJoints_R[10] = trackingJoints_R[9].GetChild(0);

        trackingJoints_R[11] = rightHand.Find(hand_Short + "_Wrist/" + hand_Short + "_MiddleMetacarpal");
        trackingJoints_R[12] = trackingJoints_R[11].GetChild(0);
        trackingJoints_R[13] = trackingJoints_R[12].GetChild(0);
        trackingJoints_R[14] = trackingJoints_R[13].GetChild(0);
        trackingJoints_R[15] = trackingJoints_R[14].GetChild(0);

        trackingJoints_R[16] = rightHand.Find(hand_Short + "_Wrist/" + hand_Short + "_RingMetacarpal");
        trackingJoints_R[17] = trackingJoints_R[16].GetChild(0);
        trackingJoints_R[18] = trackingJoints_R[17].GetChild(0);
        trackingJoints_R[19] = trackingJoints_R[18].GetChild(0);
        trackingJoints_R[20] = trackingJoints_R[19].GetChild(0);

        trackingJoints_R[21] = rightHand.Find(hand_Short + "_Wrist/" + hand_Short + "_LittleMetacarpal");
        trackingJoints_R[22] = trackingJoints_R[21].GetChild(0);
        trackingJoints_R[23] = trackingJoints_R[22].GetChild(0);
        trackingJoints_R[24] = trackingJoints_R[23].GetChild(0);
        trackingJoints_R[25] = trackingJoints_R[24].GetChild(0);
    }

    //define filePath
#if WINDOWS_UWP
    Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
    Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

    StorageFile sampleFile;

    async void WriteData(string buf)
    {
        await FileIO.AppendTextAsync(sampleFile, buf);
    }

    async void CreateFile()
    {
        string fileName = "FingerData" + System.DateTime.Now.ToString("yyyy_MMdd_HHmmss") + ".csv";
        sampleFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

        string headers = GetHeaders();
        WriteData(headers);
    }

#endif

    private string GetHeaders()
    {
        string headers = savingFrequency.ToString() + ",Hz" + "\r\n";
        headers += "time,";

        for (int i = 1; i < 27; i++)
        {
            string jointName = ((TrackedHandJoint)i).ToString();
            headers += "L" + jointName + "_Px,";
            headers += "L" + jointName + "_Py,";
            headers += "L" + jointName + "_Pz,";
            headers += "L" + jointName + "_Qx,";
            headers += "L" + jointName + "_Qy,";
            headers += "L" + jointName + "_Qz,";
            headers += "L" + jointName + "_Qw,";
        }

        for (int i = 1; i < 27; i++)
        {
            string jointName = ((TrackedHandJoint)i).ToString();
            headers += "R" + jointName + "_Px,";
            headers += "R" + jointName + "_Py,";
            headers += "R" + jointName + "_Pz,";
            headers += "R" + jointName + "_Qx,";
            headers += "R" + jointName + "_Qy,";
            headers += "R" + jointName + "_Qz,";
            headers += "R" + jointName + "_Qw,";
        }

        headers += "\r\n";

        return headers;
    }

    public void GetJointPose(TrackedHandJoint joint, out Vector3 pos, out Quaternion rot)
    {
        if (HandJointUtils.TryGetJointPose(joint, Handedness.Left, out pose))
        {
            pos = refTransform.InverseTransformPoint(pose.Position);
            rot = Quaternion.Inverse(refTransform.rotation) * pose.Rotation;

            //if (joint == TrackedHandJoint.Wrist)
            //{
            //    pos = refTransform.InverseTransformPoint(pose.Position);
            //    rot = Quaternion.Inverse(refTransform.rotation) * (pose.Rotation * Quaternion.Euler(rotOffsetPalmLeft));
            //    //pos = Quaternion.Inverse(refTransform.rotation) * (pose.Position - refTransform.position);
            //    //rot = pose.Rotation * Quaternion.Euler(rotOffsetPalmLeft) * Quaternion.Inverse(refTransform.rotation);
            //}
            //else
            //{
            //    pos = refTransform.InverseTransformPoint(pose.Position);
            //    rot = Quaternion.Inverse(refTransform.rotation) * (pose.Rotation * Quaternion.Euler(rotOffsetFingerLeft));
            //    //rot = pose.Rotation * Quaternion.Euler(rotOffsetFingerLeft) * Quaternion.Inverse(refTransform.rotation);
            //}
            
        }
        else
        {
            pos = Vector3.zero;
            rot = Quaternion.identity;
        }
    }


    public void StartRecord()
    {
        //dialogText = "Start Recording";
        //Dialog.Open(dialog, DialogButtonType.OK, "Hololens2", dialogText, true);

#if WINDOWS_UWP
        //Create csv file
        CreateFile();      
#endif
        GameObject trackingRef = GameObject.Find("HandTrackingRef");
        if (trackingRef != null)
        {
            refTransform = trackingRef.transform;
            fileCreate = true;
        }
        
        
    }

    public void EndRecord()
    {
        fileCreate = false;

        refTransform = null;

        //dialogText = "Finger Movement Data Saved!";
        //Dialog.Open(dialog, DialogButtonType.OK, "Hololens2", dialogText, true);

#if WINDOWS_UWP
        sampleFile = null;
#endif
    }


    private int iniTickCount = 0;

    void Update()
    {
        if (fileCreate)
        {
            if (refTransform == null)
            {
                return;
            }

            _timer += Time.deltaTime;
            if (_timer > (1 / savingFrequency))
            {
                if (iniTickCount == 0)
                {
                    iniTickCount = Environment.TickCount;
                    lineData = "0,";
                }
                else
                {
                    lineData = (Environment.TickCount - iniTickCount).ToString() + ",";
                }
                //lineData = DateTime.Now.ToString("HH_mm_ss_fff,");

                //Vector3 position;
                //Quaternion rotation;
                //GetJointPose(TrackedHandJoint.Wrist, out position, out rotation);
                //string pos = position.ToString("F8");
                //pos = pos.Replace("(", "");
                //pos = pos.Replace(")", ",");
                //lineData += pos;

                //string rot = rotation.ToString("F8");
                //rot = rot.Replace("(", "");
                //rot = rot.Replace(")", ",");
                //lineData += rot;

                for (int i = 0; i < 26; i++)
                {
                    Vector3 relativePos = refTransform.InverseTransformPoint(trackingJoints_L[i].position);
                    string pos = relativePos.ToString("F8");
                    pos = pos.Replace("(", "");
                    pos = pos.Replace(")", ",");
                    lineData += pos;

                    Quaternion relativeRot = Quaternion.Inverse(refTransform.rotation) * trackingJoints_L[i].rotation;
                    string rot = relativeRot.ToString("F8");
                    rot = rot.Replace("(", "");
                    rot = rot.Replace(")", ",");
                    lineData += rot;
                }

                for (int i = 0; i < 26; i++)
                {
                    Vector3 relativePos = refTransform.InverseTransformPoint(trackingJoints_R[i].position);
                    string pos = relativePos.ToString("F8");
                    pos = pos.Replace("(", "");
                    pos = pos.Replace(")", ",");
                    lineData += pos;

                    Quaternion relativeRot = Quaternion.Inverse(refTransform.rotation) * trackingJoints_R[i].rotation;
                    string rot = relativeRot.ToString("F8");
                    rot = rot.Replace("(", "");
                    rot = rot.Replace(")", ",");
                    lineData += rot;
                }

                _timer -= 1 / savingFrequency;
#if WINDOWS_UWP
                lineData += "\r\n";
                WriteData(lineData);
#endif
            }
       
        }
            
    }
            
}


