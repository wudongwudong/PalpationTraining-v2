using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expand : MonoBehaviour
{
    public GameObject heart; 
    public float expansion = 2000f;
    public float sx = 1.0f;
    public float sy = 2.0f;
    public float sz = 1.0f;
    public float delay = 0.0f; 
    private float startTime, currentTime; 
    private Vector3 origScale;
    private Vector3 scaleChange;

    // Start is called before the first frame update
    void Start()
    {
        origScale = heart.transform.localScale;
        //scaleChange = new Vector3(origScale.x/expansion, 2*origScale.y/expansion, origScale.z/expansion);
        scaleChange = new Vector3(sx*origScale.x/expansion, sy*origScale.y/expansion, sz*origScale.z/expansion);
        currentTime = startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime = Time.time; 
        //Debug.Log(currentTime); 

        if (currentTime-startTime > delay)
        {
            heart.transform.localScale += scaleChange;
            if (heart.transform.localScale.y < (1.0f*origScale.y) || heart.transform.localScale.y > (1.1f*origScale.y))
            {
                scaleChange = -scaleChange;
            }
        }
    }
}
