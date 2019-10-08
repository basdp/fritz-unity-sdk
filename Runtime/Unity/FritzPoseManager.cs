using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// FritzPoseManager can be used in Unity scripts to run pose prediction
/// </summary>
public class FritzPoseManager
{

    /// <summary>
    /// Configure the Fritz project. Should be called first.
    /// </summary>
    public static void Configure()
    {
#if UNITY_IOS
        FritziOSPoseManager.Configure();
#elif UNITY_ANDROID
        FritzAndroidPoseManager.Configure();
#endif
    }

    /// <summary>
    /// Checks to see if a frame is being processed.
    /// </summary>
    /// <returns>true/false if the model is currently processing</returns>
    public static bool Processing()
    {
#if UNITY_IOS
        return FritziOSPoseManager.Processing();
#elif UNITY_ANDROID
        return FritzAndroidPoseManager.Processing();
#endif
        return false;
    }

    /// <summary>
    /// Set the max number of poses to detect.
    /// </summary>
    /// <param name="numPoses">The number of poses</param>
    public static void SetNumPoses(int numPoses)
    {
#if UNITY_IOS
        FritziOSPoseManager.SetNumPoses(numPoses);
#elif UNITY_ANDROID
        FritzAndroidPoseManager.SetNumPoses(numPoses);
#endif
    }

    /// <summary>
    /// Set the callback target as the Unity object.
    /// </summary>
    /// <param name="name">The name of the Unity object</param>
    public static void SetCallbackTarget(string name)
    {
#if UNITY_IOS
        FritziOSPoseManager.SetCallbackTarget(name);
#elif UNITY_ANDROID
        FritzAndroidPoseManager.SetCallbackTarget(name);
#endif
    }

    /// <summary>
    /// Set the script function to callback once prediction is run on each frame.
    /// </summary>
    /// <param name="name">The name of the callback function on the object target.</param>
    public static void SetCallbackFunctionTarget(string name)
    {
#if UNITY_IOS
        FritziOSPoseManager.SetCallbackFunctionTarget(name);
#elif UNITY_ANDROID
        FritzAndroidPoseManager.SetCallbackFunctionTarget(name);
#endif
    }

#if UNITY_IOS
    /// <summary>
    /// (iOS only) Run pose estimation on an XRCamera Frame (synchronized)
    /// </summary>
    /// <param name="frame">The camera frame to run prediction on</param>
    public static List<FritzPose> ProcessPoseFromFrame(XRCameraFrame frame)
    {
        IntPtr buffer = frame.nativePtr;

        if (buffer == IntPtr.Zero)
        {
            Debug.LogError("buffer is NULL!");
            return null;
        }


        string message = FritziOSPoseManager.ProcessPose(buffer);
        return ProcessEncodedPoses(message);
    }

    /// <summary>
    /// (iOS only) Run pose estimation on an XRCamera Frame (async)
    /// </summary>
    /// <param name="frame">The camera frame to run prediction on</param>
    public static void ProcessPoseFromFrameAsync(XRCameraFrame frame)
    {
        IntPtr buffer = frame.nativePtr;
        if (buffer == IntPtr.Zero)
        {
            Debug.LogError("buffer is NULL!");
            return;
        }


        FritziOSPoseManager.ProcessPoseAsync(buffer);
    }
#endif

#if UNITY_ANDROID
    /// <summary>
    /// (Android only) Run pose estimation on an XRCameraImage (synchronized)
    /// </summary>
    /// <param name="cameraImage">The camera image to run prediction on</param>
    public static List<FritzPose> ProcessPoseFromImage(XRCameraImage cameraImage)
    {
        string message = FritzAndroidPoseManager.ProcessPoseFromImage(cameraImage);
        return ProcessEncodedPoses(message);
    }

    /// <summary>
    /// (Android only) Run pose estimation on an XRCameraImage (async)
    /// </summary>
    /// <param name="cameraImage">The camera image to run prediction on</param>
    public static void ProcessPoseFromImageAsync(XRCameraImage cameraImage)
    {
        FritzAndroidPoseManager.ProcessPoseFromImageAsync(cameraImage);
    }

    /// <summary>
    /// (Android only) Log a message using Log.d
    /// </summary>
    /// <param name="message">The log message to send</param>
    public static void LogMessage(string message)
    {
        FritzAndroidPoseManager.LogMessage(message);
    }

    /// <summary>
    /// (Android only) Use GPU to run model inference. Note this is an experimental option and should NOT be used in production.
    /// </summary>
    public static void UseGPU()
    {
        FritzAndroidPoseManager.UseGPU();
    }
#endif

    /// <summary>
    /// Process the prediction result from the pose model.
    /// </summary>
    /// <param name="message">The message returned back from the native libraries</param>
    /// <returns>a list of poses detected</returns>
    public static List<FritzPose> ProcessEncodedPoses(string message)
    {
        List<List<List<float>>> poses = JsonConvert.DeserializeObject<List<List<List<float>>>>(message);

        if (poses == null)
        {
            return new List<FritzPose>();
        }
        List<FritzPose> decoded = new List<FritzPose>();
        foreach (List<List<float>> pose in poses)
        {
            decoded.Add(new FritzPose(pose));
        }
        return decoded;
    }
}