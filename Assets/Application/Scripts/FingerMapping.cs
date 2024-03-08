#define BT_Commu

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HaptGlove;

public class FingerMapping : MonoBehaviour
{
    //static FingerMapping_Left _instance;
    //public static FingerMapping_Left Instance
    //{
    //    get
    //    {
    //        if (_instance == null)
    //        {
    //            _instance = new FingerMapping_Left();
    //        }
    //        return _instance;
    //    }
    //}

    public HaptGloveHandler haptGloveHandler;

    //void Awake()
    //{
    //    _instance = this;
    //}

    public Transform[,] Joints_left = new Transform[5, 3];

    private Vector3[,] roFinger_left = new Vector3[5, 3];

    private Vector3[,] roStraightFinger_left = new Vector3[5, 3]
    {
        {new Vector3(54, -104, -39), new Vector3(0, 0, 0), new Vector3(0, 0, 0)},
        {new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)},
        {new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)},
        {new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)},
        {new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)}
    };

    private Vector3[,] roMidFinger_left = new Vector3[5, 3]
    {
        {new Vector3(34, -151, -113), new Vector3(0, 0, -20), new Vector3(0, 0, -22)},
        {new Vector3(0, 0, -48), new Vector3(0, 0, -45), new Vector3(0, 0, -15)},
        {new Vector3(0, 0, -56), new Vector3(0, 0, -36), new Vector3(0, 0, -15)},
        {new Vector3(0, 0, -50), new Vector3(0, 0, -43), new Vector3(0, 0, -13)},
        {new Vector3(0, 0, -40), new Vector3(0, 0, -40), new Vector3(0, 0, -8)}
    };

    private Vector3[,] roFistFinger_left = new Vector3[5, 3]
    {
        {new Vector3(38, -143, -140), new Vector3(0, 0, -35), new Vector3(0, 0, -70)},
        {new Vector3(0, 0, -67), new Vector3(0, 0, -95), new Vector3(0, 0, -50)},
        {new Vector3(0, 0, -67), new Vector3(0, 0, -90), new Vector3(0, 0, -64)},
        {new Vector3(0, 0, -65), new Vector3(0, 0, -100), new Vector3(0, 0, -66)},
        {new Vector3(0, 0, -57), new Vector3(0, 0, -110), new Vector3(0, 0, -52)}
    };

    private float timeCount = 0.2f;

    public Int16[] fingerMin = new Int16[5] { 420, 200, 214, 210, 187 };
    public Int16[] fingerMax = new Int16[5] { 1500, 800, 841, 730, 558 };
    public Int16[] fingerMid = new Int16[5] { 550, 500, 450, 450, 400 };

    public Int16[] microtubeData = new Int16[5];

    void Start()
    {
        //Objects mapping
        Joints_left[0, 0] = transform.Find("L_Wrist/L_Palm/L_thumb_meta");
        Joints_left[0, 1] = transform.Find("L_Wrist/L_Palm/L_thumb_meta/L_thumb_a");
        Joints_left[0, 2] = transform.Find("L_Wrist/L_Palm/L_thumb_meta/L_thumb_a/L_thumb_b");
        Joints_left[1, 0] = transform.Find("L_Wrist/L_Palm/L_index_meta/L_index_a");
        Joints_left[1, 1] = transform.Find("L_Wrist/L_Palm/L_index_meta/L_index_a/L_index_b");
        Joints_left[1, 2] = transform.Find("L_Wrist/L_Palm/L_index_meta/L_index_a/L_index_b/L_index_c");
        Joints_left[2, 0] = transform.Find("L_Wrist/L_Palm/L_middle_meta/L_middle_a");
        Joints_left[2, 1] = transform.Find("L_Wrist/L_Palm/L_middle_meta/L_middle_a/L_middle_b");
        Joints_left[2, 2] = transform.Find("L_Wrist/L_Palm/L_middle_meta/L_middle_a/L_middle_b/L_middle_c");
        Joints_left[3, 0] = transform.Find("L_Wrist/L_Palm/L_ring_meta/L_ring_a");
        Joints_left[3, 1] = transform.Find("L_Wrist/L_Palm/L_ring_meta/L_ring_a/L_ring_b");
        Joints_left[3, 2] = transform.Find("L_Wrist/L_Palm/L_ring_meta/L_ring_a/L_ring_b/L_ring_c");
        Joints_left[4, 0] = transform.Find("L_Wrist/L_Palm/L_pinky_meta/L_pinky_a");
        Joints_left[4, 1] = transform.Find("L_Wrist/L_Palm/L_pinky_meta/L_pinky_a/L_pinky_b");
        Joints_left[4, 2] = transform.Find("L_Wrist/L_Palm/L_pinky_meta/L_pinky_a/L_pinky_b/L_pinky_c");
    }

    //void Update()
    //{
    //    UpdateFingerPosLeft();
    //}

    public void UpdateFingerPosLeft()
    {
#if BT_Commu
        if (haptGloveHandler.haptics.flag_MicrotubeDataReady)
        {
            Array.Copy(haptGloveHandler.GetFingerPosition(), microtubeData, 5);

            //UpdateMicrotubeRange();
            CalculateNormalizedData();
        }
#else
        if (SerialCommu_Left.Instance.flag_MicrotubeDataReady)
        {
            Array.Copy(SerialCommu_Left.Instance.microtubeData, microtubeData, 5);
            UpdateMicrotubeRange();
            UpdateFingerPos();
        }
#endif

    }

    private bool flag_FirstData = true;
    void UpdateMicrotubeRange()
    {
        if (flag_FirstData)
        {
            Array.Copy(microtubeData, fingerMin, 5);
            Array.Copy(microtubeData, fingerMax, 5);
            flag_FirstData = false;
            return;
        }

        for (int i = 0; i < 5; i++)
        {
            if (microtubeData[i] < fingerMin[i])
            {
                fingerMin[i] = microtubeData[i];
            }
            if (microtubeData[i] > fingerMax[i])
            {
                fingerMax[i] = microtubeData[i];
            }
        }

        //Debug.Log("fingerMin = [" + fingerMin[0] + "\t" + fingerMin[1] + "\t" + fingerMin[2] + "\t" + fingerMin[3] + "\t" + fingerMin[4] + "]");
        //Debug.Log("fingerMax = [" + fingerMax[0] + "\t" + fingerMax[1] + "\t" + fingerMax[2] + "\t" + fingerMax[3] + "\t" + fingerMax[4] + "]");
        //Debug.Log("Data = [" + microtubeData[0] + "\t" + microtubeData[1] + "\t" + microtubeData[2] + "\t" + microtubeData[3] + "\t" + microtubeData[4] + "]");
    }

    public float[] normalizedData = new float[5];
    public float[] r = new float[5] { 0.587f, 0.546f, 0.472f, 0.554f, 0.464f };
    void CalculateNormalizedData()
    {
        for (int i = 0; i < 5; i++)
        {
            if (fingerMin[i] == fingerMax[i]) { return; }

            normalizedData[i] = RemapNumber(microtubeData[i], fingerMin[i], fingerMax[i], 0, 1);
        }

        UpdateFingerPos(normalizedData);
    }

    public void UpdateFingerPos(float[] normalizedDatabuf)
    {
        Array.Copy(normalizedDatabuf, normalizedData, 5);

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (normalizedData[i] < r[i])
                {
                    roFinger_left[i, j] =
                        normalizedData[i] / r[i] * (roMidFinger_left[i, j] - roStraightFinger_left[i, j]) +
                        roStraightFinger_left[i, j];
                    Joints_left[i, j].localRotation = Quaternion.Euler(roFinger_left[i, j]);
                }
                else
                {
                    roFinger_left[i, j] =
                        (normalizedData[i] - r[i]) / (1 - r[i]) *
                        (roFistFinger_left[i, j] - roMidFinger_left[i, j]) + roMidFinger_left[i, j];
                    Joints_left[i, j].localRotation = Quaternion.Euler(roFinger_left[i, j]);
                }
            }
        }
    }

    public static float RemapNumber(float num, float low1, float high1, float low2, float high2)
    {
        float n;
        if (num < low1)
        {
            n = low2;
        }
        else if (num > high1)
        {
            n = high2;
        }
        else
        {
            n = low2 + (num - low1) * (high2 - low2) / (high1 - low1);

        }
        return n;
    }

    IEnumerator ToStraightFingerPose()
    {
        float t = 0;
        while (t < timeCount)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Joints_left[i, j].localRotation = Quaternion.Slerp(Joints_left[i, j].localRotation, Quaternion.Euler(roStraightFinger_left[i, j]), t / timeCount);
                }
            }
            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ToMidFingerPose()
    {
        float t = 0;
        while (t < timeCount)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Joints_left[i, j].localRotation = Quaternion.Slerp(Joints_left[i, j].localRotation, Quaternion.Euler(roMidFinger_left[i, j]), t / timeCount);
                }
            }
            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ToFistFingerPose()
    {
        float t = 0;
        while (t < timeCount)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Joints_left[i, j].localRotation = Quaternion.Slerp(Joints_left[i, j].localRotation, Quaternion.Euler(roFistFinger_left[i, j]), t / timeCount);
                }
            }
            t += Time.deltaTime;
            yield return null;
        }
    }


}
