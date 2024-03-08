using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneObjects : MonoBehaviour
{
    public GameObject model;

    // Start is called before the first frame update
    void Start()
    {
        model.SetActive(false);
    }

    // Update is called once per frame
    public void showModel()
    {
        model.SetActive(true);
    }
}