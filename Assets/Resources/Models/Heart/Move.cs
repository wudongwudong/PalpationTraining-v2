using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject heart; 
    public float speed = 0.08f;
    public float x = 0.5f;
    public float y = -1.0f;
    public float z = 0.0f;
    public float delay = 0.0f; 
    private float startTime, currentTime; 
    private Vector3 origPos;

    // Start is called before the first frame update
    void Start()
    {
        origPos = heart.transform.position;
        currentTime = startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime = Time.time; 
        //Debug.Log(currentTime); 

        if (currentTime-startTime > delay)
        {
            float time = Mathf.PingPong(Time.time * speed, 0.3f);
            //heart.transform.position = new Vector3(origPos.x + time*0.5f, origPos.y - time, origPos.z);
            heart.transform.position = new Vector3(origPos.x + time*x, origPos.y + time*y, origPos.z + time*z);
        }
    }
}
