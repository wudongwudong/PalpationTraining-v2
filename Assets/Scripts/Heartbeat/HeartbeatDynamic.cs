using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeartbeatDynamic : MonoBehaviour
{
    public HeartbeatHandler heartHandler;
    public TMP_Text heartLabel;
    public GameObject heatmapHypertension;
    public GameObject heatmapHeartFailure;

    private bool cycleStart = false;
    private bool loopStart = false;

    void FixedUpdate()
    {
        if (heartHandler.beatOn & (loopStart == false))
        {
            loopStart = true;
        }

        if (loopStart == true)
        {
            if (cycleStart == false)
            {
                cycleStart = true;
                Debug.Log("Start dynamic");
                StartCoroutine("HeartDynamic");
            }
        }

        if ((heartHandler.beatOn == false) & (loopStart == true))
        {
            loopStart = false;
            cycleStart = false;
            Debug.Log("Stop dynamic");
            StopCoroutine("HeartDynamic");

            UpdateHeartPara(20, 50, 1.167f); //70hz
            heartLabel.text = "Healthy heart";
            heatmapHypertension.SetActive(false);
            heatmapHeartFailure.SetActive(false);
        }


        //if (heartHandler.beatOn & (cycleStart == false))
        //{
        //    cycleStart = true;
        //    StartCoroutine("HeartDynamic");
        //}
        //else
        //{
        //    StopCoroutine("HeartDynamic");
        //}
    }

    IEnumerator HeartDynamic()
    {
        heartHandler.isInteractable = true;

        //Healthy heart
        UpdateHeartPara(20, 50, 1.167f); //70hz
        heartLabel.text = "Healthy heart";
        yield return new WaitForSeconds(5);

        //Hypertensive stress 
        UpdateHeartPara(20, 50, 1.667f); //100hz
        heartLabel.text = "Hypertensive stress";
        heatmapHypertension.SetActive(true);
        yield return new WaitForSeconds(5);
        heatmapHypertension.SetActive(false);

        //Heart failure
        UpdateHeartPara(10, 15, 1.667f); //100hz
        heartLabel.text = "Heart failure";
        heatmapHeartFailure.SetActive(true);
        yield return new WaitForSeconds(5);
        heatmapHeartFailure.SetActive(false);

        //Non-beating heart
        heartHandler.isInteractable = false;
        heartHandler.StopBeating(); //0hz
        heartLabel.text = "Non-beating heart";
        yield return new WaitForSeconds(3);

        heartHandler.isInteractable = true;

        cycleStart = false;
    }

    private void UpdateHeartPara(byte pres1, byte pres2, float hz)
    {
        heartHandler.pres1 = pres1;
        heartHandler.pres2 = pres2;
        heartHandler.heartBeat_Hz = hz;
    }
}
