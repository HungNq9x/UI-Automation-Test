using System.Collections;
using UnityEngine;
//using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "UI Tests/Steps/Wait Time", fileName = "WaitTimeStep")]
public class WaitTimeStepSO : TestStepBase
{
    //[MinValue(0)]
    [Tooltip("Seconds to wait (unscaled time)")]
    public float seconds = 1f;

    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        yield return new WaitForSeconds(seconds);
    }

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(note)) return note;
        return $"WaitTime: {seconds}s";
    }
}
