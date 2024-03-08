using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if !UNITY_EDITOR
    using Windows.Networking;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;
#endif

//Able to act as a reciever 
public class TcpServer : MonoBehaviour
{
    public String _input = "Waiting";
    private string screenText;
    public TMP_Text logText;

#if !UNITY_EDITOR
        StreamSocket socket;
        StreamSocketListener listener;
        String port;
        String message;
#endif

    // Use this for initialization
    void Start()
    {
#if !UNITY_EDITOR
        listener = new StreamSocketListener();
        port = "33333";
        listener.ConnectionReceived += Listener_ConnectionReceived;
        listener.Control.KeepAlive = false;

        Listener_Start();
#endif
    }

#if !UNITY_EDITOR
    private async void Listener_Start()
    {
        Debug.Log("Listener started");
        screenText =screenText +  "\nListener started";

        try
        {
            //await listener.BindServiceNameAsync(port);

            HostName hostName = new HostName("172.20.10.5");
            await listener.BindEndpointAsync(hostName, port);
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e.Message);
            screenText =screenText +  "\nError: " + e.Message;
        }

        Debug.Log("Listening");
        screenText =screenText +  "\nListening";
    }

    private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        Debug.Log("Connection received");
        screenText = screenText + "\nConnection received";

        try
        {
            while (true) {

                    //using (var dw = new DataWriter(args.Socket.OutputStream))
                    //{
                    //    dw.WriteString("Hello There");
                    //    await dw.StoreAsync();
                    //    dw.DetachStream();
                    //}  

                    using (var dr = new DataReader(args.Socket.InputStream))
                    {
                        dr.InputStreamOptions = InputStreamOptions.Partial;
                        await dr.LoadAsync(51);
                        byte[] dataBuf = new byte[51];
                        dr.ReadBytes(dataBuf);
                        Debug.Log("received: " + BitConverter.ToString(dataBuf));
                        screenText = "received: " + BitConverter.ToString(dataBuf);
                    }
            }
        }
        catch (Exception e)
        {
            Debug.Log("disconnected!!!!!!!! " + e);
            screenText = "disconnected!!!!!!!! " + e;
        }

    }

#endif

    void Update()
    {
        //this.GetComponent<TextMesh>().text = _input;
        logText.text = screenText;
    }
}









//using System;
//using System.IO;
//using System.Collections;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Sockets;
//using System.Threading;
//using TMPro;
//using UnityEngine;

//#if !UNITY_EDITOR
//using Windows.Networking;
//#endif


//public class TcpServer : MonoBehaviour
//{
//    public string deviceName = "HaptGloveAR_L_Left";
//    private string PortNumber = "33333";

//#if UNITY_EDITOR
//    private Socket tcpServer, tcpClient;
//    private Thread serverStartThread, tcpRecevingThread;
//    private static List<TcpClient> clientList = new List<TcpClient>();
//#else

//#endif

//    public bool isConnected = false;
//    private float timer = 1;


//    public Int16[] microtubeData = new Int16[5];
//    public float[] pressureData = new float[6];
//    public byte sourcePres = 68; //kpa

//    public bool flag_MicrotubeDataReady = false;
//    public bool flag_BMP280DataReady = false;

//    private List<byte> buffer = new List<byte>(1024);
//    private byte[] oneFrame = new byte[128];

//    public bool isCalibration = false;
//    public float[] InitialData;


//    public TMP_Text logText;
//    private string screenText;


//    void Start()
//    {
//#if UNITY_EDITOR
//        tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

//        serverStartThread = new Thread(ServerStart);
//        serverStartThread.Start();
//        serverStartThread.IsBackground = true;

//        InitialData = new float[5];
//#else
//        ServerStart();
//#endif
//    }

//#if !UNITY_EDITOR
//    private async void ServerStart()
//    {
//        try
//        {
//            var streamSocketListener = new Windows.Networking.Sockets.StreamSocketListener();

//            // The ConnectionReceived event is raised when connections are received.
//            streamSocketListener.ConnectionReceived += this.StreamSocketListener_ConnectionReceived;

//            // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
//            HostName hostName = new HostName("172.20.10.5");
//            await streamSocketListener.BindEndpointAsync(hostName, PortNumber);
//            //await streamSocketListener.BindServiceNameAsync(StreamSocketAndListenerPage.PortNumber);

//            screenText = "server is listening...";
//        }
//        catch (Exception ex)
//        {
//            Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
//            screenText = webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message;
//        }
//    }

//        private async void StreamSocketListener_ConnectionReceived(Windows.Networking.Sockets.StreamSocketListener sender, Windows.Networking.Sockets.StreamSocketListenerConnectionReceivedEventArgs args)
//        {
//            screenText = "client accepted";
//        }
//#endif

//#if UNITY_EDITOR
//    public void ServerStartThreadInit()
//    {
//        Debug.Log("AAAA");
//        if (serverStartThread.IsAlive)
//        {
//            Debug.Log("Abort");
//            serverStartThread.Abort();
//            if (tcpRecevingThread.IsAlive)
//            {
//                tcpRecevingThread.Abort();
//            }
//        }
//        else
//        {
//            if (tcpRecevingThread.IsAlive)
//            {
//                tcpRecevingThread.Abort();
//            }
//            Debug.Log("Create");

//            serverStartThread = new Thread(ServerStart);
//            serverStartThread.Start();
//            serverStartThread.IsBackground = true;
//        }
//    }

//    void ServerStart()
//    {
//        if (!tcpServer.IsBound)
//        {
//            screenText = "Start bind";
//            Debug.Log("Start bind");
//            EndPoint point = new IPEndPoint(IPAddress.Parse("172.20.10.3"), 33333);//"172.20.10.3"
//            tcpServer.Bind(point);
//            screenText = "Bind success";
//            Debug.Log("Bind success");
//        }

//        screenText = "Start listen";
//        Debug.Log("Start listen");
//        tcpServer.Listen(100);

//        screenText = "Waiting client to connect...";
//        Debug.Log("Waiting client to connect...");
//        tcpClient = tcpServer.Accept();
//        tcpRecevingThread = new Thread(ReceiveData);
//        tcpRecevingThread.Start();
//        tcpRecevingThread.IsBackground = true;

//        screenText = "Client accepted";
//        Debug.Log("Client accepted");

//        isConnected = true;
//        //while (true)
//        //{
//        //    tcpClient = tcpServer.Accept();
//        //    TcpClient client = new TcpClient(tcpClient);
//        //    clientList.Add(client);
//        //}
//        serverStartThread.Abort();
//    }


//    private enum funList : byte
//    {
//        FI_BMP280 = 0x01,

//        FI_MICROTUBE = 0x04,
//        FI_CLUTCHGOTACTIVATED = 0x05
//    };
//    void ReceiveData()
//    {
//        int length = 0;
//        byte[] tem_data = new byte[1024];
//        Queue DataQueue = new Queue();
//        screenText = "Receiving data from client...";
//        Debug.Log("Receiving data from client...");

//        while (true)
//        {
//            try
//            {
//                length = tcpClient.Receive(tem_data);
//                if (length > 0)
//                {
//                    Decode decode = new Decode();
//                    Queue FrameQueue = decode.GetFrameQueue(tem_data, length);
//                    while (FrameQueue.Count > 0)
//                    {
//                        byte[] frame = (byte[])FrameQueue.Dequeue();
//                        int func = decode.GetFunc(frame);
//                        switch (func)
//                        {
//                            case (byte)funList.FI_BMP280:         //BMP280
//                                decodePressure(frame);
//                                break;
//                            case (byte)funList.FI_MICROTUBE:      //Microtube
//                                decodeMicrotube(frame);
//                                break;
//                            case (byte)funList.FI_CLUTCHGOTACTIVATED:
//                                decodeMicrotube(frame);
//                                break;
//                            default:
//                                break;
//                        }
//                    }
//                }
//            }
//            catch { }
//        }

//    }


//    void decodePressure(byte[] frame)
//    {
//        pressureData[0] = BitConverter.ToSingle(frame, 3);
//        pressureData[1] = BitConverter.ToSingle(frame, 8);
//        pressureData[2] = BitConverter.ToSingle(frame, 13);
//        pressureData[3] = BitConverter.ToSingle(frame, 18);
//        pressureData[4] = BitConverter.ToSingle(frame, 23);
//        pressureData[5] = BitConverter.ToSingle(frame, 28);

//        flag_BMP280DataReady = true;

//        //sw.WriteLine( "," + pressureData[0] );

//        //Debug.Log("AirPressure:  "+ pressureData[0]+ "\t" + pressureData[1] + "\t" + pressureData[2] + "\t" + pressureData[3] + "\t" + pressureData[4] + "\t" + pressureData[5]);

//        //Grapher.Log(pressureData[1], "Pressure Source", Color.white);
//    }

//    void decodeMicrotube(byte[] frame)
//    {
//        microtubeData[0] = (BitConverter.ToInt16(frame, 3));
//        microtubeData[1] = (BitConverter.ToInt16(frame, 6));
//        microtubeData[2] = (BitConverter.ToInt16(frame, 9));
//        microtubeData[3] = (BitConverter.ToInt16(frame, 12));
//        microtubeData[4] = (BitConverter.ToInt16(frame, 15));

//        if (isCalibration == false)
//        {
//            for (int i = 0; i < 5; i++)
//            {
//                InitialData[i] = microtubeData[i];
//            }

//            isCalibration = true;
//        }
//        flag_MicrotubeDataReady = true;


//        //fingerMappingLeftScript.UpdateFingerPosLeft();
//        //Debug.Log(microtubeData[0] + "\t" + microtubeData[1] + "\t" + microtubeData[2] + "\t" + microtubeData[3] + "\t" + microtubeData[4]);
//        screenText = "Data: " + microtubeData[0] + "--" + microtubeData[1] + "--" + microtubeData[2] + "--" +
//                       microtubeData[3] + "--" + microtubeData[4] + "  ini: " + InitialData[0];
//        //screenText = "cur: " + microtubeData[0] + "  ini: " + InitialData[0];
//    }
//#endif

//    void Update()
//    {
//        logText.text = screenText;
//        //if (tcpClient != null)
//        //{
//        //    timer -= Time.deltaTime;
//        //    if (timer < 0)
//        //    {
//        //        //Debug.Log("wolaile");
//        //        byte[] data = new byte[] { 0x01, 0x02, 0x04 };
//        //        tcpClient.Send(data);
//        //        timer = 1;
//        //    }

//        //}
//    }


//    void OnDestroy()
//    {
//        isConnected = false;

//#if UNITY_EDITOR
//        tcpServer.Close();
//#endif
//    }

//}
