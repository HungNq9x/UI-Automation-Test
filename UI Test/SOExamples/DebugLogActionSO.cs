using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DebugLogAction", menuName = "UI Tests/Actions/Debug Log Action")]
public class DebugLogActionSO : TestStepBase
{
    [SerializeField]
    private string message = "Debug message";


    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        Debug.Log(message);
        yield return null;
    }


}