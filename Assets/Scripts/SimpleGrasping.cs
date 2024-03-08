using System.Collections;
using System.Collections.Generic;
using HaptGlove;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SimpleGrasping : MonoBehaviour
{
    [SerializeField] private bool ObjectFreeze = true;
    [SerializeField] private GameObject graspedObject;
    [SerializeField] private List<string> fingerLeftList = new List<string>();
    [SerializeField] private List<string> fingerRightList = new List<string>();
    [SerializeField] private GameObject targetHand;
    private bool leftGrasped = false;
    private bool rightGrasped = false;
    private FixedJoint fixedJoint;

    public UnityAction<HaptGloveHandler.HandType> onGrasped;
    public UnityAction<HaptGloveHandler.HandType> onReleased;

    private void Start()
    {
        //gameObject.GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
    }


    void OnTriggerEnter(Collider col)
    {
        HaptGloveHandler gloveHandler = col.GetComponentInParent<HaptGloveHandler>();
        //GraspingLeft graspingScript = col.GetComponentInParent<GraspingLeft>();
        List<string> fingerList = new List<string>();

        if (gloveHandler != null)
        {
            if (gloveHandler.whichHand == HaptGloveHandler.HandType.Left)
            {
                fingerList = fingerLeftList;
            }
            else if (gloveHandler.whichHand == HaptGloveHandler.HandType.Right)
            {
                fingerList = fingerRightList;
            }

            targetHand = gloveHandler.gameObject;
            //Debug.Log(targetHand.name);
        }
        else
        {
            return;
        }

        if (fingerList.Contains(col.name))
        {
            return;
        }


        switch (col.name)
        {
            case "L_ThumbDistal":
                fingerList.Add("L_ThumbDistal");
                break;
            case "L_IndexDistal":
                fingerList.Add("L_IndexDistal");
                break;
            case "L_MiddleDistal":
                fingerList.Add("L_MiddleDistal");
                break;
            case "L_RingDistal":
                fingerList.Add("L_RingDistal");
                break;
            case "L_LittleDistal":
                fingerList.Add("L_LittleDistal");
                break;

            case "R_ThumbDistal":
                fingerList.Add("R_ThumbDistal");
                break;
            case "R_IndexDistal":
                fingerList.Add("R_IndexDistal");
                break;
            case "R_MiddleDistal":
                fingerList.Add("R_MiddleDistal");
                break;
            case "R_RingDistal":
                fingerList.Add("R_RingDistal");
                break;
            case "R_LittleDistal":
                fingerList.Add("R_LittleDistal");
                break;
        }
    }

    void OnTriggerExit(Collider col)
    {
        HaptGloveHandler gloveHandler = col.GetComponentInParent<HaptGloveHandler>();
        //GraspingLeft graspingScript = col.GetComponentInParent<GraspingLeft>();
        List<string> fingerList = new List<string>();

        if (gloveHandler != null)
        {
            if (gloveHandler.whichHand == HaptGloveHandler.HandType.Left)
            {
                fingerList = fingerLeftList;
            }
            else if (gloveHandler.whichHand == HaptGloveHandler.HandType.Right)
            {
                fingerList = fingerRightList;
            }

            targetHand = gloveHandler.gameObject;
        }
        else
        {
            return;
        }

        switch (col.name)
        {
            case "L_ThumbDistal":
                fingerList.Remove("L_ThumbDistal");
                break;
            case "L_IndexDistal":
                fingerList.Remove("L_IndexDistal");
                break;
            case "L_MiddleDistal":
                fingerList.Remove("L_MiddleDistal");
                break;
            case "L_RingDistal":
                fingerList.Remove("L_RingDistal");
                break;
            case "L_LittleDistal":
                fingerList.Remove("L_LittleDistal");
                break;

            case "R_ThumbDistal":
                fingerList.Remove("R_ThumbDistal");
                break;
            case "R_IndexDistal":
                fingerList.Remove("R_IndexDistal");
                break;
            case "R_MiddleDistal":
                fingerList.Remove("R_MiddleDistal");
                break;
            case "R_RingDistal":
                fingerList.Remove("R_RingDistal");
                break;
            case "R_LittleDistal":
                fingerList.Remove("R_LittleDistal");
                break;
        }
    }

    void Update()
    {
        if ((fingerLeftList.Count >= 2) & fingerLeftList.Contains("L_ThumbDistal"))
        {
            if (leftGrasped == false)
            {
                leftGrasped = true;

                if (onGrasped != null)
                {
                    onGrasped(HaptGloveHandler.HandType.Left);
                }

                if (targetHand.GetComponent<FixedJoint>() == null)
                {
                    fixedJoint = targetHand.AddComponent<FixedJoint>();
                    fixedJoint.connectedBody = graspedObject.GetComponent<Rigidbody>();
                    fixedJoint.massScale = 1e-05f;
                    //fixedJoint = gameObject.AddComponent<FixedJoint>();
                    //fixedJoint.connectedBody = targetHand.GetComponent<Rigidbody>();
                    if (!ObjectFreeze)
                    {
                        graspedObject.GetComponent<Rigidbody>().isKinematic = false;
                    }
                    //Debug.Log("isKinematic = false");
                }

            }
        }
        else
        {
            if (leftGrasped == true)
            {
                leftGrasped = false;

                if (onReleased != null)
                {
                    onReleased(HaptGloveHandler.HandType.Left);
                }
                
                //Destroy(targetHand.GetComponent<FixedJoint>());

                if (targetHand.GetComponent<FixedJoint>() != null)
                {
                    Destroy(targetHand.GetComponent<FixedJoint>());
                }

                if (!ObjectFreeze)
                {
                    graspedObject.GetComponent<Rigidbody>().isKinematic = true;
                }
                //Debug.Log("isKinematic = true");

            }
        }

        if ((fingerRightList.Count >= 2) & fingerRightList.Contains("R_ThumbDistal"))
        {
            if (rightGrasped == false)
            {
                rightGrasped = true;

                if (onGrasped != null)
                {
                    onGrasped(HaptGloveHandler.HandType.Right);
                }

                if (targetHand.GetComponent<FixedJoint>() == null)
                {
                    fixedJoint = targetHand.AddComponent<FixedJoint>();
                    fixedJoint.connectedBody = graspedObject.GetComponent<Rigidbody>();
                    fixedJoint.massScale = 1e-05f;
                    //fixedJoint = gameObject.AddComponent<FixedJoint>();
                    //fixedJoint.connectedBody = targetHand.GetComponent<Rigidbody>();
                    if (!ObjectFreeze)
                    {
                        graspedObject.GetComponent<Rigidbody>().isKinematic = false;
                    }

                    Debug.Log("Create fixed joint on right hand");
                    //Debug.Log("isKinematic = false");
                }


            }
        }
        else
        {
            if (rightGrasped == true)
            {
                rightGrasped = false;
                //Destroy(targetHand.GetComponent<FixedJoint>());

                if (onReleased != null)
                {
                    onReleased(HaptGloveHandler.HandType.Right);
                }

                if (targetHand.GetComponent<FixedJoint>() != null)
                {
                    Destroy(targetHand.GetComponent<FixedJoint>());
                }

                if (!ObjectFreeze)
                {
                    graspedObject.GetComponent<Rigidbody>().isKinematic = true;
                }

                Debug.Log("Destroy fixed joint on right hand");
                //Debug.Log("isKinematic = true");
            }
        }

    }
}
