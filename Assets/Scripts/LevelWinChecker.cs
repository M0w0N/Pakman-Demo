using UnityEngine;
using UnityEngine.UI;

public class LevelWinChecker : MonoBehaviour
{
    [Header("Level Config")]
    [Tooltip("Drag a LevelConfig asset from Project window here")]
    public LevelConfig config;

    [Header("UI Bindings")]
    [Tooltip("Progress bar Slider, leave empty to hide")]
    public Slider progressSlider;
    [Tooltip("Return point marker (ReturnToStart mode only), auto-activated when enough pellets eaten")]
    public GameObject returnPointMarker;
    [Tooltip("Hint text (ReturnToStart mode only), auto-activated together with the return point marker")]
    public GameObject returnPointHint;
    [Header("Progress Bar Target Marker")]
    [Tooltip("Static marker RectTransform (arrow/line) inside Slider's Fill Area. Auto-positioned at the minimum threshold. Leave empty to skip.")]
    public RectTransform targetMarker;
    [Tooltip("If true, hides the Slider's built-in handle so only this static marker and fill bar show")]
    public bool hideSliderHandle = true;

    // Internal state
    private int totalPellets = 0;
    private int currentEaten = 0;
    private bool hasWon = false;
    private bool returnUnlocked = false;

    void Start()
    {
        if (config == null)
        {
            Debug.LogWarning("LevelWinChecker: No LevelConfig assigned, script disabled.");
            return;
        }

        totalPellets = config.totalPellets;

        // Hide return marker and hint initially
        if (returnPointMarker != null)
        {
            returnPointMarker.SetActive(false);
        }
        if (returnPointHint != null)
        {
            returnPointHint.SetActive(false);
        }

        // Init progress bar
        if (progressSlider != null && totalPellets > 0)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;

            // Hide Slider's built-in handle so it doesn't move with fill
            if (hideSliderHandle && progressSlider.handleRect != null)
            {
                progressSlider.handleRect.gameObject.SetActive(false);
            }
        }

        // Position the static target marker at the minimum threshold
        // Parent to the Fill Area *container* (fillRect's parent), not the fill image itself,
        // so the marker stays fixed while the fill bar grows.
        if (targetMarker != null && totalPellets > 0)
        {
            float threshold = (float)config.pelletsRequiredForReturn / totalPellets;
            RectTransform fillAreaContainer = progressSlider?.fillRect?.parent as RectTransform;
            if (fillAreaContainer != null)
            {
                targetMarker.SetParent(fillAreaContainer, false);
                targetMarker.pivot = new Vector2(0.5f, 0.5f);
                // Anchor at the threshold percentage — stays fixed regardless of Fill Area's pixel size
                targetMarker.anchorMin = new Vector2(threshold, 0.5f);
                targetMarker.anchorMax = new Vector2(threshold, 0.5f);
                targetMarker.anchoredPosition = Vector2.zero;
            }
        }

        Debug.Log($"LevelWinChecker: [{config.levelName}], winCondition = {config.winCondition}");
    }

    void Update()
    {
        if (config == null || hasWon) return;

        int remaining = GameObject.FindGameObjectsWithTag("Reward").Length;
        currentEaten = totalPellets - remaining;

        // Progress bar — always totalPellets as denominator
        UpdateProgressBar();

        // Win condition check
        switch (config.winCondition)
        {
            case LevelConfig.WinCondition.EatAllPellets:
                if (remaining <= 0) TriggerWin();
                break;

            case LevelConfig.WinCondition.ReturnToStart:
                if (currentEaten >= config.pelletsRequiredForReturn && !returnUnlocked)
                {
                    UnlockReturnPoint();
                }
                // Win is triggered by OnPlayerReachReturnPoint()
                break;

            case LevelConfig.WinCondition.ReachExitTrigger:
                // TriggerWin() is called externally by an exit trigger
                break;
        }
    }

    private void UpdateProgressBar()
    {
        if (progressSlider == null || totalPellets <= 0) return;

        // Always denominator = totalPellets.  ReturnToStart can pass without full bar.
        progressSlider.value = Mathf.Clamp01((float)currentEaten / totalPellets);
    }

    private void UnlockReturnPoint()
    {
        returnUnlocked = true;

        if (returnPointMarker != null)
        {
            returnPointMarker.SetActive(true);
        }
        if (returnPointHint != null)
        {
            returnPointHint.SetActive(true);
        }

        Debug.Log($"Return point unlocked! Eaten {config.pelletsRequiredForReturn} pellets, head back now.");
    }

    /// <summary>
    /// Called by ReturnPointTrigger when player reaches the return zone
    /// </summary>
    public void OnPlayerReachReturnPoint()
    {
        Debug.Log($"OnPlayerReachReturnPoint called: hasWon={hasWon}, winCondition={config?.winCondition}, returnUnlocked={returnUnlocked}");

        if (config == null || hasWon) return;

        if (config.winCondition != LevelConfig.WinCondition.ReturnToStart)
        {
            Debug.LogWarning($"OnPlayerReachReturnPoint: winCondition is {config.winCondition}, not ReturnToStart — ignoring.");
            return;
        }

        if (!returnUnlocked)
        {
            Debug.Log("OnPlayerReachReturnPoint: return point NOT yet unlocked — ignoring.");
            return;
        }

        TriggerWin();
    }

    /// <summary>
    /// Public win entry point. Callable from exit triggers, buttons, etc.
    /// </summary>
    public void TriggerWin()
    {
        if (hasWon) return;
        hasWon = true;

        // Rating: >= total means 100%, otherwise partial
        int ratingPercent = totalPellets > 0
            ? Mathf.RoundToInt((float)currentEaten / totalPellets * 100f)
            : 100;

        Debug.Log($"LevelWinChecker: WIN! Pellets eaten: {currentEaten}/{totalPellets} ({ratingPercent}%)");

        if (ScoreManager.Instance != null)
        {
            // TODO: pass ratingPercent into ShowLevelClearWindow for star/grade display
            ScoreManager.Instance.ShowLevelClearWindow();
        }
    }
}
