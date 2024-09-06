using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using System;
using System.Linq;

public class MetaCompressionWindow : EditorWindow
{
    public static readonly string assetsDir = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
    public static readonly string cacheDir = Path.Combine(Directory.GetCurrentDirectory(), "cache");
    public static readonly string zipFile = Path.Combine(assetsDir, "MetaFilesCompressed.zip");
    public static bool DebugLogMode = false;
    public static string LabelMetaAsset = "MetaCompressed";

    private static readonly string CheckboxPrefKey = "MetaCompressionWindow_FeatureEnabled";
    private static bool featureEnabled = false; 
    public static bool IsFeatureEnabled => featureEnabled;

    private static bool CompressionStarted = false; 
    public static bool IsCompressionStarted => CompressionStarted;

    private void OnEnable()
    {
        featureEnabled = EditorPrefs.GetBool(CheckboxPrefKey, false);
    }

    [MenuItem("Window/MetaCompress")]
    public static void ShowWindow()
    {
        var window = GetWindow<MetaCompressionWindow>("MetaCompress");
        var icon = Resources.Load<Texture>("PluginIcon");
        window.titleContent = new GUIContent("MetaCompress", icon);
    }

    // Draw the window
    private void OnGUI()
    {
        GUILayout.Label("Compress all \".meta\" files into one \".zip\" file.", EditorStyles.boldLabel);

        if (GUILayout.Button("Compress Now"))
        {
            CompressAll(true);
        }

        if (GUILayout.Button("Unzip Now"))
        {
            UnzipAll(true);
        }

        if (GUILayout.Button("Delete all meta files"))
        {
            DeleteAll(true);
        }
        if (GUILayout.Button("Reimport Assets"))
        {
            ReimportAssets(true);
        }

        featureEnabled = EditorGUILayout.Toggle("Automatic", featureEnabled);
        EditorPrefs.SetBool(CheckboxPrefKey, featureEnabled);

        if (featureEnabled)
        {
            EditorGUILayout.HelpBox("Automatic compression may slow down the import process", MessageType.Info);
        }
    }

    public static void CompressAll(bool debugMode)
    {
        if (CompressionStarted)
        {
            if (debugMode)
                Debug.Log("Meta Compression Is Already Running");
            return;
        }

        CompressionStarted = true;

        try
        {
            AddLabelToAsset("Assets/MetaFilesCompressed.zip",LabelMetaAsset);

            if (Directory.Exists(cacheDir))
                Directory.Delete(cacheDir, true);
            Directory.CreateDirectory(cacheDir);

            bool hasFilesToCompress = false;

            if (debugMode)
                Debug.Log("Meta Compression Starts");

            foreach (string file in Directory.GetFiles(assetsDir, "*.meta", SearchOption.AllDirectories))
            {
                if (!file.StartsWith(cacheDir))
                {
                    string copyName = Path.Combine(cacheDir, Path.GetRelativePath(assetsDir, file));
                    Directory.CreateDirectory(Path.GetDirectoryName(copyName));
                    File.Copy(file, copyName, true);
                    hasFilesToCompress = true;
                }
            }

            if (hasFilesToCompress)
            {
                if (File.Exists(zipFile))
                    File.Delete(zipFile);

                ZipFile.CreateFromDirectory(cacheDir, zipFile);
                Directory.Delete(cacheDir, true);

                Debug.Log("SUCCESS: Meta files compressed into 'MetaFilesCompressed.zip'.");
            }
            else
            {
                if (debugMode)
                    Debug.Log("No meta files found to compress. Ignoring Request.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error occurred: " + ex.Message);
        }
        finally
        {
            CompressionStarted = false;
        }
    }


    public static void UnzipAll(bool debugMode)
    {
        if(CompressionStarted == true)
        {
            if(debugMode)
                Debug.Log("Meta Compression Is Already Running");
            return;
        }
        try
        {
            if (File.Exists(zipFile))
            {
                if(debugMode)
                    Debug.Log("Meta Extract Start");

                using (var archive = ZipFile.OpenRead(zipFile))
                {
                    foreach (var entry in archive.Entries)
                    {
                        string destinationPath = Path.Combine(assetsDir, entry.FullName);

                        string directoryPath = Path.GetDirectoryName(destinationPath);
                        if (!Directory.Exists(directoryPath))
                            Directory.CreateDirectory(directoryPath);
                        
                        if (!string.IsNullOrEmpty(entry.Name))
                        {
                            using (var entryStream = entry.Open())
                            using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                            {
                                entryStream.CopyTo(fileStream);
                            }
                        }
                    }
                }
                Debug.Log("SUCCESS: Meta files extracted.");
            }
            else
            {
                Debug.LogError("MetaFilesCompressed.zip not found.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error occurred while extracting: " + ex.Message);
        }
    }


    private void DeleteAll(bool debugMode)
    {
        if(CompressionStarted == true)
        {
            if(debugMode)
                Debug.Log("Meta Compression Is Already Running");
            return;
        }
        try
        {
            if(debugMode)
                Debug.Log("Meta Delete Start");
            foreach (string file in Directory.GetFiles(assetsDir, "*.meta", SearchOption.AllDirectories))
                File.Delete(file);
            
            Debug.Log("SUCCESS: All .meta files deleted.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error occurred while deleting .meta files: " + ex.Message);
        }
    }

    public static void ReimportAssets(bool debugMode)
    {
        try
        {
            if(debugMode)
                Debug.Log("Reimporting assets...");
            AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
            if(debugMode)
                Debug.Log("SUCCESS: Assets reimported.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error occurred while reimporting assets: " + ex.Message);
        }
    }


    public static void AddLabelToAsset(string path, string label)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(label))
        {
            Debug.LogError("Path or label cannot be empty.");
            return;
        }

        var assetImporter = AssetImporter.GetAtPath(path);
        if (assetImporter == null)
        {
            Debug.LogError("No asset found at the given path.");
            return;
        }

        var labels = assetImporter.assetBundleName;
        var existingLabels = AssetDatabase.GetLabels(assetImporter);
        if (!existingLabels.Contains(label))
        {
            AssetDatabase.SetLabels(assetImporter, existingLabels.Concat(new[] { label }).ToArray());
            Debug.Log($"Label '{label}' added to asset at path '{path}'.");
        }
        else
        {
            Debug.Log($"Asset at path '{path}' already has the label '{label}'.");
        }
    }

    public static bool AssetHasLabel(string assetPath, string label)
    {
        // Get the AssetImporter for the asset at the given path
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);

        // Check if the assetImporter is valid
        if (assetImporter == null)
        {
            Debug.LogError("No asset found at the given path: " + assetPath);
            return false;
        }

        // Get all the labels associated with the asset
        string[] labels = AssetDatabase.GetLabels(assetImporter);

        // Check if the label is present in the array of labels
        bool hasLabel = System.Array.Exists(labels, l => l == label);

        return hasLabel;
    }
}
