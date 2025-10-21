using UnityEngine;

public class ProximityHighlighter : MonoBehaviour {
    public string deviceId;
    public float triggerDistance = 2.0f;
    public Renderer targetRenderer;     // assign a renderer to pulse
    public Color baseColor = Color.white;
    public float emissionStrength = 1.5f;

    private Transform player;
    private InventoryModel inv;
    private Material matInstance;
    private bool active;

    void Start() {
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        inv = FindObjectOfType<InventoryModel>();
        if (targetRenderer != null) {
            matInstance = targetRenderer.material;
            matInstance.EnableKeyword("_EMISSION");
        }
    }

    void Update() {
        if (player == null || inv == null || matInstance == null) return;
        if (inv.Contains(deviceId)) {
            if (active) {
                matInstance.SetColor("_EmissionColor", Color.black);
                active = false;
            }
            return;
        }

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist <= triggerDistance) {
            float t = 0.5f + 0.5f * Mathf.Sin(Time.time * 2f);
            var color = baseColor * (t * emissionStrength);
            matInstance.SetColor("_EmissionColor", color);
            active = true;
        } else if (active) {
            matInstance.SetColor("_EmissionColor", Color.black);
            active = false;
        }
    }
}
