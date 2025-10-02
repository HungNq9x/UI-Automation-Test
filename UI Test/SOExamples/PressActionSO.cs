using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Tests/Actions/Press", fileName = "PressAction")]
public class PressActionSO : TestActionSO
{
    [Tooltip("Hierarchy path or ObjectID with 'id:' prefix")]
    public string path;

    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        yield return uiTest.Press(host, path);
    }
}
