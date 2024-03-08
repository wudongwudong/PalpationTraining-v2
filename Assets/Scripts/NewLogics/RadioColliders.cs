using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioColliders : MonoBehaviour
{
    public bool isSelected = false;
    [SerializeField] private GameObject otherButton1, otherButton2;
    [SerializeField] private GameObject objectToShow;
    [SerializeField] private string nameThis, otherName1, otherName2;
    [SerializeField] private Vector3 position;
    [SerializeField] private Vector3 eulerAngle;

    [SerializeField] private Material selectedMaterial, defaultMaterial;

    private MeshRenderer meshRenderer;

    void Start()
    {
        //selectedMaterial = Resources.Load<Material>("Materials/Green_Transparent");
        //defaultMaterial = Resources.Load<Material>("Materials/Blue_Transparent");

        meshRenderer = gameObject.GetComponent<MeshRenderer>();

        if (isSelected)
            meshRenderer.material = selectedMaterial;
        else
            meshRenderer.material = defaultMaterial;

    }

    void OnTriggerEnter(Collider col)
    {
        if (col.name.Contains("_PalmCollider"))
        {
            //Delete other objects,
            GameObject previousObject = GameObject.Find(nameThis);
            if (previousObject != null)
            {
                Destroy(previousObject);
            }
            previousObject = GameObject.Find(otherName1);
            if (previousObject != null)
            {
                Destroy(previousObject);
                otherButton1.GetComponent<MeshRenderer>().material = defaultMaterial;
            }
            previousObject = GameObject.Find(otherName2);
            if (previousObject != null)
            {
                Destroy(previousObject);
                otherButton2.GetComponent<MeshRenderer>().material = defaultMaterial;
            }

            //generate objects
            GameObject generatedObject = Instantiate(objectToShow);
            generatedObject.name = nameThis;
            generatedObject.transform.SetParent(transform.parent.parent);
            generatedObject.transform.localPosition = position;
            generatedObject.transform.localRotation = Quaternion.Euler(eulerAngle);

            meshRenderer.material = selectedMaterial;
        }
        
    }
}
