using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace AI.Fritz.Vision
{
    public class FritzHumanTrackableManager : MonoBehaviour
    {

        /// <summary>
        /// The name prefix that should be used when instantiating new <c>GameObject</c>s.
        /// </summary>
        protected string gameObjectName { get { return "Human"; } }

        private ARSessionOrigin origin;

        /// <summary>
        /// Maximum size that human bounding box can be. Changing these values help better
        /// filter out innacurate positions.
        /// </summary>
        [SerializeField]
        public Vector3 maxHumanSize = new Vector3(0.5f, 3f, 0.5f);

        /// <summary>
        /// The confidence cutoff for parts when updating the location of a human body.
        /// </summary>
        [SerializeField]
        public float partConfidenceCutoff = 0.2f;

        /// <summary>
		/// Size of keypoint spheres.
		/// </summary>
		[SerializeField]
        public float sphereSize = 0.005f;

        /// <summary>
		/// If True, Trackables will render debug spheres on each detected keypoint.
		/// </summary>
        [SerializeField]
        public bool DebugSpheresEnabled = false;

        /// <summary>
        /// If True, Trackables will render debug spheres on each detected keypoint.
        /// </summary>
        [SerializeField]
        public bool DebugBoundingBoxEnabled = false;

        /// <summary>
        /// Minimum session relative distance point must be away from camera for hit
        /// test result to count.
        /// </summary>
        public float minDistance = 0.3f;

        /// <summary>
        /// A dictionary of all trackables, keyed by <c>TrackableId</c>.
        /// </summary>
        protected Dictionary<int, FritzHumanTrackable> trackables = new Dictionary<int, FritzHumanTrackable>();

        /// <summary>
        /// Raycast Manager.  The ARRaycastManager must be added to the AR Session Origin.
        /// </summary>
        public ARRaycastManager RaycastManager { get; private set; }

        private ARCameraManager cameraManager;

        internal Vector2Int Resolution
        {
            get
            {
                if (cameraManager.currentConfiguration.HasValue)
                {
                    return cameraManager.currentConfiguration.Value.resolution;
                }
                return new Vector2Int(Screen.height, Screen.width);
            }
        }

        private void Awake()
        {
            origin = GetComponent<ARSessionOrigin>();
            RaycastManager = origin.GetComponent<ARRaycastManager>();
            cameraManager = origin.camera.GetComponent<ARCameraManager>();

        }
        /// <summary>
        /// The prefab that should be instantiated when adding a trackable.
        /// </summary>
        protected GameObject GetPrefab()
        {
            return null;
        }

        /// <summary>
        /// A dictionary of all trackables, keyed by <c>TrackableId</c>.
        /// </summary>
        protected Dictionary<int, List<GameObject>> points = new Dictionary<int, List<GameObject>>();

        string GetTrackableName(int trackableId)
        {
            return gameObjectName + " " + trackableId.ToString();
        }

        GameObject CreateGameObject()
        {
            var prefab = GetPrefab();
            if (prefab == null)
            {
                var go = new GameObject();
                go.transform.parent = transform;
                return go;
            }

            return Instantiate(prefab);
        }

        GameObject CreateGameObject(int trackableId)
        {
            var go = CreateGameObject();
            go.name = GetTrackableName(trackableId);
            return go;
        }

        FritzHumanTrackable CreateTrackable(int trackableId)
        {
            var go = CreateGameObject(trackableId);
            var trackable = go.GetComponent<FritzHumanTrackable>();
            if (trackable == null)
            {
                trackable = go.AddComponent<FritzHumanTrackable>();
            }
            trackable.SetManager(this);


            return trackable;
        }

        /// <summary>
		/// Main method that creates and updates a trackable. The caller is responsible for
		/// passing in the Trackable ID that the Pose corresponds to.
		/// </summary>
		/// <param name="trackableId"></param>
		/// <param name="pose"></param>
		/// <returns></returns>
		public FritzHumanTrackable CreateOrUpdateTrackable(int trackableId, FritzPose pose)
        {
            FritzHumanTrackable trackable;
            if (!trackables.TryGetValue(trackableId, out trackable))
            {
                trackable = CreateTrackable(trackableId);
                trackables.Add(trackableId, trackable);
            }
            if (trackable == null)
            {
                return null;
            }

            return UpdateTrackable(trackable, pose);
        }

        FritzHumanTrackable UpdateTrackable(FritzHumanTrackable trackable, FritzPose pose)
        {
            trackable.UpdateWithPose(pose);
            return trackable;
        }

        void DestroyTrackable(FritzHumanTrackable trackable)
        {
            Destroy(trackable.gameObject);
        }

    }

}
