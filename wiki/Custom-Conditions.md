# Tạo Custom Conditions

Hướng dẫn tạo custom conditions để mở rộng WaitForConditionStepSO với logic kiểm tra tùy chỉnh.

## Tổng quan

Custom Conditions cho phép bạn:
- Tạo reusable condition logic
- Sử dụng trong WaitForConditionStepSO
- Kiểm tra complex game states
- Tích hợp với game systems

## Base Class

```csharp
public abstract class TestCondition : ScriptableObject
{
    public abstract bool Evaluate(MonoBehaviour host, UITest uiTest);
}
```

## Ví dụ cơ bản

### Ví dụ 1: Object Appeared Condition

```csharp
using UnityEngine;

[CreateAssetMenu(menuName = "UI Tests/Conditions/Object Appeared", fileName = "ObjectAppearedCondition")]
public class ObjectAppearedConditionSO : TestCondition
{
    [Tooltip("Path or ObjectID to check")]
    public string objectPath = "id:MyObject";
    
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        var obj = UITest.FindByPathOrId(objectPath);
        bool exists = obj != null && obj.activeInHierarchy;
        
        if (!exists)
        {
            Debug.Log($"[ObjectAppeared] Object not found or inactive: {objectPath}");
        }
        
        return exists;
    }
}
```

**Sử dụng**:
1. Create asset: `Create → UI Tests → Conditions → Object Appeared`
2. Set objectPath
3. Thêm vào WaitForConditionStepSO trong list `preconditionSOs`

### Ví dụ 2: Label Text Condition

```csharp
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "UI Tests/Conditions/Label Text Match", fileName = "LabelTextCondition")]
public class LabelTextConditionSO : TestCondition
{
    [Tooltip("Path to label")]
    public string labelPath = "id:StatusLabel";
    
    [Tooltip("Expected text")]
    public string expectedText = "Ready";
    
    [Tooltip("Case-sensitive comparison")]
    public bool caseSensitive = true;
    
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        var obj = UITest.FindByPathOrId(labelPath);
        if (obj == null) return false;
        
        var text = obj.GetComponent<Text>();
        if (text == null)
        {
            // Try TextMeshPro
            var tmpType = System.Type.GetType("TMPro.TMP_Text, Unity.TextMeshPro");
            if (tmpType != null)
            {
                var tmp = obj.GetComponent(tmpType);
                if (tmp != null)
                {
                    var textProp = tmpType.GetProperty("text");
                    string actualText = textProp?.GetValue(tmp) as string;
                    return CompareText(actualText, expectedText);
                }
            }
            return false;
        }
        
        return CompareText(text.text, expectedText);
    }
    
    private bool CompareText(string actual, string expected)
    {
        if (actual == null) return false;
        
        if (caseSensitive)
        {
            return actual == expected;
        }
        else
        {
            return actual.Equals(expected, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
```

## Ví dụ nâng cao

### Ví dụ 3: Game State Condition

```csharp
using UnityEngine;

[CreateAssetMenu(menuName = "UI Tests/Conditions/Game State", fileName = "GameStateCondition")]
public class GameStateConditionSO : TestCondition
{
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        GameOver
    }
    
    [Tooltip("Expected game state")]
    public GameState expectedState = GameState.Playing;
    
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        // Replace with your actual game manager
        var gameManager = GameManager.Instance;
        
        if (gameManager == null)
        {
            Debug.LogWarning("[GameState] GameManager not found");
            return false;
        }
        
        bool matches = gameManager.CurrentState == (GameManager.State)expectedState;
        
        if (!matches)
        {
            Debug.Log($"[GameState] Expected: {expectedState}, Actual: {gameManager.CurrentState}");
        }
        
        return matches;
    }
}
```

### Ví dụ 4: Score Threshold Condition

```csharp
using UnityEngine;

[CreateAssetMenu(menuName = "UI Tests/Conditions/Score Threshold", fileName = "ScoreThresholdCondition")]
public class ScoreThresholdConditionSO : TestCondition
{
    public enum ComparisonType
    {
        GreaterThan,
        GreaterOrEqual,
        Equal,
        LessOrEqual,
        LessThan
    }
    
    [Tooltip("Comparison type")]
    public ComparisonType comparison = ComparisonType.GreaterOrEqual;
    
    [Tooltip("Threshold value")]
    public int threshold = 100;
    
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        // Replace with your actual score manager
        int currentScore = ScoreManager.Instance?.CurrentScore ?? 0;
        
        bool result = comparison switch
        {
            ComparisonType.GreaterThan => currentScore > threshold,
            ComparisonType.GreaterOrEqual => currentScore >= threshold,
            ComparisonType.Equal => currentScore == threshold,
            ComparisonType.LessOrEqual => currentScore <= threshold,
            ComparisonType.LessThan => currentScore < threshold,
            _ => false
        };
        
        if (!result)
        {
            Debug.Log($"[ScoreThreshold] Score {currentScore} {comparison} {threshold} = false");
        }
        
        return result;
    }
}
```

### Ví dụ 5: Button Interactable Condition

```csharp
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "UI Tests/Conditions/Button Interactable", fileName = "ButtonInteractableCondition")]
public class ButtonInteractableConditionSO : TestCondition
{
    [Tooltip("Path to button or selectable")]
    public string buttonPath = "id:MyButton";
    
    [Tooltip("Expected interactable state")]
    public bool shouldBeInteractable = true;
    
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        var obj = UITest.FindByPathOrId(buttonPath);
        if (obj == null)
        {
            Debug.Log($"[ButtonInteractable] Button not found: {buttonPath}");
            return false;
        }
        
        var selectable = obj.GetComponent<Selectable>();
        if (selectable == null)
        {
            Debug.LogWarning($"[ButtonInteractable] No Selectable component on: {buttonPath}");
            return false;
        }
        
        bool isInteractable = selectable.interactable && obj.activeInHierarchy;
        bool matches = isInteractable == shouldBeInteractable;
        
        if (!matches)
        {
            Debug.Log($"[ButtonInteractable] Expected: {shouldBeInteractable}, Actual: {isInteractable}");
        }
        
        return matches;
    }
}
```

### Ví dụ 6: Network Connected Condition

```csharp
using UnityEngine;

[CreateAssetMenu(menuName = "UI Tests/Conditions/Network Connected", fileName = "NetworkConnectedCondition")]
public class NetworkConnectedConditionSO : TestCondition
{
    [Tooltip("Required reachability type")]
    public NetworkReachability requiredReachability = NetworkReachability.ReachableViaLocalAreaNetwork;
    
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        var currentReachability = Application.internetReachability;
        
        bool connected = false;
        
        switch (requiredReachability)
        {
            case NetworkReachability.NotReachable:
                connected = currentReachability == NetworkReachability.NotReachable;
                break;
            case NetworkReachability.ReachableViaCarrierDataNetwork:
            case NetworkReachability.ReachableViaLocalAreaNetwork:
                connected = currentReachability != NetworkReachability.NotReachable;
                break;
        }
        
        if (!connected)
        {
            Debug.Log($"[NetworkConnected] Current: {currentReachability}, Required: {requiredReachability}");
        }
        
        return connected;
    }
}
```

### Ví dụ 7: Multiple Objects Condition

```csharp
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "UI Tests/Conditions/Multiple Objects", fileName = "MultipleObjectsCondition")]
public class MultipleObjectsConditionSO : TestCondition
{
    public enum MatchMode
    {
        All,  // All objects must exist
        Any,  // At least one object must exist
        None  // No objects should exist
    }
    
    [Tooltip("Paths to check")]
    public List<string> objectPaths = new List<string>();
    
    [Tooltip("Match mode")]
    public MatchMode matchMode = MatchMode.All;
    
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        if (objectPaths == null || objectPaths.Count == 0)
        {
            Debug.LogWarning("[MultipleObjects] No paths specified");
            return false;
        }
        
        int foundCount = 0;
        foreach (var path in objectPaths)
        {
            var obj = UITest.FindByPathOrId(path);
            if (obj != null && obj.activeInHierarchy)
            {
                foundCount++;
            }
        }
        
        bool result = matchMode switch
        {
            MatchMode.All => foundCount == objectPaths.Count,
            MatchMode.Any => foundCount > 0,
            MatchMode.None => foundCount == 0,
            _ => false
        };
        
        if (!result)
        {
            Debug.Log($"[MultipleObjects] Found {foundCount}/{objectPaths.Count}, Mode: {matchMode}");
        }
        
        return result;
    }
}
```

## Advanced Patterns

### Pattern 1: Time-based Condition

```csharp
[CreateAssetMenu(menuName = "UI Tests/Conditions/Time Elapsed")]
public class TimeElapsedConditionSO : TestCondition
{
    [Tooltip("Required elapsed time (seconds)")]
    public float requiredTime = 5f;
    
    private float startTime = -1f;
    
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        if (startTime < 0)
        {
            startTime = Time.unscaledTime;
            Debug.Log($"[TimeElapsed] Starting timer for {requiredTime}s");
        }
        
        float elapsed = Time.unscaledTime - startTime;
        bool ready = elapsed >= requiredTime;
        
        if (!ready)
        {
            Debug.Log($"[TimeElapsed] {elapsed:F1}/{requiredTime}s");
        }
        else if (startTime > 0) // Only log once
        {
            Debug.Log($"[TimeElapsed] Time elapsed: {elapsed:F1}s");
            startTime = -2f; // Prevent logging again
        }
        
        return ready;
    }
}
```

### Pattern 2: Composite Condition

```csharp
[CreateAssetMenu(menuName = "UI Tests/Conditions/Composite Condition")]
public class CompositeConditionSO : TestCondition
{
    public enum LogicMode
    {
        And,  // All must be true
        Or,   // At least one must be true
        Not   // All must be false
    }
    
    [Tooltip("Sub-conditions to evaluate")]
    public List<TestCondition> conditions = new List<TestCondition>();
    
    [Tooltip("Logic mode")]
    public LogicMode logicMode = LogicMode.And;
    
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        if (conditions == null || conditions.Count == 0)
        {
            return true;
        }
        
        int trueCount = 0;
        foreach (var condition in conditions)
        {
            if (condition == null) continue;
            
            if (condition.Evaluate(host, uiTest))
            {
                trueCount++;
            }
        }
        
        return logicMode switch
        {
            LogicMode.And => trueCount == conditions.Count,
            LogicMode.Or => trueCount > 0,
            LogicMode.Not => trueCount == 0,
            _ => false
        };
    }
}
```

### Pattern 3: Cached Condition

```csharp
[CreateAssetMenu(menuName = "UI Tests/Conditions/Cached Lookup")]
public class CachedLookupConditionSO : TestCondition
{
    [Tooltip("Path to object")]
    public string objectPath = "id:MyObject";
    
    [Tooltip("Cache duration (seconds)")]
    public float cacheDuration = 1f;
    
    private GameObject cachedObject;
    private float lastCacheTime = -1f;
    
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        float currentTime = Time.unscaledTime;
        
        // Refresh cache if needed
        if (cachedObject == null || 
            (currentTime - lastCacheTime) > cacheDuration)
        {
            cachedObject = UITest.FindByPathOrId(objectPath);
            lastCacheTime = currentTime;
        }
        
        return cachedObject != null && cachedObject.activeInHierarchy;
    }
}
```

## Best Practices

### 1. Clear Parameters

```csharp
✅ Tốt:
[Tooltip("Path to the status label")]
public string labelPath = "id:StatusLabel";

[Tooltip("Expected status text")]
public string expectedText = "Ready";

❌ Không rõ:
public string path;
public string text;
```

### 2. Informative Logging

```csharp
public override bool Evaluate(MonoBehaviour host, UITest uiTest)
{
    bool result = CheckCondition();
    
    if (!result)
    {
        Debug.Log($"[MyCondition] Not satisfied: {reasonWhy}");
    }
    
    return result;
}
```

### 3. Null Safety

```csharp
var obj = UITest.FindByPathOrId(path);
if (obj == null)
{
    Debug.LogWarning($"[MyCondition] Object not found: {path}");
    return false;
}

var component = obj.GetComponent<MyComponent>();
if (component == null)
{
    Debug.LogWarning($"[MyCondition] Component not found on: {path}");
    return false;
}
```

### 4. Performance

```csharp
// Cache expensive lookups
private GameObject cachedObject;

public override bool Evaluate(MonoBehaviour host, UITest uiTest)
{
    if (cachedObject == null)
    {
        cachedObject = UITest.FindByPathOrId(path);
    }
    
    return cachedObject != null && cachedObject.activeInHierarchy;
}
```

### 5. State Management

```csharp
// Reset state nếu cần
private bool hasLogged = false;

public override bool Evaluate(MonoBehaviour host, UITest uiTest)
{
    bool result = CheckCondition();
    
    if (result && !hasLogged)
    {
        Debug.Log("[MyCondition] Condition satisfied!");
        hasLogged = true;
    }
    
    return result;
}
```

## Using Custom Conditions

### In WaitForConditionStepSO

1. Create WaitForConditionStepSO asset
2. Add custom condition assets vào list `preconditionSOs`
3. Set `preconditionMode` (All hoặc Any)
4. Set `timeout`

### In Code

```csharp
// Create and evaluate directly
var condition = ScriptableObject.CreateInstance<MyConditionSO>();
condition.parameter = "value";

var uiTest = new UITest();
bool result = condition.Evaluate(this, uiTest);

// Or use with WaitFor
yield return uiTest.WaitFor(this, new UITest.BoolCondition(() => 
{
    return condition.Evaluate(this, uiTest);
}));
```

## Testing Custom Conditions

```csharp
// Test trong MonoBehaviour
void TestMyCondition()
{
    var condition = ScriptableObject.CreateInstance<MyConditionSO>();
    condition.targetPath = "id:TestObject";
    
    var uiTest = new UITest();
    
    bool result = condition.Evaluate(this, uiTest);
    
    Debug.Log($"Condition result: {result}");
    Assert.IsTrue(result, "Condition should be true");
}
```

## Debugging Custom Conditions

```csharp
public override bool Evaluate(MonoBehaviour host, UITest uiTest)
{
    Debug.Log($"[{GetType().Name}] Evaluating...");
    Debug.Log($"[{GetType().Name}] Parameters: {JsonUtility.ToJson(this)}");
    
    bool result = false;
    
    try
    {
        result = CheckMyCondition();
        Debug.Log($"[{GetType().Name}] Result: {result}");
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"[{GetType().Name}] Exception: {ex}");
    }
    
    return result;
}
```

## Common Pitfalls

### ❌ Avoid Side Effects

```csharp
// BAD - modifies state
public override bool Evaluate(MonoBehaviour host, UITest uiTest)
{
    counter++; // Side effect!
    return counter > 10;
}

// GOOD - pure check
public override bool Evaluate(MonoBehaviour host, UITest uiTest)
{
    return GameManager.Instance.Counter > 10;
}
```

### ❌ Avoid Heavy Operations

```csharp
// BAD - expensive every evaluation
public override bool Evaluate(MonoBehaviour host, UITest uiTest)
{
    var allObjects = GameObject.FindObjectsOfType<GameObject>(); // Slow!
    return allObjects.Length > 100;
}

// GOOD - efficient check
public override bool Evaluate(MonoBehaviour host, UITest uiTest)
{
    return GameManager.Instance.ObjectCount > 100;
}
```

### ❌ Avoid Inconsistent Results

```csharp
// BAD - random results
public override bool Evaluate(MonoBehaviour host, UITest uiTest)
{
    return Random.value > 0.5f; // Unpredictable!
}

// GOOD - deterministic
public override bool Evaluate(MonoBehaviour host, UITest uiTest)
{
    return GameManager.Instance.IsReady;
}
```

## Examples trong Repository

Xem `SOExamples/` folder:
- `ObjectAppearedConditionSO.cs` - Check object exists

## Xem thêm

- [[Custom Actions]] - Tạo custom actions
- [[Custom Test Steps]] - Tạo custom test steps
- [[Test Steps]] - WaitForConditionStepSO usage
- [[API Reference]] - Framework API
