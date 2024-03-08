using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Change : MonoBehaviour
{
    public Animator anim;
    public static int state;

    // Start is called before the first frame update
    void Start()
    {
        state = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.collider.name == "Wolf2") {
                    if (state == 0) {
                        state = 1;
                        anim.SetTrigger("Walk");
                        Debug.Log("Walking");
                    } else if (state == 1) {
                        state = 0;
                        anim.SetTrigger("Run");
                        Debug.Log("Running");
                    } else if (state == 2) {
                        state = 1;
                        anim.SetTrigger("Walk");
                        Debug.Log("Walking");
                    }
                }
                if (hit.collider.name == "Wolf3") {
                    state = 2;
                    anim.SetTrigger("Idle");
                    Debug.Log("Idle");
                }
            }
        }
    }
}
