using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCoachMoving : MonoBehaviour
{
    public bool isActive = true;
    [SerializeField] private Transform handCoach;
    [SerializeField] private Vector3 offSet;
    [SerializeField] private Vector3 distance;
    [SerializeField] private float timeGo;
    [SerializeField] private float timeStay;
    [SerializeField] private float timeBack;

    private Vector3 iniPosition;
    private Vector3 tarPosition;
    private float timeFrame;

    void Start()
    {
        iniPosition = handCoach.transform.localPosition + offSet;
        tarPosition = iniPosition + distance;
    }

    void Update()
    {
        if (!isActive)
        {
            return;
        }

        timeFrame += Time.deltaTime;

        if (timeFrame <= timeGo)
        {
            handCoach.transform.localPosition = Vector3.Lerp(iniPosition, tarPosition, timeFrame / timeGo);
        }
        else if ((timeFrame > timeGo) & (timeFrame <= (timeGo + timeStay)))
        {

        }
        else if ((timeFrame > (timeGo + timeStay)) & (timeFrame <= (timeGo + timeStay + timeBack)))
        {
            handCoach.gameObject.SetActive(false);

            handCoach.transform.localPosition =
                Vector3.Lerp(tarPosition, iniPosition, (timeFrame - timeGo - timeStay) / timeBack);
        }
        else
        {
            timeFrame = 0;
            handCoach.gameObject.SetActive(true);
        }

    }



    void OnTriggerEnter(Collider col)
    {
        if (col.name.Contains("_PalmCollider"))
        {
            gameObject.SetActive(false);
        }
    }


}
