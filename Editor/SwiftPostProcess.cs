using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public static class SwiftPostProcess
{

    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
    {
        #if UNITY_IOS
            var projPath = buildPath + "/Unity-Iphone.xcodeproj/project.pbxproj";
            var proj = new PBXProject();
            proj.ReadFromFile(projPath);

            var targetGuid = proj.TargetGuidByName(PBXProject.GetUnityTargetName());

            proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
            proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_BRIDGING_HEADER", "Libraries/ai.fritz.vision/Runtime/iOS/Source/FritzVisionUnity-Bridging-Header.h");
            proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_INTERFACE_HEADER_NAME", "FritzVisionUnity-Swift.h");
            proj.AddBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks $(PROJECT_DIR)/lib/$(CONFIGURATION) $(inherited)");
            proj.AddBuildProperty(targetGuid, "FRAMERWORK_SEARCH_PATHS", "$(inherited) $(PROJECT_DIR) $(PROJECT_DIR)/Frameworks");
            proj.AddBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            proj.AddBuildProperty(targetGuid, "DYLIB_INSTALL_NAME_BASE", "@rpath");
            proj.AddBuildProperty(targetGuid, "LD_DYLIB_INSTALL_NAME",
                "@executable_path/../Frameworks/$(EXECUTABLE_PATH)");
            proj.AddBuildProperty(targetGuid, "DEFINES_MODULE", "YES");
            proj.AddBuildProperty(targetGuid, "SWIFT_VERSION", "5.0");
            proj.AddBuildProperty(targetGuid, "COREML_CODEGEN_LANGUAGE", "Swift");
            proj.AddShellScriptBuildPhase(targetGuid, "Remove x86/x64 binaries from frameworks", "/bin/sh", @"
                echo ""Target architectures: $ARCHS""

                APP_PATH=""${TARGET_BUILD_DIR}/${WRAPPER_NAME}""

                find ""$APP_PATH"" -name '*.framework' -type d | while read -r FRAMEWORK
                do
                FRAMEWORK_EXECUTABLE_NAME=$(defaults read ""$FRAMEWORK/Info.plist"" CFBundleExecutable)
                FRAMEWORK_EXECUTABLE_PATH=""$FRAMEWORK/$FRAMEWORK_EXECUTABLE_NAME""
                echo ""Executable is $FRAMEWORK_EXECUTABLE_PATH""
                echo $(lipo -info ""$FRAMEWORK_EXECUTABLE_PATH"")

                FRAMEWORK_TMP_PATH=""$FRAMEWORK_EXECUTABLE_PATH-tmp""

                : remove simulator's archs if location is not simulator's directory
                case ""${TARGET_BUILD_DIR}"" in
                *""iphonesimulator"")
                    echo ""No need to remove archs""
                    ;;
                *)
                    if $(lipo ""$FRAMEWORK_EXECUTABLE_PATH"" -verify_arch ""i386"") ; then
                    lipo -output ""$FRAMEWORK_TMP_PATH"" -remove ""i386"" ""$FRAMEWORK_EXECUTABLE_PATH""
                    echo ""i386 architecture removed""
                    rm ""$FRAMEWORK_EXECUTABLE_PATH""
                    mv ""$FRAMEWORK_TMP_PATH"" ""$FRAMEWORK_EXECUTABLE_PATH""
                    fi
                    if $(lipo ""$FRAMEWORK_EXECUTABLE_PATH"" -verify_arch ""x86_64"") ; then
                    lipo -output ""$FRAMEWORK_TMP_PATH"" -remove ""x86_64"" ""$FRAMEWORK_EXECUTABLE_PATH""
                    echo ""x86_64 architecture removed""
                    rm ""$FRAMEWORK_EXECUTABLE_PATH""
                    mv ""$FRAMEWORK_TMP_PATH"" ""$FRAMEWORK_EXECUTABLE_PATH""
                    fi
                    ;;
                esac

                echo ""Completed for executable $FRAMEWORK_EXECUTABLE_PATH""
                echo $(lipo -info ""$FRAMEWORK_EXECUTABLE_PATH"")

                done
            ");
        
            proj.WriteToFile(projPath);
        #endif
    }

}
