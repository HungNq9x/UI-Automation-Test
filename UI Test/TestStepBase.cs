using System.Collections;
using UnityEngine;

/// <summary>
/// Base ScriptableObject for all test steps.
/// Each step can be created as a reusable asset and sequenced in UIAutomatedTestCase.
/// </summary>
public abstract class TestStepBase : ScriptableObject
{
    [TextArea]
    public string note;

    [Tooltip("Optional per-step timeout in seconds. 0 means use UITest.WaitTimeout.")]
    public float timeoutSeconds = 0f;

    /// <summary>
    /// Execute this test step.
    /// </summary>
    public abstract IEnumerator Execute(MonoBehaviour host, UITest uiTest);

    public override string ToString()
    {
        return string.IsNullOrEmpty(note) ? name : note;
    }
}
