using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Simple runtime demo that creates a small UI and runs an in-memory test case using the framework.
// Attach this MonoBehaviour to an empty GameObject in a scene (or run via an initialization script).
public class UITestRuntimeDemo : MonoBehaviour
{
    private GameObject canvasGO;
    private Text statusLabel;

    private void Start()
    {
        SetupUI();
        StartDemoTest();
    }

    private void SetupUI()
    {
        // Create Canvas
        canvasGO = new GameObject("DemoCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Ensure EventSystem exists (use new API when available)
        #if UNITY_2023_1_OR_NEWER
        if (EventSystem.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
        #else
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
        #endif

        // Create a Button
        var btnGO = new GameObject("PlayButton");
        btnGO.transform.SetParent(canvasGO.transform, false);
        var rect = btnGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(160, 30);
        rect.anchoredPosition = new Vector2(0, 30);

        var img = btnGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 1f);

        var button = btnGO.AddComponent<Button>();

        // Button label
        var labelGO = new GameObject("Text");
        labelGO.transform.SetParent(btnGO.transform, false);
        var labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.sizeDelta = rect.sizeDelta;
        var lbl = labelGO.AddComponent<Text>();
        lbl.text = "Play";
        lbl.alignment = TextAnchor.MiddleCenter;
        lbl.color = Color.white;
        lbl.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Create a status label
        var statusGO = new GameObject("StatusLabel");
        statusGO.transform.SetParent(canvasGO.transform, false);
        var statusRect = statusGO.AddComponent<RectTransform>();
        statusRect.anchoredPosition = new Vector2(0, -20);
        statusRect.sizeDelta = new Vector2(300, 30);
        statusLabel = statusGO.AddComponent<Text>();
        statusLabel.text = "Idle";
        statusLabel.alignment = TextAnchor.MiddleCenter;
        statusLabel.color = Color.black;
        statusLabel.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Wire button to change the status label when clicked
        button.onClick.AddListener(() => OnPlayClicked());
    }

    private void OnPlayClicked()
    {
        statusLabel.text = "Game Started";
        Debug.Log("Demo: Play button clicked, status set to 'Game Started'");
    }

    private void StartDemoTest()
    {
        // Create steps in-memory
        var steps = new List<TestStepBase>();

        // small wait to let UI settle
        var waitShort = ScriptableObject.CreateInstance<WaitTimeStepSO>();
        waitShort.seconds = 0.2f;
        waitShort.timeoutSeconds = 2f;
        steps.Add(waitShort);

        // Press the Play button
        var pressStep = ScriptableObject.CreateInstance<ActionStepSO>();
        pressStep.actionType = ActionStepSO.ActionType.Press;
        pressStep.path = "DemoCanvas/PlayButton";
        pressStep.timeoutSeconds = 3f;
        steps.Add(pressStep);

        // Wait for the label to equal "Game Started"
        var waitFor = ScriptableObject.CreateInstance<WaitForConditionStepSO>();
        var cond = new WaitForConditionStepSO.ConditionSpec();
        cond.conditionType = WaitForConditionStepSO.ConditionType.LabelTextEquals;
        cond.path = "DemoCanvas/StatusLabel";
        cond.expectedText = "Game Started";
        waitFor.preconditions = new List<WaitForConditionStepSO.ConditionSpec> { cond };
        waitFor.timeout = 5f; // uses its own timeout field for condition checking
        waitFor.timeoutSeconds = 6f; // also set per-step timeout for demonstration
        steps.Add(waitFor);

        // AssertLabel via ActionStep (will internally wait for the label)
        var assertStep = ScriptableObject.CreateInstance<ActionStepSO>();
        assertStep.actionType = ActionStepSO.ActionType.AssertLabel;
        assertStep.path = "DemoCanvas/StatusLabel";
        assertStep.text = "Game Started";
        assertStep.timeoutSeconds = 2f;
        steps.Add(assertStep);

        // Build a test case instance
        var testCase = ScriptableObject.CreateInstance<UIAutomatedTestCase>();
        testCase.description = "Demo runtime test: press Play and expect 'Game Started'";
        testCase.stopOnError = true;
        testCase.steps = steps;

        // Register with UITestRunner and ensure runner gets created after setting case (so Start will pick it up)
        UITestRunner.SetTestCase(testCase);
        var runnerGO = new GameObject("UITestRunner");
        runnerGO.AddComponent<UITestRunner>();

        Debug.Log("UITestRuntimeDemo: Test started (in-memory test case)");
    }
}
