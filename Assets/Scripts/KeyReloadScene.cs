using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KeyReloadScene : MonoBehaviour
{
    private int currentSceneID;

    private void OnTriggerEnter(Collider collider)
    {
        currentSceneID = SceneManager.GetActiveScene().buildIndex;
        LevelManager.singleton.GoToLevel(currentSceneID);
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
