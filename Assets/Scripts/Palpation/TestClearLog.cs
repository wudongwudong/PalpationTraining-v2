using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestClearLog : MonoBehaviour
{
    public HoloLensClient holoClient;

    void OnTriggerEnter(Collider col)
    {
        if (col.name == "_PalmCollider")
        {
            holoClient.debugLog = "";
        }
    }
}
