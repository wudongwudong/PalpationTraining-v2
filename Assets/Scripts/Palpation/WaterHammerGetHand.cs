using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HaptGlove;

public class WaterHammerGetHand : MonoBehaviour
{
    public bool pulseReadyToApply = false;
    //public bool fingerTouchPulse = false;
    public GameObject rightJointGameObject;
    public Vector3 iniRotation;

    private ConfigurableJoint joint;
    public HaptGloveHandler interactingHand;
    //public HaptGloveHandler gloveHandler;
    private HapMaterial hapMaterial;

    void Start()
    {
        joint = rightJointGameObject.GetComponent<ConfigurableJoint>();

        ShoulderJointReset();
        rightJointGameObject.transform.localRotation = Quaternion.Euler(iniRotation);

        hapMaterial = GetComponent<HapMaterial>();
    }

    void Update()
    {
        if (hapMaterial.isGrasped)
        {
            if (pulseReadyToApply == false)
            {
                pulseReadyToApply = true;
            }
        }
        else
        {
            if (pulseReadyToApply == true)
            {
                pulseReadyToApply = false;
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Tracker"))
        {
            switch (col.gameObject.transform.parent.name)
            {
                case "ViveTracker_Left":
                    //whichHand = "LargeLeft";
                    interactingHand = GameObject.Find("Hand").transform.Find("Generic Hand_Left").gameObject.GetComponent<HaptGloveHandler>();
                    break;
                case "ViveTracker_Right":
                    //whichHand = "LargeRight";
                    interactingHand = GameObject.Find("Hand").transform.Find("Generic Hand_Right").gameObject.GetComponent<HaptGloveHandler>();
                    break;
                case "ViveTracker_Left_Medium":
                    //whichHand = "MediumLeft";
                    interactingHand = GameObject.Find("Hands_Medium").transform.Find("Generic Hand_Left").gameObject.GetComponent<HaptGloveHandler>();
                    break;
                case "ViveTracker_Right_Medium":
                    //whichHand = "MediumRight";
                    interactingHand = GameObject.Find("Hands_Medium").transform.Find("Generic Hand_Right").gameObject.GetComponent<HaptGloveHandler>();
                    break;
                case "ViveTracker":
                    //whichHand = "MediumRight";
                    interactingHand = GameObject.Find("Hand Tutor").gameObject.GetComponent<HaptGloveHandler>();
                    break;
            }

            if (interactingHand != null)
            {
                Debug.Log("Interacting Hand: " + interactingHand.GetComponent<HaptGloveHandler>().whichHand);
            }
            else
            {
                Debug.Log("Interacting Hand null");
            }

        }

        if (hapMaterial.isGrasped)
        {
            ShoulderJointFree();
        }
    }

    void OnTriggerExit()
    {
        if (hapMaterial.isGrasped == false)
        {
            ShoulderJointReset();
        }
    }

    void ShoulderJointReset()
    {
        JointDrive jd = new JointDrive();
        jd.positionSpring = 40;
        jd.positionDamper = 20;
        jd.maximumForce = 100;
        joint.angularXDrive = jd;
        joint.angularYZDrive = jd;
    }

    void ShoulderJointFree()
    {
        JointDrive jd = new JointDrive();
        jd.positionSpring = 0;
        jd.positionDamper = 0;
        jd.maximumForce = 0;
        joint.angularXDrive = jd;
        joint.angularYZDrive = jd;
    }

}
