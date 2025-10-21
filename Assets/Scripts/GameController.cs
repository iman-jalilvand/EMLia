using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Use/Drop")]
    public KeyCode useKey  = KeyCode.F;
    public KeyCode dropKey = KeyCode.Q;

    // runtime state
    private DeviceInfo heldDevice;                // current selected/held device (logical)
    private PickupInteractable heldPickupSource;  // optional: the source pedestal we picked from


    [Header("References")]
    public Camera playerCamera;
    public CharacterController controller;
    public InventoryModel inventory;
    public UIController ui;
    public TextAsset deviceCatalogJson;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float lookSensitivity = 2f;
    private float yaw;
    private float pitch;

    [Header("Interact")]
    public float interactDistance = 2.5f;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode inventoryKey = KeyCode.Tab;

    void Start()
    {
        var catalog = CatalogLoader.LoadCatalog(deviceCatalogJson);
        inventory.LoadAllDevices(catalog.devices);
        ui.BuildInventoryList(catalog.devices); // draws the whole list greyed out
        ui.BindInventory(inventory);            // subscribes to OnAdded/OnCompleted
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // apply pending load (if any), then sync scene pickups
        if (SaveLoadManager.PendingLoad != null)
        {
            inventory.LoadFromSave(SaveLoadManager.PendingLoad);   // restore collectedIds
            SaveLoadManager.PendingLoad = null;                    // clear for safety

            // Hide already collected pickups in the scene
            var pickups = FindObjectsOfType<PickupInteractable>();
            foreach (var p in pickups)
            {
                if (inventory.Contains(p.deviceId))                // uses your model's Contains(...):contentReference[oaicite:2]{index=2}
                {
                    foreach (var r in p.GetComponentsInChildren<Renderer>()) r.enabled = false;
                    foreach (var c in p.GetComponentsInChildren<Collider>()) c.enabled = false;
                }
            }

            // refresh the inventory list UI (so it matches the restored state)
            // If you added a public refresh method, call it; otherwise re-build:
            ui.BuildInventoryList(inventory.AllDevices);           // rebuilds the list from current state:contentReference[oaicite:3]{index=3}
        }
    }

    void Update() 
    {
        HandleLook();
        HandleMove();
        HandleInteract();
        HandleInventoryToggle();
        HandleUseDrop();
    }

    void HandleLook() 
    {
        var dx = Input.GetAxis("Mouse X") * lookSensitivity;
        var dy = Input.GetAxis("Mouse Y") * lookSensitivity;
        yaw += dx;
        pitch -= dy;
        pitch = Mathf.Clamp(pitch, -80f, 80f);
        transform.rotation = Quaternion.Euler(0, yaw, 0);
        playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    void HandleMove() 
    {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        var move = transform.right * h + transform.forward * v;
        controller.SimpleMove(move * moveSpeed);
    }

void HandleInteract() 
{
    if (!Input.GetKeyDown(interactKey)) return;

    Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
    int mask = 1 << LayerMask.NameToLayer("Interactable");  // only hit Interactable layer

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, mask))
        {
            var pick = hit.collider.GetComponentInParent<PickupInteractable>();
            if (pick != null) pick.TryPickup(inventory, ui);
            if (pick != null)
            {
                // If pickup succeeded, InventoryModel now contains this id
                if (inventory.Contains(pick.deviceId))
                {
                    heldDevice = inventory.AllDevices.Find(d => d.id == pick.deviceId);
                    heldPickupSource = pick; // kept if you later want a physical re-spawn on true drop
                }
            }
        }
}

void HandleUseDrop()
{
    // Use with F: show SOP for held device (4s handled by UI)
    if (Input.GetKeyDown(useKey))
    {
        if (heldDevice != null)
        {
            ui.ShowSOP(heldDevice);
            ui.PlayUseSfx();
        }
        else
        {
            ui.ShowToast("No device in hand.");
        }
    }

    // Drop with Q: logical drop (deselect); does NOT un-collect or respawn
    if (Input.GetKeyDown(dropKey))
    {
        if (heldDevice != null)
        {
            // 1) Un-collect so it turns grey again in UI
            bool removed = inventory.Remove(heldDevice);

            // 2) Respawn the world object at its original pedestal
            if (heldPickupSource != null)
                heldPickupSource.Respawn();

            // 3) UI feedback + refresh
            ui.ShowToast($"Returned: {heldDevice.displayName}");
            ui.PlayDropSfx();
            ui.HideSOP();
            ui.RenderList(); // if you made RenderList() public

            // 4) Clear held state
            heldDevice = null;
            heldPickupSource = null;
        }
        else
        {
            ui.ShowToast("Nothing to drop.");
        }
    }

}

    void HandleInventoryToggle() 
    {
        if (Input.GetKeyDown(inventoryKey)) {
            ui.ToggleInventoryPanel();
        }
    }
}
