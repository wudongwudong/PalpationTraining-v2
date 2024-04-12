using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PainRegionDetector : MonoBehaviour
{
    [SerializeField] private LiverEdgeHaptics liverEdgePalpation;

    void OnTriggerEnter(Collider col)
    {
        if (col.name == "GhostPalm")
            liverEdgePalpation.isInPainRegion = true;
        
    }

    void OnTriggerExit(Collider col)
    {
        if (col.name == "GhostPalm")
            liverEdgePalpation.isInPainRegion = false;
    }
}
