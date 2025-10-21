using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveLoadManager
{
    public static InventorySave PendingLoad;  // survives scene reload

    private const int MaxSlots = 3;

    private static string SaveDir => InventorySerializer.EnsureSaveDir();

    public static List<string> GetRecentSaves()
    {
        var saves = new List<string>(Directory.GetFiles(SaveDir, "*.json"));
        saves.Sort((a, b) => File.GetCreationTimeUtc(b).CompareTo(File.GetCreationTimeUtc(a))); // newest first
        return saves.Count > MaxSlots ? saves.GetRange(0, MaxSlots) : saves;
    }

    public static void Save(InventorySave data, string inventoryName)
    {
        InventorySerializer.SaveToDisk(data, inventoryName);

        // clean up older saves if more than 3 exist
        var allSaves = GetRecentSaves();
        if (allSaves.Count > MaxSlots)
        {
            for (int i = MaxSlots; i < allSaves.Count; i++)
                File.Delete(allSaves[i]);
        }
    }

    public static InventorySave Load(int slotIndex)
    {
        var saves = GetRecentSaves();
        if (saves.Count == 0)
        {
            Debug.Log("No saves found — returning null to reset scene.");
            return null;
        }

        if (slotIndex < 0 || slotIndex >= saves.Count)
        {
            Debug.LogWarning($"Invalid slot index {slotIndex}.");
            return null;
        }

        var json = File.ReadAllText(saves[slotIndex]);
        return JsonUtility.FromJson<InventorySave>(json);
    }
}
