using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTransform : MonoBehaviour
{
    void Start()
    {
        // Set the position and rotation when the game starts
        transform.position = new Vector3(-0.5f, -0.2f, 0.75f);
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}

