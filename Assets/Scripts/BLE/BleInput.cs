using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
//using System.IO.Ports;
using System.Linq;
using UnityEngine.SceneManagement;


//NOTE: Upon building for windows standalone, make sure architecture is x86_64 (64 bits) for BLE functionality to work
public class BleInput : MonoBehaviour
{
    public static BleInput _instance;
    int number = 0;
    int firstcount = 0;
    string[] streamserial;
    float m_Timer = 10.0f;
    float deltat;
    bool isSleeve = false;

    public float[] sensorArray;
    public float[] ExtendSensorArray = new float[] { 9999, 9999, 9999, 9999, 9999 };
    public float[] FlexSensorArray = new float[] { 0, 0, 0, 0, 0 };
    public float[] InitialData;
    private float heading, pitch, roll;
    private float[] q = new float[] { 1.0f, 0.0f, 0.0f, 0.0f };    // vector to hold quaternion
    private float gx_ema, gy_ema, gz_ema, gx_bias, gy_bias, gz_bias;
    float b_x = 1, b_z = 0;                                // reference direction of flux in earth frame
    float w_bx = 0, w_by = 0, w_bz = 0;                    // estimate gyroscope biases error


    [Header("BLE Connection")]
    [Tooltip("Select the proper device name")]
    public string targetDeviceName;
    public string serviceUuid;
    [Tooltip("Service UUID for each finger")]
    public List<string> characteristicUuids;

    BLE ble;
    BLE.BLEScan scan;
    public bool isScanning = false, _connected = false, isTimerRunning = false, isCalibration = false;
    string deviceId = null;
    IDictionary<string, string> discoveredDevices = new Dictionary<string, string>();
    int devicesCount = 0;

    // BLE Threads 
    Thread scanningThread, connectionThread, readingThread, serialthread, calibrationThread;

    // GUI elements
    public Text TextThread, CalibrationText, MaxDataPointText; string screentext, m_text;
    public Button ButtonEstablishConnection, ButtonStartScan, ButtonCalibrate, ButtonBlueFPS, ButtonMySensor;
    public Toggle m_Toggle;

    bool Sensor1 = true;
    bool Sensor2 = true;
    bool Sensor3 = false;
    bool Sensor4 = false;
    bool Sensor5 = false;
    bool rollSensor = false;
    bool pitchSensor = false;
    bool headingSensor = false;
    private bool MiddlePanelDisplay;
    private bool ObjectPanelDisplay;
    private bool valueSet = false;
    private float valueChangeTimer = 0.5f;
    public Dropdown myDropdown;


    public float x, y, z;
    public int[] fingers;

    public bool isPause;

    public void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        fingers = new int[5];
        ble = new BLE();
        readingThread = new Thread(ReadBleData);
        sensorArray = new float[16];
        InitialData = new float[16];

        Scene scene = SceneManager.GetActiveScene();

        Debug.Log(scene.name);

        /*if (scene.name == "straightPathsLevel" || scene.name == "shooting")
        {
            myDropdown.onValueChanged.AddListener(delegate {
                DropdownValueChanged(myDropdown);
            });
        }

        else
        {
            if (scene.name == "FlappyBall_PC")
            {
                ButtonBlueFPS.gameObject.SetActive(false);
                ButtonMySensor.gameObject.SetActive(false);
            }


            if (ButtonEstablishConnection != null)
            {
                ButtonEstablishConnection.enabled = false;
            }
        }*/


        isPause = false;
    }

    void Update()
    {
        if (!isPause)
        {
            Scene scene = SceneManager.GetActiveScene();

            deltat = Time.deltaTime / 100f;
            //Scan BLE devices 
            if (isScanning)
            {
                /*if (scene.name != "straightPathsLevel" && scene.name != "shooting")
                {
                    if (ButtonStartScan.enabled)
                        ButtonStartScan.enabled = false;
                }*/

                if (discoveredDevices.Count > devicesCount)
                {
                    foreach (KeyValuePair<string, string> entry in discoveredDevices.ToList())
                    {
                        Debug.Log("Added device: " + entry.Key);
                    }
                    devicesCount = discoveredDevices.Count;
                }
            }

            // The target device was found.
            if (deviceId != null && deviceId != "-1")
            {
                // Target device is connected and GUI knows.
                if (ble.isConnected && _connected)
                {
                    if (!readingThread.IsAlive)
                    {
                        readingThread = new Thread(ReadBleData);
                        readingThread.Start();
                    }
                }
                // Target device is connected, but GUI hasn't updated yet.
                else if (ble.isConnected && !_connected)
                {
                    _connected = true;
                    /*if (scene.name != "straightPathsLevel" && scene.name != "shooting")
                    {
                        ButtonEstablishConnection.enabled = false;
                    }*/
                    TextThread.text = "Connected to target device:\n" + targetDeviceName;
                    Debug.Log("Connected to target device:\n" + targetDeviceName);
                }

                /*else if (scene.name != "straightPathsLevel" && scene.name != "shooting" && !ButtonEstablishConnection.enabled && !_connected)
                {
                    ButtonEstablishConnection.enabled = true;
                    Debug.Log("Found target device:\n" + targetDeviceName);
                }*/


            }


            // Display unto UI
            try
            {
                TextThread.text = screentext;
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            if (scene.name != "straightPathsLevel" && scene.name != "shooting")
            {
                if (isTimerRunning)
                {
                    if (ButtonCalibrate.enabled)
                    {
                        ButtonCalibrate.enabled = false;
                        ExtendSensorArray = new float[] { 9999, 9999, 9999, 9999, 9999 };
                        FlexSensorArray = new float[] { 0, 0, 0, 0, 0 };
                    }

                    if (m_Timer < 0)
                    {
                        isTimerRunning = false;
                        ButtonCalibrate.enabled = true;
                        m_Timer = 10;
                    }

                    m_Timer -= Time.deltaTime;
                    CalibrationText.text = m_text;
                    calibrationThread = new Thread(CalibrateFingers);
                    calibrationThread.Start();
                }
            }
        }
    }

    public void ToggleValueChanged(Toggle change)
    {
        //Find BLE device named "Sleeve"
        targetDeviceName = "Sleeve";
        isSleeve = true;
    }

    public void DropdownValueChanged(Dropdown thisDropdown)
    {
        switch (thisDropdown.value)
        {
            case 0:
                //do nothing
                break;
            case 1:
                //BlueFPS via BLE
                setScanTarget(1);
                break;
            case 2:
                //MySensor via BLE
                Debug.Log("Insert Here");
                setScanTarget(2);
                break;
            case 3:
                //MySensor via BLE
                setScanTarget(3);
                break;
        }
    }

    /* Functions to handle BLE */

    public void showScanOptions()
    {
        ButtonEstablishConnection.interactable = false;
        ButtonBlueFPS.gameObject.SetActive(true);
        ButtonMySensor.gameObject.SetActive(true);
    }

    public void setScanTarget(int typeNo)
    {
        Scene scene = SceneManager.GetActiveScene();

        switch (typeNo)
        {
            case 1:
                targetDeviceName = "BlueFPS";
                serviceUuid = "{6e400001-b5a3-f393-e0a9-e50e24dcca9e}";
                characteristicUuids = new List<string>() { "{6e400003-b5a3-f393-e0a9-e50e24dcca9e}" };
                break;

            case 2:
                targetDeviceName = "MySensor";
                serviceUuid = "{056C01FA-4FA1-44E6-972E-820248BEE382}";
                characteristicUuids = new List<string>() { "{59D0DB08-B55E-49A6-A2C8-2AA42CCBAAFC}" };
                break;
            case 3:
                targetDeviceName = "Tv450u";
                serviceUuid = "{0000ffe0-0000-1000-8000-00805f9b34fb}";
                characteristicUuids = new List<string>() { "{0000ffe4-0000-1000-8000-00805f9b34fb}" };
                break;
            case 4:
                targetDeviceName = "HaptGlove L Left";
                serviceUuid = "{000000ff-0000-1000-8000-00805f9b34fb}";
                characteristicUuids = new List<string>() { "{0000ff01-0000-1000-8000-00805f9b34fb}" };
                break;
        }
        /*if (scene.name != "straightPathsLevel" && scene.name != "shooting")
        {
            ButtonEstablishConnection.interactable = true;
            ButtonBlueFPS.gameObject.SetActive(false);
            ButtonMySensor.gameObject.SetActive(false);
        }*/

        if (SceneManager.GetActiveScene().name == "Pagoda_Street_V4")
        {
            //LevelManager.singleton.BTconnectionType = typeNo;
        }

        //ButtonEstablishConnection.interactable = true;
        //ButtonBlueFPS.gameObject.SetActive(false);
        //ButtonMySensor.gameObject.SetActive(false);
        StartScanHandler();
    }

    //Start BLE Scan
    public void StartScanHandler()
    {
        devicesCount = 0;
        isScanning = true;
        discoveredDevices.Clear();
        deviceId = null;
        scanningThread = new Thread(ScanBleDevices);
        scanningThread.Start();
        Debug.Log("Scanning for..." + targetDeviceName);
        screentext = "Scanning for.. " + targetDeviceName;
    }

    // Start establish BLE connection with
    // target device in dedicated thread.
    public void StartConHandler()
    {
        connectionThread = new Thread(ConnectBleDevice);
        connectionThread.Start();
    }

    // Scan BLE devices
    private void ScanBleDevices()
    {
        scan = BLE.ScanDevices();
        Debug.Log("BLE.ScanDevices() started.");
        screentext = "BLE.ScanDevices() started.";
        scan.Found = (_deviceId, deviceName) =>
        {
            Debug.Log("found device with name: " + deviceName);
            screentext = "found device with name: " + deviceName;
            discoveredDevices.Add(_deviceId, deviceName);

            //if found the target device, immediately stop scan and attempt to connect
            if (deviceId == null && deviceName.Contains(targetDeviceName))
            {
                deviceId = _deviceId;
                StartConHandler();
            }
        };

        scan.Finished = () =>
        {
            isScanning = false;
            screentext = "scan finished";
            Debug.Log("scan finished");
            if (deviceId == null)
                deviceId = "-1";
        };
        while (deviceId == null)
            Thread.Sleep(500);
        scan.Cancel();
        scanningThread = null;
        isScanning = false;

        if (deviceId == "-1")
        {
            screentext = "no device found!";
            Debug.Log("no device found!");
            return;
        }
    }

    // Connect BLE device
    private void ConnectBleDevice()
    {
        if (deviceId != null)
        {
            try
            {
                screentext = "Attempting to connect";
                ble.Connect(deviceId,
                serviceUuid,
                characteristicUuids.ToArray());
            }
            catch (Exception e)
            {
                Debug.Log("Could not establish connection to device with ID " + deviceId + "\n" + e);
                screentext = "Could not establish connection to device with ID " + deviceId + "\n";
            }
        }
        if (ble.isConnected)
            Debug.Log("Connected to: " + targetDeviceName);
            screentext = "Connected to: " + targetDeviceName;
    }

    // Read BLE Data
    private void ReadBleData(object obj)
    {
        //string value = BLE.ReadPackage();
        byte[] bytes = BLE.ReadBytes(); //data input via bytes
        Debug.Log(BitConverter.ToString(bytes));
        //ProcessByteData(bytes);
    }

    // Process BLE Port Data
    void ProcessByteData(byte[] bytes)
    {
        if (targetDeviceName == "MySensor")
        {
            screentext = "Streaming via BLE, ";

            for (int i = 0; i < 5; i++)
            {
                sensorArray[i] = (bytes[2 + i * 3] | (bytes[1 + i * 3] << 8) | (bytes[i * 3] << 16));
            }


            screentext +=  sensorArray[0];

            

        }

        if (targetDeviceName.Contains("Tv450u"))
        {
            screentext = "Streaming via BLE, ";
            //Debug.Log("Streaming");

            byte[] temp = new byte[4];
            for (int i = 0; i < 16; i++)
            {
                temp[0] = bytes[i * 4 + 2 + 3];
                temp[1] = bytes[i * 4 + 2 + 2];
                temp[2] = bytes[i * 4 + 2 + 1];
                temp[3] = bytes[i * 4 + 2 + 0];
                sensorArray[i] = (float)BitConverter.ToInt32(temp, 0);

                //Debug.Log(sensorArray[i] + "," + i);
            }


            //ConvertIMU(sensorArray);


            heading = heading * Mathf.Rad2Deg + 180;
            pitch = pitch * Mathf.Rad2Deg + 180;
            roll = roll * Mathf.Rad2Deg + 180;
            Baseline(sensorArray);

            screentext += "\nSensorArray[0]: " + sensorArray[0] + "," + "Baseline[0]: " + InitialData[0];

            //screentext += "\nSensorArray: " + sensorArray[0] + "," + sensorArray[1] + "," + sensorArray[2] + "," + +sensorArray[3] + "," + sensorArray[4] + ",\n" + sensorArray[8] + ", " + sensorArray[9] + ", " + sensorArray[10] + ",\n" + sensorArray[11] + ", " + sensorArray[12] + ", " + sensorArray[13] + ",\n " + roll + "," + pitch + "," + heading;
            //ToggleScreenDisplay(sensorArray[0], sensorArray[1], sensorArray[2], sensorArray[3], sensorArray[4], ((roll * 100f) + 2000f), ((pitch * 100) + 2000), ((heading*100) + 2000));
        }
        
    }


    // Reset BLE handler
    public void ResetHandler()
    {
        // Reset previous discovered devices
        discoveredDevices.Clear();
        deviceId = null;
        CleanUp();

    }
    
    /* Functions to initiate calibration */

    // Begin calibration of fingers
    public void StartCalibrationHandler()
    {
        isTimerRunning = true;
    }

    // Store calibrated values
    public void CalibrateFingers()
    {

        if (m_Timer > 5)
        {

            for (int i = 0; i < ExtendSensorArray.Length; i++)
            {
                ExtendSensorArray[i] = (sensorArray[i] + ExtendSensorArray[i]) / 2; //obtain a moving average
            }

            m_text = "Starting calibration: please open your fingers fully for " + (m_Timer - 5f).ToString("00") + " seconds.\n" + "Extend fingers: " + ExtendSensorArray[0] + "," + ExtendSensorArray[1] + "," + ExtendSensorArray[2] + "," + ExtendSensorArray[3];
        }

        else
        {
            if (m_Timer > 0)
            {
                for (int i = 0; i < FlexSensorArray.Length; i++)
                {
                    FlexSensorArray[i] = (sensorArray[i] + FlexSensorArray[i]) / 2; //obtain a moving average
                }
                m_text = "Please close your fingers fully for " + m_Timer.ToString("00") + " seconds.\n" + "Flex fingers: " + FlexSensorArray[0] + "," + FlexSensorArray[1] + "," + FlexSensorArray[2] + "," + FlexSensorArray[3];
            }

            else
            {
                m_text = "Calibration is done!\n" + "Extend fingers: " + ExtendSensorArray[0] + "," + ExtendSensorArray[1] + "," + ExtendSensorArray[2] + "," + ExtendSensorArray[3] + "\n" + "Flex fingers: " + FlexSensorArray[0] + "," + FlexSensorArray[1] + "," + FlexSensorArray[2] + "," + FlexSensorArray[3];
                
                //Set calibration data to initial data
                InitialData[0] = ExtendSensorArray[0]; 
                InitialData[1] = ExtendSensorArray[1];
                InitialData[2] = ExtendSensorArray[2];
                InitialData[3] = ExtendSensorArray[3];
                isCalibration = true;
            }
        }
    }

    // Set initial data as threshold
    void Baseline(float[] sensordata)
    {
        //Debug.Log(sensordata[1]);
        if(!isCalibration)
        {
            if (number == 0 && sensordata[0] > 1)
            {
                InitialData[0] = sensordata[0];

                number = 1;
            }

            if (firstcount == 0)
            {
                InitialData[1] = sensordata[1];
                InitialData[2] = sensordata[2];
                InitialData[3] = sensordata[3];
                firstcount = 1;
            }

            firstcount++;
            //Debug.Log(sensordata[1] - InitialData[1] + "," + firstcount);
            if (firstcount > 60) firstcount = 0;
        }
    }
    
  
    // Handle GameObject destroy
    void OnDestroy()
    {
        ResetHandler();
    }

    // Handle Quit Game
    void OnApplicationQuit()
    {
        ResetHandler();
    }

    // Prevent threading issues and free BLE stack.
    // Can cause Unity to freeze and lead
    // to errors when omitted.
    void CleanUp()
    {
        try
        {
            scan.Cancel();
        }
        catch (NullReferenceException e)
        {
            Debug.Log("Scan never initialized.\n" + e);
        }


        try
        {
            ble.Close();
        }
        catch (NullReferenceException e)
        {
            Debug.Log("ble never initialized.\n" + e);
        }

        try
        {
            scanningThread.Abort();
        }
        catch (NullReferenceException e)
        {
            Debug.Log("Scan thread never initialized.\n" + e);
        }

        try
        {
            connectionThread.Abort();
        }
        catch (NullReferenceException e)
        {
            Debug.Log("Connection thread never initialized.\n" + e);
        }
    }


    void ExitProgram()
    {
        OnApplicationQuit();
        CleanUp();
        Application.Quit();
    }
}
