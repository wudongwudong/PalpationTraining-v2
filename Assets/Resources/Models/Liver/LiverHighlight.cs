﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiverHighlight : MonoBehaviour
{
    public GameObject liverPart;
    public Material matHighlight;
    private Material matDefault;

    void Start()
    {
        matDefault = liverPart.GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            if (hit.collider.name == liverPart.gameObject.name)
            {
                liverPart.GetComponent<MeshRenderer>().material = matHighlight;
            }
            else
            {
                liverPart.GetComponent<MeshRenderer>().material = matDefault;
            }
        }
    }
}