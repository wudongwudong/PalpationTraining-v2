using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleButton : MonoBehaviour
{
    public bool isToggle;
    public bool toggled;
    public HaptGloveUI haptGloveUI;
    public Material unpressedMaterial, pressedMaterial;

    

    void OnTriggerEnter(Collider col)
    {
        if (col.name == "GhostIndex")
        {
            if (isToggle)
            {
                if (toggled)
                {
                    gameObject.GetComponent<MeshRenderer>().material = unpressedMaterial;
                    toggled = false;
                }
                else
                {
                    gameObject.GetComponent<MeshRenderer>().material = pressedMaterial;
                    toggled = true;
                }
                
            }
            else
            {
                gameObject.GetComponent<MeshRenderer>().material = pressedMaterial;
            }

            haptGloveUI.UI_Actions(gameObject.name);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.name == "GhostIndex")
        {
            if (isToggle)
            {
                // do noting
            }
            else
            {
                gameObject.GetComponent<MeshRenderer>().material = unpressedMaterial;
            }

            StartCoroutine("DelayFunc");
        }
    }

    IEnumerator DelayFunc()
    {
        gameObject.GetComponent<BoxCollider>().enabled = false;
        yield return new WaitForSeconds(0.5f);
        gameObject.GetComponent<BoxCollider>().enabled = true;
    }
}
