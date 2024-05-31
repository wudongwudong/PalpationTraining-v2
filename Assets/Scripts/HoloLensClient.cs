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
using UnityEngine.UI;
// using System.Text.Json.Serialization;
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

    #region Variables
    // UI
    //public TMP_InputField inputField;
    public TextMeshProUGUI displayText;
    public TextMeshProUGUI RecognizedText;
    private string recognizedString = "";
    //public Button saveConversationButton;
    private System.Object threadLocker = new System.Object();

    // Microsoft Cognitive Speech Service
    // public string SpeechServiceAPIKey = "your speech service api key";
    // public string SpeechServiceRegion = "your service region";
    [HideInInspector] public string SpeechServiceAPIKey = "890b6fc23e9742eca9f5912f65186a1a";
    [HideInInspector] public string SpeechServiceRegion = "southeastasia";
    private SpeechRecognizer recognizer;
    public static string fromLanguage = "en-US";   //zh-CN
    private bool micPermissionGranted = false;
    public AudioSource audioSource;
    private AudioClip audioClip;
    private readonly Queue<Action> _executeOnMainThread = new Queue<Action>();
    private string gender = "Male";
    public static System.Random random = new System.Random();
    private string selectedVoiceName = null;
    //[HideInInspector] public bool isRecognitionPaused = false;
    public enum RecognizerState
    {
        Stop,
        Start,
        Processing,
        Error
    }
    public RecognizerState recognizerState = RecognizerState.Stop;
    private SpeechSynthesizer synthesizer;
    private bool isSynthesizing = false;
    private Palpation chat;
    private bool enableVocalize = true;

    //Save Conversations
    private string user = "Doctor";
    private string speaker = "Patient";
    private string persistentPath;
    string fileName = "ConvData";
    [HideInInspector] public InteractionData interactionData = new InteractionData();
    private bool isRecording = false;

#if !UNITY_EDITOR
    private MediaPlayer mediaPlayer = new MediaPlayer(); 
        
#endif

    //Unity events
    public UnityAction<string> onSpeechReconized;
    public UnityAction<string> onGPTReplyReceived;

    //Show Debug.Log
    public TextMeshProUGUI debugLogText;
    [HideInInspector] public string debugLog;

    #endregion

    //void Awake()
    //{
    //    persistentPath = Application.persistentDataPath;
    //    audioSource = GetComponent<AudioSource>();
    //    if (audioSource == null) {
    //        UnityEngine.Debug.LogError("No AudioSource component found on this GameObject.");
    //    }
    //    else{
    //        audioSource.playOnAwake = false;
    //    }
    //}

    // Use this for initialization
    async void Start()
    {
        persistentPath = Application.persistentDataPath;
        
        UnityEngine.Debug.Log("Start");
        debugLog += "\n" + "start";

        recognizerState = RecognizerState.Processing;

        try
        {
            await HoloLensClient.GPTInilization(this);
        }
        catch (Exception e)
        {
            debugLog += e.ToString();
        }

        debugLog += "\n" + "Role settings generate.";
        micPermissionGranted = true;
        StartContinuous();

        ChooseVoiceName();

        var config = SpeechConfig.FromSubscription(SpeechServiceAPIKey, SpeechServiceRegion);
        config.SpeechSynthesisLanguage = fromLanguage;  // 设置语言
        config.SpeechSynthesisVoiceName = selectedVoiceName;
        synthesizer = new SpeechSynthesizer(config);

        // saveConversationButton.onClick.AddListener(HandleSaveConversationButtonClick);
#if !UNITY_EDITOR
        StartCoroutine(InitializeMediaCapture());
#endif
    }

    public async void ReconnectGPT()
    {
        await ReconnectGPTTask();
    }
    private async Task ReconnectGPTTask()
    {
        recognizerState = RecognizerState.Processing;

        if (recognizer != null)
        {
            await recognizer.StopContinuousRecognitionAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    debugLog += "\n" + "Error stopping continuous recognition: " + task.Exception.ToString();
                    recognizerState = RecognizerState.Error;
                    //debugLog += "\nrecognizerState: " + recognizerState;
                }
                else if (task.IsCanceled)
                {
                    debugLog += "\n" + "Continuous recognition stopping is canceled.";
                    recognizerState = RecognizerState.Error;
                    //debugLog += "\nrecognizerState: " + recognizerState;
                }
                else
                {
                    //debugLog += "\n" + "Continuous recognition stopped successfully.";
                    //recognizerState = RecognizerState.Stop;
                    //debugLog += "\nrecognizerState: " + recognizerState;
                }

                recognizer.Dispose();
                recognizer = null;
            });
        }

        try
        {
            debugLog += "\n Reconnect GPT";
            await HoloLensClient.GPTInilization(this);
        }
        catch (Exception e)
        {
            debugLog += "\n" + e.ToString();
        }

        debugLog += "\n" + "Role settings generate.";
        micPermissionGranted = true;
        StartContinuous();

        ChooseVoiceName();

        var config = SpeechConfig.FromSubscription(SpeechServiceAPIKey, SpeechServiceRegion);
        config.SpeechSynthesisLanguage = fromLanguage;  // 设置语言
        config.SpeechSynthesisVoiceName = selectedVoiceName;
        synthesizer = new SpeechSynthesizer(config);

#if !UNITY_EDITOR
        StartCoroutine(InitializeMediaCapture());
#endif
    }

    #region GPT Initialization
    public static async Task GPTInilization(HoloLensClient instance)
    {
        PatientRoleGenerator patient = new PatientRoleGenerator();
        string patientRole = await patient.GenerateRoleSettingAsync();
        patientRole = patientRole.Replace("role_settings = ", "");
        UnityEngine.Debug.Log(patientRole);
        instance.chat = new Palpation(patientRole);
    }
    #endregion

    #region Receive Data and Vocalize

    public async void ChangeVoiceToEn()
    {
        fromLanguage = "en-US";

        //await StopContinuousRecognition();
        await ReconnectGPTTask();
    }

    public async void ChangeVoiceToZh()
    {
        fromLanguage = "zh-CN";

        //await StopContinuousRecognition();
        await ReconnectGPTTask();
    }

    private void ChooseVoiceName()
    {
        var config = SpeechConfig.FromSubscription(SpeechServiceAPIKey, SpeechServiceRegion);
        config.SpeechSynthesisLanguage = fromLanguage;

        switch (fromLanguage)
        {
            case "en-US":
                if (gender == "Male")
                {
                    string[] maleVoices = new string[]
                    {
                        "en-US-GuyNeural",
                        "en-US-DavisNeural",
                        "en-US-ChristopherNeural",
                        "en-US-BrianNeural",
                        "en-US-AndrewNeural",
                        "en-GB-RyanNeural",
                        "en-AU-WilliamNeural",
                        "en-CA-LiamNeural"
                    };
                    selectedVoiceName = maleVoices[random.Next(maleVoices.Length)];
                }
                else
                {
                    string[] femaleVoices = new string[]
                    {
                        "en-US-JessaNeural",
                        "en-US-JaneNeural",
                        "en-US-EmmaNeural",
                        "en-US-AvaNeural",
                        "en-US-AriaNeural",
                        "en-US-SaraNeural",
                        "en-GB-MiaNeural",
                        "en-GB-LibbyNeural"
                    };
                    selectedVoiceName = femaleVoices[random.Next(femaleVoices.Length)];
                }

                break;
            case "zh-CN":
                if (gender == "Male")
                {
                    string[] maleVoices = new string[]
                    {
                        "zh-CN-YunzeNeural",
                        "zh-CN-YunxiNeural",
                        "zh-CN-YunyeNeural",
                    };
                    selectedVoiceName = maleVoices[random.Next(maleVoices.Length)];
                }
                else
                {
                    string[] femaleVoices = new string[]
                    {
                        "zh-CN-XiaoxiaoNeural",
                        "zh-CN-XiaohanNeural",
                        "zh-CN-XiaomoNeural",
                        "zh-CN-XiaoruiNeural",
                        "zh-CN-XiaoshuangNeural",
                        "zh-CN-XiaoyiNeural",
                        "zh-CN-XiaozhenNeural",
                    };
                    selectedVoiceName = femaleVoices[random.Next(femaleVoices.Length)];
                }

                break;
        }

        UnityEngine.Debug.Log($"Selected Voice Name: {selectedVoiceName}");
    }

    public void EnableVocalize()
    {
        enableVocalize = true;
    }

    public void DisableVocalize()
    {
        enableVocalize = false;
    }

    private async Task VocalizeMessage(string message)
    {
        if (enableVocalize == false)
        {
            return;
        }
        if (recognizerState != RecognizerState.Start)
        {
            return;
        }

        if (isSynthesizing)
        {
            await synthesizer.StopSpeakingAsync();
        }
        isSynthesizing = true;
        UnityEngine.Debug.Log($"VocalizeMessage: Trying to vocalize message: {message}");

        try
        {
            // Using SSML to specify pitch, speaking rate, etc.
            string ssml = $@"
                <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='{selectedVoiceName}'>
                    <voice name='{selectedVoiceName}'>
                        <prosody contour='(60%,-60%) (100%,+80%)' >
                            {message}
                        </prosody>
                    </voice>
                </speak>";

            // Synthesize the SSML to a stream
            using (var result = await synthesizer.SpeakSsmlAsync(ssml))
            {
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    UnityEngine.Debug.Log("Speech synthesis succeeded.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    UnityEngine.Debug.Log($"Speech synthesis canceled: {cancellation.Reason}.");
                    debugLog += "\n" + "Speech synthesis canceled: " + cancellation.Reason.ToString();
                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        UnityEngine.Debug.LogError($"Error code: {cancellation.ErrorCode}");
                        UnityEngine.Debug.LogError($"Error details: {cancellation.ErrorDetails}");
                        UnityEngine.Debug.LogError("Did you update the subscription info?");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error creating SpeechSynthesizer: {ex.Message}");
            debugLog += "\n" + "Error creating SpeechSynthesizer: " + ex.Message;
        }
        finally
        {
            isSynthesizing = false;
        }
    }

    #endregion

    #region Send Force Data to GPT
    public enum forceLevel
    {
        small,
        medium,
        large
    }

    public async Task SendForceDetectedMessage(forceLevel forcelevel)
    {
        try
        {
            string text = "FORCE PRESS DETECTED. [" + forcelevel.ToString() + "]";
            AddPalpation(user, text);
            UnityEngine.Debug.Log(text);
            string patientResponse = await chat.ChatWithPatientAsync(text);

            debugLog += "\n" + forcelevel.ToString() + patientResponse;

            if (forcelevel == forceLevel.large)
            {
                UnityEngine.Debug.Log("GPT (Patient): " + patientResponse);
                await VocalizeMessage(patientResponse);
                _executeOnMainThread.Enqueue(() => UpdateDisplayText(patientResponse));
                //UpdateDisplayText(patientResponse);
                AddDialogue(speaker, patientResponse);
            }


        }
        catch (Exception e)
        {
            debugLog += "\n" + e.ToString();
        }

    }
    #endregion

#if !UNITY_EDITOR
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
            //debugLog += "\n" + "InitializeMediaCapture failed";
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
        // }
        UnityEngine.Debug.LogFormat("CreateSpeechRecognizer exit");
        //debugLog += "\n" + "CreateSpeechRecognizer exit";
    }

    public async Task StartContinuousRecognition()
    {
        recognizerState = RecognizerState.Processing;
        //debugLog += "\nrecognizerState: " + recognizerState;
        //debugLog += "\nStart enter";

        try
        {
            UnityEngine.Debug.LogFormat("Starting Continuous Speech Recognition.");
            //debugLog += "\n" + "Starting Continuous Speech Recognition.";
            CreateSpeechRecognizer();

            if (recognizer != null)
            {
                UnityEngine.Debug.LogFormat("Starting Speech Recognizer.");
                await recognizer.StartContinuousRecognitionAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        debugLog += "\n" + "Error starting continuous recognition: " + task.Exception.ToString();
                        recognizerState = RecognizerState.Error;
                        //debugLog += "\nrecognizerState: " + recognizerState;
                    }
                    else if (task.IsCanceled)
                    {
                        debugLog += "\n" + "Continuous starting pausing is canceled.";
                        recognizerState = RecognizerState.Error;
                        //debugLog += "\nrecognizerState: " + recognizerState;
                    }
                    else
                    {
                        //debugLog += "\n" + "Continuous recognition started successfully.";
                        recognizerState = RecognizerState.Start;
                        //debugLog += "\nrecognizerState: " + recognizerState;
                    }
                });//.ConfigureAwait(false);
                UnityEngine.Debug.LogFormat("Speech Recognizer is now running.");
                //debugLog += "\n" + "Speech Recognizer is now running.";
            }
            UnityEngine.Debug.LogFormat("Start Continuous Speech Recognition exit");
            //debugLog += "\n" + "Start Continuous Speech Recognition exit.";


        }
        catch (Exception e)
        {
            debugLog += "\n" + "Error: Starting Continuous Speech Recognition. " + e;
            recognizerState = RecognizerState.Error;
            //debugLog += "\nrecognizerState: " + recognizerState;
        }

    }

    public async Task PauseContinuousRecognition()
    {
        //debugLog += "\nPause enter";

        try
        {
            //debugLog += "\nEnter PauseContinuousRecognition";
            if (recognizer != null && recognizerState == RecognizerState.Start)
            {
                recognizerState = RecognizerState.Processing;
                //debugLog += "\nrecognizerState: " + recognizerState;

                await recognizer.StopContinuousRecognitionAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        debugLog += "\n" + "Error pausing continuous recognition: " + task.Exception.ToString();
                        recognizerState = RecognizerState.Error;
                        //debugLog += "\nrecognizerState: " + recognizerState;
                    }
                    else if (task.IsCanceled)
                    {
                        debugLog += "\n" + "Continuous recognition pausing is canceled.";
                        recognizerState = RecognizerState.Error;
                        //debugLog += "\nrecognizerState: " + recognizerState;
                    }
                    else
                    {
                        //debugLog += "\n" + "Continuous recognition paused successfully.";
                        recognizerState = RecognizerState.Stop;
                        //debugLog += "\nrecognizerState: " + recognizerState;
                    }
                });
                //isRecognitionPaused = true;
                //debugLog += "\nPauseContinuousRecognition. isRecognitionPaused = " + recognizerState.ToString();
            }
            debugLog += "\nExit PauseContinuousRecognition";
        }
        catch (Exception e)
        {
            debugLog += "\nPauseContinuousRecognition" + e.ToString();
            recognizerState = RecognizerState.Error;
            //debugLog += "\nrecognizerState: " + recognizerState;
        }

    }

    public async Task ResumeContinuousRecognition()
    {
        //debugLog += "\nResume enter";

        try
        {
            //debugLog += "\nEnter ResumeContinuousRecognition";
            if (recognizer != null && recognizerState == RecognizerState.Stop)
            {
                recognizerState = RecognizerState.Processing;
                //debugLog += "\nrecognizerState: " + recognizerState;

                await recognizer.StartContinuousRecognitionAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        debugLog += "\n" + "Error resuming continuous recognition: " + task.Exception.ToString();
                        recognizerState = RecognizerState.Error;
                        //debugLog += "\nrecognizerState: " + recognizerState;
                    }
                    else if (task.IsCanceled)
                    {
                        debugLog += "\n" + "Continuous recognition resuming is canceled.";
                        recognizerState = RecognizerState.Error;
                        //debugLog += "\nrecognizerState: " + recognizerState;
                    }
                    else
                    {
                        //debugLog += "\n" + "Continuous recognition resumed successfully.";
                        recognizerState = RecognizerState.Start;
                        //debugLog += "\nrecognizerState: " + recognizerState;
                    }
                });
                //isRecognitionPaused = false;
                debugLog += "\nResumeContinuousRecognition. isRecognitionPaused = " + recognizerState;
            }
            //debugLog += "\nExit ResumeContinuousRecognition";
        }
        catch (Exception e)
        {
            debugLog += "\nResumeContinuousRecognition" + e.ToString();
            recognizerState = RecognizerState.Error;
            //debugLog += "\nrecognizerState: " + recognizerState;
        }

    }

    public async Task StopContinuousRecognition()
    {
        //debugLog += "\nStop enter";

        try
        {
            UnityEngine.Debug.LogFormat("Stopping Continuous Speech Recognition.");
            //debugLog += "\n" + "Stopping Continuous Speech Recognition.";
            //CreateSpeechRecognizer();

            if (recognizer != null)
            {
                recognizerState = RecognizerState.Processing;
                //debugLog += "\nrecognizerState: " + recognizerState;
                //debugLog += "\n" + "StopContinuousRecognition.";

                UnityEngine.Debug.LogFormat("Stopping Speech Recognizer.");
                await recognizer.StopContinuousRecognitionAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        debugLog += "\n" + "Error stopping continuous recognition: " + task.Exception.ToString();
                        recognizerState = RecognizerState.Error;
                        //debugLog += "\nrecognizerState: " + recognizerState;
                    }
                    else if (task.IsCanceled)
                    {
                        debugLog += "\n" + "Continuous recognition stopping is canceled.";
                        recognizerState = RecognizerState.Error;
                        //debugLog += "\nrecognizerState: " + recognizerState;
                    }
                    else
                    {
                        //debugLog += "\n" + "Continuous recognition stopped successfully.";
                        recognizerState = RecognizerState.Stop;
                        //debugLog += "\nrecognizerState: " + recognizerState;
                    }

                    recognizer.Dispose();
                    recognizer = null;
                });
                UnityEngine.Debug.LogFormat("Speech Recognizer is now stopping.");
                //debugLog += "\n" + "Speech Recognizer is now stopping.";
            }
            UnityEngine.Debug.LogFormat("Stop Continuous Speech Recognition exit");
            //debugLog += "\n" + "Stop Continuous Speech Recognition exit";


        }
        catch (Exception e)
        {
            debugLog += "\n" + "Error: Stopping Continuous Speech Recognition. " + e;
            recognizerState = RecognizerState.Error;
            //debugLog += "\nrecognizerState: " + recognizerState;
        }

    }

    private void SessionStartedHandler(object sender, SessionEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"\n    Session started event. Event: {e.ToString()}.");
    }

    private void SessionStoppedHandler(object sender, SessionEventArgs e)
    {
        UnityEngine.Debug.LogFormat($"\n    Session event. Event: {e.ToString()}.");
        UnityEngine.Debug.LogFormat($"Session Stop detected. Stop the recognition.");

        debugLog += "\nSpeech Recognition SessionStoppedHandler: Reason" + e.ToString();
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
            if (!String.IsNullOrEmpty(text))
            {
                recognizedString = text;

                UnityEngine.Debug.LogFormat($"RECOGNIZED: Text={text}, Language={fromLanguage}");
                AddDialogue(user, text);
                UnityEngine.Debug.Log("Human (Doctor): " + text);
                string patientResponse = await chat.ChatWithPatientAsync(text);
                UnityEngine.Debug.Log(string.Join(", ", chat.History));

                try
                {
                    _executeOnMainThread.Enqueue(() => UpdateDisplayText(patientResponse));
                }
                catch (Exception exception)
                {
                    debugLog += "\n" + exception.ToString();
                }

                UnityEngine.Debug.Log("GPT (Patient): " + patientResponse);
                await VocalizeMessage(patientResponse);
                AddDialogue(speaker, patientResponse);
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

        debugLog += "\nSpeech Recognition CANCELED: Reason: " + e.Reason;


        errorString = e.ToString();
        if (e.Reason == CancellationReason.Error)
        {
            UnityEngine.Debug.LogFormat($"CANCELED: ErrorDetails={e.ErrorDetails}");
            UnityEngine.Debug.LogFormat($"CANCELED: Did you update the subscription info?");

            debugLog += "\nSpeech Recognition CANCELED: ErrorDetails: " + e.ErrorDetails;
        }
    }
    #endregion


    #region Save Conversation
    [Serializable]
    public class Interaction
    {
        public string type;
        public string performer;
        public string timestamp;
        public string message;
    }

    [Serializable]
    public class InteractionData
    {
        public List<Interaction> interactions;
    }
    public void AddDialogue(string performer, string message)
    {
        Interaction dialogue = new Interaction
        {
            type = "dialogue",
            performer = performer,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            message = message
        };
        interactionData.interactions.Add(dialogue);
        string dialogueDetails = $"Type: {dialogue.type}, Performer: {dialogue.performer}, Timestamp: {dialogue.timestamp}, Message: {dialogue.message}";
        UnityEngine.Debug.Log(dialogueDetails);
    }

    public void AddPalpation(string performer, string message)
    {
        Interaction palpation = new Interaction
        {
            type = "palpation",
            performer = performer,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            message = message
        };
        interactionData.interactions.Add(palpation);
        string palpationDetails = $"Type: {palpation.type}, Performer: {palpation.performer}, Timestamp: {palpation.timestamp}, Message: {palpation.message}";
        UnityEngine.Debug.Log(palpationDetails);
    }

    public void HandleSaveConversationButtonClick(bool startRecording)
    {
        if (startRecording)
        {
            // First click, record data
            StartRecordingConversation();
        }
        else
        {
            // Second click, save data
            SaveConversation();
        }
    }

    void StartRecordingConversation()
    {
        // Clear all the interation history
        interactionData.interactions.Clear();
        UnityEngine.Debug.Log("Start Recording New Conversation");
        debugLog += "\n" + "Start Recording New Conversation";
    }

    void SaveConversation()
    {
        string jsonData = JsonUtility.ToJson(interactionData, true);
        string fullPath = GenerateUniqueFilePath(persistentPath, fileName + System.DateTime.Now.ToString("yyyy_MMdd_HHmmss"), "json");
        File.WriteAllText(fullPath, jsonData);   // write data
        UnityEngine.Debug.Log($"Conversation saved to {fullPath}");
        debugLog += "\n" + $"Conversation saved to {fullPath}";
    }

    string GenerateUniqueFilePath(string path, string fileName, string extension)
    {
        UnityEngine.Debug.Log("Generating FilePath");
        string filePathWithoutSuffix = Path.Combine(path, fileName);
        string fullFilePath = $"{filePathWithoutSuffix}.{extension}";
        int count = 1;
        while (File.Exists(fullFilePath))
        {
            UnityEngine.Debug.Log("Generating Unique FilePath");
            fullFilePath = $"{filePathWithoutSuffix}({count}).{extension}";
            count++;
        }
        return fullFilePath;
    }
    #endregion

    #region Text
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


    void Update()
    {
        while (_executeOnMainThread.Count > 0)
        {
            _executeOnMainThread.Dequeue().Invoke();
        }
        if (Input.GetKeyUp(KeyCode.Return))
        {
            if (!isRecording)
            {
                // First click, record data
                StartRecordingConversation();
                isRecording = true;
            }
            else
            {
                // Second click, save data
                SaveConversation();
                isRecording = false;
            }
        }

#if !UNITY_EDITOR
        // Used to update results on screen during updates
        lock (threadLocker)
        {
            RecognizedText.text = recognizedString;
            debugLogText.text = debugLog;
        }
#endif
    }

    private async void OnDestroy()
    {
        if (recognizer != null)
        {
            // Stop continuous recognition when the object is destroyed
            await recognizer.StopContinuousRecognitionAsync();
            recognizer.Dispose();

        }
    }
}


public class Palpation
{
    #region Palpation Variable
    private readonly string url = "http://43.163.219.59:8001/beta";
    private readonly string gptModel = "gpt-3.5-turbo";
    private string StartSequence = "\nAI (as Patient):";
    private string RestartSequence = "\nHuman (as Doctor):";
    private string InitialPrompt;
    public System.Collections.Generic.List<string> History;
    private readonly HttpClient Client = new HttpClient();

    [Serializable]
    public class Data
    {
        public string model;
        public Message[] messages;
        public int max_tokens;
    }

    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class ResponseClass
    {
        public Choice[] choices;
    }

    [Serializable]
    public class Choice
    {
        public Message message;
    }

    private readonly string force_detected_prompt_high = @"
    FORCE PRESS DETECTED: There are three levels of forces to be defined: Small, Medium, High.
    Force pressed by the doctor on the abdomen is High.
    Talk like a real human, Express body reaction and emotion to tell that you are pain.
    Do not talk too long.";
    private readonly string force_detected_prompt_small = @"
    FORCE PRESS DETECTED: There are three levels of forces to be defined: Small, Medium, High.
    Force pressed by the doctor on the abdomen is Small. You don't feel any pain because of the doctor's pressing.
    So, if doctor ask if you feel pain there, you should talk like a real human and say no. 
    You do not need to give any reaction and say anything.
    Just keep it in your memory.";
    private readonly string force_detected_prompt_medium = @"
    FORCE PRESS DETECTED: There are three levels of forces to be defined: Small, Medium, High.
    Force pressed by the doctor on the abdomen is Medium. You feel some pressure because of the doctor's pressing but it's not painful.
    So, if doctor ask if you feel pain there, you should talk like a real human and say I feel some pressure but not painful.
    You do not need to give any reaction and say anything.
    Just keep it in your memory.";
    #endregion

    public Palpation(string roleSettings)
    {
        // // 使用 JSON 序列化来格式化角色设置
        // string formattedRoleSettings = JsonUtility.ToJson(roleSettings, true);
        InitialPrompt = $@"
            You are a patient to the hospital for a palpation medical check up. 
            Please answer in an easy and short way when talking to the doctor. Don't talk too polite and formal.
            Talk based on the 'Character Traits' listed in above. Reply according to the language that the person talked to you. 
            You need to have emotion and personality and talk like a real human, e.g., Feel shock and worried when you are told having certain disease.
            (And also other appropriate emotion such as sad, happy, angry etc.)
            YOU SHOULD TALK ONLY AS A PATIENT, and tell your name correctly.
            You should talk in {HoloLensClient.fromLanguage}.
            Below is your personal detail:
            {roleSettings}
        ";
        History = new System.Collections.Generic.List<string> { InitialPrompt };
    }

    public async Task<string> ChatWithPatientAsync(string message)
    {
        if (message == "FORCE PRESS DETECTED. [large]")
        {
            message = force_detected_prompt_high;
            UnityEngine.Debug.Log(message);
            History.Add(message);
        }
        else if (message == "FORCE PRESS DETECTED. [medium]")
        {
            message = force_detected_prompt_medium;
            History.Add(message);
            //return "";
        }
        else if (message == "FORCE PRESS DETECTED. [small]")
        {
            message = force_detected_prompt_small;
            History.Add(message);
            //return "";
        }
        else
        {
            History.Add(RestartSequence + message);
        }
        string prompt = string.Join("", History) + StartSequence;
        var data = new Data
        {
            model = gptModel,
            messages = new[] { new Message { role = "user", content = prompt } },
            max_tokens = 1024
        };
        string jsonData = JsonUtility.ToJson(data);

        try
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes("thumt:Thumt@2023")));
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            string responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JsonUtility.FromJson<ResponseClass>(responseString);

            if (responseJson != null && responseJson.choices != null && responseJson.choices.Length > 0)
            {
                string patientResponse = responseJson.choices[0].message.content;
                History.Add(StartSequence + patientResponse);
                return patientResponse;
            }
            else
            {
                return "{\"error\": \"No response or invalid format from API\"}";
            }
        }
        catch (Exception e)
        {
            return $"{{\"error\": \"Network error: {e.Message}\"}}";
        }
    }
}

public class PatientRoleGenerator
{
    #region Patient Variable
    private readonly string gptModel = "gpt-3.5-turbo";
    private readonly HttpClient Client = new HttpClient();
    private readonly string url = "http://43.163.219.59:8001/beta";
    [Serializable]
    public class Data
    {
        public string model;
        public Message[] messages;
        public int max_tokens;
    }

    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class ResponseClass
    {
        public Choice[] choices;
    }

    [Serializable]
    public class Choice
    {
        public Message message;
    }

    private string basePrompt = @"
        Generate a detailed role setting for a patient with hepatomegaly. The sample format is as below. 
        Change all the settings below and generate a new role. The patient MUST feel pain in the right upper abdomen.
        The gender of the patient must be Male.
        role_settings = {
            ""Role Overview"": {
                ""Name"": ""Li Ming"",
                ""Age"": ""52"",
                ""Gender"": ""Male"",
                ""Occupation"": ""Senior Engineer"",
                ""Residence"": ""Urban areas, with a fast pace of life""
            },
            ""Character traits"": {
                ""Communication style"": ""gentle and detailed, willing to share their symptoms and lifestyle habits"",
                ""Response mode"": ""sensitive to pain, able to respond realistically to different palpation pressures"",
                ""Emotional state"": ""usually remains calm, but may appear anxious or worried when expressing symptoms""
            },
            ""Visible or Palpable Physical Conditions"": {
                ""Abdominal swelling"": ""Noticeable bulge in the upper right abdomen, visible when wearing tight clothing"",
                ""Location of pain"": ""Right upper abdomen"",
                ""Skin changes"": ""Yellowing of the skin and sclera, signifying jaundice; possibly spider angiomas on the skin due to liver disease"",
                ""Other physical signs"": ""Mild peripheral edema in lower extremities, especially noticeable in the ankles by end of day"",
                ""Sensitivity to pain"": ""Sensitive to pain but can realistically respond to varying pressures during palpation"",
            },
            ""Health and Medical Background"": {
                ""Causes of hepatomegaly"": ""chronic alcoholic liver disease"",
                ""Symptoms"": ""Initial stage: no obvious symptoms, occasional fatigue and indigestion; progressive stage: pain in the upper right abdomen, weight loss, loss of appetite; recently: significant liver swelling, yellowing of the skin and whites of the eyes (jaundice)"",
                ""Other health problems"": ""High blood pressure, taking blood pressure medication; Mild obesity""
            },
            ""Historical Cases"": {
                ""Diagnosis time of liver enlargement"": ""1 year ago"",
                ""Previous medical history"": ""non-alcoholic fatty liver disease (NAFLD), diagnosed 5 years ago; type 2 diabetes, diagnosed 3 years ago; hypertension, diagnosed with diabetes"",
                ""Family medical history"": ""Father has a history of hypertension and coronary heart disease""
            },
            ""Personal lifestyle"": {
                ""Habits"": ""Long-term drinking, high work pressure, especially frequent social occasions, preference for high-fat and high-salt foods"",
                ""Exercise habits"": ""Due to busy work, Li Ming rarely has time for physical exercise"",
                ""Family status"": ""Married with two children, good family relationships but often lack family time due to busy work""
            }
        }
        ";

    #endregion

    public PatientRoleGenerator()
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("thumt:Thumt@2023")));
    }

    public async Task<string> GenerateRoleSettingAsync()
    {
        var data = new Data
        {
            model = gptModel,
            messages = new[] { new Message { role = "user", content = basePrompt } },
            max_tokens = 1024
        };
        string jsonData = JsonUtility.ToJson(data);

        try
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes("thumt:Thumt@2023")));
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            string responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JsonUtility.FromJson<ResponseClass>(responseString);

            if (responseJson != null && responseJson.choices != null && responseJson.choices.Length > 0)
            {
                string patientResponse = responseJson.choices[0].message.content;
                return patientResponse;
            }
            else
            {
                return "{\"error\": \"No response or invalid format from API\"}";
            }
        }
        catch (Exception e)
        {
            return $"{{\"error\": \"Network error: {e.Message}\"}}";
        }
    }
}



