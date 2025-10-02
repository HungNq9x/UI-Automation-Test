# Object Lookup - Tìm kiếm UI Elements

Hướng dẫn chi tiết về cách tìm kiếm UI elements trong UI Test Framework.

## Tổng quan

Framework hỗ trợ hai phương pháp tìm kiếm GameObjects:

1. **Hierarchy Path**: Tìm theo đường dẫn trong hierarchy
2. **ObjectID**: Tìm theo unique identifier

## 1. Hierarchy Path

### Cú pháp

```
"Canvas/Panel/Button"
"RootObject/ChildObject/TargetObject"
```

### Cách hoạt động

- Sử dụng `GameObject.Find()` hoặc duyệt hierarchy
- Path phân cách bởi `/`
- Case-sensitive
- Parent phải đến child theo đúng structure

### Ví dụ

```csharp
// Hierarchy trong scene:
Canvas
  └─ MainMenu
      └─ ButtonPanel
          └─ PlayButton

// Path:
path = "Canvas/MainMenu/ButtonPanel/PlayButton"
```

### Ưu điểm

✅ Dễ hiểu và trực quan  
✅ Không cần setup bổ sung  
✅ Phù hợp cho prototyping nhanh  
✅ Hoạt động với bất kỳ GameObject nào  

### Nhược điểm

❌ Dễ vỡ khi refactor UI  
❌ Cần update path nếu di chuyển objects  
❌ Path dài khó maintain  
❌ Case-sensitive dễ typo  

### Best Practices

```
✅ Tốt cho:
- Quick prototypes
- Temporary tests
- Simple hierarchies
- Objects không di chuyển

❌ Tránh cho:
- Production tests
- Deep hierarchies
- Frequently moved objects
- Team collaboration (paths khác nhau)
```

## 2. ObjectID Lookup

### Cú pháp

```
"id:UniqueIdentifier"
"id:PlayButton"
"id:HealthBar"
```

### Setup

1. **Add ObjectID Component**
   ```
   Select GameObject → Add Component → ObjectID
   ```

2. **Set Unique Id**
   ```
   ObjectID component:
     Id: "PlayButton"
   ```

3. **Use in Tests**
   ```
   path = "id:PlayButton"
   ```

### Cách hoạt động

```csharp
// Framework tìm tất cả ObjectID components
var allIds = GameObject.FindObjectsOfType<ObjectID>();

// So sánh Id với giá trị sau "id:"
foreach (var obj in allIds)
{
    if (obj.Id == requestedId)
        return obj.gameObject;
}
```

### Prefix Rules

**Case-insensitive prefix**:
- `id:PlayButton` ✅
- `ID:PlayButton` ✅
- `Id:PlayButton` ✅
- `iD:PlayButton` ✅

**Case-sensitive value**:
- Component Id: `PlayButton`
- Path: `id:PlayButton` ✅
- Path: `id:playbutton` ❌

### Ưu điểm

✅ Ổn định khi refactor UI  
✅ Không phụ thuộc hierarchy  
✅ Short và dễ đọc  
✅ Unique identifiers rõ ràng  
✅ Tốt cho team collaboration  

### Nhược điểm

❌ Cần setup component trước  
❌ Thêm component overhead (minimal)  
❌ Phải manage unique Ids  

### Best Practices

```
✅ Tốt cho:
- Production tests
- Frequently refactored UI
- Long-term maintainability
- Team projects

❌ Tránh:
- Duplicate Ids (framework returns first match)
```

## So sánh hai phương pháp

| Tiêu chí | Hierarchy Path | ObjectID |
|----------|---------------|----------|
| Setup | Không cần | Cần add component |
| Stability | Thấp | Cao |
| Maintainability | Khó | Dễ |
| Performance | Tương đương | Tương đương |
| Readability | Trung bình | Cao |
| Use Case | Prototype | Production |

## Chi tiết kỹ thuật

### Hierarchy Path Implementation

```csharp
public static GameObject FindByPath(string path)
{
    if (string.IsNullOrEmpty(path)) return null;
    
    // Try direct Find first
    GameObject obj = GameObject.Find(path);
    if (obj != null) return obj;
    
    // Traverse hierarchy
    string[] parts = path.Split('/');
    Transform current = null;
    
    foreach (string part in parts)
    {
        if (current == null)
        {
            // Find root
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            current = roots.FirstOrDefault(r => r.name == part)?.transform;
        }
        else
        {
            // Find child
            current = current.Find(part);
        }
        
        if (current == null) return null;
    }
    
    return current?.gameObject;
}
```

### ObjectID Implementation

```csharp
public static GameObject FindById(string id)
{
    var allIds = GameObject.FindObjectsOfType<ObjectID>();
    foreach (var objId in allIds)
    {
        if (string.Equals(objId.Id, id, StringComparison.Ordinal))
        {
            return objId.gameObject;
        }
    }
    return null;
}
```

### Combined Lookup

```csharp
public static GameObject FindByPathOrId(string pathOrId)
{
    if (string.IsNullOrEmpty(pathOrId)) return null;
    
    // Check for "id:" prefix (case-insensitive)
    if (pathOrId.Length > 3 && 
        pathOrId.Substring(0, 3).ToLower() == "id:")
    {
        string id = pathOrId.Substring(3); // Skip "id:"
        return FindById(id);
    }
    
    // Otherwise use hierarchy path
    return FindByPath(pathOrId);
}
```

## Setup ObjectID cho project

### 1. Identify Critical UI Elements

Đánh dấu objects thường sử dụng trong tests:
- Buttons (Play, Settings, Back, etc.)
- Input fields (Username, Password, Search, etc.)
- Labels (Status, Title, Score, etc.)
- Panels (Menu, Dialog, HUD, etc.)

### 2. Naming Convention

```
Consistent naming:
- PlayButton, BackButton, SettingsButton
- UsernameField, PasswordField, EmailField
- StatusLabel, TitleLabel, ScoreLabel
- MainMenuPanel, SettingsPanel, GamePanel
```

### 3. Batch Setup

```csharp
// Editor script để batch add ObjectID
[MenuItem("Tools/Setup ObjectIDs")]
static void SetupObjectIDs()
{
    // Find all buttons
    var buttons = GameObject.FindObjectsOfType<Button>();
    foreach (var btn in buttons)
    {
        if (btn.GetComponent<ObjectID>() == null)
        {
            var objId = btn.gameObject.AddComponent<ObjectID>();
            objId.Id = btn.name; // Use GameObject name as default
        }
    }
    
    Debug.Log($"Setup {buttons.Length} ObjectIDs");
}
```

### 4. Documentation

Maintain danh sách ObjectIDs:

```markdown
# UI ObjectID Reference

## Main Menu
- id:PlayButton - Start game button
- id:SettingsButton - Open settings
- id:QuitButton - Quit game

## Login Screen
- id:UsernameField - Username input
- id:PasswordField - Password input
- id:LoginButton - Login submit button
```

## Debugging Lookup Issues

### Check if Object Exists

```csharp
// Test lookup
string testPath = "id:PlayButton";
var obj = UITest.FindByPathOrId(testPath);

Debug.Log($"Path: {testPath}");
Debug.Log($"Found: {obj != null}");
if (obj != null)
{
    Debug.Log($"Name: {obj.name}");
    Debug.Log($"Active: {obj.activeInHierarchy}");
    Debug.Log($"Scene: {obj.scene.name}");
}
```

### List All ObjectIDs

```csharp
[MenuItem("Tools/List All ObjectIDs")]
static void ListObjectIDs()
{
    var allIds = GameObject.FindObjectsOfType<ObjectID>();
    Debug.Log($"Found {allIds.Length} ObjectIDs:");
    
    foreach (var objId in allIds)
    {
        Debug.Log($"  - id:{objId.Id} → {objId.gameObject.name}");
    }
}
```

### Verify Hierarchy Path

```csharp
static void VerifyPath(string path)
{
    var obj = GameObject.Find(path);
    if (obj == null)
    {
        Debug.LogError($"Path not found: {path}");
        
        // Try to find similar names
        var allObjects = GameObject.FindObjectsOfType<GameObject>();
        var similar = allObjects.Where(o => o.name.Contains(path.Split('/').Last()))
                                .Take(5);
        
        Debug.Log("Similar objects:");
        foreach (var s in similar)
        {
            Debug.Log($"  - {GetFullPath(s)}");
        }
    }
    else
    {
        Debug.Log($"Found: {GetFullPath(obj)}");
    }
}

static string GetFullPath(GameObject obj)
{
    string path = obj.name;
    Transform parent = obj.transform.parent;
    while (parent != null)
    {
        path = parent.name + "/" + path;
        parent = parent.parent;
    }
    return path;
}
```

## Performance Considerations

### Hierarchy Path
- **FindByPath**: O(n) where n = số objects trong scene
- **Cache**: Có thể cache GameObject reference

### ObjectID
- **FindById**: O(n) where n = số ObjectID components
- **Optimization**: Ít components hơn objects → nhanh hơn trong scenes lớn

### Caching Strategy

```csharp
// Cache trong test step
private Dictionary<string, GameObject> cache = new Dictionary<string, GameObject>();

GameObject GetObject(string pathOrId)
{
    if (!cache.ContainsKey(pathOrId))
    {
        cache[pathOrId] = UITest.FindByPathOrId(pathOrId);
    }
    return cache[pathOrId];
}
```

## Migration Guide

### Chuyển từ Hierarchy Path sang ObjectID

```csharp
// Before
path = "Canvas/MainMenu/ButtonPanel/PlayButton"

// After
// 1. Add ObjectID component to PlayButton
// 2. Set Id = "PlayButton"
// 3. Update path
path = "id:PlayButton"
```

### Migration Script

```csharp
[MenuItem("Tools/Migrate to ObjectID")]
static void MigrateToObjectID()
{
    // Find all test case assets
    var testCases = AssetDatabase.FindAssets("t:UIAutomatedTestCase");
    
    foreach (var guid in testCases)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        var testCase = AssetDatabase.LoadAssetAtPath<UIAutomatedTestCase>(path);
        
        // Process each step
        foreach (var step in testCase.steps)
        {
            if (step is ActionStepSO actionStep)
            {
                // Convert long paths to ObjectID
                if (actionStep.path.Split('/').Length > 3)
                {
                    string objectName = actionStep.path.Split('/').Last();
                    actionStep.path = $"id:{objectName}";
                }
            }
        }
        
        EditorUtility.SetDirty(testCase);
    }
    
    AssetDatabase.SaveAssets();
    Debug.Log("Migration complete!");
}
```

## Best Practices Summary

### ✅ Do's

- Use ObjectID cho production tests
- Use consistent naming conventions
- Document all ObjectIDs
- Cache lookups khi appropriate
- Verify object exists before test

### ❌ Don'ts

- Đừng mix styles trong cùng project (chọn một)
- Đừng dùng duplicate ObjectIDs
- Đừng hardcode deep hierarchy paths
- Đừng forget to add ObjectID component
- Đừng use special characters trong Ids

## Xem thêm

- [[Getting Started]] - Basic usage
- [[Test Steps]] - Using paths in steps
- [[Best Practices]] - General recommendations
- [[Troubleshooting]] - Lookup issues
