using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KeyChangeScene : MonoBehaviour
{
    public bool toNextScene = false;

    private static int currentSceneID;
    private static int LoadSceneID;

    private int changeSceneIndex = 0;

    public static void ManageScene(int index)
    {
        currentSceneID = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneID < SceneManager.sceneCountInBuildSettings - 1)
        {
            LoadSceneID = currentSceneID + index;
        }
        else
        {
            LoadSceneID = 1;
        }

        if (LoadSceneID >= 1)
        {
            LevelManager.singleton.GoToLevel(LoadSceneID);
        }

    }

    private void OnTriggerEnter(Collider collider)
    {
#if !UNITY_ANDROID
        if (toNextScene)
        {
            changeSceneIndex = 1;
        }
        else
        {
            changeSceneIndex = -1;
        }
        ManageScene(changeSceneIndex);
#endif

        gameObject.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Red");

    }

    private void OnTriggerExit(Collider collider)
    {
        gameObject.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Grey_dark");
        StartCoroutine("DelayFunc");
    }

    IEnumerator DelayFunc()
    {
        gameObject.GetComponent<BoxCollider>().enabled = false;
        yield return new WaitForSeconds(1f);
        gameObject.GetComponent<BoxCollider>().enabled = true;
    }
}
