using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPainRegionDetector : MonoBehaviour
{
    private bool nonPainRegionTouched = false;
    [SerializeField] private SessionResults sessionResults;
    void OnTriggerEnter(Collider col)
    {
        if (col.name == "GhostPalm")
        {
            nonPainRegionTouched = true;
            sessionResults.nonPainRegionResult = "Yes";
            Debug.Log("Yes");
        }
    }

    //void OnTriggerExit(Collider col)
    //{
    //    if (col.name == "GhostPalm")
    //    {
    //        nonPainRegionTouched = false;
    //        sessionResults.nonPainRegionResult = "No";
    //        Debug.Log("No");
    //    }
    //}
}
