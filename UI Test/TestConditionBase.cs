using System.Collections;
using UnityEngine;

/// <summary>
/// Base ScriptableObject for reusable test conditions.
/// Implementations should provide synchronous Evaluate(host, uiTest) returning bool.
/// Keep evaluation lightweight (used inside UITest.WaitFor via BoolCondition).
/// </summary>
public abstract class TestCondition : ScriptableObject
{
    [TextArea]
    public string description;

    /// <summary>
    /// Evaluate the condition. Should be fast and synchronous.
    /// </summary>
    public abstract bool Evaluate(MonoBehaviour host, UITest uiTest);

    public override string ToString()
    {
        return string.IsNullOrEmpty(description) ? name : description;
    }
}
