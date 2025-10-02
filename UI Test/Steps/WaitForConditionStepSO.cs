using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "UI Tests/Steps/Wait For Condition", fileName = "WaitForConditionStep")]
public class WaitForConditionStepSO : TestStepBase
{
    [Serializable]
    public enum ConditionType
    {
        ObjectAppeared, 
        LabelTextEquals,
        SelectableInteractable
    }

    [Serializable]
    public class ConditionSpec
    {
        //[EnumToggleButtons]
        public ConditionType conditionType = ConditionType.ObjectAppeared;

        [Tooltip("Hierarchy path or ObjectID using 'id:...' prefix")]
        public string path;

        //[ShowIf("conditionType", ConditionType.LabelTextEquals)]
        [Tooltip("For LabelTextEquals, expected text")]
        public string expectedText;

        public bool Evaluate()
        {
            if (string.IsNullOrEmpty(path)) return false;

            // Support 'id:' lookup
            const string idPrefix = "id:";
            GameObject go = null;
            if (path.StartsWith(idPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                var id = path.Substring(idPrefix.Length);
                var objs = UnityEngine.Object.FindObjectsByType<ObjectID>(FindObjectsSortMode.None);
                foreach (var oid in objs)
                {
                    if (oid == null) continue;
                    if (string.Equals(oid.Id, id, StringComparison.Ordinal))
                    {
                        go = oid.gameObject;
                        break;
                    }
                }
            }
            else
            {
                go = GameObject.Find(path);
            }

            if (go == null) return false;

            switch (conditionType)
            {
                case ConditionType.ObjectAppeared:
                    return go.activeInHierarchy;
                case ConditionType.LabelTextEquals:
                    var t = go.GetComponent<UnityEngine.UI.Text>();
                    if (t != null) return t.text == expectedText;
                    var tmpType = Type.GetType("TMPro.TMP_Text, Unity.TextMeshPro");
                    if (tmpType != null)
                    {
                        var comp = go.GetComponent(tmpType);
                        if (comp != null)
                        {
                            var textProp = tmpType.GetProperty("text");
                            var value = textProp?.GetValue(comp) as string;
                            return value == expectedText;
                        }
                    }
                    return false;
                case ConditionType.SelectableInteractable:
                    var sel = go.GetComponent<UnityEngine.UI.Selectable>();
                    return sel != null && sel.interactable && go.activeInHierarchy;
                default:
                    return false;
            }
        }
    }

    [Tooltip("Inline preconditions that must be satisfied")]
    public List<ConditionSpec> preconditions = new List<ConditionSpec>();

    [Tooltip("ScriptableObject preconditions. Evaluated alongside inline preconditions.")]
    public List<TestCondition> preconditionSOs = new List<TestCondition>();

    public enum ConditionMatchMode { All, Any }
    //[EnumToggleButtons]
    [Tooltip("Whether all preconditions must be met (All) or any one (Any)")]
    public ConditionMatchMode preconditionMode = ConditionMatchMode.All;

    [Tooltip("Timeout in seconds for the preconditions check. 0 means use UITest.WaitTimeout.")]
    public float timeout = 0f;

    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        if (host == null) throw new ArgumentNullException(nameof(host));
        if (uiTest == null) uiTest = new UITest();

        if ((preconditions == null || preconditions.Count == 0) && (preconditionSOs == null || preconditionSOs.Count == 0))
        {
            yield break; // No conditions to wait for
        }

        float prevTimeout = uiTest.WaitTimeout;
        if (timeout > 0f) uiTest.WaitTimeout = timeout;

        System.Func<bool> predicate = () =>
        {
            try
            {
                // Evaluate inline preconditions
                if (preconditions != null && preconditions.Count > 0)
                {
                    if (preconditionMode == ConditionMatchMode.All)
                    {
                        foreach (var ps in preconditions)
                        {
                            if (!ps.Evaluate()) return false;
                        }
                    }
                    else
                    {
                        bool anyMatch = false;
                        foreach (var ps in preconditions)
                        {
                            if (ps.Evaluate()) { anyMatch = true; break; }
                        }
                        if (!anyMatch) return false;
                    }
                }

                // Evaluate SO preconditions
                if (preconditionSOs != null && preconditionSOs.Count > 0)
                {
                    foreach (var so in preconditionSOs)
                    {
                        if (so == null) continue;
                        if (!so.Evaluate(host, uiTest)) return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        };

        yield return uiTest.WaitFor(host, new UITest.BoolCondition(predicate));

        uiTest.WaitTimeout = prevTimeout;
    }

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(note)) return note;
        int count = (preconditions != null ? preconditions.Count : 0) + (preconditionSOs != null ? preconditionSOs.Count : 0);
        return $"WaitForCondition: {count} condition(s), mode={preconditionMode}, timeout={timeout}s";
    }
}
