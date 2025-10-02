using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class UITestRunnerWindow : EditorWindow
{
    private UIAutomatedTestCase selectedTestCase;
    private Vector2 scrollPos;
    private bool pendingStartAfterPlay = false;

    [MenuItem("Tools/UI Test Runner")]
    static void OpenWindow()
    {
        UITestRunnerWindow window = GetWindow<UITestRunnerWindow>();
        window.titleContent = new GUIContent("UI Test Runner");
        window.Show();
    }

    private void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        // Ensure we don't keep the play mode callback registered
        if (pendingStartAfterPlay)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            pendingStartAfterPlay = false;
        }
    }

    private void OnEditorUpdate()
    {
        // Poll for updates from runtime
        if (UITestRunner.IsRunning || UITestRunner.HasFailed || UITestRunner.CurrentTestCase != null)
        {
            Repaint();
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("UI Test Runner", EditorStyles.boldLabel);

        //pendingStartAfterPlay = EditorGUILayout.Toggle("pendingStartAfterPlay", pendingStartAfterPlay);

        // Test Case Selection
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Test Case:", GUILayout.Width(80));
        selectedTestCase = (UIAutomatedTestCase)EditorGUILayout.ObjectField(selectedTestCase, typeof(UIAutomatedTestCase), false);
        EditorGUILayout.EndHorizontal();

        // Run Button
        if (GUILayout.Button("Run Test", GUILayout.Height(30)))
        {
            if (selectedTestCase == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a test case first.", "OK");
                return;
            }

            // Ensure we're in play mode or enter it
            if (!Application.isPlaying)
            {
                // Use playModeStateChanged to reliably detect when play mode has started.
                pendingStartAfterPlay = true;
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                Debug.Log("UI Test Runner: entering Play mode and will start test when Play mode is entered.");
                EditorApplication.EnterPlaymode();
            }
            else
            {
                StartTest();
            }
        }

        // Stop Button
        if (Application.isPlaying && UITestRunner.IsRunning)
        {
            if (GUILayout.Button("Stop Test", GUILayout.Height(25)))
            {
                UITestRunner.Reset();
            }
        }

        GUILayout.Space(10);

        // Status Display
        if (UITestRunner.CurrentTestCase != null)
        {
            GUILayout.Label($"Running: {UITestRunner.CurrentTestCase.name}", EditorStyles.boldLabel);

            if (UITestRunner.HasFailed)
            {
                GUI.color = Color.red;
                GUILayout.Label($"FAILED: {UITestRunner.FailureMessage}", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }
            else if (!UITestRunner.IsRunning && UITestRunner.StepResults.Count > 0 && !UITestRunner.HasFailed)
            {
                GUI.color = Color.green;
                GUILayout.Label("SUCCESS: All steps passed!", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }
            else if (UITestRunner.IsRunning)
            {
                GUI.color = Color.yellow;
                GUILayout.Label($"Running step {UITestRunner.CurrentStepIndex + 1}...", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }

            // Steps List
            GUILayout.Label("Steps:", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));

            if (UITestRunner.CurrentTestCase.steps != null)
            {
                for (int i = 0; i < UITestRunner.CurrentTestCase.steps.Count; i++)
                {
                    var step = UITestRunner.CurrentTestCase.steps[i];
                    if (step == null) continue;

                    GUIStyle style = new GUIStyle(EditorStyles.label);

                    if (i == UITestRunner.CurrentStepIndex && UITestRunner.IsRunning)
                    {
                        style.normal.textColor = Color.yellow;
                        style.fontStyle = FontStyle.Bold;
                    }
                    else if (i < UITestRunner.StepResults.Count && UITestRunner.StepResults[i])
                    {
                        style.normal.textColor = Color.green;
                    }
                    else if (i < UITestRunner.StepResults.Count && !UITestRunner.StepResults[i])
                    {
                        style.normal.textColor = Color.red;
                    }

                    string stepName = step.note;
                    if (string.IsNullOrEmpty(stepName))
                    {
                        stepName = step.ToString();
                    }

                    // Show timeout if set on step
                    string timeoutText = "";
                    var tsa = step as TestStepBase;
                    if (tsa != null && tsa.timeoutSeconds > 0f)
                    {
                        timeoutText = $"  [timeout: {tsa.timeoutSeconds}s]";
                    }

                    GUILayout.Label($"{i + 1}. {stepName}{timeoutText}", style);
                }
            }

            EditorGUILayout.EndScrollView();
        }
        else
        {
            GUILayout.Label("No test running", EditorStyles.helpBox);
        }
    }

    private void StartTestAfterPlayMode()
    {
        if (Application.isPlaying)
        {
            StartTest();
        }
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        Debug.Log($"[UITestRunnerWindow] OnPlayModeStateChanged called with state: {state}, pendingStartAfterPlay: {pendingStartAfterPlay}");

        if (!pendingStartAfterPlay)
        {
            Debug.Log("[UITestRunnerWindow] pendingStartAfterPlay is false, returning");
            return;
        }

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("[UITestRunnerWindow] Entered Play Mode, calling StartTest");
            pendingStartAfterPlay = false;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            StartTest();
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            // Play mode was aborted or we returned to edit mode; clear pending flag
            Debug.Log("[UITestRunnerWindow] Entered Edit Mode, aborting pending test");
            pendingStartAfterPlay = false;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            Debug.Log("UI Test Runner: play mode entry aborted.");
        }
        else
        {
            Debug.Log($"[UITestRunnerWindow] Unhandled state: {state}");
        }
    }

    private void StartTest()
    {
        Debug.Log("[UITestRunner] Starting RunTest coroutine from StartTest");
        // Ensure UITestRunner exists in scene
        if (UITestRunner.Instance == null)
        {
            var runnerGO = new GameObject("UITestRunner");
            runnerGO.AddComponent<UITestRunner>();
            Debug.Log("Created UITestRunner GameObject in scene.");
        }

        UITestRunner.SetTestCase(selectedTestCase);
    }
}