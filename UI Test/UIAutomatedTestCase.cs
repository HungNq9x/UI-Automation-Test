using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Tests/Automated Test Case", fileName = "New UI Test Case")]
public class UIAutomatedTestCase : ScriptableObject
{
    [TextArea(2,4)]
    public string description;

    [Tooltip("If true, stop running this case when an action fails (exception).")]
    public bool stopOnError = true;

    [Tooltip("Sequential test steps to execute")]
    public List<TestStepBase> steps = new List<TestStepBase>();



    /// <summary>
    /// Run the test case steps sequentially on the provided host MonoBehaviour using the provided UITest helper.
    /// The coroutine yields until the case finishes or is stopped on error.
    /// </summary>
    public IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        if (host == null) throw new ArgumentNullException(nameof(host));
        if (uiTest == null) uiTest = new UITest();

        for (int i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var label = $"Step {i + 1}/{steps.Count}: {step}";
            Debug.Log($"[UIAutomatedTestCase] Starting {label}");

            Exception caught = null;
            // Run the step while capturing any exception thrown during iteration and enforcing per-step timeout
            yield return host.StartCoroutine(RunStepWithTimeout(step, host, uiTest, (ex) => caught = ex));

            if (caught == null)
            {
                Debug.Log($"[UIAutomatedTestCase] ✓ Completed {label}");
            }
            else
            {
                Debug.LogError($"[UIAutomatedTestCase] ✗ Failed {label}: {caught}");
                if (stopOnError)
                {
                    Debug.LogWarning("[UIAutomatedTestCase] Stopping test case due to error (stopOnError=true).");
                    yield break;
                }
                else
                {
                    Debug.LogWarning("[UIAutomatedTestCase] Continuing to next step (stopOnError=false).");
                }
            }

            // Small yield to avoid overwhelming the scheduler
            yield return null;
        }

        Debug.Log($"[UIAutomatedTestCase] Finished test case '{name}'");
    }

    // Run a step enumerator but enforce timeoutSeconds on the TestStepBase (0 uses uiTest.WaitTimeout).
    private static IEnumerator RunStepWithTimeout(TestStepBase step, MonoBehaviour host, UITest uiTest, Action<Exception> onException)
    {
        if (step == null) yield break;

        float timeout = step.timeoutSeconds > 0f ? step.timeoutSeconds : uiTest.WaitTimeout;

        var enumerator = step.Execute(host, uiTest);
        if (enumerator == null) yield break;

        float elapsed = 0f;

        while (true)
        {
            object current = null;
            bool moveNext;
            try
            {
                moveNext = enumerator.MoveNext();
                if (moveNext) current = enumerator.Current;
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
                yield break;
            }

            if (!moveNext) yield break;

            // Advance time while yielding the current object. Use unscaled delta time for timers.
            // If current is null we still wait a frame, otherwise yield the current value and then update elapsed based on frames.
            yield return current;

            // Update elapsed time based on frames passed since last check.
            elapsed += Time.unscaledDeltaTime;
            if (elapsed > timeout)
            {
                onException?.Invoke(new TimeoutException($"Step timed out after {timeout} seconds: {step.name}"));
                yield break;
            }
        }
    }

    // Helper to run an IEnumerator and capture any exception that occurs during iteration.
    private static IEnumerator RunEnumeratorCatchExceptions(IEnumerator enumerator, Action<Exception> onException)
    {
        if (enumerator == null) yield break;

        while (true)
        {
            object current = null;
            bool moveNext;
            try
            {
                moveNext = enumerator.MoveNext();
                if (moveNext) current = enumerator.Current;
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
                yield break;
            }

            if (!moveNext) yield break;
            yield return current;
        }
    }
}
