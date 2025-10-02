using System.Collections;
using UnityEngine;

/// <summary>
/// Base ScriptableObject for reusable test actions.
/// Implementations should implement Run(host, uiTest) as a coroutine.
/// </summary>
public abstract class TestActionSO : ScriptableObject
{
    [TextArea]
    public string description;

    /// <summary>
    /// Run the action as a coroutine on the provided host using the provided UITest helper.
    /// </summary>
    public abstract IEnumerator Run(MonoBehaviour host, UITest uiTest);

    public override string ToString() => string.IsNullOrEmpty(description) ? name : description;
}
