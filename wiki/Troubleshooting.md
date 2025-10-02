# Troubleshooting - Khắc phục sự cố

Hướng dẫn này giúp bạn giải quyết các vấn đề thường gặp khi sử dụng UI Test Framework.

## Table of Contents

- [Test không chạy](#test-không-chạy)
- [Step Timeout Issues](#step-timeout-issues)
- [GameObject không tìm thấy](#gameobject-không-tìm-thấy)
- [ObjectID Lookup Issues](#objectid-lookup-issues)
- [Action không có hiệu ứng](#action-không-có-hiệu-ứng)
- [Condition không bao giờ satisfied](#condition-không-bao-giờ-satisfied)
- [UITestRunner Issues](#uitestrunner-issues)
- [Editor Window Issues](#editor-window-issues)
- [Scene Loading Issues](#scene-loading-issues)
- [Performance Issues](#performance-issues)
- [Odin Inspector Issues](#odin-inspector-issues)

---

## Test không chạy

### Triệu chứng
- Click "Run Test" nhưng không có gì xảy ra
- Console không có log messages
- UITestRunner không được tạo

### Nguyên nhân có thể

#### 1. Test Case rỗng hoặc null
**Kiểm tra**:
```
- Test case asset có null không?
- Test case có steps không?
- Các step assets có missing references không?
```

**Giải pháp**:
- Verify test case asset tồn tại
- Đảm bảo list steps không rỗng
- Check console cho missing asset warnings

#### 2. Play Mode không khởi động
**Kiểm tra**: Console có lỗi compile không?

**Giải pháp**:
- Fix tất cả compile errors
- Restart Unity Editor
- Check Domain Reload settings

#### 3. UITestRunner không được tạo
**Kiểm tra**: Scene có UITestRunner GameObject không?

**Giải pháp**:
```csharp
// Manual create trong code
var runnerGO = new GameObject("UITestRunner");
runnerGO.AddComponent<UITestRunner>();
UITestRunner.SetTestCase(yourTestCase);
```

---

## Step Timeout Issues

### Triệu chứng
- Step thất bại với message "Step timed out after X seconds"
- Test chờ quá lâu rồi fail

### Nguyên nhân có thể

#### 1. Timeout quá ngắn
**Giải pháp**:
```
// Tăng timeout trên step
timeoutSeconds = 15  // Thay vì 5

// Hoặc tăng default timeout
var uiTest = new UITest();
uiTest.WaitTimeout = 30f;
```

#### 2. Condition không bao giờ true
**Debug**:
- Thêm Debug.Log trong condition logic
- Check object có tồn tại không
- Verify expected values

#### 3. Scene load chậm
**Giải pháp**:
```
// Scene loading steps cần timeout dài
LoadScene step:
  timeoutSeconds = 20-30
```

#### 4. Network operations
**Giải pháp**:
```
// Network requests cần timeout cao
timeoutSeconds = 30-60

// Hoặc implement retry logic
```

### Debug Timeout Issues

```csharp
// Thêm logging để track progress
public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
{
    Debug.Log($"[Step] Starting at {Time.time}");
    
    // Your step logic
    yield return SomeOperation();
    
    Debug.Log($"[Step] Completed at {Time.time}");
}
```

---

## GameObject không tìm thấy

### Triệu chứng
- "Đối tượng không tồn tại" error
- Step fail với "GameObject not found"
- Null reference exceptions

### Nguyên nhân có thể

#### 1. Hierarchy path sai
**Kiểm tra**:
- Path có case-sensitive chính xác không?
- GameObject có đúng parent không?
- Có typo trong path không?

**Giải pháp**:
```
✅ Chính xác:
"Canvas/MainMenu/PlayButton"

❌ Sai:
"Canvas/mainmenu/PlayButton"  // Lowercase 'm'
"Canvas/MainMenu/Play Button"  // Space
"MainMenu/PlayButton"          // Missing Canvas
```

#### 2. GameObject chưa active
**Kiểm tra**:
```csharp
var obj = GameObject.Find("MyObject");
Debug.Log($"Object exists: {obj != null}");
Debug.Log($"Object active: {obj?.activeInHierarchy}");
```

**Giải pháp**:
- Wait for object to become active trước
- Use WaitForCondition với ObjectAppeared

#### 3. GameObject chưa được instantiate
**Kiểm tra**: Object có được tạo trong runtime không?

**Giải pháp**:
- Thêm wait step trước khi access object
- Check scene có load đầy đủ không

#### 4. Wrong scene
**Kiểm tra**: Đúng scene đang active không?

**Giải pháp**:
```
// Load scene trước
LoadScene: "MenuScene"
WaitTime: 1s
// Then access objects
```

---

## ObjectID Lookup Issues

### Triệu chứng
- Path dạng "id:MyId" không tìm thấy object
- Object exists nhưng lookup fail

### Nguyên nhân có thể

#### 1. ObjectID component thiếu
**Kiểm tra**: GameObject có component `ObjectID` không?

**Giải pháp**:
1. Select GameObject trong Hierarchy
2. Add Component → ObjectID
3. Set Id field

#### 2. Id không khớp
**Kiểm tra**: Id trong component khớp với path không?

```
❌ Sai:
Component Id: "PlayButton"
Path: "id:playbutton"  // Case khác

✅ Đúng:
Component Id: "PlayButton"
Path: "id:PlayButton"  // Exact match
```

#### 3. Prefix không đúng
**Kiểm tra**: Có dùng prefix "id:" không?

```
❌ Sai:
path = "PlayButton"

✅ Đúng:
path = "id:PlayButton"
```

#### 4. Duplicate Ids
**Kiểm tra**: Nhiều objects có cùng Id?

**Giải pháp**:
- Đảm bảo mỗi Id là unique
- Framework sẽ return object đầu tiên tìm thấy

---

## Action không có hiệu ứng

### Triệu chứng
- Press action không trigger button
- InputText không thay đổi text
- Action complete nhưng UI không phản hồi

### Nguyên nhân có thể

#### 1. Element không interactable
**Kiểm tra**:
```csharp
var button = obj.GetComponent<Button>();
Debug.Log($"Interactable: {button?.interactable}");
Debug.Log($"Active: {button?.gameObject.activeInHierarchy}");
```

**Giải pháp**:
- Wait for element to become interactable
- Use WaitForCondition với SelectableInteractable

#### 2. EventSystem không active
**Kiểm tra**: Scene có EventSystem không?

**Giải pháp**:
```csharp
// Check for EventSystem
var eventSystem = GameObject.FindObjectOfType<EventSystem>();
if (eventSystem == null)
{
    Debug.LogError("No EventSystem in scene!");
}
```

#### 3. Button listeners chưa setup
**Kiểm tra**: Button có onClick listeners không?

**Giải pháp**:
- Verify button có hooked up methods
- Check listeners not null

#### 4. Canvas không setup đúng
**Kiểm tra**: Canvas có GraphicRaycaster không?

**Giải pháp**:
- Đảm bảo Canvas có GraphicRaycaster component
- Check Canvas render mode

---

## Condition không bao giờ satisfied

### Triệu chứng
- WaitForCondition timeout
- Condition seems correct nhưng không pass

### Debug Steps

#### 1. Add Debug Logging

```csharp
public override bool Evaluate(MonoBehaviour host, UITest uiTest)
{
    var obj = UITest.FindByPathOrId(path);
    Debug.Log($"[Condition] Object found: {obj != null}");
    
    if (obj == null) return false;
    
    Debug.Log($"[Condition] Object active: {obj.activeInHierarchy}");
    
    var text = obj.GetComponent<Text>();
    Debug.Log($"[Condition] Text component: {text != null}");
    Debug.Log($"[Condition] Current text: '{text?.text}'");
    Debug.Log($"[Condition] Expected text: '{expectedText}'");
    
    return text != null && text.text == expectedText;
}
```

#### 2. Check Timing

```
// Object có thể chưa ready khi condition check
// Thêm wait trước WaitForCondition:

WaitTime: 0.5s
WaitForCondition: [your condition]
```

#### 3. Verify Exact Match

```csharp
// Text có thể có whitespace
Debug.Log($"Length: {text.text.Length}");
Debug.Log($"Bytes: {System.Text.Encoding.UTF8.GetBytes(text.text).Length}");

// So sánh sau khi trim
return text.text.Trim() == expectedText.Trim();
```

---

## UITestRunner Issues

### UITestRunner không tự xóa

**Triệu chứng**: UITestRunner GameObject còn lại sau test

**Nguyên nhân**:
- Test bị treo trong một step
- Exception không được caught
- Coroutine không hoàn thành

**Giải pháp**:
```csharp
// Manual cleanup
var runner = GameObject.FindObjectOfType<UITestRunner>();
if (runner != null)
{
    Destroy(runner.gameObject);
}
```

### Multiple UITestRunner instances

**Triệu chứng**: Nhiều UITestRunner trong scene

**Giải pháp**: UITestRunner sử dụng singleton pattern, nhưng nếu vẫn có duplicates:
```csharp
// Check Awake() logic in UITestRunner.cs
// Đảm bảo chỉ có một instance
```

---

## Editor Window Issues

### Window không update

**Triệu chứng**: Step colors không thay đổi, progress không hiển thị

**Giải pháp**:
1. Close và reopen window: `Tools → UI Test Runner`
2. Check console cho errors
3. Restart Unity Editor

### Window bị lag

**Triệu chứng**: Window chậm, freeze

**Giải pháp**:
```csharp
// Reduce polling frequency trong OnEditorUpdate()
// Modify UITestRunnerWindow.cs nếu cần
```

---

## Scene Loading Issues

### Scene không load

**Triệu chứng**: LoadScene timeout hoặc không có gì xảy ra

**Kiểm tra**:
1. Scene có trong Build Settings không?
2. Scene name đúng không? (case-sensitive)
3. Scene có errors không?

**Giải pháp**:
```
File → Build Settings → Add Open Scenes

// Hoặc use build index
LoadScene: "0"  // Load scene đầu tiên
```

### Scene load nhưng objects missing

**Nguyên nhân**: Scene chưa fully loaded

**Giải pháp**:
```
LoadScene: "GameScene"
WaitTime: 2s  // Wait for scene to settle
// Then continue test
```

---

## Performance Issues

### Tests chạy chậm

**Nguyên nhân**:
- WaitIntervalFrames quá nhỏ
- Quá nhiều polling
- Heavy operations trong conditions

**Giải pháp**:
```csharp
// Tăng interval
uiTest.WaitIntervalFrames = 30;  // Check every 30 frames

// Cache expensive lookups
private GameObject cachedObject;
public override bool Evaluate(MonoBehaviour host, UITest uiTest)
{
    if (cachedObject == null)
        cachedObject = UITest.FindByPathOrId(path);
    return cachedObject != null && cachedObject.activeInHierarchy;
}
```

### Memory leaks

**Triệu chứng**: Memory tăng theo thời gian

**Giải pháp**:
- Đảm bảo test cleanup properly
- Destroy test objects after use
- Unload unused scenes

---

## Odin Inspector Issues

### MissingMethodException

**Error message**: `MissingMethodException: Method not found`

**Giải pháp**:
1. Update Odin Inspector plugin
2. Disable automatic height adjustment:
   - Odin → Preferences → Inspector
   - Uncheck "Automatic Height Adjustment"
3. Comment out Odin attributes nếu cần:
```csharp
//[EnumToggleButtons]
//[ShowIf("condition")]
```

### Attributes không hoạt động

**Giải pháp**:
- Import Odin Inspector properly
- Check Odin version compatibility với Unity version
- Restart Unity after import

---

## Common Error Messages

### "Đối tượng label X không tồn tại"

**Nguyên nhân**: Label GameObject không tìm thấy

**Fix**: Verify path và đảm bảo object tồn tại trong scene

### "Label X văn bản mong đợi: Y, thực tế: Z"

**Nguyên nhân**: Text không khớp

**Fix**: 
- Check expected text spelling
- Verify text không có extra whitespace
- Consider using Contains() thay vì exact match

### "Step timed out after X seconds"

**Nguyên nhân**: Operation không hoàn thành trong timeout

**Fix**: Tăng timeoutSeconds hoặc fix logic causing hang

### "TimeoutException"

**Nguyên nhân**: WaitFor condition không satisfied

**Fix**: Debug condition logic, verify objects exist

---

## Debug Tools

### Enable Verbose Logging

```csharp
// Trong test setup
public class DebugTestRunner : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== Test Starting ===");
        Debug.Log($"Scene: {SceneManager.GetActiveScene().name}");
        Debug.Log($"Time: {System.DateTime.Now}");
    }
}
```

### Screenshot on Failure

```csharp
// Add to test case
try
{
    yield return testCase.Run(this, uiTest);
}
catch (Exception ex)
{
    ScreenCapture.CaptureScreenshot($"failure_{Time.time}.png");
    Debug.LogError($"Test failed: {ex}");
    throw;
}
```

### Step-by-Step Debugging

```
// Thêm WaitTime steps để pause và inspect
Step 1: Action
Step 2: WaitTime (5s)  ← Pause để inspect UI
Step 3: Continue
```

---

## Getting Help

Nếu vẫn gặp vấn đề:

1. **Check Console**: Đọc kỹ error messages và stack traces
2. **Simplify Test**: Tạo minimal test case để reproduce issue
3. **Verify Setup**: Đảm bảo framework được import đúng
4. **Check Examples**: Xem demo tests trong `Test Demo/`
5. **Debug Step by Step**: Isolate which step causes issue
6. **Review Logs**: Check Unity Editor log file

---

## Xem thêm

- [[Getting Started]] - Setup và basic usage
- [[Test Steps]] - Chi tiết về test steps
- [[Best Practices]] - Tips tránh common issues
- [[API Reference]] - Technical documentation
