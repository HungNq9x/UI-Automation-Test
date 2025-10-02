# Tạo Custom Actions

Hướng dẫn tạo custom actions để mở rộng ActionStepSO với logic tùy chỉnh.

## Tổng quan

Custom Actions cho phép bạn:
- Tạo reusable action logic
- Sử dụng trong ActionStepSO thay vì inline actions
- Đóng gói complex operations
- Tích hợp với external systems

## Base Class

```csharp
public abstract class TestActionSO : ScriptableObject
{
    public abstract IEnumerator Run(MonoBehaviour host, UITest uiTest);
}
```

Tên class sử dụng `TestActionSO` là base class trong framework (có thể đặt tên khác tùy implementation).

## Ví dụ cơ bản

### Ví dụ 1: Debug Log Action

```csharp
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Tests/Actions/Debug Log", fileName = "DebugLogAction")]
public class DebugLogActionSO : TestActionSO
{
    [Tooltip("Message to log")]
    public string message = "Debug message";
    
    [Tooltip("Log type")]
    public LogType logType = LogType.Log;
    
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        switch (logType)
        {
            case LogType.Log:
                Debug.Log($"[CustomAction] {message}");
                break;
            case LogType.Warning:
                Debug.LogWarning($"[CustomAction] {message}");
                break;
            case LogType.Error:
                Debug.LogError($"[CustomAction] {message}");
                break;
        }
        
        yield return null;
    }
}
```

**Sử dụng**:
1. Create asset: `Create → UI Tests → Actions → Debug Log`
2. Set message và logType
3. Trong ActionStepSO, gán vào field `customActionSO`

### Ví dụ 2: Press Button Action

```csharp
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Tests/Actions/Press Button", fileName = "PressAction")]
public class PressActionSO : TestActionSO
{
    [Tooltip("Path or ObjectID of button to press")]
    public string buttonPath = "id:MyButton";
    
    [Tooltip("Wait time after press (seconds)")]
    public float waitAfterPress = 0.5f;
    
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        Debug.Log($"[PressAction] Pressing button: {buttonPath}");
        
        // Press the button
        yield return uiTest.Press(host, buttonPath);
        
        // Wait if configured
        if (waitAfterPress > 0)
        {
            Debug.Log($"[PressAction] Waiting {waitAfterPress}s after press");
            yield return new WaitForSecondsRealtime(waitAfterPress);
        }
        
        Debug.Log($"[PressAction] Completed");
    }
}
```

## Ví dụ nâng cao

### Ví dụ 3: Login Sequence Action

```csharp
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Tests/Actions/Login Sequence", fileName = "LoginAction")]
public class LoginActionSO : TestActionSO
{
    [Header("Credentials")]
    public string username = "testuser@example.com";
    public string password = "password123";
    
    [Header("UI Paths")]
    public string usernameFieldPath = "id:UsernameField";
    public string passwordFieldPath = "id:PasswordField";
    public string loginButtonPath = "id:LoginButton";
    
    [Header("Options")]
    [Tooltip("Wait for login to complete")]
    public bool waitForSuccess = true;
    public string successIndicatorPath = "id:HomeScreen";
    public float successTimeout = 10f;
    
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        Debug.Log($"[LoginAction] Starting login for: {username}");
        
        // Input username
        yield return uiTest.InputText(host, usernameFieldPath, username);
        yield return new WaitForSecondsRealtime(0.2f);
        
        // Input password
        yield return uiTest.InputText(host, passwordFieldPath, password);
        yield return new WaitForSecondsRealtime(0.2f);
        
        // Click login
        yield return uiTest.Press(host, loginButtonPath);
        
        // Wait for success if configured
        if (waitForSuccess)
        {
            Debug.Log($"[LoginAction] Waiting for success indicator: {successIndicatorPath}");
            
            float prevTimeout = uiTest.WaitTimeout;
            uiTest.WaitTimeout = successTimeout;
            
            yield return uiTest.WaitFor(
                host,
                new UITest.ObjectAppeared(successIndicatorPath)
            );
            
            uiTest.WaitTimeout = prevTimeout;
        }
        
        Debug.Log($"[LoginAction] Login completed successfully");
    }
}
```

### Ví dụ 4: Navigation Action

```csharp
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Tests/Actions/Navigate To", fileName = "NavigateAction")]
public class NavigateToActionSO : TestActionSO
{
    public enum Destination
    {
        MainMenu,
        Settings,
        Shop,
        Inventory,
        Profile
    }
    
    [Tooltip("Destination to navigate to")]
    public Destination destination = Destination.MainMenu;
    
    [Tooltip("Start from home screen")]
    public bool fromHome = true;
    
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        Debug.Log($"[Navigate] Going to: {destination}");
        
        // Go home first if configured
        if (fromHome)
        {
            yield return GoHome(host, uiTest);
        }
        
        // Navigate to destination
        switch (destination)
        {
            case Destination.MainMenu:
                yield return uiTest.Press(host, "id:MainMenuButton");
                break;
            case Destination.Settings:
                yield return uiTest.Press(host, "id:SettingsButton");
                break;
            case Destination.Shop:
                yield return uiTest.Press(host, "id:ShopButton");
                break;
            case Destination.Inventory:
                yield return uiTest.Press(host, "id:InventoryButton");
                break;
            case Destination.Profile:
                yield return uiTest.Press(host, "id:ProfileButton");
                break;
        }
        
        // Wait for destination to load
        yield return new WaitForSecondsRealtime(0.5f);
        
        Debug.Log($"[Navigate] Arrived at: {destination}");
    }
    
    private IEnumerator GoHome(MonoBehaviour host, UITest uiTest)
    {
        Debug.Log("[Navigate] Going to home first");
        yield return uiTest.Press(host, "id:HomeButton");
        yield return new WaitForSecondsRealtime(0.5f);
    }
}
```

### Ví dụ 5: Wait for Network Action

```csharp
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Tests/Actions/Wait For Network", fileName = "WaitNetworkAction")]
public class WaitForNetworkActionSO : TestActionSO
{
    [Tooltip("Maximum time to wait for connection (seconds)")]
    public float timeout = 30f;
    
    [Tooltip("Check interval (seconds)")]
    public float checkInterval = 1f;
    
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        Debug.Log("[WaitNetwork] Waiting for network connection...");
        
        float elapsed = 0f;
        
        while (elapsed < timeout)
        {
            // Check if network is connected
            // Replace with your actual network manager check
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                Debug.Log("[WaitNetwork] Network connected!");
                yield break;
            }
            
            // Wait before next check
            yield return new WaitForSecondsRealtime(checkInterval);
            elapsed += checkInterval;
            
            Debug.Log($"[WaitNetwork] Still waiting... ({elapsed:F1}/{timeout}s)");
        }
        
        // Timeout
        throw new System.TimeoutException(
            $"Network connection not established within {timeout} seconds"
        );
    }
}
```

### Ví dụ 6: Screenshot Action

```csharp
using System.Collections;
using UnityEngine;
using System.IO;

[CreateAssetMenu(menuName = "UI Tests/Actions/Take Screenshot", fileName = "ScreenshotAction")]
public class ScreenshotActionSO : TestActionSO
{
    [Tooltip("Screenshot filename prefix")]
    public string filenamePrefix = "test_screenshot";
    
    [Tooltip("Save location relative to project")]
    public string savePath = "Screenshots";
    
    [Tooltip("Add timestamp to filename")]
    public bool addTimestamp = true;
    
    [Tooltip("Super size factor")]
    [Range(1, 4)]
    public int superSize = 1;
    
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        // Create directory if needed
        string fullPath = Path.Combine(Application.dataPath, "..", savePath);
        Directory.CreateDirectory(fullPath);
        
        // Build filename
        string filename = filenamePrefix;
        if (addTimestamp)
        {
            filename += $"_{System.DateTime.Now:yyyyMMdd_HHmmss}";
        }
        filename += ".png";
        
        string filePath = Path.Combine(fullPath, filename);
        
        Debug.Log($"[Screenshot] Capturing to: {filePath}");
        
        // Capture
        ScreenCapture.CaptureScreenshot(filePath, superSize);
        
        // Wait for capture to complete
        yield return null;
        yield return null;
        
        Debug.Log($"[Screenshot] Saved: {filePath}");
    }
}
```

## Advanced Patterns

### Pattern 1: Chained Actions

```csharp
[CreateAssetMenu(menuName = "UI Tests/Actions/Action Chain")]
public class ActionChainSO : TestActionSO
{
    [Tooltip("Actions to execute in sequence")]
    public List<TestActionSO> actions = new List<TestActionSO>();
    
    [Tooltip("Delay between actions (seconds)")]
    public float delayBetweenActions = 0.1f;
    
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        Debug.Log($"[ActionChain] Running {actions.Count} actions");
        
        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i] == null)
            {
                Debug.LogWarning($"[ActionChain] Action {i} is null, skipping");
                continue;
            }
            
            Debug.Log($"[ActionChain] Executing action {i + 1}/{actions.Count}: {actions[i].name}");
            yield return actions[i].Run(host, uiTest);
            
            if (delayBetweenActions > 0)
            {
                yield return new WaitForSecondsRealtime(delayBetweenActions);
            }
        }
        
        Debug.Log("[ActionChain] All actions completed");
    }
}
```

### Pattern 2: Conditional Action

```csharp
[CreateAssetMenu(menuName = "UI Tests/Actions/Conditional Action")]
public class ConditionalActionSO : TestActionSO
{
    [Tooltip("Condition to check")]
    public TestCondition condition;
    
    [Tooltip("Action if condition is true")]
    public TestActionSO actionIfTrue;
    
    [Tooltip("Action if condition is false")]
    public TestActionSO actionIfFalse;
    
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        bool conditionMet = false;
        
        if (condition != null)
        {
            conditionMet = condition.Evaluate(host, uiTest);
            Debug.Log($"[ConditionalAction] Condition result: {conditionMet}");
        }
        
        if (conditionMet && actionIfTrue != null)
        {
            Debug.Log("[ConditionalAction] Running actionIfTrue");
            yield return actionIfTrue.Run(host, uiTest);
        }
        else if (!conditionMet && actionIfFalse != null)
        {
            Debug.Log("[ConditionalAction] Running actionIfFalse");
            yield return actionIfFalse.Run(host, uiTest);
        }
    }
}
```

### Pattern 3: Retry Action

```csharp
[CreateAssetMenu(menuName = "UI Tests/Actions/Retry Action")]
public class RetryActionSO : TestActionSO
{
    [Tooltip("Action to retry")]
    public TestActionSO action;
    
    [Tooltip("Maximum number of attempts")]
    public int maxAttempts = 3;
    
    [Tooltip("Delay between attempts (seconds)")]
    public float retryDelay = 1f;
    
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        if (action == null)
        {
            Debug.LogError("[RetryAction] No action specified");
            yield break;
        }
        
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            Debug.Log($"[RetryAction] Attempt {attempt}/{maxAttempts}");
            
            bool success = true;
            
            var enumerator = action.Run(host, uiTest);
            while (true)
            {
                object current = null;
                bool moveNext;
                
                try
                {
                    moveNext = enumerator.MoveNext();
                    if (moveNext) current = enumerator.Current;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[RetryAction] Attempt {attempt} failed: {ex.Message}");
                    success = false;
                    break;
                }
                
                if (!moveNext) break;
                yield return current;
            }
            
            if (success)
            {
                Debug.Log($"[RetryAction] Success on attempt {attempt}");
                yield break;
            }
            
            // Wait before retry (if not last attempt)
            if (attempt < maxAttempts)
            {
                Debug.Log($"[RetryAction] Waiting {retryDelay}s before retry...");
                yield return new WaitForSecondsRealtime(retryDelay);
            }
        }
        
        // All attempts failed
        throw new System.Exception(
            $"Action failed after {maxAttempts} attempts"
        );
    }
}
```

## Best Practices

### 1. Clear Parameters

```csharp
✅ Tốt:
[Tooltip("Path to the target button")]
public string buttonPath = "id:MyButton";

[Tooltip("Wait time after action (seconds)")]
public float waitTime = 0.5f;

❌ Không rõ:
public string path;
public float time;
```

### 2. Logging

```csharp
public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
{
    Debug.Log($"[MyAction] Starting with parameter: {myParam}");
    
    // Do work
    yield return DoSomething();
    
    Debug.Log($"[MyAction] Completed successfully");
}
```

### 3. Error Handling

```csharp
if (criticalParameter == null)
{
    throw new System.ArgumentNullException(
        nameof(criticalParameter),
        "Critical parameter must be set"
    );
}
```

### 4. Yield Properly

```csharp
// Yield UITest operations
yield return uiTest.Press(host, path);

// Yield wait operations
yield return new WaitForSecondsRealtime(delay);

// Yield at least once
yield return null;
```

### 5. Reusability

```csharp
// Make actions configurable
public string targetPath = "id:DefaultTarget";
public float timeout = 5f;
public bool waitForComplete = true;

// Instead of hardcoding
// const string TARGET = "id:SpecificThing";
```

## Using Custom Actions

### In ActionStepSO

1. Create ActionStepSO asset
2. Leave `actionType` as default (hoặc bất kỳ)
3. Gán custom action vào field `customActionSO`
4. Custom action sẽ override inline action

### In Code

```csharp
// Create and run directly
var myAction = ScriptableObject.CreateInstance<MyCustomActionSO>();
myAction.parameter = "value";

var uiTest = new UITest();
yield return StartCoroutine(myAction.Run(this, uiTest));
```

## Testing Custom Actions

```csharp
// Test trong MonoBehaviour
IEnumerator TestMyAction()
{
    var action = ScriptableObject.CreateInstance<MyActionSO>();
    action.targetPath = "id:TestButton";
    
    var uiTest = new UITest();
    
    yield return StartCoroutine(action.Run(this, uiTest));
    
    // Verify results
    Assert.IsTrue(expectedCondition);
}
```

## Debugging Custom Actions

```csharp
public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
{
    Debug.Log($"[{GetType().Name}] Started");
    Debug.Log($"[{GetType().Name}] Parameters: {JsonUtility.ToJson(this)}");
    
    try
    {
        // Your logic
        yield return DoWork(host, uiTest);
        
        Debug.Log($"[{GetType().Name}] Completed successfully");
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"[{GetType().Name}] Failed: {ex}");
        throw;
    }
}
```

## Examples trong Repository

Xem `SOExamples/` folder:
- `DebugLogActionSO.cs` - Simple logging action
- `PressActionSO.cs` - Button press with options

## Xem thêm

- [[Custom Conditions]] - Tạo custom conditions
- [[Custom Test Steps]] - Tạo custom test steps
- [[Test Steps]] - Built-in steps
- [[API Reference]] - Framework API
