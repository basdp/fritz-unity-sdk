using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace AI.Fritz.Vision
{

    public class FritzHumanTrackable : MonoBehaviour
    {
        /// <summary>
        /// The <c>TrackableId</c> associated with this trackable. <c>TrackableId</c>s
        /// are typically unique to a particular session.
        /// </summary>
        public int trackableId = -1;

        private static HashSet<FritzPoseParts> partsToSkip = new HashSet<FritzPoseParts>()
            {
            FritzPoseParts.LeftElbow,
            FritzPoseParts.RightElbow,
            FritzPoseParts.RightWrist,
            FritzPoseParts.LeftWrist
        };
        private static HashSet<FritzPoseParts> partsToSkipZ = new HashSet<FritzPoseParts>()
        {
            FritzPoseParts.LeftEye,
            FritzPoseParts.LeftEar,
            FritzPoseParts.RightEar,
            FritzPoseParts.RightEye,
            FritzPoseParts.Nose
        };

        private Material glassMaterial;

        private static float baseFrequency = 30f;

        FritzHumanTrackableManager trackableManager;

        OneEuroFilter<Vector3>[] filters;

        private FritzPose currentPose;

        private Vector3?[] current3DPoints;

        private Vector3? estimatedCenter;
        private Vector3? estimatedScale;

        private Vector3 MaxBoundarySize
        {
            get
            {
                return trackableManager.maxHumanSize;
            }
        }

        private GameObject[] spheres;

        private GameObject centerPoint;

        private GameObject boundingBox;
        private GameObject cube;

        private OneEuroFilter<Vector3> centerPointFilter = new OneEuroFilter<Vector3>(baseFrequency);
        private OneEuroFilter<Vector3> scaleFilter = new OneEuroFilter<Vector3>(baseFrequency);

        private int PosePartCount
        {
            get
            {
                return Enum.GetNames(typeof(FritzPoseParts)).Length;
            }
        }

        private void Awake()
        {
            glassMaterial = Resources.Load("Glass", typeof(Material)) as Material;
            current3DPoints = new Vector3?[PosePartCount];
            filters = new OneEuroFilter<Vector3>[PosePartCount];
            spheres = new GameObject[PosePartCount];
            for (var i = 0; i < PosePartCount; i++)
            {
                filters[i] = new OneEuroFilter<Vector3>(baseFrequency);
            }
        }

        /// <summary>
        /// Set HumanTrackableManager and create underlying game objects.
        /// </summary>
        /// <param name="manager"></param>
        public void SetManager(FritzHumanTrackableManager manager)
        {
            trackableManager = manager;
            for (var i = 0; i < PosePartCount; i++)
            {
                spheres[i] = CreatePoint(trackableManager.sphereSize);
                spheres[i].SetActive(false);
            }
            centerPoint = CreatePoint(trackableManager.sphereSize);
            centerPoint.SetActive(false);
            boundingBox = CreateBoundingBox();
            boundingBox.SetActive(false);
        }

        /// <summary>
        /// Get the Estimated position (in World Space) of a specific part.
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public Vector3? GetEstimatedPosition(FritzPoseParts part)
        {
            return current3DPoints[(int)part];
        }

        /// <summary>
        /// Gets estimated position of the center of the body.
        /// </summary>
        /// <returns></returns>
        public Vector3? GetEstimatedCenterPosition()
        {
            return estimatedCenter;
        }

        public Transform GetEstimatedTransform()
        {
            if (estimatedCenter.HasValue)
            {
                return boundingBox.transform;
            }
            return null;
        }

        /// <summary>
        /// Update Estimated Scale and Center of Human Body.
        /// </summary>
        public void UpdateEstimatedScaleAndCenter()
        {

            var maxVector = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            var minVector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            foreach (var part in (FritzPoseParts[])Enum.GetValues(typeof(FritzPoseParts)))
            {
                if (partsToSkip.Contains(part))
                {
                    continue;
                }

                if (current3DPoints[(int)part].HasValue)
                {
                    var vector = current3DPoints[(int)part].Value;
                    minVector.x = Math.Min(minVector.x, vector.x);
                    minVector.y = Math.Min(minVector.y, vector.y);
                    maxVector.x = Math.Max(maxVector.x, vector.x);
                    maxVector.y = Math.Max(maxVector.y, vector.y);

                    if (!partsToSkipZ.Contains(part))
                    {
                        minVector.z = Math.Min(minVector.z, vector.z);
                        maxVector.z = Math.Max(maxVector.z, vector.z);
                    }

                }
            }
            if (Math.Abs(minVector.x - float.MaxValue) < 1f || Math.Abs(maxVector.x - float.MinValue) < 1f)
            {
                estimatedScale = null;
                Debug.Log("A value was not initialized, exiting");
                return;
            }

            var scale = maxVector - minVector;
            scale.x = Math.Min(MaxBoundarySize.x, scale.x);
            scale.y = Math.Min(MaxBoundarySize.y, scale.y);
            scale.z = Math.Min(MaxBoundarySize.z, scale.z);
            estimatedScale = scaleFilter.Filter(scale);
            estimatedCenter = centerPointFilter.Filter((maxVector + minVector) / 2);
            boundingBox.transform.position = estimatedCenter.Value;
            boundingBox.transform.localScale = estimatedScale.Value;
            return;
        }

        /// <summary>
        /// Update debug BoundingBox.
        /// </summary>
        public void UpdateBoundingBox()
        {
            if (!estimatedCenter.HasValue || !estimatedScale.HasValue)
            {
                boundingBox.SetActive(false);
                return;
            }
            boundingBox.SetActive(true);

        }

        public void UpdateSpheres()
        {
            for (int i = 0; i < PosePartCount; i++)
            {
                var position = current3DPoints[i];
                var sphere = spheres[i];
                if (position.HasValue)
                {
                    sphere.transform.position = position.Value;
                    sphere.SetActive(true);
                }
                else
                {
                    sphere.SetActive(false);
                }
            }
        }

        GameObject CreatePoint(float scale)
        {
            var point = new GameObject();
            point.transform.parent = transform;
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = Vector3.one * trackableManager.sphereSize;
            sphere.transform.parent = point.transform;
            return point;
        }

        GameObject CreateBoundingBox()
        {
            var go = new GameObject();
            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Renderer rend = cube.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = glassMaterial;
            }
            cube.transform.parent = go.transform;
            return go;
        }

        /// <summary>
        /// Performs raycasting for all keypoints for pose. If point not found,
        /// sets position to null.
        /// </summary>
        /// <param name="pose"></param>
        public void Update3DKeypoints(FritzPose pose)
        {
            var totalDistance = 0.0f;

            foreach (var keypoint in pose.keypoints)
            {
                var viewportPoint = PlacePointInViewport(keypoint.position);

                var screenPoint = new Vector2(
                    viewportPoint.x * Screen.width,
                    viewportPoint.y * Screen.height
                );

                if (keypoint.confidence < trackableManager.partConfidenceCutoff)
                {
                    Debug.LogFormat("Skipping {0} for low confidence of {1}", keypoint.part, keypoint.confidence);
                    current3DPoints[(int)keypoint.part] = null;
                    continue;
                }

                List<ARRaycastHit> results = new List<ARRaycastHit>();

                var raycastSuccess = trackableManager.RaycastManager.Raycast(
                    screenPoint,
                    results,
                    UnityEngine.XR.ARSubsystems.TrackableType.FeaturePoint
                );

                if (raycastSuccess)
                {
                    if (results.Count == 0)
                    {
                        continue;
                    }
                    var kpVector = Vector3.zero;
                    var accepted = 0;
                    foreach (var hit in results)
                    {
                        if (hit.sessionRelativeDistance < trackableManager.minDistance)
                        {
                            continue;
                        }
                        accepted += 1;
                        totalDistance += hit.sessionRelativeDistance;
                        kpVector += hit.pose.position;
                    }
                    if (accepted == 0)
                    {
                        current3DPoints[(int)keypoint.part] = null;
                        continue;
                    }

                    var avgPoint = kpVector / results.Count;
                    var filter = filters[(int)keypoint.part];
                    current3DPoints[(int)keypoint.part] = filter.Filter(avgPoint);
                }
            }
        }

        /// <summary>
        /// Places a point from a pose prediction and places it in viewport coordinates.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        Vector2 PlacePointInViewport(Vector2 point)
        {
            // Resolution is rotated... Note that this only works on right oriented cameras which is the default.
            // Also only handles cases where input image is "taller" than the screen.
            point.y = 1.0f - point.y;
            var originalResolution = trackableManager.Resolution;
            var resolution = new Vector2((float)originalResolution.y, (float)originalResolution.x);

            var scaledPoint = new Vector2(point.x * resolution.x, point.y * resolution.y);
            var xScale = (Screen.width / resolution.x);

            scaledPoint *= xScale;
            var scaledHeight = resolution.y * xScale;
            var yDelta = (scaledHeight - Screen.height) / 2;
            scaledPoint.y -= yDelta;

            var scaled = new Vector2(scaledPoint.x / Screen.width, scaledPoint.y / Screen.height);
            return scaled;
        }

        /// <summary>
		/// Update both Keypoints, Scale, and Center and location of deug spheres and bounding box (if enabled).
		/// </summary>
		/// <param name="pose"></param>
		public void UpdateWithPose(FritzPose pose)
        {
            Update3DKeypoints(pose);
            UpdateEstimatedScaleAndCenter();
            if (trackableManager.DebugSpheresEnabled)
            {
                UpdateSpheres();
            }
            if (trackableManager.DebugBoundingBoxEnabled)
            {
                UpdateBoundingBox();
            }
        }
    }
}