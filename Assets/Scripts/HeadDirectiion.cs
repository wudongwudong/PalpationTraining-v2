using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class HeadDirectiion : MonoBehaviour
{
    [SerializeField] private HoloLensClient holoClient;
    private Transform camera;
    [SerializeField] private Transform[] objects = new Transform[0];
    //[SerializeField] private TMP_Text text;

    private float angleThreshold = 25;

    private bool enable = true;

    private bool isPaused = false;

    void Start()
    {
        camera = Camera.main.transform;
    }

    async void Update()
    {
        if (!enable)
            return;
        

        int objectCount = objects.Length;
        if (objectCount == 0)
            return;

        float[] angles = new float[objectCount];

        for (int i = 0; i < objectCount; i++)
        {
            angles[i] = Vector3.Angle(objects[i].position - camera.position, camera.forward);
            if (angles[i] < angleThreshold)
            {

                if (isPaused == true)
                {
                    isPaused = false;

                    await holoClient.ResumeContinuousRecognition();
                    //holoClient.speechDetectSwitch = true;
                    //text.text += "start";
                }

                return;
            }
        }

        if (isPaused == false)
        {
            isPaused = true;

            //holoClient.speechDetectSwitch = false;
            await holoClient.PauseContinuousRecognition();
            //text.text += "stop";
        }

        
    }

    public void EnableLookSpeech()
    {
        enable = true;
    }

    public void DisableLookSpeech()
    {
        enable = false;

        if (holoClient.isRecognitionPaused == true)
        {
            holoClient.ResumeContinuousRecognition();
            holoClient.isRecognitionPaused = false;
            //text.text += "start";
        }
    }
}
