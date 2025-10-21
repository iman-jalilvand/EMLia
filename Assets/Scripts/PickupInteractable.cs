using UnityEngine;

public class PickupInteractable : MonoBehaviour
{
    public string deviceId;            // set in Inspector to match JSON id
    public string displayNameOverride; // optional

    private bool collected = false;

    public void TryPickup(InventoryModel inv, UIController ui)
    {
        if (collected) return;

        var info = inv.AllDevices.Find(d => d.id == deviceId);
        if (info == null)
        {
            Debug.LogWarning($"Device id {deviceId} not found in catalog.");
            return;
        }

        inv.Add(info);  // fires OnAdded → UI updates list + glow + toast
        collected = true;

        // hide visuals + collider
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
    }
}
