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
    public Transform leftHand;
    public Transform rightHand;
    public Transform refTransform;
    //public TMP_Text logText;
    //public Vector3 rotOffsetPalmLeft = new Vector3(0, -90, 180);
    //public Vector3 rotOffsetFingerLeft = new Vector3(180, 90, 0);

    private Transform[] trackingJoints_L = new Transform[26];
    private Transform[] trackingJoints_R = new Transform[26];

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

    public void StartRead()
    {
        //logText.text = "Start read";

#if WINDOWS_UWP
        OpenFile();
#endif
    }

#if WINDOWS_UWP
    async void OpenFile()
    {
        //logText.text = "Start playing\n";
        try
        {
            sampleFile = await storageFolder.GetFileAsync("Liver.csv");
            dataStringList = await Windows.Storage.FileIO.ReadLinesAsync(sampleFile);
            rows = dataStringList.Count;
            //logText.text += "Get file " + "Rows:" + rows.ToString() + "\n";
            
            if (rows > 2)
            {
                startPlaying = true;
            }
            else
            {
                //logText.text += "Not enough data in the file\n";
                Debug.Log("Not enough data in the file");
            }
        }
        catch (Exception e)
        {
            //logText.text += "OpenFile exception" + e.ToString() + "\n";
            Debug.Log(e.ToString());
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
                        //logText.text += "index: " +index + "\n " + "Move joint exception" + e.ToString() + "\n";
                        Debug.Log("Move joint exception" + e.ToString());
                    }
                    
                }
            }
            else
            {
                //end reading
                //logText.text += "Finish playing\n";
                startPlaying = false;
                index = 0;

                trackingJoints_L[0].localPosition = Vector3.zero;
                trackingJoints_R[0].localPosition = Vector3.zero;
            }

        }
    }

    private void MoveJoints(string[] jointsStrings)
    {
        //logText.text += "jointsStrings Length: " + jointsStrings.Length + "\n";

        //Left hand

        for (int i = 0; i < 26; i++)
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

            trackingJoints_L[i].position = refTransform.TransformPoint(bufPos);
            trackingJoints_L[i].rotation = refTransform.rotation * bufRot;
        }


        //Right hand

        for (int i = 26; i < 52; i++)
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

            trackingJoints_R[i - 26].position = refTransform.TransformPoint(bufPos);
            trackingJoints_R[i - 26].rotation = refTransform.rotation * bufRot;
        }

    }

}
