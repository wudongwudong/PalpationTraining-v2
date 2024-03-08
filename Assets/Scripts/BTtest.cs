using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using ArduinoBluetoothAPI;
using HaptGlove;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;

public class BTtest : MonoBehaviour
{
    public BluetoothHelper btHelper;
    public bool btConnection = false;
    public string deviceName = "PneuHapGlove L";
    public byte sourcePres = 68; //kpa

    public Int16[] microtubeData = new Int16[5];
    public float[] pressureData = new float[6];

    public bool flag_MicrotubeDataReady = false;
    public bool flag_BMP280DataReady = false;

    private List<byte> buffer = new List<byte>(1024);
    private byte[] oneFrame = new byte[128];

    private Int32[] tick = new int[2];

    private string btText;
    private string sensorText;
    private string dialogText;

    public TMP_Text btTextLog;
    public TMP_Text sensorTextLog;
    public GameObject dialog;
    public GameObject dialog_2;

    private bool isError;
    private bool pumpShouldConnect = false;
    private bool btShouldConnect = false;

    private enum funList : byte
    {
        FI_BMP280 = 0x01,
        FI_MICROTUBE = 0x04,
        FI_CLUTCHGOTACTIVATED = 0x05
    };

    void Start()
    {
        btHelper = BluetoothHelper.GetNewInstance(deviceName);
        btHelper.OnConnected += BtHelper_OnConnected;
        btHelper.OnConnectionFailed += BtHelper_OnConnectionFailed;
        btHelper.OnDataReceived += BtHelper_OnDataReceived;
        btHelper.setFixedLengthBasedStream(51);
    }

    private int index = 0;

    void Update()
    {
        if (btHelper.isConnected())
        {
            if (!isError)
            {
                btConnection = true;

                sensorText =
                    "\n" + "Pressure: " +
                    "\n" + "Thumb: " + pressureData[0] +
                    "\n" + "Index: " + pressureData[1] +
                    "\n" + "Middle: " + pressureData[2] +
                    "\n" + "Ring: " + pressureData[3] +
                    "\n" + "Pinky: " + pressureData[4] +
                    "\n\n" + "Pump: " + pressureData[5] +
                    "\n\n" + "index:       " + index;
            }
            else
            {
                btConnection = false;
                sensorText = "N/A";
            }
        }
        else
        {
            btConnection = false;
            sensorText = "N/A";
        }

        btTextLog.text = btText;
        sensorTextLog.text = sensorText;
    }

    // Establish communication
    public void BTConnection()
    {
        //if (btHelper == null)
        //{
        //    btHelper = BluetoothHelper.GetInstance(deviceName);
        //    btHelper.OnConnected += BtHelper_OnConnected;
        //    btHelper.OnConnectionFailed += BtHelper_OnConnectionFailed;
        //    btHelper.OnDataReceived += BtHelper_OnDataReceived;
        //    btHelper.setFixedLengthBasedStream(51);
        //}

        if (btHelper.isConnected())
        {
            btHelper.Disconnect();
            flag_MicrotubeDataReady = false;

            //btHelper.OnConnected -= BtHelper_OnConnected;
            //btHelper.OnConnectionFailed -= BtHelper_OnConnectionFailed;
            //btHelper.OnDataReceived -= BtHelper_OnDataReceived;
            //btHelper = null;

        }
        else
        {
            try
            {
                dialogText = "Try bt connection";
                Dialog.Open(dialog, DialogButtonType.OK, "Bluetooth", dialogText, true);

                btText = "Try bt connection";
                btHelper.Connect();
                btText = "bt connection finished";
            }
            catch (Exception e)
            {
                btText = e.ToString();
            }

        }

    }


    private void BtHelper_OnConnected(BluetoothHelper helper)
    {
        btText = "BLE Connection Successful";
        dialogText = deviceName + " Connected!";
        Dialog.Open(dialog_2, DialogButtonType.OK, "Bluetooth", dialogText, true);

        BluetoothDevice btServer = btHelper.getBluetoothDevice();
        Debug.Log("Name: " + btServer.DeviceName);

        try
        {
            btHelper.StartListening();
            Debug.Log("StartListening 1");
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }


    public int? BTSend(byte[] data)
    {
        try
        {
            int length = data.Length;
            btHelper.SendData(data);

            Debug.Log("发送的数据：" + BitConverter.ToString(data));
            return length;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return null;
        }
    }

    private void BtHelper_OnConnectionFailed(BluetoothHelper helper)
    {
        btText = "BT Connection Failed";
        dialogText = "Bluetooth Connection Failed";
        Dialog.Open(dialog_2, DialogButtonType.OK, "Bluetooth", dialogText, true);
        Debug.Log("fail 1");
        //throw new NotImplementedException();
    }

    private void BtHelper_OnDataReceived(BluetoothHelper helper)
    {
        //tick[0] = tick[1];
        //tick[1] = System.Environment.TickCount;
        //Debug.Log("Frequency =" + (float) (1000 / (tick[1] - tick[0] + 1)));
        //Debug.Log("DataReceived");
        //try
        //{
        byte[] buf = btHelper.ReadBytes();
        buffer.AddRange(buf);
        //Debug.Log(buf.Length + "\t" + buffer.Count);
        //Debug.Log(BitConverter.ToString(buf));
        while (buffer.Count >= 5)
        {
            if (Enum.IsDefined(typeof(funList), buffer[1]))
            {
                int len = buffer[0];//帧长度
                if (buffer.Count < len) break;//数据不够直接跳出

                byte checkSum = 0;
                for (int i = 0; i < len - 1; i++)
                {
                    checkSum ^= buffer[i];
                }

                if (checkSum != buffer[len - 1])
                {
                    buffer.RemoveRange(0, len);
                    continue;
                }

                buffer.CopyTo(0, oneFrame, 0, len);
                buffer.RemoveRange(0, len);
                FrameDataAnalysis(oneFrame);
            }
            else
            {
                buffer.RemoveAt(0);
            }
        }
        //}
        //catch (Exception e)
        //{
        //    Debug.Log(e.ToString());
        //}
    }

    void FrameDataAnalysis(byte[] frame)
    {
        sensorText = BitConverter.ToString(frame);

        //Debug.Log("frame[1]:  " + frame[1]);
        switch (frame[1])
        {
            case (byte)funList.FI_BMP280:         //BMP280
                decodePressure(frame);
                break;
            case (byte)funList.FI_MICROTUBE:      //Microtube
                decodeMicrotube(frame);
                break;
            case (byte)funList.FI_CLUTCHGOTACTIVATED:
                decodeMicrotube(frame);
                break;
            default:
                break;
        }
    }

    void decodePressure(byte[] frame)
    {
        pressureData[0] = BitConverter.ToSingle(frame, 3);
        pressureData[1] = BitConverter.ToSingle(frame, 8);
        pressureData[2] = BitConverter.ToSingle(frame, 13);
        pressureData[3] = BitConverter.ToSingle(frame, 18);
        pressureData[4] = BitConverter.ToSingle(frame, 23);
        pressureData[5] = BitConverter.ToSingle(frame, 28);

        flag_BMP280DataReady = true;

        //sw.WriteLine( "," + pressureData[0] );

        //Debug.Log("AirPressure:  "+ pressureData[0]+ "\t" + pressureData[1] + "\t" + pressureData[2] + "\t" + pressureData[3] + "\t" + pressureData[4] + "\t" + pressureData[5]);

        //Grapher.Log(pressureData[1], "Pressure Source", Color.white);
    }


    void decodeMicrotube(byte[] frame)
    {
        microtubeData[0] = (BitConverter.ToInt16(frame, 3));
        microtubeData[1] = (BitConverter.ToInt16(frame, 6));
        microtubeData[2] = (BitConverter.ToInt16(frame, 9));
        microtubeData[3] = (BitConverter.ToInt16(frame, 12));
        microtubeData[4] = (BitConverter.ToInt16(frame, 15));
        flag_MicrotubeDataReady = true;
        //fingerMappingLeftScript.UpdateFingerPosLeft();
        //graspingScript.GetCurrentMicrotubeData(fingerMappingLeftScript.normalizedData);

        //if (scissors != null)
        //{
        //    scissors.Scissors(fingerMappingLeftScript.normalizedData);
        //}

        //sw.Write(Environment.TickCount + "," + microtubeData[0]);
        //Debug.Log(DateTime.Now.ToString("HH:mm:ss.fff"));
        //sw.Flush();

        //Debug.Log(fingerMappingLeftScript.normalizedData[0] + "\t" + fingerMappingLeftScript.normalizedData[1] + "\t" + fingerMappingLeftScript.normalizedData[2] + "\t" + fingerMappingLeftScript.normalizedData[3] + "\t" + fingerMappingLeftScript.normalizedData[4]);

        //if (frame[1] == (byte)funList.FI_CLUTCHGOTACTIVATED)
        //{
        //    byte[] buf = { frame[2], frame[5], frame[8], frame[11], frame[14] };

        //    for (int i = 0; i < 5; i++)
        //    {
        //        if (buf[i] == (byte)0xff)
        //        {
        //            //graspingScript.hapticStartPosition[i] = microtubeData[i];
        //            graspingScript.hapticStartPosition[i] = fingerMappingLeftScript.normalizedData[i];
        //        }
        //    }



        //    //int clutchID = Array.IndexOf(buf, (byte)0xff);

        //    //graspingScript.hapticStartPosition[clutchID] = microtubeData[clutchID];
        //}
    }


    void OnDestroy()
    {
        if (btHelper != null)
            btHelper.Disconnect();
        flag_MicrotubeDataReady = false;

        //sw.Close();
        //fs.Close();
    }


    public bool airPresSourceCtrlStarted = false;
    public void AirPressureSourceControl()
    {
        if (airPresSourceCtrlStarted == false)
        {
            try
            {
                Encode.Instance.add_u8(0x01);               // Pressure source control start
                Encode.Instance.add_u8(sourcePres);                 // set pressure source to 50kPa
                byte[] buf = Encode.Instance.add_fun(0x02); // FI_STABLE_PRESSURE_CTRL
                Encode.Instance.clear_list();
                BTSend(buf);
                airPresSourceCtrlStarted = true;
            }
            catch (Exception e) { Debug.Log(e); }
        }
        else
        {
            try
            {
                Encode.Instance.add_u8(0x00);               // Pressure source control stop
                Encode.Instance.add_u8(0);                  // set pressure source to 0
                byte[] buf = Encode.Instance.add_fun(0x02); // FI_STABLE_PRESSURE_CTRL
                Encode.Instance.clear_list();
                BTSend(buf);
                airPresSourceCtrlStarted = false;
            }
            catch (Exception e) { Debug.Log(e); }
        }

    }


}
