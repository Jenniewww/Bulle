using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Display brief save success/failure feedback in a Quest-compatible world-space Canvas.
/// Attach to Acquisition (same scene as CoordinateManager); connect manager and XR Main Camera.
/// The existing ControllerAcquisition.cs trigger code does NOT need to change.
/// </summary>
public class AcquisitionFeedbackUI : MonoBehaviour
{
    [SerializeField] private CoordinateManager coordinateManager;
    [SerializeField] private Transform xrHead;
    [SerializeField] private float durationSeconds = 2.5f;
    [SerializeField] private Vector3 offsetFromHead = new Vector3(0f, -0.18f, 1.05f);

    private GameObject panel;
    private TextMeshProUGUI message;
    private Image background;
    private Coroutine hideRoutine;

    private void Start()
    {
        if (coordinateManager == null)
            coordinateManager = FindFirstObjectByType<CoordinateManager>();
        if (xrHead == null && Camera.main != null)
            xrHead = Camera.main.transform;

        if (coordinateManager == null || xrHead == null)
        {
            Debug.LogWarning("[BULLE UI] Assign CoordinateManager and XR Main Camera.");
            return;
        }

        CreateCanvas();
        coordinateManager.SaveSucceeded += OnSaved;
        coordinateManager.SaveFailed += OnFailed;
    }

    private void CreateCanvas()
    {
        GameObject root = new GameObject("Bulle_SaveFeedbackCanvas", typeof(RectTransform), typeof(Canvas));
        root.transform.SetParent(xrHead, false);
        root.transform.localPosition = offsetFromHead;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one * 0.0015f;

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(500, 90);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = 300;

        panel = new GameObject("FeedbackPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(root.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        background = panel.GetComponent<Image>();
        background.color = new Color(0.05f, 0.14f, 0.12f, 0.90f);

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panel.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12, 8);
        textRect.offsetMax = new Vector2(-12, -8);
        message = textObject.GetComponent<TextMeshProUGUI>();
        message.alignment = TextAlignmentOptions.Center;
        message.fontSize = 27;
        message.enableWordWrapping = false;
        message.enableAutoSizing = true;
        message.fontSizeMin = 18;
        message.fontSizeMax = 27;
        message.color = Color.white;

        root.SetActive(false);
        // We enable the full Canvas root when displaying feedback.
        canvasRoot = root;
    }

    private GameObject canvasRoot;

    private void OnSaved(string path)
    {
        Show("CSV sauvegarde (" + coordinateManager.ObjectCount + " points)", false);
    }

    private void OnFailed(string reason)
    {
        Show("Enregistrement impossible", true);
        Debug.LogWarning("[BULLE UI] " + reason);
    }

    private void Show(string value, bool error)
    {
        if (canvasRoot == null || message == null) return;
        message.text = value;
        background.color = error
            ? new Color(0.55f, 0.12f, 0.12f, 0.9f)
            : new Color(0.05f, 0.24f, 0.16f, 0.9f);
        canvasRoot.SetActive(true);
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideLater());
    }

    private IEnumerator HideLater()
    {
        yield return new WaitForSecondsRealtime(durationSeconds);
        if (canvasRoot != null) canvasRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (coordinateManager != null)
        {
            coordinateManager.SaveSucceeded -= OnSaved;
            coordinateManager.SaveFailed -= OnFailed;
        }
        if (canvasRoot != null) Destroy(canvasRoot);
    }
}
