using UnityEditor;
using System.Net;
using System.IO;
using System;
using System.Diagnostics;

// Downloads framework for a specific SDK version.
public class DownloadFramework
{

    private readonly string FRAMEWORK_FMT =
        "https://github.com/fritzlabs/swift-framework/archive/{0}.zip";
    public string version;


    public DownloadFramework(string version)
    {
        this.version = version;
    }

    public void Download()
    {
        using (var client = new WebClient())
        {
            var tempFile = Path.GetTempFileName();
            var tempDir = Path.GetTempPath();

            var path = String.Format(FRAMEWORK_FMT, version);

            client.DownloadFile(new Uri(path), tempFile);

            ExecuteBashCommand(String.Format("unzip -q -o {0} -d {1}", tempFile, tempDir));
            ExecuteBashCommand(String.Format("mkdir -p Assets/Plugins/iOS/Frameworks/"));
            var result = ExecuteBashCommand(
                String.Format("rsync -a {0}/swift-framework-{1}/Frameworks ./Assets/Plugins/iOS/Frameworks --exclude FritzVisionHumanPoseModelSmall.framework --exclude FritzVisionDepthModel.framework", tempDir, version)
            );
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            PluginImporter[] importers = PluginImporter.GetImporters(BuildTarget.iOS);
            foreach (var importer in importers)
            {
                if (importer.assetPath.StartsWith("Assets/Plugins/iOS/Frameworks/Fritz") && importer.assetPath.EndsWith(".framework"))
                {
                    importer.SetPlatformData(BuildTarget.iOS, "AddToEmbeddedBinaries", "true");
                }
            }
        }
    }

    static string ExecuteBashCommand(string command)
    {
        // According to: https://stackoverflow.com/a/15262019/637142
        // this will properly escape double quotes
        command = command.Replace("\"", "\"\"");

        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"" + command + "\"",
                RedirectStandardOutput = true,
                RedirectStandardInput = false,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        proc.Start();
        proc.WaitForExit();

        return proc.StandardOutput.ReadToEnd();
    }
}
