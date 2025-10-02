using UnityEngine;

[CreateAssetMenu(menuName = "UI Tests/Conditions/Object Appeared", fileName = "ObjectAppearedCondition")]
public class ObjectAppearedConditionSO : TestCondition
{
    [Tooltip("Hierarchy path or ObjectID with 'id:' prefix")]
    public string path;

    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        if (string.IsNullOrEmpty(path)) return false;
        // Support simple id: lookup like earlier logic
        const string idPrefix = "id:";
        GameObject go = null;
        if (path.StartsWith(idPrefix, System.StringComparison.InvariantCultureIgnoreCase))
        {
            var id = path.Substring(idPrefix.Length);
            var objs = UnityEngine.Object.FindObjectsByType<ObjectID>(FindObjectsSortMode.None);
            foreach (var oid in objs)
            {
                if (oid == null) continue;
                if (string.Equals(oid.Id, id, System.StringComparison.Ordinal))
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

        return go != null && go.activeInHierarchy;
    }
}
