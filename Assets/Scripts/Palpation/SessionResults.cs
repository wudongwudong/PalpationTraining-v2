using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SessionResults : MonoBehaviour
{
    [HideInInspector] public string nonPainRegionResult = "No";
    [HideInInspector] public string checkListResult = "NA";
    public TMP_Text resultPart1TMP, resultPart2TMP;

    void Start()
    {
        nonPainRegionResult = "No";
        checkListResult = "NA";
    }

    public void ShowResults()
    {
        resultPart1TMP.text = nonPainRegionResult;
        resultPart2TMP.text = checkListResult;
    }

    public void ClearResults()
    {
        resultPart1TMP.text = "";
        resultPart2TMP.text = "";

        nonPainRegionResult = "No";
        checkListResult = "NA";
    }
}
