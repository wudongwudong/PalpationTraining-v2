using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFollow : MonoBehaviour
{
    public float heightOffset;
    public GameObject vrCamera;
    public GameObject neck;

    void Start()
    {
    }

    void Update()
    {
        if ((vrCamera != null)&(neck != null))
        {
            gameObject.transform.position = new Vector3(vrCamera.transform.position.x, vrCamera.transform.position.y - heightOffset, vrCamera.transform.position.z);
            Vector3 cameraEulerAngles = vrCamera.transform.rotation.eulerAngles;
            Vector3 currentEulerAngles = gameObject.transform.rotation.eulerAngles;
            gameObject.transform.rotation = Quaternion.Euler(currentEulerAngles.x, cameraEulerAngles.y, currentEulerAngles.z);

            neck.transform.rotation = vrCamera.transform.rotation;
        }
    }
}
