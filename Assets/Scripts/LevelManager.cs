using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using HaptGlove;

public class LevelManager : MonoBehaviour
{
    public static LevelManager singleton;
    public GameObject[] viveTrackerArray;
    public GameObject arrowPrefab;
    public GameObject LargeLeft;
    public GameObject LargeRight;
    public GameObject MediumLeft;
    public GameObject MediumRight;
    public GameObject UIPanel;

    private void Awake()
    {
        //this gameobject (and script) goes between scenes
        DontDestroyOnLoad(this.gameObject);

        if (singleton == null) //ensure that there is only one instance of this gameObject present after switching scenes
        {
            singleton = this;
#if !UNITY_ANDROID
            LargeLeft.AddComponent<HaptGloveHandler>().deviceName = "HaptGloveAR L Left";
            //LargeLeft.AddComponent<FingerMapping_Left>();
            LargeLeft.AddComponent<HaptGloveHandler>().whichHand = HaptGloveHandler.HandType.Left;
            LargeLeft.AddComponent<Grasping>();

            MediumLeft.AddComponent<HaptGloveHandler>().deviceName = "PneuHapGlove M Left";
            //MediumLeft.AddComponent<FingerMapping_Left>();
            MediumLeft.AddComponent<HaptGloveHandler>().whichHand = HaptGloveHandler.HandType.Left;
            MediumLeft.AddComponent<Grasping>();

            LargeRight.AddComponent<HaptGloveHandler>().deviceName = "PneuHapGlove L Right";
            //LargeRight.AddComponent<FingerMapping_Left>();
            LargeRight.AddComponent<HaptGloveHandler>().whichHand = HaptGloveHandler.HandType.Right;
            LargeRight.AddComponent<Grasping>();

            MediumRight.AddComponent<HaptGloveHandler>().deviceName = "PneuHapGlove M Right";
            //MediumRight.AddComponent<FingerMapping_Left>();
            MediumRight.AddComponent<HaptGloveHandler>().whichHand = HaptGloveHandler.HandType.Right;
            MediumRight.AddComponent<Grasping>();

#else
            Collider[] largeLeftColliders = LargeLeft.GetComponentsInChildren<Collider>();
            foreach (var collider in largeLeftColliders)
            {
                collider.isTrigger = true;
            }
            Collider[] mediumLeftColliders = MediumLeft.GetComponentsInChildren<Collider>();
            foreach (var collider in mediumLeftColliders)
            {
                collider.isTrigger = true;
            }
            Collider[] largeRightColliders = MediumLeft.GetComponentsInChildren<Collider>();
            foreach (var collider in largeRightColliders)
            {
                collider.isTrigger = true;
            }

            Collider[] mediumRightColliders = MediumLeft.GetComponentsInChildren<Collider>();
            foreach (var collider in mediumRightColliders)
            {
                collider.isTrigger = true;
            }
#endif
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
#if !UNITY_ANDROID
        //LargeLeft.GetComponent<FingerMapping_Left>().btInput = LargeLeft.GetComponent<BTCommu_Left>();
        //LargeLeft.GetComponent<BTCommu_Left>().fingerMappingLeftScript = LargeLeft.GetComponent<FingerMapping_Left>();
        //LargeLeft.GetComponent<BTCommu_Left>().graspingScript = LargeLeft.GetComponent<GraspingLeft>();


        //MediumLeft.GetComponent<FingerMapping_Left>().btInput = MediumLeft.GetComponent<BTCommu_Left>();
        //MediumLeft.GetComponent<BTCommu_Left>().fingerMappingLeftScript = MediumLeft.GetComponent<FingerMapping_Left>();
        //MediumLeft.GetComponent<BTCommu_Left>().graspingScript = MediumLeft.GetComponent<GraspingLeft>();

        //LargeRight.GetComponent<FingerMapping_Left>().btInput = LargeRight.GetComponent<BTCommu_Left>();
        //LargeRight.GetComponent<BTCommu_Left>().fingerMappingLeftScript = LargeRight.GetComponent<FingerMapping_Left>();
        //LargeRight.GetComponent<BTCommu_Left>().graspingScript = LargeRight.GetComponent<GraspingLeft>();

        //MediumRight.GetComponent<FingerMapping_Left>().btInput = MediumRight.GetComponent<BTCommu_Left>();
        //MediumRight.GetComponent<BTCommu_Left>().fingerMappingLeftScript = MediumRight.GetComponent<FingerMapping_Left>();
        //MediumRight.GetComponent<BTCommu_Left>().graspingScript = MediumRight.GetComponent<GraspingLeft>();

        //FingerMapping_Left.Instance.btInput = BTCommu_Left.Instance;
        //BTCommu_Left.Instance.fingerMappingLeftScript = FingerMapping_Left.Instance;
        //UI_BTTrigger.Instance.btInput = BTCommu_Left.Instance;
        //UI_ExTrigger.Instance.btInput = BTCommu_Left.Instance;
        //UI_PSTrigger.Instance.btInput = BTCommu_Left.Instance;
#endif
    }

    public void GoToLevel(int sceneNo)
    {
        //scene refs
        //0: StartScene
        //1: Training
        //2: Sorting

        StartCoroutine(LevelLoading(sceneNo));
    }

    private IEnumerator LevelLoading(int sceneNo)
    {
        //if (SceneManager.GetActiveScene().name == "Dynamic_us")
        //{
        //    foreach (var tracker in viveTrackerArray)
        //    {
        //        ArrowHand_PneuClutch buf = tracker.GetComponent<ArrowHand_PneuClutch>();
        //        if (buf != null)
        //        {
        //            Destroy(buf);
        //        }
        //    }
        //}

        //AsyncOperation asyncload = PhotonNetwork.LoadLevel(sceneNo); //PhotonNetwork.LoadLevel function is modified by adding a return value
        //networkManager.InitiliazeRoom(SceneManager.GetSceneByBuildIndex(sceneNo).name);

        AsyncOperation asyncload = SceneManager.LoadSceneAsync(sceneNo);
        while (!asyncload.isDone)
        {
            yield return null;
        }
        if (asyncload.isDone)
        {

            //add small delay before ressignement just to make sure there are no errors
            //StartCoroutine(LevelLoaded(1f));
            LevelLoaded();
        }
    }

    private IEnumerator RecreatUIpanel()
    {
        //UIPanel.SetActive(false);
        ConfigurableJoint[] bufJoints = transform.GetComponentsInChildren<ConfigurableJoint>();
        foreach (var bufJoint in bufJoints)
        {
            bufJoint.zMotion = ConfigurableJointMotion.Locked;
        }
        yield return new WaitForSeconds(1);

        //UIPanel.SetActive(true);
        foreach (var bufJoint in bufJoints)
        {
            bufJoint.zMotion = ConfigurableJointMotion.Limited;
        }
    }

    private void LevelLoaded()
    //private IEnumerator LevelLoaded(float waitTime)
    {
        //yield return new WaitForSeconds(waitTime);
        //reassign variables and resume BT output
        switch (SceneManager.GetActiveScene().name)     // set references
        {
            case "StartScene_us":
                StartCoroutine(RecreatUIpanel());

#if !UNITY_ANDROID
                //LargeLeft.GetComponent<FingerMapping_Left>().btInput = LargeLeft.GetComponent<BTCommu_Left>();
                //LargeLeft.GetComponent<BTCommu_Left>().fingerMappingLeftScript = LargeLeft.GetComponent<FingerMapping_Left>();
                //LargeLeft.GetComponent<BTCommu_Left>().graspingScript = LargeLeft.GetComponent<GraspingLeft>();

                //MediumLeft.GetComponent<FingerMapping_Left>().btInput = MediumLeft.GetComponent<BTCommu_Left>();
                //MediumLeft.GetComponent<BTCommu_Left>().fingerMappingLeftScript = MediumLeft.GetComponent<FingerMapping_Left>();
                //MediumLeft.GetComponent<BTCommu_Left>().graspingScript = MediumLeft.GetComponent<GraspingLeft>();

                //LargeRight.GetComponent<FingerMapping_Left>().btInput = LargeRight.GetComponent<BTCommu_Left>();
                //LargeRight.GetComponent<BTCommu_Left>().fingerMappingLeftScript = LargeRight.GetComponent<FingerMapping_Left>();
                //LargeRight.GetComponent<BTCommu_Left>().graspingScript = LargeRight.GetComponent<GraspingLeft>();

                //MediumRight.GetComponent<FingerMapping_Left>().btInput = MediumRight.GetComponent<BTCommu_Left>();
                //MediumRight.GetComponent<BTCommu_Left>().fingerMappingLeftScript = MediumRight.GetComponent<FingerMapping_Left>();
                //MediumRight.GetComponent<BTCommu_Left>().graspingScript = MediumRight.GetComponent<GraspingLeft>();

#endif
                break;


//            case "Dynamic_us":
//#if !UNITY_ANDROID
//                //viveTrackerLeft.AddComponent<ArrowHand_PneuClutch>();
//                //viveTrackerLeft.GetComponent<ArrowHand_PneuClutch>().arrowHand = LargeLeft;

//                //viveTrackerLeftMedium.AddComponent<ArrowHand_PneuClutch>();
//                //viveTrackerLeftMedium.GetComponent<ArrowHand_PneuClutch>().arrowHand = MediumLeft;


//                Transform changeMode = GameObject.Find("ChangeMode").transform;
//                ChangeMode_Dynamic[] changeModeDynamic = changeMode.GetComponentsInChildren<ChangeMode_Dynamic>();
//                //for (int i = 0; i < changeModeDynamic.Length; i++)
//                //{
//                //    changeModeDynamic[i].archeryLarge = viveTrackerLeft;
//                //    changeModeDynamic[i].archeryMedium = viveTrackerLeftMedium;
//                //}
//                changeModeDynamic[0].SetMode("visual");
//#endif
//                break;
            default:
                break;
        }
    }

    //private IEnumerator LevelLoaded(float waitTime)
    //{
    //    yield return new WaitForSeconds(waitTime);
    //    //reassign variables and resume BT output
    //    switch (SceneManager.GetActiveScene().name)     // set references
    //    {
    //        case "StartScene":
    //            FingerMapping_Left.Instance.btInput = BTCommu_Left.Instance;
    //            UI_BTTrigger.Instance.btInput = BTCommu_Left.Instance;
    //            UI_ExTrigger.Instance.btInput = BTCommu_Left.Instance;
    //            UI_PSTrigger.Instance.btInput = BTCommu_Left.Instance;
    //            BTCommu_Left.Instance.fingerMappingLeftScript = FingerMapping_Left.Instance;
    //            break;

    //        case "Training":
    //            break;
    //    }
    //}
}
