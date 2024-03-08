using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using Environment = System.Environment;

#if WINDOWS_UWP
using Windows.Storage;
using Windows.System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
#endif

public class PlayRecordedHandMove : MonoBehaviour
{
    public Transform handLeft;
    public Transform handRight;
    public Transform refTransform;
    public TMP_Text logText;
    public Vector3 rotOffsetPalmLeft = new Vector3(0, -90, 180);
    public Vector3 rotOffsetFingerLeft = new Vector3(180, 90, 0);

    private Rigidbody rbLeft;
    private Rigidbody rbRight;
    private Transform[] JointsLeft = new Transform[16];
    private Transform[] bufJointsLeft = new Transform[16];
    private Transform[] JointsRight = new Transform[16];
    private Transform[] bufJointsRight = new Transform[16];

    private bool startPlaying = false;
    private IList<string> dataStringList;
    private int rows = 0;
    private int index = 0;
    private float playFrequency;

#if WINDOWS_UWP
    Windows.Storage.StorageFolder storageFolder =
        Windows.Storage.ApplicationData.Current.LocalFolder;
    Windows.Storage.StorageFile sampleFile;
#endif

    void Start()
    {
        rbLeft = handLeft.GetComponent<Rigidbody>();
        rbRight = handRight.GetComponent<Rigidbody>();

        //left
        JointsLeft[0] = handLeft.Find("L_Wrist");

        JointsLeft[1] = handLeft.Find("L_Wrist/L_Palm/L_thumb_meta");
        JointsLeft[2] = JointsLeft[1].GetChild(0);
        JointsLeft[3] = JointsLeft[2].GetChild(0);

        JointsLeft[4] = handLeft.Find("L_Wrist/L_Palm/L_index_meta/L_index_a");
        JointsLeft[5] = JointsLeft[4].GetChild(0);
        JointsLeft[6] = JointsLeft[5].GetChild(0);

        JointsLeft[7] = handLeft.Find("L_Wrist/L_Palm/L_middle_meta/L_middle_a");
        JointsLeft[8] = JointsLeft[7].GetChild(0);
        JointsLeft[9] = JointsLeft[8].GetChild(0);

        JointsLeft[10] = handLeft.Find("L_Wrist/L_Palm/L_ring_meta/L_ring_a");
        JointsLeft[11] = JointsLeft[10].GetChild(0);
        JointsLeft[12] = JointsLeft[11].GetChild(0);

        JointsLeft[13] = handLeft.Find("L_Wrist/L_Palm/L_pinky_meta/L_pinky_a");
        JointsLeft[14] = JointsLeft[13].GetChild(0);
        JointsLeft[15] = JointsLeft[14].GetChild(0);

        //
        bufJointsLeft[0] = handLeft.Find("L_Wrist_Ghost");

        bufJointsLeft[1] = handLeft.Find("L_Wrist_Ghost/L_Palm/L_thumb_meta");
        bufJointsLeft[2] = bufJointsLeft[1].GetChild(0);
        bufJointsLeft[3] = bufJointsLeft[2].GetChild(0);

        bufJointsLeft[4] = handLeft.Find("L_Wrist_Ghost/L_Palm/L_index_meta/L_index_a");
        bufJointsLeft[5] = bufJointsLeft[4].GetChild(0);
        bufJointsLeft[6] = bufJointsLeft[5].GetChild(0);

        bufJointsLeft[7] = handLeft.Find("L_Wrist_Ghost/L_Palm/L_middle_meta/L_middle_a");
        bufJointsLeft[8] = bufJointsLeft[7].GetChild(0);
        bufJointsLeft[9] = bufJointsLeft[8].GetChild(0);

        bufJointsLeft[10] = handLeft.Find("L_Wrist_Ghost/L_Palm/L_ring_meta/L_ring_a");
        bufJointsLeft[11] = bufJointsLeft[10].GetChild(0);
        bufJointsLeft[12] = bufJointsLeft[11].GetChild(0);

        bufJointsLeft[13] = handLeft.Find("L_Wrist_Ghost/L_Palm/L_pinky_meta/L_pinky_a");
        bufJointsLeft[14] = bufJointsLeft[13].GetChild(0);
        bufJointsLeft[15] = bufJointsLeft[14].GetChild(0);

        //right
        JointsRight[0] = handRight.Find("L_Wrist");

        JointsRight[1] = handRight.Find("L_Wrist/L_Palm/L_thumb_meta");
        JointsRight[2] = JointsRight[1].GetChild(0);
        JointsRight[3] = JointsRight[2].GetChild(0);

        JointsRight[4] = handRight.Find("L_Wrist/L_Palm/L_index_meta/L_index_a");
        JointsRight[5] = JointsRight[4].GetChild(0);
        JointsRight[6] = JointsRight[5].GetChild(0);

        JointsRight[7] = handRight.Find("L_Wrist/L_Palm/L_middle_meta/L_middle_a");
        JointsRight[8] = JointsRight[7].GetChild(0);
        JointsRight[9] = JointsRight[8].GetChild(0);

        JointsRight[10] = handRight.Find("L_Wrist/L_Palm/L_ring_meta/L_ring_a");
        JointsRight[11] = JointsRight[10].GetChild(0);
        JointsRight[12] = JointsRight[11].GetChild(0);

        JointsRight[13] = handRight.Find("L_Wrist/L_Palm/L_pinky_meta/L_pinky_a");
        JointsRight[14] = JointsRight[13].GetChild(0);
        JointsRight[15] = JointsRight[14].GetChild(0);

        //
        bufJointsRight[0] = handRight.Find("L_Wrist_Ghost");

        bufJointsRight[1] = handRight.Find("L_Wrist_Ghost/L_Palm/L_thumb_meta");
        bufJointsRight[2] = bufJointsRight[1].GetChild(0);
        bufJointsRight[3] = bufJointsRight[2].GetChild(0);

        bufJointsRight[4] = handRight.Find("L_Wrist_Ghost/L_Palm/L_index_meta/L_index_a");
        bufJointsRight[5] = bufJointsRight[4].GetChild(0);
        bufJointsRight[6] = bufJointsRight[5].GetChild(0);

        bufJointsRight[7] = handRight.Find("L_Wrist_Ghost/L_Palm/L_middle_meta/L_middle_a");
        bufJointsRight[8] = bufJointsRight[7].GetChild(0);
        bufJointsRight[9] = bufJointsRight[8].GetChild(0);

        bufJointsRight[10] = handRight.Find("L_Wrist_Ghost/L_Palm/L_ring_meta/L_ring_a");
        bufJointsRight[11] = bufJointsRight[10].GetChild(0);
        bufJointsRight[12] = bufJointsRight[11].GetChild(0);

        bufJointsRight[13] = handRight.Find("L_Wrist_Ghost/L_Palm/L_pinky_meta/L_pinky_a");
        bufJointsRight[14] = bufJointsRight[13].GetChild(0);
        bufJointsRight[15] = bufJointsRight[14].GetChild(0);
    }

    public void StartRead()
    {
        logText.text = "Start read";

#if WINDOWS_UWP
        OpenFile();
#endif
    }

#if WINDOWS_UWP
    async void OpenFile()
    {
        logText.text = "Start playing";
        try
        {
            sampleFile = await storageFolder.GetFileAsync("Liver.csv");
            dataStringList = await Windows.Storage.FileIO.ReadLinesAsync(sampleFile);
            rows = dataStringList.Count;
            logText.text = "Get file " + "Rows:" + rows.ToString();
            
            if (rows > 2)
            {
                startPlaying = true;
            }
            else
            {
                logText.text += "Not enough data in the file";
            }
        }
        catch (Exception e)
        {
            logText.text = e.ToString();
        }
        
        
    }
#endif


    private string lineData;
    private int iniTickCount = 0;
    private string[] frameData;

    void Update()
    {
        if (startPlaying)
        {
            if (index == 0)
            {
                frameData = dataStringList[index].Split(',');
                playFrequency = Convert.ToSingle(frameData[0]);
                index = 2;
                iniTickCount = Environment.TickCount;
            }

            if (index < rows)
            {
                if ((Environment.TickCount - iniTickCount) > (1000 / playFrequency))
                {
                    try
                    {
                        frameData = dataStringList[index].Split(',');
                        MoveJoints(frameData);
                        index++;
                        iniTickCount = Environment.TickCount;
                    }
                    catch (Exception e)
                    {
                        logText.text = e.ToString();
                    }
                    
                }
            }
            else
            {
                //end reading
                logText.text = "Finish playing";
                startPlaying = false;
                index = 0;
            }

        }
    }

    private void MoveJoints(string[] jointsStrings)
    {
        //Left hand

        //new wrist world transform with respect to reference origin
        Vector3 wristTarPos = refTransform.TransformPoint(new Vector3(Convert.ToSingle(jointsStrings[1]),
            Convert.ToSingle(jointsStrings[2]), Convert.ToSingle(jointsStrings[3])));
        Quaternion wristTarRot = refTransform.rotation * new Quaternion(Convert.ToSingle(jointsStrings[4]),
            Convert.ToSingle(jointsStrings[5]), Convert.ToSingle(jointsStrings[6]),
            Convert.ToSingle(jointsStrings[7]));



        //Vector3 wristTarPos = refTransform.rotation * (refTransform.position + new Vector3(Convert.ToSingle(jointsStrings[1]), Convert.ToSingle(jointsStrings[2]), Convert.ToSingle(jointsStrings[3])));
        //Quaternion wristTarRot = (new Quaternion(Convert.ToSingle(jointsStrings[4]),
        //    Convert.ToSingle(jointsStrings[5]), Convert.ToSingle(jointsStrings[6]),
        //    Convert.ToSingle(jointsStrings[7]))) * refTransform.rotation;

        // wrist position
        rbLeft.velocity = (wristTarPos - handLeft.position) * playFrequency;
        // wrist rotation
        Quaternion deltaRotation = wristTarRot * Quaternion.Inverse(handLeft.rotation);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
        if (float.IsInfinity(axis.x)) { return; }
        if (angle > 180f) { angle -= 360f; };
        rbLeft.angularVelocity = angle * axis * Mathf.Deg2Rad * playFrequency;

        //other joints
        for (int i = 1; i < 16; i++)
        {
            Vector3 bufPos = Vector3.zero;
            Quaternion bufRot = Quaternion.identity;

            for (int j = 0; j < 3; j++)
            {
                bufPos[j] = Convert.ToSingle(jointsStrings[1 + j + i * 7]);
            }

            for (int j = 0; j < 4; j++)
            {
                bufRot[j] = Convert.ToSingle(jointsStrings[1 + j + 3 + i * 7]);
            }

            bufJointsLeft[i].position = refTransform.TransformPoint(bufPos);
            //bufJointsLeft[i].position = refTransform.rotation * (refTransform.position + bufPos);
            bufJointsLeft[i].rotation = refTransform.rotation * bufRot;

            JointsLeft[i].localPosition = bufJointsLeft[i].localPosition;
            JointsLeft[i].localRotation = bufJointsLeft[i].localRotation;
        }


        //Right hand
    }

}
