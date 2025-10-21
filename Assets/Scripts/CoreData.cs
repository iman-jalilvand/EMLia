using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class DeviceInfo
{
    public string id;
    public string displayName;
    public string category;
    public string sop;
}

[Serializable]
public class DeviceCatalogData
{
    public List<DeviceInfo> devices = new List<DeviceInfo>();
}

public static class CatalogLoader
{
    public static DeviceCatalogData LoadCatalog(TextAsset jsonAsset)
    {
        if (jsonAsset == null)
        {
            Debug.LogError("Device catalog JSON missing.");
            return new DeviceCatalogData();
        }
        return JsonUtility.FromJson<DeviceCatalogData>(jsonAsset.text);
    }
}

[Serializable]
public class SavedItem
{
    public string id;
    public string displayName;
    public string collectedAtLocal;
}

[Serializable]
public class InventorySave
{
    public string inventoryName;
    public string timestampLocal;
    public List<SavedItem> items = new List<SavedItem>();
}

public static class InventorySerializer
{
    public static string GetDocumentsFolder()
    {
#if UNITY_STANDALONE_OSX
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#else
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
    }

    public static string EnsureSaveDir()
    {
        var docs = GetDocumentsFolder();
        var dir = Path.Combine(docs, "EMLInventory");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return dir;
    }

    public static string SafeFile(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c.ToString(), "_");
        return name;
    }

    public static void SaveToDisk(InventorySave data, string inventoryName)
    {
        var dir = EnsureSaveDir();
        var filename = $"{SafeFile(inventoryName)}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        var path = Path.Combine(dir, filename);
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"Saved inventory to {path}");
    }
}
