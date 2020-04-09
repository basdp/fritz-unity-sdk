# Fritz Unity SDK

## Installation instructions

In the Unity project, click Edit -> Project Settings -> Fritz. (If you do not see Fritz, make sure the package has been added. [See the documentation](https://docs.fritz.ai/develop/get-started/Unity.html?utc_source=github&utc_campaign=fritz-unity-sdk)).

### Add Fritz API Key

Create an app in Fritz that matches your bundle identifier set in Unity.

To access your Fritz API Key, make sure you've created an account and added your app [in the webapp](https://app.fritz.ai/login?utc_source=github&utc_campaign=fritz-unity-sdk).

You can change the bundle identifier in the iOS player settings in Unity by going to `Edit -> Project Settings -> Player -> Other Settings`.

### iOS Setup

To modify your iOS project settings in Unity, go to `File > Build Settings` and open the Build Settings window.
Select `iOS` and click `Switch Platform`. After the process finishes, click on `Player Settings`.

1. In the `Project Settings` window, click `Player`.
2. Click on `Other Settings` to expand settings for the iOS player.
3. Configure the following settings:
   - Architecture: ARM64
   - Bundle Identifier: "Your Bundle ID"
   - Camera Usage Description: For AR processing
4. In Project Settings, click `Fritz`.
5. If you have not done so, [Sign up for a free account](https://app.fritz.ai/register?utc_source=github&utc_campaign=fritz-unity-sdk) then create an app on Fritz that matches the Bundle ID defined in unity.
6. Copy the API Key from the Fritz webapp (`Project Settings > "Your App" > Show API Key`) into the iOS API Key input.
7. Click download to download the necessary Fritz Frameworks. If you are using Xcode 11.4, set version to 5.3.1.
8. In the Unity `Build Settings` window, click `Build and Run`.
9. Click on Window -> Package Manager. From there, add the ARFoundation and ARKit XR Plugin.

### Android Setup

To modify your Android project settings in Unity, go to `Edit > Project Settings`, click on the Player option and under "Other Settings", click on the Android tab.

1.  In the `Project Settings` window, click `Player`.
2.  Click on `Other Settings` to expand settings for the Android player.
3.  Configure the following settings:
    - Graphics API: OpenGLES3 (only)
    - Multi-threaded Rendering: Unselected
    - Package Name: "Your Package Name"
    - Minimum API Level: Android 7.0 Nougat (API level 24)
    - Target Architecture: ARMv7
4.  In Project Settings, click `Fritz`.
5.  If you have not done so, [Sign up for a free account](https://app.fritz.ai/register?utc_source=github&utc_campaign=fritz-unity-sdk) then create an app on Fritz that matches the Bundle ID defined in unity.
6.  Copy the API Key from the Fritz webapp (`Project Settings > "Your App" > Show API Key`) into the iOS API Key input.
7.  In the Unity `Build Settings` window, click `Build and Run`.

## Stay in touch with Fritz AI

To keep tabs on what we’re up to, and for an inside look at the opportunities, challenges, and tools for mobile machine learning, subscribe to the [Fritz AI Newsletter](https://www.fritz.ai/newsletter?utm_campaign=unity-sdk&utm_source=github).

## Join the Community
[Heartbeat](https://heartbeat.fritz.ai/?utm_campaign=unity-sdk&utm_source=github) is a community of developers interested in the intersection of mobile and machine learning. Chat with us in [Slack](https://www.fritz.ai/slack?utm_campaign=unity-sdk&utm_source=github), and stay up-to-date on industry news, trends, and more by subscribing to [Deep Learning Weekly](https://www.deeplearningweekly.com/?utm_campaign=unity-sdk&utm_source=github).
