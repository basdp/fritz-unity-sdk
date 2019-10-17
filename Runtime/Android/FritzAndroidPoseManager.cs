using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

namespace AI.Fritz.Vision
{
	/// <summary>
	/// FritzAndroidPoseManager accesses the native library for Android.
	/// </summary>
	public class FritzAndroidPoseManager : MonoBehaviour
	{
		AndroidJavaClass ajc;

		static AndroidJavaObject poseManager;
		static bool processing = false;
		static bool isGPUPredictorCreated = false;

		void Start()
		{

		}

		/// <summary>
		/// Configure the Fritz project. Should be called first.
		/// </summary>
		public static void Configure()
		{
			AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject currentActivityObject = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaClass fritzCore = new AndroidJavaClass("ai.fritz.core.Fritz");

			fritzCore.CallStatic("configure", currentActivityObject);
			fritzCore.CallStatic("enableModelUpdates", false);

			poseManager = new AndroidJavaObject("ai.fritz.visionunity.FritzPoseUnityManager");
			poseManager.Call("buildCPUPredictor");
		}

		/// <summary>
		/// Checks to see if a frame is being processed.
		/// </summary>
		/// <returns>true/false if the model is currently processing</returns>
		public static bool Processing()
		{
			return poseManager.Call<bool>("isProcessing");
		}

		/// <summary>
		/// Use GPU to run model inference. Note this is an experimental option and should NOT be used in production.
		/// </summary>
		public static void UseGPU()
		{
			if (isGPUPredictorCreated)
				return;
			poseManager.Call("buildGPUPredictor");
			isGPUPredictorCreated = true;
		}

		/// <summary>
		/// Set the max number of poses to detect.
		/// </summary>
		/// <param name="numPoses">The number of poses</param>
		public static void SetNumPoses(int numPoses)
		{
			poseManager.Call("setNumPoses", numPoses);
		}

		/// <summary>
		/// Set the callback target as the Unity object.
		/// </summary>
		/// <param name="name">The name of the Unity object</param>
		public static void SetCallbackTarget(string name)
		{
			poseManager.Call("setCallbackTarget", name);
		}

		/// <summary>
		/// Set the script function to callback once prediction is run on each frame.
		/// </summary>
		/// <param name="name">The name of the callback function on the object target.</param>
		public static void SetCallbackFunctionTarget(string name)
		{
			poseManager.Call("setCallbackFunctionTarget", name);
		}

		/// <summary>
		/// Run pose estimation on an XRCameraImage (synchronized)
		/// </summary>
		/// <param name="image">The camera image to run prediction on</param>
		public static string ProcessPoseFromImage(XRCameraImage image)
		{
			object[] methodParams = extractYUVFromImage(image);
			string message = poseManager.Call<string>("processPose", methodParams);
			return message;
		}

		/// <summary>
		/// Log a message using Log.d
		/// </summary>
		/// <param name="message">The log message to send</param>
		public static void LogMessage(string message)
		{
			poseManager.Call("logMessage", message);
		}

		/// <summary>
		/// Run pose estimation on an XRCameraImage (async)
		/// </summary>
		/// <param name="image">The camera image to run prediction on</param>
		public static void ProcessPoseFromImageAsync(XRCameraImage image)
		{
			object[] methodParams = extractYUVFromImage(image);
			poseManager.Call("processPoseAsync", methodParams);
		}

		private static object[] extractYUVFromImage(XRCameraImage image)
		{
			// Consider each image plane
			XRCameraImagePlane plane = image.GetPlane(0);
			var yRowStride = plane.rowStride;
			var y = plane.data;

			XRCameraImagePlane plane2 = image.GetPlane(1);
			var uvRowStride = plane2.rowStride;
			var uvPixelStride = plane2.pixelStride;
			var u = plane2.data;

			XRCameraImagePlane plane3 = image.GetPlane(2);
			var v = plane3.data;

			byte[] yDst = new byte[y.Length];
			byte[] uDst = new byte[u.Length];
			byte[] vDst = new byte[v.Length];

			object[] objParams = new object[8];
			NativeArray<byte>.Copy(y, yDst);
			NativeArray<byte>.Copy(u, uDst);
			NativeArray<byte>.Copy(v, vDst);

			objParams[0] = yDst;
			objParams[1] = uDst;
			objParams[2] = vDst;
			objParams[3] = yRowStride;
			objParams[4] = uvRowStride;
			objParams[5] = uvPixelStride;
			objParams[6] = image.width;
			objParams[7] = image.height;

			return objParams;
		}
	}

}
