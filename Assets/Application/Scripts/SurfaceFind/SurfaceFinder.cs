using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace LocalJoost.Examples
{
    public class SurfaceFinder : MonoBehaviour
    {
        [Tooltip("Surface magnetism component being used to control the process")]
        [SerializeField]
        private SurfaceMagnetism surfaceMagnet;
        
        [SerializeField]
        [Tooltip("Prompt to encourage the user to look at the floor")]
        private GameObject lookPrompt;

        [SerializeField]
        [Tooltip("Prompt to ask the user if this is indeed the floor")]
        private GameObject confirmPrompt;
        
        [SerializeField]
        [Tooltip("Sound that should be played when the conform prompt is displayed")]
        private AudioSource locationFoundSound;
        
        [SerializeField]
        [Tooltip("Triggered once when the location is accepted.")]
        private UnityEvent<MixedRealityPose> locationFound = new UnityEvent<MixedRealityPose>();

        private Vector3 curPosition;
        private Vector3? foundPosition = null;
        private Vector3 previousPosition;
        private SolverHandler solverHandler;

        [SerializeField] 
        [Tooltip("Confirmation dialog")]
        private GameObject dialog;

        public ObjectPlacer objectPlacer;
        //public TMP_Text logText;
        
        private void Awake()
        {
            solverHandler = surfaceMagnet.GetComponent<SolverHandler>();
            surfaceMagnet.enabled = true;
        }

        private void OnEnable()
        {
            GameObject previousPlacedObj = GameObject.Find(objectPlacer.placedObjName);
            if (previousPlacedObj != null)
            {
                Destroy(previousPlacedObj);
            }
            Reset();
        }

        private void Update()
        {
            CheckLocationOnSurface();
        }

        public void Reset()
        {
            previousPosition = Vector3.zero;
            foundPosition = null;
            lookPrompt.SetActive(true);
            confirmPrompt.SetActive(false);
            solverHandler.enabled = true;
            staySillTime = 0;
        }

        public void Accept()
        {
            if (foundPosition != null)
            {
                locationFound?.Invoke(new MixedRealityPose(
                    foundPosition.Value, solverHandler.transform.rotation));
                lookPrompt.SetActive(false);
                confirmPrompt.SetActive(false);
                gameObject.SetActive(false);
            }
        }

        private float staySillTime = 0;
        //private int index;
        private void CheckLocationOnSurface()
        {
            //logText.text = "index: " + index + "\nStay time: " + staySillTime;

            if (foundPosition == null)
            {
                if (surfaceMagnet.OnSurface)
                {
                    curPosition = surfaceMagnet.transform.position;
                    //index++;
                }
                
                if (curPosition != Vector3.zero)
                {
                    var isMoving = Mathf.Abs((previousPosition - curPosition).magnitude) > 0.005;
                    previousPosition = curPosition;
                    if( !isMoving )
                    {
                        if (staySillTime > 1)
                        {
                            foundPosition = curPosition;

                            solverHandler.enabled = false;
                            lookPrompt.SetActive(false);
                            confirmPrompt.SetActive(true);
                            locationFoundSound.Play();
                            staySillTime = 0;

                            //Show confirmation dialog
                            dialog.SetActive(true);
                        }
                        else
                        {
                            staySillTime += Time.deltaTime;
                        }
                    }
                    else
                    {
                        staySillTime = 0;
                    }
                }
            }
        }
    }
}