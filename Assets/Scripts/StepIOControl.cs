using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HaptGlove;

public class StepIOControl : MonoBehaviour
{
    public HaptGloveHandler bt;

    public byte dutyCycle;
    //IO mapping
    private byte thumbVib = 19;
    private byte indexVib = 21;
    private byte middleVib = 22;
    private byte ringVib = 23;
    private byte PinkyVib = 18;     //Originally set to ValvePinkyEx

    void OnTriggerEnter(Collider col)
    {
        switch (col.name)
        {
            case "GhostThumbB":
                StepControl(0, true, dutyCycle);
                break;
            case "GhostIndexC":
                StepControl(1, true, dutyCycle);
                break;
            case "GhostMiddleC":
                StepControl(2, true, dutyCycle);
                break;
            case "GhostRingC":
                StepControl(3, true, dutyCycle);
                break;
            case "GhostPinkyC":
                StepControl(4, true, dutyCycle);
                break;
        }
    }

    void OnTriggerExit(Collider col)
    {
        switch (col.name)
        {
            case "GhostThumbB":
                StepControl(0, false, dutyCycle);
                break;
            case "GhostIndexC":
                StepControl(1, false, dutyCycle);
                break;
            case "GhostMiddleC":
                StepControl(2, false, dutyCycle);
                break;
            case "GhostRingC":
                StepControl(3, false, dutyCycle);
                break;
            case "GhostPinkyC":
                StepControl(4, false, dutyCycle);
                break;
        }
    }

    private void StepControl(byte fingerID, bool turnOn, byte dutyCycle)
    {
        Encode.Instance.add_u8(fingerID);
        Encode.Instance.add_b1(turnOn);
        Encode.Instance.add_u8(dutyCycle);
        byte[] buf = Encode.Instance.add_fun(0x01);       // FI = 3
        Encode.Instance.clear_list();
        bt.BTSend(buf);
    }
}
