using UnityEditor;
using UnityEngine;
using System.IO;

public class MetaFileChangeListener : AssetPostprocessor
{
    public static bool IsPythonInstalled()
    {
        string pythonRegistryPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Python\PythonCore";
        using (RegistryKey pythonKey = Registry.LocalMachine.OpenSubKey(pythonRegistryPath))
        {
            if (pythonKey != null)
            {
                string[] subKeyNames = pythonKey.GetSubKeyNames();
                foreach (string subKeyName in subKeyNames)
                {
                    using (RegistryKey pythonVersionKey = pythonKey.OpenSubKey(subKeyName))
                    {
                        string pythonPath = pythonVersionKey?.GetValue("InstallPath")?.ToString();
                        if (!string.IsNullOrEmpty(pythonPath))
                        {
                            return true; // Python is installed
                        }
                    }
                }
            }
        }
        return false; // Python is not installed
    }


    // This method is called whenever an asset is imported or its .meta file changes
    // Note: Only .meta files are supported in Unity AssetPostprocessor
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {

        if(IsPythonInstalled())
        {
            // If meta package is uncompressed (unvalid guid)
            if(AssetDatabase.AssetPathToGUID("Assets/_CompressMeta.py")=="2eafba05e4b08124381e9dd1ba5e1d25")
            {
                Debug.Log("Importing new meta files");
                DoProcess("_ImportMeta.py");
            }else 
            {
                Debug.Log("Meta file changed");
                DoProcess("_CompressMeta");
            }
        }else
        {
            Debug.LogError("Python is not installed!");
        }

    }

    // Execute python script
    private static void DoProcess(string scriptName)
    {
        //Get path to the project
        string projectPath = Application.dataPath;
        //Get absolute path
        DirectoryInfo directoryInfo = new DirectoryInfo(projectPath);
        string projectFolderPath = directoryInfo.Parent.FullName;

        // Create a new process instance
        Process process = new Process();

        // Set the process start info
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "python"; // The command to execute (e.g. cmd.exe for a Windows command prompt)
        
        startInfo.Arguments = projectFolderPath+"/Assets/"+scriptName+".py"; // The arguments to pass to the command
        startInfo.RedirectStandardOutput = true; // Redirect the standard output so that we can capture it
        startInfo.UseShellExecute = false; // Don't use the shell to execute the command
        startInfo.CreateNoWindow = true; // Don't create a window for the command prompt

        process.StartInfo = startInfo;

        // Start the process
        process.Start();

        // Read the output of the command
        string output = process.StandardOutput.ReadToEnd();

        // Wait for the process to exit
        process.WaitForExit();

        // Close the process
        process.Close();

        // Print the output of the command
        Console.WriteLine("output: " + output);
    }
}
