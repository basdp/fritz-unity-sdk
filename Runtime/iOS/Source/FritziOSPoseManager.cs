using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

/// <summary>
/// FritziOSPoseManager access the native library for iOS.
/// </summary>
public class FritziOSPoseManager
{
    #region Declare external C interface

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string _configure();

    [DllImport("__Internal")]
    private static extern void _setMinPartThreshold(double threshold);

    [DllImport("__Internal")]
    private static extern void _setMinPoseThreshold(double threshold);

    [DllImport("__Internal")]
    private static extern void _setNumPoses(int poses);

    [DllImport("__Internal")]
    private static extern string _processPose(IntPtr buffer);

    [DllImport("__Internal")]
    private static extern void _processPoseAsync(IntPtr buffer);


    [DllImport("__Internal")]
    private static extern bool _processing();

    [DllImport("__Internal")]
    private static extern void _setCallbackFunctionTarget(string name);
    
    [DllImport("__Internal")]
    private static extern void _setCallbackTarget(string name);
#endif

    #endregion

    #region Wrapped methods and properties

    /// <summary>
    /// Configure the Fritz project. Should be called first.
    /// </summary>
    public static void Configure()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _configure();
#endif
	}

    /// <summary>
    /// Checks to see if a frame is being processed.
    /// </summary>
    /// <returns>true/false if the model is currently processing</returns>
    public static bool Processing()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return _processing();
#endif
		return false;
    }

    /// <summary>
    /// Set the max number of poses to detect.
    /// </summary>
    /// <param name="numPoses">The number of poses</param>
    public static void SetNumPoses(int numPoses)
    {
#if UNITY_IOS && !UNITY_EDITOR
        _setNumPoses(numPoses);
#endif
    }

    /// <summary>
    /// Set the callback target as the Unity object.
    /// </summary>
    /// <param name="name">The name of the Unity object</param>
    public static void SetCallbackTarget(string name)
    {
#if UNITY_IOS && !UNITY_EDITOR
        _setCallbackTarget(name);
#endif
    }

    /// <summary>
    /// Set the script function to callback once prediction is run on each frame.
    /// </summary>
    /// <param name="name">The name of the callback function on the object target.</param>
    public static void SetCallbackFunctionTarget(string name)
    {
#if UNITY_IOS && !UNITY_EDITOR
        _setCallbackFunctionTarget(name);
#endif
    }

    /// <summary>
    /// Run pose estimation on an camera buffer reference (synchronized)
    /// </summary>
    /// <param name="buffer">A native pointer to the camera buffer</param>
    public static string ProcessPose(IntPtr buffer)
    {

#if UNITY_IOS && !UNITY_EDITOR
        return _processPose(buffer);
#endif
        return null;
    }

    /// <summary>
    /// Run pose estimation on an camera buffer reference (async)
    /// </summary>
    /// <param name="buffer">A native pointer to the camera buffer</param>
    public static void ProcessPoseAsync(IntPtr buffer)
    {
#if UNITY_IOS && !UNITY_EDITOR
        _processPoseAsync(buffer);
#endif
    }
#endregion
}