using System.Collections;
using System.Collections.Generic;
using HaptGlove;
using UnityEngine;

public class HeartBeat_Heart : MonoBehaviour
{
    public float heartBeat_Hz = 1f;
    public Animation animation;

    public bool visual = false;
    public bool vibration = false;
    public bool multimode = true;

    private float beatHitInterval = 0;
    private float beatStayInterval = 0;
    private float beatStayInterval_buf = 0;
    private System.Random rdm = new System.Random();
    private bool beatOn = false;

    private HaptGloveHandler gloveHandler;

    void Start()
    {
        beatHitInterval = (float)1 / heartBeat_Hz;
        beatStayInterval = (float)300 / 1000; //s
        beatStayInterval_buf = beatStayInterval;
        Debug.Log("Beat Hit Interval: " + beatHitInterval);

        animation.Stop();
        //Debug.Log("Animation clip length: " + animation.clip.length);
    }

    IEnumerator DelayFunc()
    {
        yield return new WaitForSeconds(0.3f);
        animation.Play();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "L_Palm")
        {
            Debug.Log("HAHA");
            gloveHandler = collider.GetComponentInParent<HaptGloveHandler>();
            beatOn = true;
            StartCoroutine("DelayFunc");
        }
    }
    private void OnTriggerExit(Collider collider)
    {
        if (collider.name == "L_Palm")
        {
            beatOn = false;
            if (beatHapticsIsApplied)
            {
                //byte[] clutchState = new byte[] { fingerID, 2 };
                //Haptics.ApplyHaptics(clutchState, targetPres);
                //Debug.Log("Beat Off: " + fingerID + "\t" + targetPres);

                if (visual) { }
                else if (vibration) { }
                else
                {
                    byte[][] clutchStates =
                        {new byte[] {0, 2}, new byte[] {1, 2}, new byte[] {2, 2}, new byte[] {3, 2}, new byte[] {4, 2}};
                    //Haptics.ApplyHaptics(clutchStates, targetPres, whichHand,false);
                    byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchStates, targetPres, false);
                    gloveHandler.BTSend(btData);
                }

                beatHapticsIsApplied = false;
                beatStayInterval_buf = beatStayInterval;
            }

            animation.Stop();
        }
    }

    private bool beatHapticsIsApplied = false;
    private byte fingerID = 0;
    private byte targetPres = 20;
    void Update()
    {
        if (beatOn)
        {
            beatHitInterval -= Time.deltaTime;
            if (beatHitInterval <= 0)
            {
                //fingerID = (byte)rdm.Next(5);
                //targetPres = (byte)rdm.Next(10, 20);
                //byte[] clutchState = new byte[] { fingerID, 0 };
                //Haptics.ApplyHaptics(clutchState, targetPres);
                //Debug.Log("Beat On: " + fingerID + "\t" + targetPres);

                if (visual) {}
                else if (vibration)
                {
                    byte[][] clutchStates =
                        {new byte[] {0, 0}, new byte[] {1, 0}, new byte[] {2, 0}, new byte[] {3, 0}, new byte[] {4, 0}};
                    //Haptics.ApplyHaptics(0xff, clutchStates, 6, whichHand, false);
                    byte[] btData = gloveHandler.haptics.ApplyHaptics(0xff, clutchStates, 6, false);
                    gloveHandler.BTSend(btData);
                }
                else
                {
                    byte[][] clutchStates =
                        {new byte[] {0, 0}, new byte[] {1, 0}, new byte[] {2, 0}, new byte[] {3, 0}, new byte[] {4, 0}};
                    //Haptics.ApplyHaptics(clutchStates, targetPres, whichHand, false);
                    byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchStates, targetPres, false);
                    gloveHandler.BTSend(btData);
                }

                beatHapticsIsApplied = true;
                beatHitInterval = (float)1 / heartBeat_Hz;
            }

            if (beatHapticsIsApplied)
            {
                beatStayInterval_buf -= Time.deltaTime;
                if (beatStayInterval_buf < 0)
                {
                    //byte[] clutchState = new byte[] { fingerID, 2 };
                    //Haptics.ApplyHaptics(clutchState, targetPres);
                    //Debug.Log("Beat Off: " + fingerID + "\t" + targetPres);

                    if (visual) {}
                    else if (vibration) {}
                    else
                    {
                        byte[][] clutchStates =
                            {new byte[] {0, 2}, new byte[] {1, 2}, new byte[] {2, 2}, new byte[] {3, 2}, new byte[] {4, 2}};
                        //Haptics.ApplyHaptics(clutchStates, targetPres, whichHand, false);
                        byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchStates, targetPres, false);
                        gloveHandler.BTSend(btData);
                    }
                    
                    beatHapticsIsApplied = false;
                    beatStayInterval_buf = beatStayInterval;
                }
            }
        }
    }

    /// <summary>
    /// mode name: visual, vibration, multimode
    /// </summary>
    /// <param name="mode"></param>
    public void SetMode(string mode)
    {
        switch (mode)
        {
            case "visual":
                vibration = false;
                multimode = false;
                visual = true;
                break;
            case "vibration":
                visual = false;
                multimode = false;
                vibration = true;
                break;
            case "multimode":
                visual = false;
                vibration = false;
                multimode = true;
                break;
            default:
                break;
        }
    }
}
