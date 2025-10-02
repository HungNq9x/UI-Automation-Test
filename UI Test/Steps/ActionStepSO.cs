using System;
using System.Collections;
using UnityEngine;
//using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "UI Tests/Steps/Action Step", fileName = "ActionStep")]
public class ActionStepSO : TestStepBase
{
    public enum ActionType
    {
        Press,
        AssertLabel,
        LoadScene,
        InputText,
        SetToggle,
        WaitSeconds,
        DragAndDrop,
        RaycastClick,
        SelectDropdown,
        SetSlider,
        Hover,
        Hold
    }

    //[EnumToggleButtons]
    public ActionType actionType = ActionType.Press;

    //[ShowIf("@actionType != ActionType.WaitSeconds")]
    [Tooltip("Hierarchy path or ObjectID lookup using prefix 'id:MyId'")]
    public string path;

    //[ShowIf("actionType", ActionType.DragAndDrop)]
    [Tooltip("Second path (for actions that need two targets, e.g. DragAndDrop)")]
    public string path2;

    //[ShowIf("@actionType == ActionType.AssertLabel || actionType == ActionType.InputText")]
    [Tooltip("Text payload (AssertLabel, InputText, etc.)")]
    public string text;

    //[ShowIf("@actionType == ActionType.WaitSeconds || actionType == ActionType.SelectDropdown || actionType == ActionType.SetSlider || actionType == ActionType.Hold")]
    [Tooltip("Numeric payload (seconds, slider value, dropdown index)")]
    public float floatValue;

    //[ShowIf("actionType", ActionType.SetToggle)]
    [Tooltip("Boolean payload (SetToggle)")]
    public bool boolValue;

    [Tooltip("Optional custom action SO. If set, this will be executed instead of the inline action.")]
    public TestActionSO customActionSO;

    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        if (host == null) throw new ArgumentNullException(nameof(host));
        if (uiTest == null) uiTest = new UITest();

        // If a custom action SO is provided, run it instead
        if (customActionSO != null)
        {
            yield return host.StartCoroutine(customActionSO.Run(host, uiTest));
            yield break;
        }

        switch (actionType)
        {
            case ActionType.Press:
                yield return uiTest.Press(host, path);
                break;
            case ActionType.AssertLabel:
                yield return uiTest.AssertLabel(host, path, text);
                break;
            case ActionType.LoadScene:
                yield return uiTest.LoadScene(host, path);
                break;
            case ActionType.InputText:
                yield return uiTest.InputText(host, path, text);
                break;
            case ActionType.SetToggle:
                yield return uiTest.SetToggle(host, path, boolValue);
                break;
            case ActionType.WaitSeconds:
                yield return new WaitForSecondsRealtime(floatValue);
                break;
            case ActionType.DragAndDrop:
                yield return uiTest.DragAndDrop(host, path, path2);
                break;
            case ActionType.RaycastClick:
                yield return uiTest.RaycastClick(host, path);
                break;
            case ActionType.SelectDropdown:
                yield return uiTest.SelectDropdown(host, path, (int)floatValue);
                break;
            case ActionType.SetSlider:
                yield return uiTest.SetSlider(host, path, floatValue);
                break;
            case ActionType.Hover:
                yield return uiTest.Hover(host, path);
                break;
            case ActionType.Hold:
                yield return uiTest.Hold(host, path, floatValue);
                break;
            default:
                throw new NotSupportedException("Unsupported action type: " + actionType);
        }
    }

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(note)) return note;
        return customActionSO != null
            ? $"Action: {customActionSO.name}"
            : $"Action: {actionType} ({path})";
    }
}
