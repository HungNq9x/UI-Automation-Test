using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UITestManager : MonoBehaviour
{
    [Tooltip("List of scriptable test cases to run sequentially")]
    public List<UIAutomatedTestCase> testCases = new List<UIAutomatedTestCase>();

    [Tooltip("If true, tests start automatically on Start()")]
    public bool autoRunOnStart = false;

    [Tooltip("Seconds to wait between test cases (unscaled)")]
    public float gapBetweenCases = 0.5f;

    private UITest _uiTest;

    void Awake()
    {
        _uiTest = new UITest();
    }

    void Start()
    {
        if (autoRunOnStart)
        {
            StartCoroutine(RunAll());
        }
    }

    [ContextMenu("Run All Tests")]
    public void RunAllContext()
    {
        StartCoroutine(RunAll());
    }

    /// <summary>
    /// Run all configured test cases sequentially. Each case is run on this MonoBehaviour as host.
    /// </summary>
    public IEnumerator RunAll()
    {
        if (testCases == null || testCases.Count == 0)
        {
            Debug.LogWarning("[UITestManager] No test cases configured.");
            yield break;
        }

        Debug.Log($"[UITestManager] Running {testCases.Count} test case(s)...");

        for (int i = 0; i < testCases.Count; i++)
        {
            var tc = testCases[i];
            if (tc == null)
            {
                Debug.LogWarning($"[UITestManager] Skipping null test case at index {i}.");
                continue;
            }

            Debug.Log($"[UITestManager] Starting case {i + 1}/{testCases.Count}: '{tc.name}' - {tc.description}");

            bool caseError = false;
            Exception caseException = null;
            IEnumerator caseRoutine = null;
            try
            {
                caseRoutine = tc.Run(this, _uiTest);
            }
            catch (Exception ex)
            {
                caseError = true;
                caseException = ex;
            }
            if (!caseError && caseRoutine != null)
            {
                yield return StartCoroutine(caseRoutine);
                Debug.Log($"[UITestManager] Completed case '{tc.name}'");
            }
            else if (caseError)
            {
                Debug.LogError($"[UITestManager] Exception while preparing case '{tc.name}': {caseException}");
                // If a case requested stop on error it will have already stopped itself; continue to next case
            }

            // Wait a short gap before next case (use unscaled seconds)
            if (gapBetweenCases > 0f)
                yield return new WaitForSecondsRealtime(gapBetweenCases);
        }

        Debug.Log("[UITestManager] All test cases finished.");
    }
}
