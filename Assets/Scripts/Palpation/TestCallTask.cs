using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCallTask : MonoBehaviour
{
    public HoloLensClient hololensClient;

    public enum PressState
    {
        Small,
        Medium,
        Large
    }

    public PressState pressState;

    void OnTriggerEnter(Collider col)
    {

        if (col.name.Contains("_PalmCollider"))
        {
            switch (pressState)
            {
                case PressState.Small:
                    Debug.Log("Force level: small");
#if !UNITY_EDITOR
            hololensClient.SendForceDetectedMessage(HoloLensClient.forceLevel.small);
#endif
                    break;
                case PressState.Medium:
                    Debug.Log("Force level: medium");
#if !UNITY_EDITOR
            hololensClient.SendForceDetectedMessage(HoloLensClient.forceLevel.medium);
#endif
                    break;
                case PressState.Large:
                    Debug.Log("Force level: large");
#if !UNITY_EDITOR
            hololensClient.SendForceDetectedMessage(HoloLensClient.forceLevel.large);
#endif
                    break;
            }
        }
    }

//    void OnTriggerExit(Collider col)
//    {
//        if (col.name.Contains("_PalmCollider"))
//        {
//#if !UNITY_EDITOR
//            hololensClient.SendForceDetectedMessage(HoloLensClient.forceLevel.small);
//#endif
//        }
//    }
}
