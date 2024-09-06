using UnityEngine;
using UnityEditor;
using System.IO;
using System;

[InitializeOnLoad]
public static class AssetModificationHandler
{
    private static FileSystemWatcher fileSystemWatcher;
    private static Action mainThreadAction;

    static AssetModificationHandler()
    {
        SetupFileSystemWatcher();
        EditorApplication.quitting += OnEditorQuitting;
        EditorApplication.update += FirstTimeUnzip;
        EditorApplication.update += ProcessMainThreadActions;
    }

    private static void FirstTimeUnzip()
    {
        if (!MetaCompressionWindow.AssetHasLabel("Assets/MetaFilesCompressed.zip", MetaCompressionWindow.LabelMetaAsset))
        {
            Debug.Log("First time initialized MetaCompress. Trying to unzip the \"MetaFilesCompressed.zip\".");
            MetaCompressionWindow.UnzipAll(MetaCompressionWindow.DebugLogMode);
            MetaCompressionWindow.AddLabelToAsset("Assets/MetaFilesCompressed.zip", MetaCompressionWindow.LabelMetaAsset);
            MetaCompressionWindow.ReimportAssets(MetaCompressionWindow.DebugLogMode);
        }
    }

    private static void SetupFileSystemWatcher()
    {
        fileSystemWatcher = new FileSystemWatcher
        {
            Path = MetaCompressionWindow.assetsDir,
            Filter = "*.*",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName | NotifyFilters.CreationTime,
            IncludeSubdirectories = true
        };

        fileSystemWatcher.Changed += OnFileSystemChanged;
        fileSystemWatcher.Created += OnFileSystemChanged;
        fileSystemWatcher.Deleted += OnFileSystemChanged;
        fileSystemWatcher.Renamed += OnFileSystemChanged;

        fileSystemWatcher.EnableRaisingEvents = true;
    }

    private static void OnFileSystemChanged(object sender, FileSystemEventArgs e)
    {
        if (MetaCompressionWindow.IsFeatureEnabled && !IsInCacheDirectory(e.FullPath) && !IsZipFile(e.FullPath) && !MetaCompressionWindow.IsCompressionStarted)
        {
            // Schedule the compression to be run on the main thread.
            mainThreadAction = () => MetaCompressionWindow.CompressAll(MetaCompressionWindow.DebugLogMode);
        }
    }

    private static bool IsInCacheDirectory(string filePath)
    {
        return filePath.StartsWith(MetaCompressionWindow.cacheDir, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsZipFile(string filePath)
    {
        return string.Equals(filePath, MetaCompressionWindow.zipFile, StringComparison.OrdinalIgnoreCase);
    }

    private static void OnEditorQuitting()
    {
        fileSystemWatcher?.Dispose();
    }

    // This will be called on each update in the main thread to process queued actions
    private static void ProcessMainThreadActions()
    {
        if (mainThreadAction != null)
        {
            mainThreadAction.Invoke();
            mainThreadAction = null; // Clear the action after it's executed
        }
    }
}
