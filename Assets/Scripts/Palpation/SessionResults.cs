using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SessionResults : MonoBehaviour
{
    [HideInInspector] public string nonPainRegionResult = "No";
    [HideInInspector] public string checkListResult = "NA";
    public TMP_Text resultPart1TMP, resultPart2TMP;
    public HoloLensClient holoClient;
    public LiverEdgeHaptics liverEdgeFelt;

    void Start()
    {
        nonPainRegionResult = "No";
        checkListResult = "NA";
    }

    public void ShowResults()
    {
        //resultPart1TMP.text = nonPainRegionResult;
        //resultPart2TMP.text = checkListResult;

        string[] questions = holoClient.Questions;
        string[] questionResults = holoClient.questionResults.ToArray();

        Debug.Log(questionResults.Length);
        resultPart2TMP.text = questionResults[0] + "\n" + questionResults[1] + "\n" + questionResults[2];

        string liverFelt = "No";
        if (liverEdgeFelt.liverEdgeFelt)
        {
            liverFelt = "Yes";
        }

        resultPart1TMP.text = nonPainRegionResult + "\n" + "No" + "\n" + liverFelt;
    }

    public void ClearResults()
    {
        resultPart1TMP.text = "";
        resultPart2TMP.text = "";

        nonPainRegionResult = "No";
        checkListResult = "NA";
    }
}
