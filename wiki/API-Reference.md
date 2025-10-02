# API Reference - Tài liệu tham khảo API

Tài liệu chi tiết về các class và method trong UI Test Framework.

## Core Classes

### UITest

Helper class cung cấp các phương thức tương tác với UI.

#### Constructor

```csharp
public UITest()
```

Tạo instance UITest mới với default settings.

#### Properties

| Property | Type | Default | Mô tả |
|----------|------|---------|-------|
| `WaitTimeout` | float | 10f | Timeout mặc định cho WaitFor operations (giây) |
| `WaitIntervalFrames` | int | 10 | Số frames chờ giữa các lần check condition |

#### Methods

##### Press

```csharp
public IEnumerator Press(MonoBehaviour host, string pathOrId)
```

Nhấn một button hoặc UI element.

**Parameters:**
- `host`: MonoBehaviour để chạy coroutine
- `pathOrId`: Hierarchy path hoặc "id:ObjectID"

**Returns:** IEnumerator

**Example:**
```csharp
yield return uiTest.Press(this, "id:PlayButton");
```

##### InputText

```csharp
public IEnumerator InputText(MonoBehaviour host, string pathOrId, string text)
```

Nhập text vào InputField.

**Parameters:**
- `host`: MonoBehaviour host
- `pathOrId`: Path đến InputField
- `text`: Text cần nhập

**Example:**
```csharp
yield return uiTest.InputText(this, "id:UsernameField", "testuser");
```

##### AssertLabel

```csharp
public IEnumerator AssertLabel(MonoBehaviour host, string pathOrId, string expectedText)
```

Kiểm tra text của một label.

**Parameters:**
- `host`: MonoBehaviour host
- `pathOrId`: Path đến label
- `expectedText`: Text mong đợi

**Throws:** `UITestException` nếu text không khớp

**Example:**
```csharp
yield return uiTest.AssertLabel(this, "id:StatusLabel", "Ready");
```

##### LoadScene

```csharp
public IEnumerator LoadScene(MonoBehaviour host, string sceneName)
```

Load một scene.

**Parameters:**
- `host`: MonoBehaviour host
- `sceneName`: Tên scene hoặc build index (dạng số)

**Example:**
```csharp
yield return uiTest.LoadScene(this, "MainMenu");
```

##### WaitFor

```csharp
public IEnumerator WaitFor(MonoBehaviour host, Condition condition)
```

Chờ đến khi condition được thỏa mãn.

**Parameters:**
- `host`: MonoBehaviour host
- `condition`: Condition object để check

**Throws:** `TimeoutException` nếu vượt quá WaitTimeout

**Example:**
```csharp
yield return uiTest.WaitFor(this, new UITest.ObjectAppeared("id:Menu"));
```

##### FindByPathOrId

```csharp
public static GameObject FindByPathOrId(string pathOrId)
```

Tìm GameObject bằng path hoặc ObjectID.

**Parameters:**
- `pathOrId`: Hierarchy path hoặc "id:ObjectID"

**Returns:** GameObject nếu tìm thấy, null nếu không

**Example:**
```csharp
var button = UITest.FindByPathOrId("id:PlayButton");
```

### UIAutomatedTestCase

ScriptableObject chứa một test case với chuỗi steps.

#### Properties

| Property | Type | Mô tả |
|----------|------|-------|
| `description` | string | Mô tả test case |
| `stopOnError` | bool | Dừng khi có step fail |
| `steps` | List<TestStepBase> | Danh sách test steps |

#### Methods

##### Run

```csharp
public IEnumerator Run(MonoBehaviour host, UITest uiTest)
```

Chạy test case.

**Parameters:**
- `host`: MonoBehaviour để chạy coroutines
- `uiTest`: UITest helper instance

**Returns:** IEnumerator

**Example:**
```csharp
var testCase = LoadTestCase();
var uiTest = new UITest();
yield return StartCoroutine(testCase.Run(this, uiTest));
```

### TestStepBase

Abstract base class cho tất cả test steps.

#### Properties

| Property | Type | Mô tả |
|----------|------|-------|
| `note` | string | Ghi chú cho step |
| `timeoutSeconds` | float | Timeout riêng (0 = dùng default) |

#### Methods

##### Execute

```csharp
public abstract IEnumerator Execute(MonoBehaviour host, UITest uiTest)
```

Thực thi step. Phải implement trong subclass.

##### ToString

```csharp
public override string ToString()
```

Trả về mô tả step (note hoặc name).

### ActionStepSO

Test step để thực hiện actions.

#### Properties

| Property | Type | Mô tả |
|----------|------|-------|
| `actionType` | ActionType | Loại action cần thực hiện |
| `path` | string | Path đến target object |
| `path2` | string | Path thứ hai (cho DragAndDrop) |
| `text` | string | Text parameter (InputText, AssertLabel) |
| `floatValue` | float | Float parameter (WaitSeconds, SetSlider) |
| `intValue` | int | Int parameter (SelectDropdown) |
| `boolValue` | bool | Bool parameter (SetToggle) |
| `customActionSO` | TestActionSO | Custom action (override inline) |

#### ActionType Enum

```csharp
public enum ActionType
{
    Press,
    AssertLabel,
    LoadScene,
    InputText,
    SetToggle,
    WaitSeconds,
    DragAndDrop,
    RaycastClick,
    SelectDropdown,
    SetSlider,
    Hover,
    Hold
}
```

### WaitForConditionStepSO

Test step chờ điều kiện.

#### Properties

| Property | Type | Mô tả |
|----------|------|-------|
| `preconditions` | List<ConditionSpec> | Inline conditions |
| `preconditionSOs` | List<TestCondition> | Custom condition assets |
| `preconditionMode` | ConditionMatchMode | All hoặc Any |
| `timeout` | float | Timeout riêng cho step |

#### ConditionType Enum

```csharp
public enum ConditionType
{
    ObjectAppeared,
    LabelTextEquals,
    SelectableInteractable
}
```

#### ConditionMatchMode Enum

```csharp
public enum ConditionMatchMode
{
    All,  // Tất cả conditions phải true
    Any   // Một trong các conditions true
}
```

### WaitTimeStepSO

Test step chờ thời gian.

#### Properties

| Property | Type | Mô tá |
|----------|------|-------|
| `seconds` | float | Số giây chờ (>= 0) |

### TestActionSO

Abstract base class cho custom actions.

#### Methods

##### Run

```csharp
public abstract IEnumerator Run(MonoBehaviour host, UITest uiTest)
```

Thực thi action. Phải implement trong subclass.

**Example:**
```csharp
public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
{
    Debug.Log("My custom action");
    yield return null;
}
```

### TestCondition

Abstract base class cho custom conditions.

#### Methods

##### Evaluate

```csharp
public abstract bool Evaluate(MonoBehaviour host, UITest uiTest)
```

Đánh giá condition. Phải implement trong subclass.

**Returns:** true nếu condition satisfied, false nếu không

**Example:**
```csharp
public override bool Evaluate(MonoBehaviour host, UITest uiTest)
{
    return GameManager.Instance.IsReady;
}
```

### UITestRunner

MonoBehaviour runtime component để chạy tests.

#### Static Properties

| Property | Type | Mô tả |
|----------|------|-------|
| `Instance` | UITestRunner | Singleton instance |
| `CurrentTestCase` | UIAutomatedTestCase | Test case đang chạy |
| `CurrentStepIndex` | int | Index của step hiện tại |
| `IsRunning` | bool | Test có đang chạy không |
| `HasFailed` | bool | Test có fail không |
| `FailureMessage` | string | Error message nếu fail |
| `StepResults` | List<bool> | Kết quả từng step |

#### Static Methods

##### SetTestCase

```csharp
public static void SetTestCase(UIAutomatedTestCase testCase)
```

Set test case để chạy.

**Parameters:**
- `testCase`: Test case cần chạy

### UITestManager

MonoBehaviour để chạy batch tests.

#### Properties

| Property | Type | Mô tả |
|----------|------|-------|
| `testCases` | List<UIAutomatedTestCase> | Danh sách test cases |
| `autoRunOnStart` | bool | Tự động chạy khi Start |
| `logResults` | bool | Log kết quả ra console |

### ObjectID

Component để đánh dấu GameObjects với unique ID.

#### Properties

| Property | Type | Mô tả |
|----------|------|-------|
| `Id` | string | Unique identifier |

## UITest Conditions

### Condition

Abstract base class cho conditions trong UITest.

```csharp
public abstract class Condition
{
    public abstract bool Satisfied();
}
```

### Built-in Conditions

#### ObjectAppeared

```csharp
public class ObjectAppeared : Condition
{
    public ObjectAppeared(string pathOrId)
}
```

Kiểm tra object tồn tại và active.

#### LabelTextAppeared

```csharp
public class LabelTextAppeared : Condition
{
    public LabelTextAppeared(string pathOrId, string expectedText)
}
```

Kiểm tra label có text mong đợi.

#### TMPLabelTextAppeared

```csharp
public class TMPLabelTextAppeared : Condition
{
    public TMPLabelTextAppeared(string pathOrId, string expectedText)
}
```

Kiểm tra TextMeshPro label có text mong đợi.

#### BoolCondition

```csharp
public class BoolCondition : Condition
{
    public BoolCondition(System.Func<bool> predicate)
}
```

Condition với lambda function tùy chỉnh.

**Example:**
```csharp
yield return uiTest.WaitFor(this, new UITest.BoolCondition(() => 
{
    return GameManager.Instance.IsReady;
}));
```

## Exceptions

### UITestException

```csharp
public class UITestException : System.Exception
{
    public UITestException(string message)
}
```

Exception cho UI test failures.

### TimeoutException

```csharp
public class System.TimeoutException : System.Exception
```

Thrown khi operation vượt quá timeout.

## Extension Points

### Creating Custom Steps

```csharp
[CreateAssetMenu(menuName = "UI Tests/Steps/My Step")]
public class MyStepSO : TestStepBase
{
    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        // Your implementation
        yield return null;
    }
}
```

### Creating Custom Actions

```csharp
[CreateAssetMenu(menuName = "UI Tests/Actions/My Action")]
public class MyActionSO : TestActionSO
{
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        // Your implementation
        yield return null;
    }
}
```

### Creating Custom Conditions

```csharp
[CreateAssetMenu(menuName = "UI Tests/Conditions/My Condition")]
public class MyConditionSO : TestCondition
{
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        // Your implementation
        return true;
    }
}
```

## Usage Patterns

### Basic Test Flow

```csharp
IEnumerator RunTest()
{
    var uiTest = new UITest();
    uiTest.WaitTimeout = 15f;
    
    // Load scene
    yield return uiTest.LoadScene(this, "MainMenu");
    
    // Wait for UI
    yield return uiTest.WaitFor(this, new UITest.ObjectAppeared("id:Menu"));
    
    // Interact
    yield return uiTest.Press(this, "id:PlayButton");
    
    // Verify
    yield return uiTest.AssertLabel(this, "id:Title", "Game Started");
}
```

### Custom Step with Parameters

```csharp
[CreateAssetMenu(menuName = "UI Tests/Steps/Login")]
public class LoginStepSO : TestStepBase
{
    public string username;
    public string password;
    
    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        yield return uiTest.InputText(host, "id:Username", username);
        yield return uiTest.InputText(host, "id:Password", password);
        yield return uiTest.Press(host, "id:LoginButton");
    }
}
```

### Conditional Execution

```csharp
IEnumerator ConditionalTest()
{
    var uiTest = new UITest();
    
    var obj = UITest.FindByPathOrId("id:Tutorial");
    if (obj != null && obj.activeInHierarchy)
    {
        // Skip tutorial
        yield return uiTest.Press(this, "id:SkipButton");
    }
    
    // Continue test
    yield return uiTest.Press(this, "id:StartButton");
}
```

## Performance Tips

### Caching Lookups

```csharp
private GameObject cachedButton;

IEnumerator UseButton()
{
    if (cachedButton == null)
    {
        cachedButton = UITest.FindByPathOrId("id:MyButton");
    }
    
    // Use cached reference
}
```

### Adjusting Poll Rate

```csharp
var uiTest = new UITest();
uiTest.WaitIntervalFrames = 30; // Check every 30 frames instead of 10
```

## Xem thêm

- [[Getting Started]] - Basic usage examples
- [[Test Steps]] - Detailed step documentation
- [[Custom Test Steps]] - Creating extensions
- [[Architecture]] - System design
