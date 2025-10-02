using System.Text.RegularExpressions;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ObjectID : MonoBehaviour
{

    [SerializeField]
    private string id = "NewID";


    public string Id => id;



    private static readonly Regex s_idPattern = new Regex("^[A-Za-z0-9_\\-]+$", RegexOptions.Compiled);

    void Reset()
    {
        if (string.IsNullOrWhiteSpace(id)) id = gameObject.name ?? "NewID";
    }

    void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            id = gameObject != null ? gameObject.name : "NewID";
        }

        if (!s_idPattern.IsMatch(id))
        {
            // Warn when id contains unexpected characters. This runs in editor only.
            Debug.LogWarning($"ObjectID '{id}' on '{gameObject?.name}' contains characters outside [A-Za-z0-9_-]. Consider using only alphanumeric, '_' or '-'.");
        }
    }

    [ContextMenu("Regenerate GUID")]
    public void RegenerateGuid()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

}
