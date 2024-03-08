using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour
{
    public GameObject heart;
    private string lastHit; 

    void Start()
    {
        lastHit = "none";
        Debug.Log("Collider activated for " + gameObject.name);
    }

    void Update()
    {
        heart.GetComponent<Animate>().partHit = "sarvendocardialContour";

        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit;

        //if (Physics.Raycast(ray, out hit, 100))
        //{
        //    if (hit.collider.name.StartsWith(gameObject.name)) {
        //        if (lastHit != heart.GetComponent<Animate>().partHit)
        //        {
        //            lastHit = heart.GetComponent<Animate>().partHit;
        //            Debug.Log("Hit part => " + lastHit);
        //        }
        //        heart.GetComponent<Animate>().partHit = hit.collider.name;
        //    } 
        //}
        //else
        //{
        //    heart.GetComponent<Animate>().partHit = "none";
        //}
    }
}
