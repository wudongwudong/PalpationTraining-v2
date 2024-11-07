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
    // public TMP_InputField inputField;
    public TextMeshProUGUI displayText;
    public TextMeshProUGUI RecognizedText;
    private string recognizedString = "";
    // public Button saveConversationButton;
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
    // [HideInInspector] public bool isRecognitionPaused = false;
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

    // Save Conversations
    private string user = "Doctor";
    private string speaker = "Patient";
    private string evaluator = "GPT Evaluator";
    private string persistentPath;
    string fileName = "ConvData";
    [HideInInspector] public InteractionData interactionData = new InteractionData();
    private bool isRecording = false;

    #if !UNITY_EDITOR
    private MediaPlayer mediaPlayer = new MediaPlayer();   
    #endif
    
    // Unity events
    public UnityAction<string> onSpeechReconized;
    public UnityAction<string> onGPTReplyReceived;

    // Show Debug.Log
    public TextMeshProUGUI debugLogText;
    [HideInInspector] public string debugLog;

    // Checklist Questioning
    //public Button endClinicalDiagnoseButton;
    [HideInInspector] public List<string> questionResults = new List<string>();
    [HideInInspector] public string[] Questions = new string[]
    {
        "Did the person introduce himself as a doctor?",
        "Did the doctor explain the examination procedure to the patient?",
        "Did the doctor ask about the patient's symptoms?"
    };

#endregion

    #region Main Task
    // Initialization
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
        ClearInteractionsHistory();
        // saveConversationButton.onClick.AddListener(HandleSaveConversationButtonClick);
        // endClinicalDiagnoseButton.onClick.AddListener(delegate { SaveInteraction(true); });
#if !UNITY_EDITOR
        StartCoroutine(InitializeMediaCapture());
#endif

        await VocalizeMessage("Hi Doctor.", true);
    }
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
                // First click, end diagnose session
                // ClearInteractionsHistory();
                SaveInteraction(true);
                isRecording = true;
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
#endregion

#region Reconnect GPT
    public async void ReconnectGPT()
    {
        await ReconnectGPTTask();
        await VocalizeMessage("Hi Doctor.", true);
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
            UnityEngine.Debug.Log("Reconnect GPT");
            await HoloLensClient.GPTInilization(this);
        }
        catch (Exception e)
        {
            debugLog += "\n" + e.ToString();
        }

        debugLog += "\n" + "Role settings generate.";
        UnityEngine.Debug.Log("Role settings generate.");
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
        ClearInteractionsHistory();
    }
#endregion

#region GPT Initialization
    public static async Task GPTInilization(HoloLensClient instance)
    {
        PatientRoleGenerator patient = new PatientRoleGenerator();
        string patientRole = await patient.GenerateRoleSettingAsync();
        patientRole = patientRole.Replace("role_settings = ", "").Replace("```json","").Replace("````","");
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

    private async Task VocalizeMessage(string message, bool forceSpeak)
    {
        if (!enableVocalize & !forceSpeak)
        {
            return;
        }
        if ((recognizerState != RecognizerState.Start) & !forceSpeak)
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
            string patientResponse = await chat.ChatWithPatientAsync(chat.instruction, text);

            debugLog += "\n" + forcelevel.ToString() + patientResponse;

            if (forcelevel == forceLevel.large)
            {
                UnityEngine.Debug.Log("GPT (Patient): " + patientResponse);
                await VocalizeMessage(patientResponse, false);
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

#region Microphone
    #if !UNITY_EDITOR
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
    #endif
#endregion

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
                string patientResponse = await chat.ChatWithPatientAsync(chat.instruction, text);
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
                await VocalizeMessage(patientResponse, false);
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

#region Save Conversation & Checklist
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
    public void AddQuestion(string performer, string message)
    {
        Interaction question = new Interaction
        {
            type = "question",
            performer = performer,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            message = message
        };
        interactionData.interactions.Add(question);
        string questionDetails = $"Type: {question.type}, Performer: {question.performer}, Timestamp: {question.timestamp}, Message: {question.message}";
        UnityEngine.Debug.Log(questionDetails);
    }
    public async void SaveInteraction(bool startChecklist){
        if (startChecklist){
            await checklistQuestioning();
            SaveConversation();
        }
        
    }
    public void HandleSaveConversationButtonClick(bool startRecording)
    {
        if (startRecording)
        {
            // First click, record data
            ClearInteractionsHistory();
        }
        else
        {
            // Second click, save data
            SaveInteraction(true);

            //SaveConversation();
        }
    }

    public async Task checklistQuestioning(){
        string instruction = "You are an evaluator. I will ask you some questions about the clinical interactions mentioned. Please answer with 'yes' or 'no' based on the dialogues.\n";
        
        foreach (string question in Questions){
            string answer = "";
            UnityEngine.Debug.Log("Evaluation Question: " + question);
            int retryCount = 0;
            while (retryCount < 5){    // Limit to 5 retries
                try{
                    string patientResponse = await chat.ChatWithPatientAsync(instruction, "\nEvaluation Question: " + question);
                    UnityEngine.Debug.Log(string.Join("", chat.History));
                    UnityEngine.Debug.Log(patientResponse);
                    patientResponse = patientResponse.ToLower(); 
                    answer = "";
                    if (patientResponse.Contains("yes")){
                        answer = " [yes]";
                        questionResults.Add("yes");
                        break;
                    }
                    else if (patientResponse.Contains("no")){
                        answer = " [no]";
                        questionResults.Add("no");
                        break;
                    }
                    retryCount++;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError("Checklist Questioning Error: " + ex.Message);
                    retryCount++;
                    if (retryCount >= 5)
                    {
                        answer = " [error]";
                        questionResults.Add("error");
                        break;
                    }
                }
            }
            AddQuestion(evaluator, question + answer);
        }
        debugLog += "\nHistory: " + string.Join(", ", chat.History);
    }

    void ClearInteractionsHistory()
    {
        // Clear all the interation history
        interactionData.interactions.Clear();
        UnityEngine.Debug.Log("Start Recording New Conversation");
        debugLog += "\n" + "Start Recording New Conversation";
    }

    public void SaveConversation()
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

#region Text UI
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

public class Palpation
{
#region Palpation Variable
    private readonly string url = "http://43.163.219.59:8001/beta";
    private readonly string gptModel = "gpt-4o-mini";         //"gpt-3.5-turbo";  
    private string StartSequence = "\nPatient: ";
    private string RestartSequence = "\nDoctor: ";
    private string InitialPrompt;
    public string instruction;
    private string conversationStyle;
    private string agent_name = "the patient";
    public System.Collections.Generic.List<string> History;
    private readonly HttpClient Client = new HttpClient();
    public static System.Random random = new System.Random();


    private string[] conversation_styles = new string[]{
        "Plain: Direct, straightforward.",
        "Upset: An upset patient may 1) exhibit anger or resistance towards the therapist or the therapeutic process, 2) may be challenging or dismissive of the therapist's suggestions and interventions, 3) have difficulty trusting the therapist and forming a therapeutic alliance, and 4) be prone to arguing, criticizing, or expressing frustration during therapy sessions.",
        "Verbose: A verbose patient may 1) provide detailed responses to questions, even if directly relevant, 2) elaborate on personal experiences, thoughts, and feelings extensively, and 3) demonstrate difficulty in allowing the therapist to guide the conversation.",
        "Reserved: A reserved patient may 1) provide brief, vague, or evasive answers to questions, 2) demonstrate reluctance to share personal information or feelings, 3) require more prompting and encouragement to open up, and 4) express distrust or skepticism towards the therapist.",
        "Tangent: A patient who goes off on tangent may 1) start answering a question but quickly veer off into unrelated topics, 2) share personal anecdotes or experiences that are not relevant to the question asked, 3) demonstrate difficulty staying focused on the topic at hand, and 4) require redirection to bring the conversation back to the relevant points.",
        "Pleasing: A pleasing patient may 1) minimize or downplay your own concerns or symptoms to maintain a positive image, 2) demonstrate eager-to-please behavior and avoid expressing disagreement or dissatisfaction, 3) seek approval or validation from the therapist frequently, and 4) agree with the therapist’s statements or suggestions readily, even if they may not fully understand or agree." 
    };

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
        conversationStyle = conversation_styles[random.Next(conversation_styles.Length)];
        // InitialPrompt = $@"
        //     You are a patient to the hospital for a palpation medical check up. 
        //     Please answer in an easy and short way when talking to the doctor. Don't talk too polite and formal.
        //     Talk based on the 'Character Traits' listed in above. Reply according to the language that the person talked to you. 
        //     You need to have emotion and personality and talk like a real human, e.g., Feel shock and worried when you are told having certain disease.
        //     (And also other appropriate emotion such as sad, happy, angry etc.)
        //     YOU SHOULD TALK ONLY AS A PATIENT, and tell your name correctly.
        //     You should talk in {HoloLensClient.fromLanguage}.
        //     Below is your personal detail:
        //     {roleSettings}
        // ";
        instruction = $@"Imagine you are a patient who has been experiencing body health challenges. Your task is to engage in a conversation with the doctor as {agent_name} would during an outpatient session. Align your responses with {agent_name}'s background information provided in the 'Patient Profile' section.

        Patient Profile: {roleSettings}

        In the upcoming conversation, you will simulate {agent_name} during the outpatient session, while the user will play the role of the doctor. Adhere to the following guidelines:
        1. Conversational Style: {conversationStyle}
        2. Emulate the demeanor and responses of a genuine patient to ensure authenticity in your interactions. Use natural language, including hesitations, pauses, and emotional expressions, to enhance the realism of your responses. 
        3. Gradually reveal deeper concerns and core issues, as a real patient often requires extensive dialogue before delving into more sensitive topics. This gradual revelation creates challenges for doctors in identifying the patient's true thoughts and emotions. 
        4. Maintain consistency with {agent_name}'s profile throughout the conversation. Ensure that your responses align with the provided background information. 
        5. Engage in a dynamic and interactive conversation with the doctor. Respond to their questions and prompts in a way that feels authentic and true to {agent_name}'s character. Allow the conversation to flow naturally, and avoid providing abrupt or disconnected responses. 

        You are now {agent_name}. Respond to the doctor's prompts as {agent_name} would, regardless of the specific questions asked. Limit each of your responses to a maximum of 3 sentences.
        ";

        History = new System.Collections.Generic.List<string> { instruction };
    }

    public async Task<string> ChatWithPatientAsync(string instruction, string message)
    {
        string prompt = "";
        if (message == "FORCE PRESS DETECTED. [large]")
        {
            message = force_detected_prompt_high;
            UnityEngine.Debug.Log(message);
            History.Add(message);
            prompt = string.Join("", History) + StartSequence;
        }
        else if (message == "FORCE PRESS DETECTED. [medium]")
        {
            message = force_detected_prompt_medium;
            History.Add(message);
            //return "";
            prompt = string.Join("", History) + StartSequence;
        }
        else if (message == "FORCE PRESS DETECTED. [small]")
        {
            message = force_detected_prompt_small;
            History.Add(message);
            //return "";
            prompt = string.Join("", History) + StartSequence;
        }
        else if (message.Contains("Evaluation Question")){ 
            History.Add(message);
            prompt = string.Join("", History);
        }
        else
        {
            History.Add(RestartSequence + message);
            prompt = string.Join("", History) + StartSequence;
        }
        
        List<Message> messages = new List<Message>();
        if (!string.IsNullOrEmpty(instruction)){
            messages.Add(new Message{role = "system", content = instruction});
        }
        messages.Add(new Message{role = "user", content = prompt});
    
        var data = new Data
        {
            model = gptModel,
            messages = messages.ToArray(), 
            max_tokens = 1024
        };
        string jsonData = JsonUtility.ToJson(data);

        try
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes("AgentHospital:Macon")));
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            string responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JsonUtility.FromJson<ResponseClass>(responseString);

            if (responseJson != null && responseJson.choices != null && responseJson.choices.Length > 0)
            {
                string patientResponse = responseJson.choices[0].message.content;
                if (message.Contains("Evaluation Question")){ 
                    History.Add("\nEvaluator (GPT): " + patientResponse);
                }
                else{
                    History.Add(StartSequence + message);
                }
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
    private readonly string gptModel = "gpt-4o";   // "gpt-3.5-turbo";
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
        Change all the settings below and generate a new role. Generate the patient's name from any country of the world. 
        The patient must feel pain in the right upper abdomen. The gender of the patient must be Male. Return in JSON format.
        role_settings = {
            ""role_overview"": {
                ""Name"": ""Li Ming"",
                ""Age"": ""52"",
                ""Gender"": ""Male"",
                ""Occupation"": ""Senior Engineer"",
                ""Residence"": ""Urban areas, with a fast pace of life""
            },
            ""character_traits"": {
                ""Response mode"": ""sensitive to pain, able to respond realistically to different palpation pressures"",
                ""Emotional state"": ""usually remains calm, but may appear anxious or worried when expressing symptoms""
            },
            ""visible_or_palpable_physical_conditions"": {
                ""Abdominal swelling"": ""Noticeable bulge in the upper right abdomen, visible when wearing tight clothing"",
                ""Location of pain"": ""Right upper abdomen"",
                ""Skin changes"": ""Yellowing of the skin and sclera, signifying jaundice; possibly spider angiomas on the skin due to liver disease"",
                ""Other physical signs"": ""Mild peripheral edema in lower extremities, especially noticeable in the ankles by end of day"",
                ""Sensitivity to pain"": ""Sensitive to pain but can realistically respond to varying pressures during palpation"",
            },
            ""health_and_medical_background"": {
                ""Causes of hepatomegaly"": ""chronic alcoholic liver disease"",
                ""Symptoms"": ""Initial stage: no obvious symptoms, occasional fatigue and indigestion; progressive stage: pain in the upper right abdomen, weight loss, loss of appetite; recently: significant liver swelling, yellowing of the skin and whites of the eyes (jaundice)"",
                ""Other health problems"": ""High blood pressure, taking blood pressure medication; Mild obesity""
            },
            ""historical_cases"": {
                ""Diagnosis time of liver enlargement"": ""1 year ago"",
                ""Previous medical history"": ""non-alcoholic fatty liver disease (NAFLD), diagnosed 5 years ago; type 2 diabetes, diagnosed 3 years ago; hypertension, diagnosed with diabetes"",
                ""Family medical history"": ""Father has a history of hypertension and coronary heart disease""
            },
            ""personal_lifestyle"": {
                ""Habits"": ""Long-term drinking, high work pressure, especially frequent social occasions, preference for high-fat and high-salt foods"",
                ""Exercise habits"": ""Due to busy work, Li Ming rarely has time for physical exercise"",
                ""Family status"": ""Married with two children, good family relationships but often lack family time due to busy work""
            }
        }
        ";

#endregion

    public PatientRoleGenerator()
    {
        // Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
        //     "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("thumt:Thumt@2023")));
    }

    public async Task<string> GenerateRoleSettingAsync()
    {
        UnityEngine.Debug.Log("GENERATE ROLE");
        var data = new Data
        {
            model = gptModel,
            messages = new[] { new Message { role = "user", content = basePrompt } },
            max_tokens = 1024
        };
        string jsonData = JsonUtility.ToJson(data);

        try
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("AgentHospital:Macon")));
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            string responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JsonUtility.FromJson<ResponseClass>(responseString);

            if (responseJson != null && responseJson.choices != null && responseJson.choices.Length > 0)
            {
                string patientResponse = responseJson.choices[0].message.content;
                UnityEngine.Debug.Log(patientResponse);
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


