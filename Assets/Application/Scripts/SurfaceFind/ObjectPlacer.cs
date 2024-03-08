using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace LocalJoost.Examples
{
    public class ObjectPlacer : MonoBehaviour
    {
        [SerializeField]
        private GameObject objectToPlace;

        public string placedObjName = "Props";

        public void PlaceObject(MixedRealityPose pose)
        {
            var obj = Instantiate(objectToPlace, gameObject.transform);
            obj.name = placedObjName;
            obj.transform.position = pose.Position;

            Vector3 objToCam = Camera.main.transform.position - pose.Position;
            Vector3 buf = Vector3.Cross(objToCam, Vector3.up);
            Vector3 buf2 = Vector3.Cross(buf, Vector3.up);
            obj.transform.rotation = Quaternion.LookRotation(buf2, Vector3.up);
        }
    }
}