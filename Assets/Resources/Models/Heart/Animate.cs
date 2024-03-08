using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animate : MonoBehaviour
{
    //public int interval = 1;
    public Material heartMat;
    public Material heartMatHit;
    public string partHit;
    public string namePostfix;
    int heartFrame;
    //int count;

    public HeartbeatHandler heartbeatHandler;
    public bool animationStart;
    //private float heartBeat_Hz;
    private int totalFrame = 30;
    private int oneCycle;
    private float timeFrame = 0;
    private int startHeartFrame = 7;//7

    void Start()
    {
        heartFrame = startHeartFrame;
        //count = interval;
        partHit = "none";

        foreach (Transform child in transform)
        {
            foreach (Transform childmesh in child)
            {
                if (childmesh.gameObject.name == "default")
                {
                    //heartMat.SetFloat("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                    childmesh.gameObject.GetComponent<MeshRenderer>().material = heartMat;
                }
            }

            child.gameObject.SetActive(false);
        }

        
        //heartBeat_Hz = heartbeatHandler.heartBeat_Hz;
        oneCycle = (int)(1 / heartbeatHandler.heartBeat_Hz * 1000);
        timeFrame = ((float)startHeartFrame / totalFrame) * oneCycle;

        ToggleHeartFrame(heartFrame, namePostfix, true);

    }

    void FixedUpdate()
    {
        if (animationStart)
        {
            oneCycle = (int)(1 / heartbeatHandler.heartBeat_Hz * 1000);

            if (timeFrame > ((float)heartFrame / totalFrame) * oneCycle)
            {
                ToggleHeartFrame(heartFrame, namePostfix, false);

                heartFrame += 1;
                if (heartFrame > totalFrame)
                {
                    heartFrame = 1;
                    timeFrame = 0;
                }

                //ChangeHeartColor(heartFrame, namePostfix, heartMat);

                //if (partHit == "saendocardialContour" || partHit == "saepicardialContour" || partHit == "salaContour" || partHit == "saraContour" || partHit == "sarvendocardialContour")
                //    transform.Find($"{partHit}_{heartFrame}_{namePostfix}").GetChild(0).GetComponent<MeshRenderer>().material = heartMatHit;

                ToggleHeartFrame(heartFrame, namePostfix, true);
            }

            timeFrame += Time.fixedDeltaTime * 1000;
        }
    }

    private void ToggleHeartFrame(int frame, string namePostfix, bool setActive)
    {
        transform.Find($"saendocardialContour_{frame}_{namePostfix}").gameObject.SetActive(setActive);
        transform.Find($"saepicardialContour_{frame}_{namePostfix}").gameObject.SetActive(setActive);
        transform.Find($"salaContour_{frame}_{namePostfix}").gameObject.SetActive(setActive);
        transform.Find($"saraContour_{frame}_{namePostfix}").gameObject.SetActive(setActive);
        transform.Find($"sarvendocardialContour_{frame}_{namePostfix}").gameObject.SetActive(setActive);
    }

    private void ChangeHeartColor(int frame, string namePostfix, Material material)
    {
        transform.Find($"saendocardialContour_{frame}_{namePostfix}").GetChild(0).GetComponent<MeshRenderer>().material = material;
        transform.Find($"saepicardialContour_{frame}_{namePostfix}").GetChild(0).GetComponent<MeshRenderer>().material = material;
        transform.Find($"salaContour_{frame}_{namePostfix}").GetChild(0).GetComponent<MeshRenderer>().material = material;
        transform.Find($"saraContour_{frame}_{namePostfix}").GetChild(0).GetComponent<MeshRenderer>().material = material;
        transform.Find($"sarvendocardialContour_{frame}_{namePostfix}").GetChild(0).GetComponent<MeshRenderer>().material = material;
    }

    public void Stop()
    {
        ToggleHeartFrame(heartFrame, namePostfix, false);
        animationStart = false;
        heartFrame = startHeartFrame;
        timeFrame = ((float)startHeartFrame / totalFrame) * oneCycle;
        ToggleHeartFrame(heartFrame, namePostfix, true);
    }
}
