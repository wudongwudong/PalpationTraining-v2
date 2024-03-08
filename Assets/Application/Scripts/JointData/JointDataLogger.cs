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
    public Vector3 rotOffsetPalmLeft = new Vector3(0, -90, 180);
    public Vector3 rotOffsetFingerLeft = new Vector3(180, 90, 0);

    //private float _delay = 0.05f;
    private float savingFrequency = 20;

    private float _timer = 0.0f;

    private bool fileCreate = false;

    public GameObject dialog;
    public Transform refTransform;
    string dialogText;
    string lineData;

    MixedRealityPose pose;

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

        for (int i = 1; i < 26; i++)
        {
            if ((i == 1) | ((i != 2) & ((i - 6) % 5 != 0) & ((i - 7) % 5 != 0)))
            {
                string jointName  = ((TrackedHandJoint)i).ToString();
                headers += "L" + jointName + "_Px,";
                headers += "L" + jointName + "_Py,";
                headers += "L" + jointName + "_Pz,";
                headers += "L" + jointName + "_Qx,";
                headers += "L" + jointName + "_Qy,";
                headers += "L" + jointName + "_Qz,";
                headers += "L" + jointName + "_Qw,";
            }
        }

        for (int i = 1; i < 26; i++)
        {
            if ((i == 1) | ((i != 2) & ((i - 6) % 5 != 0) & ((i - 7) % 5 != 0)))
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
        }

        headers += "\r\n";

        return headers;
    }

    public void GetJointPose(TrackedHandJoint joint, out Vector3 pos, out Quaternion rot)
    {
        if (HandJointUtils.TryGetJointPose(joint, Handedness.Left, out pose))
        {
            if (joint == TrackedHandJoint.Wrist)
            {
                pos = refTransform.InverseTransformPoint(pose.Position);
                rot = Quaternion.Inverse(refTransform.rotation) * (pose.Rotation * Quaternion.Euler(rotOffsetPalmLeft));
                //pos = Quaternion.Inverse(refTransform.rotation) * (pose.Position - refTransform.position);
                //rot = pose.Rotation * Quaternion.Euler(rotOffsetPalmLeft) * Quaternion.Inverse(refTransform.rotation);
            }
            else
            {
                pos = refTransform.InverseTransformPoint(pose.Position);
                rot = Quaternion.Inverse(refTransform.rotation) * (pose.Rotation * Quaternion.Euler(rotOffsetFingerLeft));
                //rot = pose.Rotation * Quaternion.Euler(rotOffsetFingerLeft) * Quaternion.Inverse(refTransform.rotation);
            }
            
        }
        else
        {
            pos = Vector3.zero;
            rot = Quaternion.identity;
        }
    }


    public void StartRecord()
    {
        dialogText = "Start Recording";
        Dialog.Open(dialog, DialogButtonType.OK, "Hololens2", dialogText, true);

#if WINDOWS_UWP
        //Create csv file
        CreateFile();      
#endif
        fileCreate = true;
    }

    public void EndRecord()
    {
        fileCreate = false;

        dialogText = "Finger Movement Data Saved!";
        Dialog.Open(dialog, DialogButtonType.OK, "Hololens2", dialogText, true);

#if WINDOWS_UWP
        sampleFile = null; 
#endif
    }


    private int iniTickCount = 0;

    void Update()
    {
        if (fileCreate)
        {
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

                for (int i = 1; i < 26; i++)
                {
                    if ((i == 1) | ((i != 2) & ((i - 6) % 5 != 0) & ((i - 7) % 5 != 0)))
                    {
                        GetJointPose((TrackedHandJoint)i, out Vector3 position, out Quaternion rotation);
                        string pos = position.ToString("F8");
                        pos = pos.Replace("(", "");
                        pos = pos.Replace(")", ",");
                        lineData += pos;

                        string rot = rotation.ToString("F8");
                        rot = rot.Replace("(", "");
                        rot = rot.Replace(")", ",");
                        lineData += rot;
                    }
                    
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


