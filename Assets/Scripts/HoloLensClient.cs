using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using TMPro;
using Microsoft.CognitiveServices.Speech;
using UnityEngine.Events;
#if !UNITY_EDITOR
    using Windows.Networking;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;
    //using Windows.Media.SpeechSynthesis;
    using Windows.Media.Playback;
    using Windows.Media.Core;
    using Windows.UI.Core;
    using Windows.Media.Capture;
    using System.Diagnostics;
#endif

//Able to act as a reciever 
public class HoloLensClient : MonoBehaviour
{
    // UI
    //public TMP_InputField inputField;
    public TextMeshProUGUI displayText;
    public TextMeshProUGUI RecognizedText;

    private string recognizedString = "";
    private System.Object threadLocker = new System.Object();

    // Microsoft Cognitive Speech Service
    // public string SpeechServiceAPIKey = "your speech service api key";
    // public string SpeechServiceRegion = "your service region";
    [HideInInspector] public string SpeechServiceAPIKey = "890b6fc23e9742eca9f5912f65186a1a";
    [HideInInspector] public string SpeechServiceRegion = "southeastasia";
    private SpeechRecognizer recognizer;
    string fromLanguage = "en-US";
    private bool micPermissionGranted = false;
    public AudioSource audioSource; 
    private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
    private readonly Queue<Action> _executeOnMainThread = new Queue<Action>();

#if !UNITY_EDITOR
    private MediaPlayer mediaPlayer = new MediaPlayer();
#endif

    // Data
    private string gender;
    private string prosody_rate;
    private string pitch;

#if !UNITY_EDITOR
        StreamSocket socket;
        StreamSocketListener listener;
        String port = "5000";
        private DataWriter sharedDataWriter;       
#endif

    //Unity events
    public UnityAction<string> onSpeechReconized;
    public UnityAction<string> onGPTReplyReceived;

    //Show Debug.Log
    public TextMeshProUGUI debugLogText;
    [HideInInspector] public string debugLog;


    // Use this for initialization
    void Start()
    {
        UnityEngine.Debug.Log("Start");
        debugLog += "\n" + "start";
        micPermissionGranted = true;
        StartContinuous();
    #if !UNITY_EDITOR
        InitializeTCPListener();  // Initialize and start the TCP listener
        StartCoroutine(InitializeMediaCapture());
        //RunPythonScript();
    #endif
    }


#if !UNITY_EDITOR
    private void RunPythonScript(){
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = Application.dataPath + "/Scripts/runPythonScript.bat";
        startInfo.CreateNoWindow = true;
        startInfo.UseShellExecute = false;
        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
    }

    #region Receive Data and Vocalize
    private async Task ReceiveSettings(DataReader dr)
    {
debugLog += "\n" + "ReceiveSettings 1";
        await dr.LoadAsync(sizeof(uint));
        uint messageLength = dr.ReadUInt32();
        await dr.LoadAsync(messageLength);
        string settingsJson = dr.ReadString(messageLength);
        var settings = JsonUtility.FromJson<Settings>(settingsJson);
debugLog += "\n" + "ReceiveSettings 2";

        this.gender = settings.gender;
        this.prosody_rate = settings.prosody_rate;
        this.pitch = settings.pitch;

        UnityEngine.Debug.Log(settings.gender);
debugLog += "\n" + settings.gender;

    }
    private class Settings
    {
        public string gender;
        public string prosody_rate;
        public string pitch;
    }
    private async void VocalizeMessage2(string message){
        var config = SpeechConfig.FromSubscription(SpeechServiceAPIKey, SpeechServiceRegion);
        config.SpeechSynthesisLanguage = "en-US";  // Set the language
 
        // Set the voice name
        if (gender == "Male")
        {
            config.SpeechSynthesisVoiceName = "en-US-GuyNeural"; // Choose a male voice
        }
        else if (gender == "Female")
        {
            config.SpeechSynthesisVoiceName = "en-US-Jessa24kRUS"; // Choose a female voice
        }
        else
        {
            config.SpeechSynthesisVoiceName = "en-US-Jessa24kRUS"; // Default choice
        }

        using (var synthesizer = new SpeechSynthesizer(config))
        {
            // Using SSML to specify pitch, speaking rate, etc.
            string ssml = $@"
                <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
                    <voice name='{config.SpeechSynthesisVoiceName}'>
                        <prosody rate='{prosody_rate}' pitch='{pitch}'>
                            {message}
                        </prosody>
                    </voice>
                </speak>";

            // Synthesize the SSML to a stream
            using (var result = await synthesizer.SpeakSsmlAsync(ssml))
            {
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    var audioData = result.AudioData;
                    // Convert byte array to WAV, then to AudioClip
                    WAV wav = new WAV(audioData);
                    AudioClip audioClip = AudioClip.Create("SynthesizedSpeech", wav.SampleCount, 1, wav.Frequency, false);
                    audioClip.SetData(wav.LeftChannel, 0);
                    audioSource.clip = audioClip;
                    audioSource.Play();
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine("CANCELED: Did you update the subscription info?");
                    }
                }
            }
        }
    }
    #endregion

    #region Send Force Data to Python
public enum forceLevel{
    small,
    medium,
    large
}

public async Task SendForceDetectedMessage(forceLevel forcelevel){
//    if (!pressForce.HasValue){
//        UnityEngine.Debug.LogError("Error: Press force is null.");
//debugLog += "Error: Press force is null.";
//        return;
//    }
//    else{
//        UnityEngine.Debug.Log("Force press detected.");
//debugLog += "Force press detected.";

//    }

    string text = "FORCE PRESS DETECTED. [" + forcelevel.ToString() +"]";

    // Send data to python
    if (sharedDataWriter != null)
    {
        try
        {
            uint responseLength = sharedDataWriter.MeasureString(text);
            sharedDataWriter.WriteUInt32(responseLength);
            sharedDataWriter.WriteString(text);
            await sharedDataWriter.StoreAsync();
            await sharedDataWriter.FlushAsync();
            UnityEngine.Debug.Log("Message sent to TCP client: " + text);
debugLog += "Message sent to TCP client: " + text;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Error sending message to TCP client: " + e.Message);
debugLog += "Error sending message to TCP client: " + e.Message;
        }
    }
    else
    {
        UnityEngine.Debug.LogError("TCP connection is not established. Unable to send message.");
debugLog += "TCP connection is not established. Unable to send message.";
    }
}
    #endregion

    #region Microphone
    private IEnumerator InitializeMediaCapture()
    {
        Task initTask = InitializeMediaCaptureAsync();
        while (!initTask.IsCompleted)
        {
            yield return null;
        }

        if (initTask.IsFaulted)
        {
            // Handle any exceptions (if any)
            UnityEngine.Debug.LogError("Initialization failed: " + initTask.Exception.ToString());
debugLog += "\n" + "InitializeMediaCapture failed";
        }
    }
    public async Task InitializeMediaCaptureAsync()
    {
        MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings
        {
            StreamingCaptureMode = StreamingCaptureMode.Audio
        };

        MediaCapture mediaCapture = new MediaCapture();
        try
        {
            await mediaCapture.InitializeAsync(settings);
            UnityEngine.Debug.Log("Microphone is accessible");
debugLog += "\n" + "Microphone is accessible";
            // Additional logic for when access is granted
        }
        catch (UnauthorizedAccessException)
        {
            UnityEngine.Debug.Log("Microphone access denied");
debugLog += "\n" + "Microphone access denied";
            // Logic to handle when the user has denied access
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log($"Initialization failed: {ex.Message}");
debugLog += "\n" + "InitializeMediaCaptureAsync failed";
            // Handle other exceptions
        }
    }
    private async void VocalizeMessage(string message)
    {
        using (var synthesizer = new Windows.Media.SpeechSynthesis.SpeechSynthesizer())
        {
            // Synthesize the text to a stream
            using (var stream = await synthesizer.SynthesizeTextToStreamAsync(message))
            {
                // Set the source of the MediaPlayer to the synthesized audio stream
                mediaPlayer.Source = MediaSource.CreateFromStream(stream, stream.ContentType);
                mediaPlayer.Play(); // Play the synthesized speech
            }
        }
    }
    #endregion

    #region TCP Connection
    private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        UnityEngine.Debug.Log("Connection received");
debugLog += "\n" + "Connection received";

        //currentArgs = args; // Store the connection arguments
        sharedDataWriter = new DataWriter(args.Socket.OutputStream);
        
        try
        {
            using (var dr = new DataReader(args.Socket.InputStream))
            {
                dr.InputStreamOptions = InputStreamOptions.Partial;
                dr.ByteOrder = ByteOrder.LittleEndian;
debugLog += "\n" + "-3";
                //await ReceiveSettings(dr); 
debugLog += "\n" + "-2";
                await dr.LoadAsync(sizeof(uint)); // Load data for message length
debugLog += "\n" + "-1";

                // using (var dw = new DataWriter(args.Socket.OutputStream))
                // {
                    while (true)
                    {
                        UnityEngine.Debug.Log("0");
debugLog += "\n" + "0";
                        // Read the length of the incoming message
                        if (dr.UnconsumedBufferLength < sizeof(uint))
                        {
                            await dr.LoadAsync(sizeof(uint) - dr.UnconsumedBufferLength);
                        }

                        uint messageLength = dr.ReadUInt32();
                        if (dr.UnconsumedBufferLength < messageLength)
                        {
                            await dr.LoadAsync(messageLength - dr.UnconsumedBufferLength);
                        }

                        UnityEngine.Debug.Log("1");
debugLog += "\n" + "1";
                        string input = dr.ReadString(messageLength);
                        UnityEngine.Debug.Log("Received: " + input);
                        VocalizeMessage(input);
                        // Update UI on the main thread
                        _executeOnMainThread.Enqueue(() => UpdateDisplayText(input));

                        //onGPTReplyReceived(input);
                        

                        UnityEngine.Debug.Log("2");
debugLog += "\n" + "2";

                        // Respond back to the sender
                        // string response = "Acknowledged: " + input;
                        // uint responseLength = dw.MeasureString(response);
                        // dw.WriteUInt32(responseLength);
                        // dw.WriteString(response);
                        // await dw.StoreAsync();
                        // await dw.FlushAsync(); // Ensure the data is sent immediately
                    }
                //}
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("Error in Listener_ConnectionReceived: " + e.ToString());

            debugLog += "\n" + "Error in Listener_ConnectionReceived: ";
        }
    }  
    private async void InitializeTCPListener()
    {
        try
        {
            listener = new StreamSocketListener();
            listener.ConnectionReceived += Listener_ConnectionReceived;
            listener.Control.KeepAlive = false;
            await listener.BindServiceNameAsync(port);
            UnityEngine.Debug.Log("TCP Listener started on port " + port);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Error initializing TCP listener: " + e.Message);
debugLog += "\n" + "Error initializing TCP listener: ";
        }
    }
    #endregion

#endif

    #region Speech Recognition
    public void StartContinuous()
    {
        string errorString = "";
        if (micPermissionGranted)
        {
            StartContinuousRecognition();
        }
        else
        {
            errorString = "This app cannot function without access to the microphone.";
            UnityEngine.Debug.LogFormat(errorString);
            errorString = "ERROR: Microphone access denied.";
            debugLog += "\n" + "ERROR: Microphone access denied.";
            UnityEngine.Debug.LogFormat(errorString);
        }
    }

    void CreateSpeechRecognizer()
    {
        string errorString = "";
        if (SpeechServiceAPIKey.Length == 0 || SpeechServiceAPIKey == "YourSubscriptionKey")
        {
            errorString = "You forgot to obtain Cognitive Services Speech credentials and inserting them in this app." + Environment.NewLine +
                               "See the README file and/or the instructions in the Awake() function for more info before proceeding.";
            UnityEngine.Debug.LogFormat(errorString);
            errorString = "ERROR: Missing service credentials";
            debugLog += "\n" + "ERROR: Missing service credentials";
            UnityEngine.Debug.LogFormat(errorString);

            return;
        }
        UnityEngine.Debug.LogFormat("Creating Speech Recognizer.");

        if (recognizer == null)
        {
            SpeechConfig config = SpeechConfig.FromSubscription(SpeechServiceAPIKey, SpeechServiceRegion);
            config.SpeechRecognitionLanguage = fromLanguage;
            recognizer = new SpeechRecognizer(config);

            if (recognizer != null)
            {
                // Subscribes to speech events.
                recognizer.Recognizing += RecognizingHandler;
                recognizer.Recognized += RecognizedHandler;
                recognizer.SpeechStartDetected += SpeechStartDetectedHandler;
                recognizer.SpeechEndDetected += SpeechEndDetectedHandler;
                recognizer.Canceled += CanceledHandler;
                recognizer.SessionStarted += SessionStartedHandler;
                recognizer.SessionStopped += SessionStoppedHandler;
            }
        }
        UnityEngine.Debug.LogFormat("CreateSpeechRecognizer exit");
        debugLog += "\n" + "CreateSpeechRecognizer exit";
    }

    private async void StartContinuousRecognition()
    {
        UnityEngine.Debug.LogFormat("Starting Continuous Speech Recognition.");
        debugLog += "\n" + "Starting Continuous Speech Recognition.";
        CreateSpeechRecognizer();

        if (recognizer != null)
        {
            UnityEngine.Debug.LogFormat("Starting Speech Recognizer.");
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            UnityEngine.Debug.LogFormat("Speech Recognizer is now running.");
            debugLog += "\n" + "Speech Recognizer is now running.";
        }
        UnityEngine.Debug.LogFormat("Start Continuous Speech Recognition exit");
        debugLog += "\n" + "Start Continuous Speech Recognition exit";
    }

    private void SessionStartedHandler(object sender, SessionEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"\n    Session started event. Event: {e.ToString()}.");
    }

    private void SessionStoppedHandler(object sender, SessionEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"\n    Session event. Event: {e.ToString()}.");
        UnityEngine.Debug.LogFormat($"Session Stop detected. Stop the recognition.");
    }

    private void SpeechStartDetectedHandler(object sender, RecognitionEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"SpeechStartDetected received: offset: {e.Offset}.");
    }

    private void SpeechEndDetectedHandler(object sender, RecognitionEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"SpeechEndDetected received: offset: {e.Offset}.");
        UnityEngine.Debug.LogFormat($"Speech end detected.");
    }

    // "Recognizing" events are fired every time we receive interim results during recognition (i.e. hypotheses)
    private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizingSpeech)
        {
            //UnityEngine.Debug.LogFormat($"HYPOTHESIS: Text={e.Result.Text}");
            // lock (threadLocker)
            // {
            //     recognizedString = $"HYPOTHESIS: {Environment.NewLine}{e.Result.Text}";
            // }
        }
    }

    // "Recognized" events are fired when the utterance end was detected by the server
    private async void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizedSpeech)
        {
            string text = e.Result.Text;
            if (!String.IsNullOrEmpty(text)){
                UnityEngine.Debug.LogFormat($"RECOGNIZED: Text={text}");
                await semaphoreSlim.WaitAsync();
                try
                {
                    recognizedString = text;

                    //onSpeechReconized(text);

                    // TODO: Handle the input text, e.g., send it to another function or process it
#if !UNITY_EDITOR
                    uint responseLength = sharedDataWriter.MeasureString(text);
                    sharedDataWriter.WriteUInt32(responseLength);
                    sharedDataWriter.WriteString(text);
                    await sharedDataWriter.StoreAsync();
                    await sharedDataWriter.FlushAsync();
                    UnityEngine.Debug.Log("Input Text sent to TCP client: " + text);
#endif
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }  
        }
        else if (e.Result.Reason == ResultReason.NoMatch)
        {
            UnityEngine.Debug.LogFormat($"NOMATCH: Speech could not be recognized.");
        }
    }

    // "Canceled" events are fired if the server encounters some kind of error.
    // This is often caused by invalid subscription credentials.
    private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e)
    {
        string errorString = "";
        UnityEngine.Debug.LogFormat($"CANCELED: Reason={e.Reason}");

        errorString = e.ToString();
        if (e.Reason == CancellationReason.Error)
        {
            UnityEngine.Debug.LogFormat($"CANCELED: ErrorDetails={e.ErrorDetails}");
            UnityEngine.Debug.LogFormat($"CANCELED: Did you update the subscription info?");
        }
    }
#endregion

    void Update()
    {
        while (_executeOnMainThread.Count > 0) {
            _executeOnMainThread.Dequeue().Invoke();
        }
        // Check if the Enter key is released
        //if (Input.GetKeyUp(KeyCode.Return))
        //{
        //    SubmitInputField();
        //}

#if !UNITY_EDITOR
        // Used to update results on screen during updates
        lock (threadLocker)
        {
            RecognizedText.text = recognizedString;

            debugLogText.text = debugLog;
        }
#endif

        // if (Input.GetMouseButtonDown(0)) { // If the left mouse button is clicked
        //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //     RaycastHit hit;
        //     if (Physics.Raycast(ray, out hit)) { // If the ray hits an object in the scene
        //         Vector3 clickPosition = hit.point; // This is your (x,y,z) position
        //         Debug.Log(clickPosition);
        //     }
        // }
    }

    #region Text
    //public void OnCllllll(){
    //    SubmitInputField();
    //}
    //public async void SubmitInputField()
    //{
    //    if (inputField != null)
    //    {
    //        string text = inputField.text; // Get the text from the input field
    //        if (!String.IsNullOrEmpty(text)){
    //            // TODO: Handle the input text, e.g., send it to another function or process it
    //            UnityEngine.Debug.Log("TEXT INPUT: " + text);
    //            #if !UNITY_EDITOR
    //            uint responseLength = sharedDataWriter.MeasureString(text);
    //            sharedDataWriter.WriteUInt32(responseLength);
    //            sharedDataWriter.WriteString(text);
    //            await sharedDataWriter.StoreAsync();
    //            await sharedDataWriter.FlushAsync();
    //            UnityEngine.Debug.Log("Input Text sent to TCP client: " + text);
    //            #endif
    //        }
            
    //        // Clear the input field after submitting
    //        inputField.text = "";
    //    }
    //    else
    //    {
    //        UnityEngine.Debug.LogError("InputField is not assigned!");
    //    }
    //} 
    private void UpdateDisplayText(string text)
    {
        if (displayText != null)
        {
            displayText.text = text;
        }
        else
        {
            UnityEngine.Debug.LogError("Display TextMeshProUGUI is not assigned!");
            debugLog += "\n" + "Display TextMeshProUGUI is not assigned!";
        }
    }

#endregion  

}



