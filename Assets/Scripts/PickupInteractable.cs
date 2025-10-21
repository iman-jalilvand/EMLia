using UnityEngine;

public class PickupInteractable : MonoBehaviour
{
    public string deviceId;            // set in Inspector to match JSON id
    public string displayNameOverride; // optional

    private bool collected = false;

    private Vector3 _startPos;
    private Quaternion _startRot;

    void Awake()
    {
        _startPos = transform.position;
        _startRot = transform.rotation;
    }

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

    public void Respawn()
    {
        transform.SetPositionAndRotation(_startPos, _startRot);

        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = true;
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = true;

        collected = false;
    }
}
