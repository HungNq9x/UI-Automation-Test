# Best Practices - Thực hành tốt nhất

Document này chứa các best practices và recommendations khi sử dụng UI Test Framework.

## 1. Object Lookup Strategy

### ✅ Ưu tiên ObjectID thay vì Hierarchy Path

**Tại sao**: Hierarchy path dễ vỡ khi UI được refactor hoặc di chuyển.

```
❌ Tránh:
path = "Canvas/MainPanel/TopSection/ButtonRow/PlayButton"

✅ Khuyến nghị:
path = "id:PlayButton"
```

**Setup**:
1. Add component `ObjectID` vào GameObject
2. Set field `Id` với unique identifier
3. Sử dụng prefix `id:` trong path

### Khi nào dùng Hierarchy Path

- Prototype nhanh
- Test temporary hoặc throw-away
- Các object không có ObjectID và không cần stability

## 2. Test Step Organization

### Tạo Reusable Step Library

```
Assets/Tests/Common/
├── Navigation/
│   ├── OpenMainMenu.asset
│   ├── OpenSettings.asset
│   └── PressBackButton.asset
├── Authentication/
│   ├── Login.asset
│   ├── Logout.asset
│   └── WaitForAuth.asset
└── Assertions/
    ├── AssertNoErrors.asset
    └── AssertConnected.asset
```

**Lợi ích**:
- Tái sử dụng steps giữa nhiều test cases
- Dễ maintain (sửa một chỗ, apply cho tất cả)
- Consistency giữa các tests

### Đặt tên Steps rõ ràng

```
✅ Tốt:
- "WaitForMainMenuToLoad"
- "PressLoginButton"
- "InputValidUsername"
- "AssertLoginSuccessful"

❌ Không tốt:
- "Step1"
- "Wait"
- "Action"
- "Test"
```

**Quy tắc đặt tên**:
- Bắt đầu với động từ (Wait, Press, Input, Assert)
- Mô tả rõ ràng action/target
- Viết hoa từng từ (PascalCase) cho asset names

## 3. Timeout Management

### Set Timeout phù hợp cho từng loại action

```
// Instant actions (1-2 giây)
Press, InputText, SetToggle
timeoutSeconds = 1-2

// UI transitions (3-5 giây)
Animations, Panel switches
timeoutSeconds = 3-5

// Scene loading (5-15 giây)
LoadScene, major transitions
timeoutSeconds = 10-15

// Network operations (15-30 giây)
API calls, downloads
timeoutSeconds = 20-30
```

### Tránh timeout quá dài

```
❌ Tránh:
timeoutSeconds = 60  // Test sẽ chờ lâu nếu có bug

✅ Khuyến nghị:
timeoutSeconds = 10  // Fail fast để phát hiện issues sớm
```

### Override default timeout cho special cases

```csharp
var uiTest = new UITest();
uiTest.WaitTimeout = 30f;  // Tăng cho slow environments (CI, mobile)
```

## 4. Test Case Design

### Keep Test Cases Focused

Mỗi test case nên test một flow hoặc scenario cụ thể:

```
✅ Tốt:
- LoginWithValidCredentials.asset
- LoginWithInvalidPassword.asset
- LoginWithEmptyFields.asset

❌ Không tốt:
- CompleteGameTest.asset  // Quá rộng, khó debug
```

### Use Descriptive Test Case Names

```
✅ Tốt:
- "Login_ValidCredentials_Success"
- "Purchase_InsufficientFunds_ShowsError"
- "Settings_ToggleSound_PersistsAfterRestart"

❌ Không tốt:
- "Test1"
- "MyTest"
- "Debug"
```

### Set stopOnError appropriately

```csharp
// Đối với critical flow tests
stopOnError = true  // Dừng ngay khi có lỗi

// Đối với assertion tests
stopOnError = false // Tiếp tục để thu thập tất cả failures
```

## 5. Using Notes Effectively

### Document Step Purpose

```
Action: Press
Path: "id:PlayButton"
Note: "Start the game after tutorial is complete"
```

### Document Expected Behavior

```
Wait For Condition: LabelTextEquals
Path: "Canvas/StatusLabel"
Expected Text: "Ready"
Note: "Server connection must be established before proceeding"
```

### Document Test Data

```
Action: InputText
Path: "id:EmailField"
Text: "test@example.com"
Note: "Using test account with premium subscription"
```

## 6. Condition Design

### Combine Related Conditions

```
✅ Tốt:
WaitForConditionStep:
  - ObjectAppeared: "Canvas/Dialog"
  - SelectableInteractable: "Canvas/Dialog/OkButton"
  Mode: All

❌ Không hiệu quả:
Step1: WaitForCondition (ObjectAppeared)
Step2: WaitForCondition (SelectableInteractable)
```

### Use Appropriate Match Mode

```
// All = AND logic (tất cả phải true)
Mode: All
- ObjectAppeared: "Panel1"
- ObjectAppeared: "Panel2"
- LabelTextEquals: "Status" = "Ready"

// Any = OR logic (một trong số đó true)
Mode: Any
- LabelTextEquals: "Result" = "Success"
- LabelTextEquals: "Result" = "OK"
- LabelTextEquals: "Result" = "Complete"
```

## 7. Custom Extensions

### Create Custom Steps for Complex Logic

```csharp
// Thay vì nhiều steps:
// - InputText username
// - InputText password
// - Press Login
// - WaitForCondition

// Tạo custom step:
[CreateAssetMenu(menuName = "UI Tests/Steps/Login With Credentials")]
public class LoginStepSO : TestStepBase
{
    public string username;
    public string password;
    
    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        yield return uiTest.InputText(host, "id:UsernameField", username);
        yield return uiTest.InputText(host, "id:PasswordField", password);
        yield return uiTest.Press(host, "id:LoginButton");
        yield return uiTest.WaitFor(host, new UITest.ObjectAppeared("id:HomeScreen"));
    }
}
```

### Create Custom Actions for Reusable Logic

```csharp
[CreateAssetMenu(menuName = "UI Tests/Actions/Purchase Item")]
public class PurchaseItemActionSO : TestActionSO
{
    public string itemId;
    public int quantity;
    
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        // Navigate to shop
        yield return uiTest.Press(host, "id:ShopButton");
        
        // Find and purchase item
        var itemPath = $"id:Item_{itemId}";
        yield return uiTest.Press(host, itemPath);
        
        // Confirm purchase
        yield return uiTest.Press(host, "id:ConfirmButton");
    }
}
```

## 8. Error Handling

### Assert Early and Often

```
✅ Tốt:
Step 1: Login
Step 2: AssertLabel "Welcome"  ← Verify login success
Step 3: Open Shop
Step 4: AssertLabel "Shop Title" ← Verify shop opened
...

❌ Rủi ro:
Step 1: Login
Step 2: Open Shop
Step 3: Purchase Item
Step 4: Checkout
... (nhiều steps không verify)
Step 10: Assert Final State ← Nếu fail, khó biết step nào lỗi
```

### Use Descriptive Assertions

```
✅ Tốt:
AssertLabel:
  Path: "id:ErrorMessage"
  Text: "Invalid password"
  Note: "Verify correct error is shown for wrong password"

❌ Không rõ:
AssertLabel:
  Path: "id:Label"
  Text: "Error"
```

## 9. Test Data Management

### Avoid Hardcoded Production Data

```
❌ Tránh:
InputText: "john.doe@company.com"  // Real user account

✅ Khuyến nghị:
InputText: "test_user_001@testmail.com"  // Test account
```

### Document Test Accounts

```
note = "Using test account 'test_user_001' with basic permissions"
```

### Consider Parameterization

```csharp
// Tạo custom step với parameters thay vì hardcode
public class LoginStepSO : TestStepBase
{
    public enum TestUser { BasicUser, PremiumUser, AdminUser }
    public TestUser userType;
    
    string GetUsername()
    {
        return userType switch
        {
            TestUser.BasicUser => "test_basic@test.com",
            TestUser.PremiumUser => "test_premium@test.com",
            TestUser.AdminUser => "test_admin@test.com",
            _ => "test_default@test.com"
        };
    }
}
```

## 10. CI/CD Integration

### Use UITestManager for Batch Runs

```csharp
// Setup test suite
public class TestSuiteRunner : MonoBehaviour
{
    void Start()
    {
        var manager = gameObject.AddComponent<UITestManager>();
        manager.testCases = LoadAllTestCases();
        manager.autoRunOnStart = true;
        manager.logResults = true;
    }
}
```

### Configure Timeouts for CI

```csharp
// CI environments thường chậm hơn
var uiTest = new UITest();
if (Application.isBatchMode)
{
    uiTest.WaitTimeout = 30f; // Longer timeout for CI
}
else
{
    uiTest.WaitTimeout = 10f; // Normal timeout
}
```

### Log Strategically

```
// Enable verbose logging in CI
UITestRunner.verboseLogging = Application.isBatchMode;
```

## 11. Performance

### Use WaitIntervalFrames Wisely

```csharp
// Default: check mỗi 10 frames
uiTest.WaitIntervalFrames = 10;

// Faster polling (responsive, nhưng tốn CPU)
uiTest.WaitIntervalFrames = 1;

// Slower polling (tiết kiệm CPU)
uiTest.WaitIntervalFrames = 30;
```

### Minimize Scene Loads

```
✅ Tốt:
- Nhóm tests trong cùng scene
- Chỉ load scene khi cần thiết

❌ Không hiệu quả:
- Load scene cho mỗi test
- Load/unload scene liên tục
```

## 12. Maintenance

### Review Tests Regularly

- Xóa obsolete tests
- Update tests khi UI thay đổi
- Refactor duplicate logic thành reusable steps

### Version Control

```
✅ Commit:
- Test case assets (.asset)
- Test step assets (.asset)
- Custom step scripts (.cs)

❌ Ignore:
- Temporary test data
- Debug logs
- Generated reports
```

### Documentation

```
// Trong test case description
description = @"
Test Flow: Login with Valid Credentials
Prerequisites: Server must be running, test account exists
Expected Result: User logged in, home screen displayed
Author: John Doe
Last Updated: 2024-01-15
"
```

## 13. Debugging

### Use UITestRunner Window

- Real-time step monitoring
- Visual feedback (colors)
- Timeout visibility
- Error messages

### Strategic Wait Steps

```
// Add WaitTime để quan sát
Step 1: Login
Step 2: WaitTime (2s) ← Pause để xem UI
Step 3: Continue test
```

### Temporary Debug Actions

```csharp
[CreateAssetMenu(menuName = "UI Tests/Actions/Debug Screenshot")]
public class DebugScreenshotActionSO : TestActionSO
{
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        ScreenCapture.CaptureScreenshot($"debug_{Time.time}.png");
        yield return null;
    }
}
```

## 14. Team Collaboration

### Naming Conventions

Thống nhất naming conventions trong team:
```
TestCases: [Feature]_[Scenario]_[Expected]
Steps: [Verb][Target][Detail]
Custom Actions: [Action]ActionSO
Custom Conditions: [Check]ConditionSO
```

### Code Reviews

Review test cases như code:
- Logical flow
- Coverage
- Maintainability
- Readability

### Shared Step Library

Maintain shared library của common steps:
```
Assets/Tests/SharedSteps/
├── Navigation/
├── Authentication/
├── Common/
└── Assertions/
```

## Summary Checklist

✅ Use ObjectID cho stability  
✅ Đặt tên rõ ràng và consistent  
✅ Set timeout hợp lý  
✅ Keep test cases focused  
✅ Document với notes  
✅ Combine conditions hiệu quả  
✅ Assert early and often  
✅ Tránh hardcoded production data  
✅ Reuse steps thông qua library  
✅ Review và maintain tests regularly  
✅ Use UITestRunner Window để debug  
✅ Collaborate với team conventions  

## Xem thêm

- [[Getting Started]] - Tạo test đầu tiên
- [[Test Steps]] - Chi tiết về các step types
- [[Custom Test Steps]] - Tạo custom logic
- [[Troubleshooting]] - Giải quyết vấn đề
