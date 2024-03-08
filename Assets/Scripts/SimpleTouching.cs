using System.Collections;
using System.Collections.Generic;
using HaptGlove;
using UnityEngine;

public class SimpleTouching : MonoBehaviour
{
    //public TMP_Text text;
    [SerializeField] private GameObject graspedObject;
    [SerializeField] private List<string> fingerLeftList = new List<string>();
    [SerializeField] private List<string> fingerRightList = new List<string>();
    [SerializeField] private GameObject targetHand;
    [SerializeField] private byte tarPres = 0;

    private byte[] clutchState = new byte[2];
    private bool leftGrasped = false;
    private bool rightGrasped = false;

    public string controlPanelName = "";
    private GameObject controlPanel;

    private void Start()
    {
        controlPanel = GameObject.Find(controlPanelName);
    }

    void OnTriggerEnter(Collider col)
    {
        HaptGloveHandler gloveHandler = col.GetComponentInParent<HaptGloveHandler>();
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

        //if (fingerList.Contains(col.name))
        //{
        //    return;
        //}


        switch (col.name)
        {
            case "L_ThumbDistal":
                clutchState = new byte[2] { 0x00, 0x00 };
                break;
            case "L_IndexDistal":
                clutchState = new byte[2] { 0x01, 0x00 };
                break;
            case "L_MiddleDistal":
                clutchState = new byte[2] { 0x02, 0x00 };
                break;
            case "L_RingDistal":
                clutchState = new byte[2] { 0x03, 0x00 };
                break;
            case "L_LittleDistal":
                clutchState = new byte[2] { 0x04, 0x00 };
                break;
            case "L_PalmCollider":
                clutchState = new byte[2] { 0x05, 0x00 };
                break;

            case "R_ThumbDistal":
                clutchState = new byte[2] { 0x00, 0x00 };
                break;
            case "R_IndexDistal":
                clutchState = new byte[2] { 0x01, 0x00 };
                break;
            case "R_MiddleDistal":
                clutchState = new byte[2] { 0x02, 0x00 };
                break;
            case "R_RingDistal":
                clutchState = new byte[2] { 0x03, 0x00 };
                break;
            case "R_LittleDistal":
                clutchState = new byte[2] { 0x04, 0x00 };
                break;
            case "R_PalmCollider":
                clutchState = new byte[2] { 0x05, 0x00 };
                break;

            default:
                return;
        }


        if (!fingerList.Contains(col.name))
        {
            //Haptics.ApplyHaptics(clutchState, tarPres, targetHand.GetComponent<GraspingLeft>().whichHand, false);
            byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchState, tarPres, false);
            gloveHandler.BTSend(btData);
            Debug.Log("Haptics applied to: " + clutchState[0] + " at " + tarPres);
        }

        fingerList.Add(col.name);

        if ((fingerLeftList.Count == 1)|(fingerRightList.Count == 1))
        {
            ToggleControlPanel(false);
        }
    }

    void OnTriggerExit(Collider col)
    {
        HaptGloveHandler gloveHandler = col.GetComponentInParent<HaptGloveHandler>();
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
                clutchState = new byte[2] { 0x00, 0x02 };
                break;
            case "L_IndexDistal":
                clutchState = new byte[2] { 0x01, 0x02 };
                break;
            case "L_MiddleDistal":
                clutchState = new byte[2] { 0x02, 0x02 };
                break;
            case "L_RingDistal":
                clutchState = new byte[2] { 0x03, 0x02 };
                break;
            case "L_LittleDistal":
                clutchState = new byte[2] { 0x04, 0x02 };
                break;
            case "L_PalmCollider":
                clutchState = new byte[2] { 0x05, 0x02 };
                break;

            case "R_ThumbDistal":
                clutchState = new byte[2] { 0x00, 0x02 };
                break;
            case "R_IndexDistal":
                clutchState = new byte[2] { 0x01, 0x02 };
                break;
            case "R_MiddleDistal":
                clutchState = new byte[2] { 0x02, 0x02 };
                break;
            case "R_RingDistal":
                clutchState = new byte[2] { 0x03, 0x02 };
                break;
            case "R_LittleDistal":
                clutchState = new byte[2] { 0x04, 0x02 };
                break;
            case "R_PalmCollider":
                clutchState = new byte[2] { 0x05, 0x02 };
                break;

            default:
                return;
        }


        if (fingerList.Contains(col.name))
        {
            IEnumerator myCouroutine = Delay(gloveHandler, clutchState, fingerList, col);
            StartCoroutine(myCouroutine);
        }
        
    }

    //private IEnumerator myCouroutine;
    IEnumerator Delay(HaptGloveHandler gloveHandler, byte[] clutchState, List<string> fingerList, Collider col)
    {
        yield return new WaitForSeconds(0.01f);

        fingerList.Remove(col.name);

        if (!fingerList.Contains(col.name))
        {
            //Haptics.ApplyHaptics(clutchState, tarPres, targetHand.GetComponent<GraspingLeft>().whichHand, false);
            byte[] btData = gloveHandler.haptics.ApplyHaptics(clutchState, tarPres, false);
            gloveHandler.BTSend(btData);
            Debug.Log("Haptics removed to: " + clutchState[0] + " at " + tarPres);
        }

        if ((fingerLeftList.Count == 0) & (fingerRightList.Count == 0))
        {
            ToggleControlPanel(true);
        }
    }

    private void ToggleControlPanel(bool state)
    {
        if (controlPanel != null)
        {
            foreach (Transform child in controlPanel.transform)
            {
                child.GetComponent<SphereCollider>().enabled = state;
            }
        }
    }

    //void Update()
    //{
    //    if ((fingerLeftList.Count >= 2) & fingerLeftList.Contains("L_thumb_b"))
    //    {
    //        if (leftGrasped == false)
    //        {
    //            leftGrasped = true;

    //            if (targetHand.GetComponent<FixedJoint>() == null)
    //            {
    //                fixedJoint = targetHand.AddComponent<FixedJoint>();
    //                fixedJoint.connectedBody = graspedObject.GetComponent<Rigidbody>();
    //                fixedJoint.massScale = 1e-05f;
    //                //fixedJoint = gameObject.AddComponent<FixedJoint>();
    //                //fixedJoint.connectedBody = targetHand.GetComponent<Rigidbody>();
    //                graspedObject.GetComponent<Rigidbody>().isKinematic = false;
    //                //Debug.Log("isKinematic = false");
    //            }

    //        }
    //    }
    //    else
    //    {
    //        if (leftGrasped == true)
    //        {
    //            leftGrasped = false;
    //            //Destroy(targetHand.GetComponent<FixedJoint>());

    //            if (targetHand.GetComponent<FixedJoint>() != null)
    //            {
    //                Destroy(targetHand.GetComponent<FixedJoint>());
    //            }

    //            graspedObject.GetComponent<Rigidbody>().isKinematic = true;
    //            //Debug.Log("isKinematic = true");

    //        }
    //    }

    //    if ((fingerRightList.Count >= 2) & fingerRightList.Contains("L_thumb_b"))
    //    {
    //        if (rightGrasped == false)
    //        {
    //            rightGrasped = true;

    //            if (targetHand.GetComponent<FixedJoint>() == null)
    //            {
    //                fixedJoint = targetHand.AddComponent<FixedJoint>();
    //                fixedJoint.connectedBody = graspedObject.GetComponent<Rigidbody>();
    //                fixedJoint.massScale = 1e-05f;
    //                //fixedJoint = gameObject.AddComponent<FixedJoint>();
    //                //fixedJoint.connectedBody = targetHand.GetComponent<Rigidbody>();
    //                graspedObject.GetComponent<Rigidbody>().isKinematic = false;

    //                Debug.Log("Create fixed joint on right hand");
    //                //Debug.Log("isKinematic = false");
    //            }


    //        }
    //    }
    //    else
    //    {
    //        if (rightGrasped == true)
    //        {
    //            rightGrasped = false;
    //            //Destroy(targetHand.GetComponent<FixedJoint>());

    //            if (targetHand.GetComponent<FixedJoint>() != null)
    //            {
    //                Destroy(targetHand.GetComponent<FixedJoint>());
    //            }

    //            graspedObject.GetComponent<Rigidbody>().isKinematic = true;

    //            Debug.Log("Destroy fixed joint on right hand");
    //            //Debug.Log("isKinematic = true");
    //        }
    //    }

    //    //text.text = "right grasped = " + rightGrasped;
    //    //text.text += "\nfinger right count = " + fingerRightList.Count;
    //}
}
