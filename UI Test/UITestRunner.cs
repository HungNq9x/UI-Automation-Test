using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITestRunner : MonoBehaviour
{
    public static UITestRunner Instance { get; private set; }

    public static UIAutomatedTestCase CurrentTestCase { get; private set; }
    public static int CurrentStepIndex { get; private set; } = -1;
    public static bool IsRunning { get; private set; } = false;
    public static bool HasFailed { get; private set; } = false;
    public static string FailureMessage { get; private set; } = "";
    public static List<bool> StepResults { get; private set; } = new List<bool>();

    private void Awake()
    {
        Debug.Log("[UITestRunner] Awake called");
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[UITestRunner] Duplicate instance detected, destroying this one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[UITestRunner] Instance set and DontDestroyOnLoad applied.");
    }

    private void Start()
    {
        Debug.Log($"[UITestRunner] Start called. IsRunning={IsRunning}, CurrentTestCase={(CurrentTestCase != null ? CurrentTestCase.name : "null")}");
        if (IsRunning && CurrentTestCase != null)
        {
            Debug.Log("[UITestRunner] Starting RunTest coroutine from Start()");
            StartCoroutine(RunTest());
        }
        else
        {
            Debug.Log("[UITestRunner] Not starting test in Start() - waiting for SetTestCase call.");
        }
    }

    public static void SetTestCase(UIAutomatedTestCase testCase)
    {
        Debug.Log($"[UITestRunner] SetTestCase called with test case: {testCase.name}, steps count: {testCase.steps.Count}");
        CurrentTestCase = testCase;
        CurrentStepIndex = -1;
        IsRunning = true;
        HasFailed = false;
        FailureMessage = "";
        StepResults = new List<bool>(new bool[testCase.steps.Count]);
        
        Debug.Log($"[UITestRunner] Instance={(Instance != null ? "exists" : "null")}, Application.isPlaying={Application.isPlaying}");
        
        // If the runtime instance already exists (timing may cause Start() to have run before SetTestCase),
        // begin the coroutine immediately so the test actually starts.
        if (Instance != null && Application.isPlaying)
        {
            Debug.Log("[UITestRunner] Starting RunTest coroutine from SetTestCase");
            Instance.StartCoroutine(Instance.RunTest());
        }
        else
        {
            Debug.LogWarning("[UITestRunner] Cannot start test - Instance is null or not in play mode!");
        }
    }

    public static void Reset()
    {
        CurrentTestCase = null;
        CurrentStepIndex = -1;
        IsRunning = false;
        HasFailed = false;
        FailureMessage = "";
        StepResults.Clear();
    }

    private static void SetStepRunning(int index)
    {
        CurrentStepIndex = index;
    }

    private static void SetStepResult(int index, bool success, string error = null)
    {
        if (index >= 0 && index < StepResults.Count)
        {
            StepResults[index] = success;
            if (!success)
            {
                HasFailed = true;
                FailureMessage = error ?? $"Step {index} failed";
            }
        }
    }

    private IEnumerator ExecuteStep(TestStepBase step, int index, bool stopOnError)
    {
        bool stepSuccess = true;
        string errorMessage = null;
        Coroutine coroutine = null;

        try
        {
            coroutine = StartCoroutine(step.Execute(this, new UITest()));
        }
        catch (System.Exception ex)
        {
            stepSuccess = false;
            errorMessage = ex.Message;
        }

        if (coroutine != null)
        {
            yield return coroutine;
        }

        SetStepResult(index, stepSuccess, errorMessage);

        if (!stepSuccess && stopOnError)
        {
            FinishTest();
        }
    }

    private IEnumerator RunTest()
    {
        Debug.Log("[UITestRunner] RunTest coroutine started!");
        var testCase = CurrentTestCase;
        if (testCase == null || testCase.steps == null)
        {
            Debug.LogError("[UITestRunner] RunTest aborted - testCase or steps is null");
            yield break;
        }

        Debug.Log($"[UITestRunner] Running test with {testCase.steps.Count} steps");

        for (int i = 0; i < testCase.steps.Count; i++)
        {
            var step = testCase.steps[i];
            if (step == null)
            {
                Debug.LogWarning($"[UITestRunner] Step {i} is null, skipping");
                continue;
            }

            Debug.Log($"[UITestRunner] Executing step {i + 1}/{testCase.steps.Count}: {step.name}");
            SetStepRunning(i);

            yield return ExecuteStep(step, i, testCase.stopOnError);

            yield return null; // Small delay between steps
        }

        Debug.Log("[UITestRunner] All steps completed");
        FinishTest();
    }

    private static void FinishTest()
    {
        Debug.Log($"[UITestRunner] FinishTest called. HasFailed={HasFailed}");
        IsRunning = false;
        CurrentStepIndex = -1;
        // Auto-remove from scene when test completes
        if (Instance != null)
        {
            Debug.Log("[UITestRunner] Destroying UITestRunner GameObject");
            Destroy(Instance.gameObject);
        }
    }
}