using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class UIController : MonoBehaviour
{
    [Header("SOP Popup")]
    public GameObject sopPanel;
    public TMPro.TMP_Text sopTitle;
    public TMPro.TMP_Text sopBody;
    public float sopSeconds = 6f;

    [Header("SFX (optional)")]
    public AudioSource sfxSource;
    public AudioClip pickClip;
    public AudioClip dropClip;
    public AudioClip useClip;

    [Header("Load Slot Buttons + Labels")]
    public TMPro.TMP_Text slot1Label;
    public TMPro.TMP_Text slot2Label;
    public TMPro.TMP_Text slot3Label;

    private bool saveModalOpen = false;
    // store previous cursor state to restore later
    private CursorLockMode _prevLockMode;
    private bool _prevCursorVisible;

    [Header("Inventory (simple text list)")]
    public GameObject inventoryPanel;     // the whole panel (to show/hide)
    public TMP_Text inventoryText;        // the big text block we just added

    [Header("Toast")]
    public GameObject toastPanel;
    public TMP_Text toastText;

    [Header("Save Modal")]
    public GameObject saveModalPanel;
    public TMP_InputField inventoryNameField;
    public Button saveButton;
    public Button cancelButton;

    [Header("Colors")]
    public string highlightHex = "D7F6DF";   // light green background for last added
    public float autoHideSeconds = 2f;

    private InventoryModel inv;
    private string lastAddedId = null;
    private List<DeviceInfo> cachedDevices = new List<DeviceInfo>();

    [Header("List Colors (hex without #)")]
    public string greyHex = "B0B0B0";         // uncollected
    public string whiteHex = "FFFFFF";        // collected
    public string lastMarkHex = "D7F6DF";     // optional green background
    public bool useGreenMarkOnLast = true;    // if you want the soft green fill

    void UnlockCursorForUI()
    {
        _prevLockMode = Cursor.lockState;
        _prevCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void RestoreCursor()
    {
        Cursor.lockState = _prevLockMode;
        Cursor.visible = _prevCursorVisible;
    }

    // call this from GameController after loading catalog
    public void BuildInventoryList(List<DeviceInfo> devices)
    {
        cachedDevices = devices;
        RenderList();
        HideInventoryPanelImmediate();
    }

    // call this from GameController.Start() after BuildInventoryList(...)
    public void BindInventory(InventoryModel model)
    {
        inv = model;
        inv.OnAdded += OnAdded;
        inv.OnCompleted += OnCompleted;
    }

    void SaveInventory()
    {
        if (inv == null)
        {
            Debug.LogWarning("InventoryModel not bound. Did you call ui.BindInventory(inventory)?");
            return;
        }

        string name = string.IsNullOrWhiteSpace(inventoryNameField?.text)
            ? "EML Inventory"
            : inventoryNameField.text;

        var data = inv.ToSaveData(name);                // builds the save object
        InventorySerializer.SaveToDisk(data, name);     // writes JSON to Documents/EMLInventory
        ShowToast($"Saved: {name}");
        CloseSaveModal();                               // hides modal + restores cursor
    }

    void OnAdded(DeviceInfo d)
    {
        if (d == null)
        {
            // Just re-render without toast or highlight
            lastAddedId = null;
            RenderList();
            return;
        }
        lastAddedId = d.id;
        RenderList();
        ShowInventoryPanelTimed(autoHideSeconds);
        ShowToast($"Added: {d.displayName}");
        PlayPickSfx();

    }

    void OnCompleted()
    {
        ShowSaveModal();
    }

    void ClearHighlight()
    {
        lastAddedId = null;
        RenderList();
    }

    public void RenderList()
    {
        var sb = new StringBuilder();

        for (int i = 0; i < cachedDevices.Count; i++)
        {
            var d = cachedDevices[i];
            bool has = inv != null && inv.Contains(d.id);
            bool isLast = !string.IsNullOrEmpty(lastAddedId) && d.id == lastAddedId;

            string check = has ? "[X]" : "[ ]";     // or "•" / "-" / "✔" (if supported)
            string lineCore = $"{i + 1}) {check} {d.displayName}";

            if (!has)
            {
                // Uncollected → grey
                sb.AppendLine($"<color=#{greyHex}>{lineCore}</color>");
            }
            else if (isLast)
            {
                // Last collected → white + optional green mark
                string inner = $"<color=#{whiteHex}>{lineCore}</color>";

                if (useGreenMarkOnLast)
                    inner = $"<mark=#{lastMarkHex}>{inner}</mark>";

                sb.AppendLine(inner);
            }

            else
            {
                // Collected (not last) → white, no glow
                sb.AppendLine($"<color=#{whiteHex}>{lineCore}</color>");
            }
        }

        inventoryText.richText = true;           // make sure Rich Text is enabled
        inventoryText.text = sb.ToString();
    }

    // ----- inventory panel show/hide -----
    public void ToggleInventoryPanel()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    public void ShowInventoryPanelTimed(float seconds)
    {
        inventoryPanel.SetActive(true);
        CancelInvoke(nameof(HideInventoryPanelImmediate));
        Invoke(nameof(HideInventoryPanelImmediate), seconds);
    }

    public void HideInventoryPanelImmediate()
    {
        inventoryPanel.SetActive(false);
    }

    // ----- toast -----
    public void ShowToast(string message)
    {
        toastText.text = message;
        toastPanel.SetActive(true);
        CancelInvoke(nameof(HideToast));
        Invoke(nameof(HideToast), autoHideSeconds);
    }

    void HideToast()
    {
        toastPanel.SetActive(false);
    }

    // ----- save modal -----
    void ShowSaveModal()
    {
        if (saveModalPanel == null || saveButton == null || cancelButton == null || inventoryNameField == null)
        {
            Debug.LogWarning("Save modal references not assigned.");
            return;
        }

        saveModalPanel.SetActive(true);
        saveModalOpen = true;

        // make mouse usable for UI
        UnlockCursorForUI();

        // listeners
        saveButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        saveButton.onClick.AddListener(SaveInventory);
        cancelButton.onClick.AddListener(CloseSaveModal);

        // prep input focus
        inventoryNameField.text = "";
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(inventoryNameField.gameObject);
        inventoryNameField.ActivateInputField();

        RefreshLoadSlotButtons();

    }

    void CloseSaveModal()
    {
        saveModalOpen = false;
        saveModalPanel.SetActive(false);

        // restore prior cursor lock/visibility (typically Locked/hidden for FPS)
        RestoreCursor();
    }

    void Update()
    {
        if (saveModalOpen && saveModalPanel != null && saveModalPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                SaveInventory();

            if (Input.GetKeyDown(KeyCode.Escape))
                CloseSaveModal();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !saveModalOpen)
        {
            ShowSaveModal();
        }
    }
    private void ApplyLoadedSave(InventorySave save)
    {
        if (save == null)
        {
            ShowToast("No saved data in slot.");
            return;
        }

        if (inv != null)
        {
            inv.LoadFromSave(save);
            ClearHighlight();
            RenderList();
            ShowToast($"Loaded: {save.inventoryName}");
        }
    }

    public void RefreshLoadSlotButtons()
    {
        var saves = SaveLoadManager.GetRecentSaves(); // newest first:contentReference[oaicite:4]{index=4}

        string[] names = { "Empty", "Empty", "Empty" };

        for (int i = 0; i < saves.Count && i < 3; i++)
        {
            try
            {
                string json = File.ReadAllText(saves[i]);
                var data = JsonUtility.FromJson<InventorySave>(json);
                names[i] = string.IsNullOrWhiteSpace(data.inventoryName) ? "EML Inventory" : data.inventoryName;
            }
            catch
            {
                names[i] = "Corrupted";
            }
        }

        if (slot1Label) slot1Label.text = names[0];
        if (slot2Label) slot2Label.text = names[1];
        if (slot3Label) slot3Label.text = names[2];
    }


    public void OnLoadButton(int slotIndex)
    {
        var save = SaveLoadManager.Load(slotIndex);
        if (save == null)
        {
            ShowToast("No saved data in slot.");
            return;
        }

        // queue the save for the next scene load and reload the scene
        SaveLoadManager.PendingLoad = save;
        ShowToast($"Loaded: {save.inventoryName}");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void OnExitButton()
    {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
    }

        public void ShowSOP(DeviceInfo d)
    {
        if (d == null || sopPanel == null) return;

        if (sopTitle) sopTitle.text = d.displayName ?? "Device";
        if (sopBody)  sopBody.text  = string.IsNullOrWhiteSpace(d.sop) ? "No SOP available." : d.sop;

        sopPanel.SetActive(true);
        CancelInvoke(nameof(HideSOP));
        Invoke(nameof(HideSOP), sopSeconds);
    }

    public void HideSOP()
    {
        if (sopPanel) sopPanel.SetActive(false);
    }

    public void PlayPickSfx() { if (sfxSource && pickClip) sfxSource.PlayOneShot(pickClip); }
    public void PlayDropSfx() { if (sfxSource && dropClip) sfxSource.PlayOneShot(dropClip); }
    public void PlayUseSfx()  { if (sfxSource && useClip)  sfxSource.PlayOneShot(useClip); }

}
