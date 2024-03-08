using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform parent;
    public Transform child;
    public Transform secondParent;

    void Update()
    {
        //get child local position and local orientation
        Vector3 localPosition = Quaternion.Inverse(parent.rotation) * (child.position - parent.position);
        Debug.Log("local position:" + localPosition.ToString("F4"));

        Quaternion localOrientation = Quaternion.Inverse(parent.rotation)* child.rotation ;
        Debug.Log("local orientation:" + localOrientation.eulerAngles.ToString("F4"));


        //world position if the cube have a same local position with the first parent
        Vector3 worldPosition = Quaternion.Inverse(secondParent.rotation) * localPosition+secondParent.position;
        Debug.Log("world position:" + worldPosition.ToString("F4"));
        worldPosition = secondParent.rotation * localPosition + secondParent.position;
        Debug.Log("world position:" + worldPosition.ToString("F4"));


        worldPosition = secondParent.TransformPoint(localPosition);
        Debug.Log("world position:" + worldPosition.ToString("F4"));






        ////get child local position and local orientation
        //Vector3 localPosition = parent.InverseTransformPoint(child.position);
        //Debug.Log("local position:" + localPosition.ToString("F4"));

        //Quaternion localOrientation = Quaternion.Inverse(parent.rotation) * child.rotation;
        //Debug.Log("local orientation:" + localOrientation.eulerAngles.ToString("F4"));


        ////world position if the cube have a same local position with the first parent
        //Vector3 worldPosition = secondParent.TransformPoint(localPosition);
        //Debug.Log("world position:" + worldPosition.ToString("F4"));

        //Quaternion worldOrientation = secondParent.rotation * localOrientation;
        //Debug.Log("world orientation:" + worldOrientation.eulerAngles.ToString("F4"));
    }
}
