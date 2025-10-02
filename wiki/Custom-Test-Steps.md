# Tạo Custom Test Steps

Hướng dẫn này sẽ chỉ bạn cách tạo các custom test step types để mở rộng framework với logic tùy chỉnh.

## Khi nào cần Custom Test Steps?

Tạo custom test step khi:
- Logic phức tạp cần nhiều bước liên quan
- Cần tái sử dụng chuỗi actions thường xuyên
- Cần tích hợp với systems bên ngoài
- Built-in steps không đủ cho use case của bạn

## Base Class Overview

```csharp
public abstract class TestStepBase : ScriptableObject
{
    [TextArea]
    public string note;
    
    [Tooltip("Optional per-step timeout in seconds. 0 means use UITest.WaitTimeout.")]
    public float timeoutSeconds = 0f;
    
    public abstract IEnumerator Execute(MonoBehaviour host, UITest uiTest);
    
    public override string ToString()
    {
        return string.IsNullOrEmpty(note) ? name : note;
    }
}
```

## Ví dụ 1: Simple Custom Step

### Tạo Custom Debug Step

```csharp
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Tests/Steps/Debug Log", fileName = "DebugLogStep")]
public class DebugLogStepSO : TestStepBase
{
    [Tooltip("Message to log to console")]
    public string message = "Debug message";
    
    [Tooltip("Log type")]
    public LogType logType = LogType.Log;
    
    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        switch (logType)
        {
            case LogType.Log:
                Debug.Log($"[Test] {message}");
                break;
            case LogType.Warning:
                Debug.LogWarning($"[Test] {message}");
                break;
            case LogType.Error:
                Debug.LogError($"[Test] {message}");
                break;
        }
        
        yield return null;
    }
    
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(note)) return note;
        return $"Debug: {message}";
    }
}
```

**Sử dụng**:
1. `Create → UI Tests → Steps → Debug Log`
2. Set message: "Login flow started"
3. Thêm vào test case để debug

## Ví dụ 2: Login Step

### Tạo Complete Login Flow Step

```csharp
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Tests/Steps/Login Flow", fileName = "LoginFlowStep")]
public class LoginFlowStepSO : TestStepBase
{
    [Header("Credentials")]
    public string username = "testuser@example.com";
    public string password = "password123";
    
    [Header("UI Paths")]
    [Tooltip("Path to username input field")]
    public string usernameFieldPath = "id:UsernameField";
    
    [Tooltip("Path to password input field")]
    public string passwordFieldPath = "id:PasswordField";
    
    [Tooltip("Path to login button")]
    public string loginButtonPath = "id:LoginButton";
    
    [Header("Validation")]
    [Tooltip("Path to welcome label (to verify success)")]
    public string welcomeLabelPath = "id:WelcomeLabel";
    
    [Tooltip("Expected welcome text")]
    public string expectedWelcomeText = "Welcome!";
    
    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        // Step 1: Input username
        Debug.Log($"[LoginFlow] Entering username: {username}");
        yield return uiTest.InputText(host, usernameFieldPath, username);
        
        // Step 2: Input password
        Debug.Log($"[LoginFlow] Entering password");
        yield return uiTest.InputText(host, passwordFieldPath, password);
        
        // Step 3: Click login button
        Debug.Log($"[LoginFlow] Clicking login button");
        yield return uiTest.Press(host, loginButtonPath);
        
        // Step 4: Wait for success
        Debug.Log($"[LoginFlow] Waiting for login success...");
        yield return uiTest.WaitFor(
            host, 
            new UITest.LabelTextAppeared(welcomeLabelPath, expectedWelcomeText)
        );
        
        Debug.Log($"[LoginFlow] Login successful!");
    }
    
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(note)) return note;
        return $"Login as {username}";
    }
}
```

## Ví dụ 3: Screenshot Step

### Tạo Step để chụp màn hình

```csharp
using System.Collections;
using UnityEngine;
using System.IO;

[CreateAssetMenu(menuName = "UI Tests/Steps/Take Screenshot", fileName = "ScreenshotStep")]
public class ScreenshotStepSO : TestStepBase
{
    [Tooltip("Screenshot filename (without extension)")]
    public string filename = "screenshot";
    
    [Tooltip("Screenshot folder relative to project")]
    public string folder = "Screenshots";
    
    [Tooltip("Add timestamp to filename")]
    public bool addTimestamp = true;
    
    [Tooltip("Super size multiplier (1-4)")]
    [Range(1, 4)]
    public int superSize = 1;
    
    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        // Ensure folder exists
        string folderPath = Path.Combine(Application.dataPath, "..", folder);
        Directory.CreateDirectory(folderPath);
        
        // Build filename
        string finalFilename = filename;
        if (addTimestamp)
        {
            finalFilename += $"_{System.DateTime.Now:yyyyMMdd_HHmmss}";
        }
        finalFilename += ".png";
        
        string fullPath = Path.Combine(folderPath, finalFilename);
        
        // Take screenshot
        Debug.Log($"[Screenshot] Capturing to: {fullPath}");
        ScreenCapture.CaptureScreenshot(fullPath, superSize);
        
        // Wait a frame for capture to complete
        yield return null;
        
        Debug.Log($"[Screenshot] Saved to: {fullPath}");
    }
    
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(note)) return note;
        return $"Screenshot: {filename}";
    }
}
```

## Ví dụ 4: Wait for Manager State

### Tạo Step chờ game manager state

```csharp
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Tests/Steps/Wait For Game State", fileName = "WaitForGameStateStep")]
public class WaitForGameStateStepSO : TestStepBase
{
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        GameOver
    }
    
    [Tooltip("Game state to wait for")]
    public GameState expectedState = GameState.Playing;
    
    [Tooltip("Check interval in seconds")]
    public float checkInterval = 0.5f;
    
    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        Debug.Log($"[WaitForGameState] Waiting for state: {expectedState}");
        
        float elapsed = 0f;
        float timeout = timeoutSeconds > 0 ? timeoutSeconds : uiTest.WaitTimeout;
        
        while (elapsed < timeout)
        {
            // Check if GameManager exists and has expected state
            var gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.CurrentState == (GameManager.State)expectedState)
            {
                Debug.Log($"[WaitForGameState] State reached: {expectedState}");
                yield break;
            }
            
            // Wait before next check
            yield return new WaitForSecondsRealtime(checkInterval);
            elapsed += checkInterval;
        }
        
        // Timeout
        throw new System.TimeoutException(
            $"Game state did not reach {expectedState} within {timeout} seconds"
        );
    }
    
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(note)) return note;
        return $"Wait for state: {expectedState}";
    }
}
```

## Ví dụ 5: Network Request Step

### Tạo Step để test API calls

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[CreateAssetMenu(menuName = "UI Tests/Steps/API Request", fileName = "APIRequestStep")]
public class APIRequestStepSO : TestStepBase
{
    [Tooltip("API endpoint URL")]
    public string url = "https://api.example.com/test";
    
    [Tooltip("HTTP method")]
    public enum Method { GET, POST, PUT, DELETE }
    public Method method = Method.GET;
    
    [Tooltip("Request body (for POST/PUT)")]
    [TextArea]
    public string body = "";
    
    [Tooltip("Expected response code")]
    public int expectedStatusCode = 200;
    
    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        Debug.Log($"[APIRequest] {method} {url}");
        
        UnityWebRequest request;
        
        switch (method)
        {
            case Method.GET:
                request = UnityWebRequest.Get(url);
                break;
            case Method.POST:
                request = UnityWebRequest.Post(url, body);
                break;
            case Method.PUT:
                request = UnityWebRequest.Put(url, body);
                break;
            case Method.DELETE:
                request = UnityWebRequest.Delete(url);
                break;
            default:
                request = UnityWebRequest.Get(url);
                break;
        }
        
        // Send request
        yield return request.SendWebRequest();
        
        // Check result
        if (request.result != UnityWebRequest.Result.Success)
        {
            throw new System.Exception(
                $"API request failed: {request.error}"
            );
        }
        
        // Check status code
        if (request.responseCode != expectedStatusCode)
        {
            throw new System.Exception(
                $"Unexpected status code: {request.responseCode} (expected {expectedStatusCode})"
            );
        }
        
        Debug.Log($"[APIRequest] Success! Response: {request.downloadHandler.text}");
        
        request.Dispose();
    }
    
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(note)) return note;
        return $"{method} {url}";
    }
}
```

## Advanced Patterns

### Pattern 1: Parameterized Step

```csharp
[CreateAssetMenu(menuName = "UI Tests/Steps/Purchase Item")]
public class PurchaseItemStepSO : TestStepBase
{
    [System.Serializable]
    public class ItemData
    {
        public string itemId;
        public int quantity;
        public int expectedCost;
    }
    
    public ItemData item;
    
    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        // Navigate to shop
        yield return uiTest.Press(host, "id:ShopButton");
        
        // Wait for shop to load
        yield return uiTest.WaitFor(host, new UITest.ObjectAppeared("id:ShopPanel"));
        
        // Find item
        string itemPath = $"id:Item_{item.itemId}";
        yield return uiTest.Press(host, itemPath);
        
        // Verify price
        string pricePath = $"{itemPath}/PriceLabel";
        yield return uiTest.AssertLabel(host, pricePath, item.expectedCost.ToString());
        
        // Purchase
        yield return uiTest.Press(host, "id:BuyButton");
        
        // Confirm
        yield return uiTest.Press(host, "id:ConfirmButton");
        
        Debug.Log($"[Purchase] Bought {item.quantity}x {item.itemId}");
    }
}
```

### Pattern 2: Conditional Execution

```csharp
[CreateAssetMenu(menuName = "UI Tests/Steps/Conditional Action")]
public class ConditionalActionStepSO : TestStepBase
{
    [Tooltip("Path to check for existence")]
    public string conditionPath = "id:Tutorial";
    
    [Tooltip("Action to perform if condition is true")]
    public TestActionSO actionIfTrue;
    
    [Tooltip("Action to perform if condition is false")]
    public TestActionSO actionIfFalse;
    
    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        // Check condition
        var obj = UITest.FindByPathOrId(conditionPath);
        bool conditionMet = (obj != null && obj.activeInHierarchy);
        
        // Execute appropriate action
        if (conditionMet && actionIfTrue != null)
        {
            Debug.Log($"[Conditional] Condition true, executing actionIfTrue");
            yield return actionIfTrue.Run(host, uiTest);
        }
        else if (!conditionMet && actionIfFalse != null)
        {
            Debug.Log($"[Conditional] Condition false, executing actionIfFalse");
            yield return actionIfFalse.Run(host, uiTest);
        }
    }
}
```

### Pattern 3: Retry Logic

```csharp
[CreateAssetMenu(menuName = "UI Tests/Steps/Retry Action")]
public class RetryActionStepSO : TestStepBase
{
    [Tooltip("Action to retry")]
    public TestActionSO action;
    
    [Tooltip("Maximum retry attempts")]
    public int maxRetries = 3;
    
    [Tooltip("Delay between retries (seconds)")]
    public float retryDelay = 1f;
    
    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            Debug.Log($"[Retry] Attempt {attempt + 1}/{maxRetries}");
            
            bool success = true;
            
            // Try to execute action
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
                    Debug.LogWarning($"[Retry] Attempt failed: {ex.Message}");
                    success = false;
                    break;
                }
                
                if (!moveNext) break;
                yield return current;
            }
            
            if (success)
            {
                Debug.Log($"[Retry] Success on attempt {attempt + 1}");
                yield break;
            }
            
            // Wait before retry (if not last attempt)
            if (attempt < maxRetries - 1)
            {
                yield return new WaitForSecondsRealtime(retryDelay);
            }
        }
        
        // All attempts failed
        throw new System.Exception($"Action failed after {maxRetries} attempts");
    }
}
```

## Best Practices for Custom Steps

### 1. Clear Parameters
```csharp
✅ Tốt:
[Tooltip("Path to the username input field")]
public string usernameFieldPath = "id:UsernameField";

❌ Không rõ:
public string path;
```

### 2. Logging
```csharp
Debug.Log($"[StepName] Starting action");
Debug.Log($"[StepName] Current state: {state}");
Debug.Log($"[StepName] Completed successfully");
```

### 3. Error Handling
```csharp
if (criticalObject == null)
{
    throw new System.Exception(
        $"Critical object not found: {objectPath}"
    );
}
```

### 4. Timeout Respect
```csharp
float timeout = timeoutSeconds > 0 ? timeoutSeconds : uiTest.WaitTimeout;
float elapsed = 0f;

while (elapsed < timeout && !conditionMet)
{
    yield return null;
    elapsed += Time.unscaledDeltaTime;
}

if (!conditionMet)
{
    throw new System.TimeoutException("Operation timed out");
}
```

### 5. ToString Override
```csharp
public override string ToString()
{
    if (!string.IsNullOrEmpty(note)) return note;
    return $"MyStep: {parameter1} → {parameter2}";
}
```

## Testing Custom Steps

```csharp
// Tạo test case để verify custom step
IEnumerator TestCustomStep()
{
    var step = ScriptableObject.CreateInstance<MyCustomStepSO>();
    step.parameter = "test value";
    step.timeoutSeconds = 5f;
    
    var uiTest = new UITest();
    
    yield return StartCoroutine(step.Execute(this, uiTest));
    
    // Verify results
    Assert.IsTrue(expectedCondition, "Step should complete successfully");
}
```

## Xem thêm

- [[Custom Actions]] - Tạo custom actions cho ActionStepSO
- [[Custom Conditions]] - Tạo custom conditions cho WaitForConditionStepSO
- [[Test Steps]] - Built-in step types
- [[API Reference]] - Chi tiết API
