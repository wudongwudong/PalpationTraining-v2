using System.Collections;
using System.Collections.Generic;
using HaptGlove;
using TMPro;
using UnityEngine;

public class HeartbeatFingerCapture : MonoBehaviour
{
    public HeartbeatHandler heartbeatHandler;
    private byte thisNo = 0;

    void Start()
    {
        switch (gameObject.name)
        {
            case "Heart1":
                thisNo = 1;
                break;
            case "Heart2":
                thisNo = 2;
                break;
            case "Heart3":
                thisNo = 3;
                break;
            case "Heart4":
                thisNo = 4;
                break;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        HaptGloveHandler gloveHandler = col.GetComponentInParent<HaptGloveHandler>();

        if (gloveHandler != null)
        {
            if (gloveHandler.whichHand == HaptGloveHandler.HandType.Left)
            {
                switch (col.name)
                {
                    case "L_ThumbDistal":
                        heartbeatHandler.fingerListLeft[0].Add(thisNo);
                        break;
                    case "L_IndexDistal":
                        heartbeatHandler.fingerListLeft[1].Add(thisNo);
                        break;
                    case "L_MiddleDistal":
                        heartbeatHandler.fingerListLeft[2].Add(thisNo);
                        break;
                    case "L_RingDistal":
                        heartbeatHandler.fingerListLeft[3].Add(thisNo);
                        break;
                    case "L_LittleDistal":
                        heartbeatHandler.fingerListLeft[4].Add(thisNo);
                        break;
                    case "L_PalmCollider":
                        heartbeatHandler.fingerListLeft[5].Add(thisNo);
                        break;
                }
            }
            else if (gloveHandler.whichHand == HaptGloveHandler.HandType.Right)
            {
                //text.text += col.name;
                //Debug.Log(col.name);
                switch (col.name)
                {
                    case "R_ThumbDistal":
                        heartbeatHandler.fingerListRight[0].Add(thisNo);
                        break;
                    case "R_IndexDistal":
                        heartbeatHandler.fingerListRight[1].Add(thisNo);
                        break;
                    case "R_MiddleDistal":
                        heartbeatHandler.fingerListRight[2].Add(thisNo);
                        break;
                    case "R_RingDistal":
                        heartbeatHandler.fingerListRight[3].Add(thisNo);
                        break;
                    case "R_LittleDistal":
                        heartbeatHandler.fingerListRight[4].Add(thisNo);
                        break;
                    case "R_PalmCollider":
                        heartbeatHandler.fingerListRight[5].Add(thisNo);
                        break;
                }
            }
        }

    }

    void OnTriggerExit(Collider col)
    {
        HaptGloveHandler gloveHandler = col.GetComponentInParent<HaptGloveHandler>();

        if (gloveHandler != null)
        {
            if (gloveHandler.whichHand == HaptGloveHandler.HandType.Left)
            {
                switch (col.name)
                {
                    case "L_ThumbDistal":
                        heartbeatHandler.fingerListLeft[0].Remove(thisNo);
                        break;
                    case "L_IndexDistal":
                        heartbeatHandler.fingerListLeft[1].Remove(thisNo);
                        break;
                    case "L_MiddleDistal":
                        heartbeatHandler.fingerListLeft[2].Remove(thisNo);
                        break;
                    case "L_RingDistal":
                        heartbeatHandler.fingerListLeft[3].Remove(thisNo);
                        break;
                    case "L_LittleDistal":
                        heartbeatHandler.fingerListLeft[4].Remove(thisNo);
                        break;
                    case "L_PalmCollider":
                        heartbeatHandler.fingerListLeft[5].Remove(thisNo);
                        break;
                }
            }
            else if (gloveHandler.whichHand == HaptGloveHandler.HandType.Right)
            {
                switch (col.name)
                {
                    case "R_ThumbDistal":
                        heartbeatHandler.fingerListRight[0].Remove(thisNo);
                        break;
                    case "R_IndexDistal":
                        heartbeatHandler.fingerListRight[1].Remove(thisNo);
                        break;
                    case "R_MiddleDistal":
                        heartbeatHandler.fingerListRight[2].Remove(thisNo);
                        break;
                    case "R_RingDistal":
                        heartbeatHandler.fingerListRight[3].Remove(thisNo);
                        break;
                    case "R_LittleDistal":
                        heartbeatHandler.fingerListRight[4].Remove(thisNo);
                        break;
                    case "R_PalmCollider":
                        heartbeatHandler.fingerListRight[5].Remove(thisNo);
                        break;
                }
            }
        }

    }
}
