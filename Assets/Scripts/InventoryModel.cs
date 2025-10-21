using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryModel : MonoBehaviour
{
    public event Action<DeviceInfo> OnAdded;
    public event Action OnCompleted;

    public List<DeviceInfo> AllDevices { get; private set; } = new List<DeviceInfo>();
    private HashSet<string> collectedIds = new HashSet<string>();

    public void LoadAllDevices(List<DeviceInfo> list)
    {
        AllDevices = list ?? new List<DeviceInfo>();
        collectedIds.Clear();
    }

    public bool Contains(string id) => collectedIds.Contains(id);

    public bool Remove(DeviceInfo info)
    {
        if (info == null || string.IsNullOrEmpty(info.id)) return false;
        return collectedIds.Remove(info.id);
    }

    public void Add(DeviceInfo info)
    {
        if (info == null || Contains(info.id)) return;

        collectedIds.Add(info.id);
        OnAdded?.Invoke(info);

        if (collectedIds.Count == AllDevices.Count && AllDevices.Count > 0)
        {
            OnCompleted?.Invoke();
        }
    }

    public InventorySave ToSaveData(string inventoryName)
    {
        string localNow = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff zzz");

        var data = new InventorySave
        {
            inventoryName = inventoryName,
            timestampLocal = localNow,
            items = new List<SavedItem>()
        };

        foreach (var d in AllDevices)
        {
            if (!Contains(d.id)) continue;

            data.items.Add(new SavedItem
            {
                id = d.id,
                displayName = d.displayName,
                collectedAtLocal = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff zzz")
            });
        }

        return data;
    }

    public void LoadFromSave(InventorySave save)
    {
        if (save == null) return;

        collectedIds.Clear();

        foreach (var item in save.items)
        {
            if (!string.IsNullOrEmpty(item.id))
                collectedIds.Add(item.id);
        }

    }

}
