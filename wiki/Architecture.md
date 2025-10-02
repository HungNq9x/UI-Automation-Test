# Kiến trúc hệ thống

Tài liệu này mô tả kiến trúc tổng thể của UI Test Framework và cách các components tương tác với nhau.

## Sơ đồ kiến trúc

```
┌─────────────────────────────────────────────────────────────┐
│                    UI Test Framework                         │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐         ┌──────────────────┐          │
│  │ UITestRunner     │◄────────┤ UITestRunner     │          │
│  │ Window (Editor)  │         │ Window           │          │
│  └────────┬─────────┘         └──────────────────┘          │
│           │                                                   │
│           │ Controls                                          │
│           ▼                                                   │
│  ┌──────────────────┐         ┌──────────────────┐          │
│  │ UITestRunner     │◄────────┤ UITestManager    │          │
│  │ (Runtime)        │         │ (Batch Runner)   │          │
│  └────────┬─────────┘         └──────────────────┘          │
│           │                                                   │
│           │ Executes                                          │
│           ▼                                                   │
│  ┌──────────────────┐                                        │
│  │ UIAutomatedTest  │                                        │
│  │ Case (SO)        │                                        │
│  └────────┬─────────┘                                        │
│           │                                                   │
│           │ Contains                                          │
│           ▼                                                   │
│  ┌──────────────────┐                                        │
│  │ TestStepBase (SO)│                                        │
│  └────────┬─────────┘                                        │
│           │                                                   │
│           │ Types:                                            │
│           ├─► ActionStepSO                                   │
│           ├─► WaitForConditionStepSO                         │
│           └─► WaitTimeStepSO                                 │
│                                                               │
│  ┌──────────────────┐         ┌──────────────────┐          │
│  │ TestActionBase   │         │ TestCondition    │          │
│  │ (Custom Actions) │         │ Base (Custom)    │          │
│  └──────────────────┘         └──────────────────┘          │
│                                                               │
│  ┌──────────────────┐                                        │
│  │ UITest (Helper)  │◄────── Used by all steps              │
│  └──────────────────┘                                        │
│                                                               │
│  ┌──────────────────┐                                        │
│  │ ObjectID         │◄────── Optional for stable lookup     │
│  │ (Component)      │                                        │
│  └──────────────────┘                                        │
└─────────────────────────────────────────────────────────────┘
```

## Core Components

### 1. UITest Class

**File**: `UITest.cs`

**Mục đích**: Helper class cung cấp các phương thức cơ bản để tương tác với UI và chờ điều kiện.

**Chức năng chính**:
- Tương tác UI: Press, InputText, SetToggle, DragAndDrop, etc.
- Assert: AssertLabel để kiểm tra text
- Scene management: LoadScene
- Wait conditions: WaitFor với các Condition types
- Object lookup: FindByPathOrId (hỗ trợ cả hierarchy path và ObjectID)

**Cấu hình**:
- `WaitTimeout`: Timeout mặc định cho WaitFor operations (10s)
- `WaitIntervalFrames`: Số frames giữa các lần check condition (10 frames)

### 2. UIAutomatedTestCase

**File**: `UIAutomatedTestCase.cs`

**Mục đích**: ScriptableObject chứa một test case hoàn chỉnh với chuỗi steps tuần tự.

**Cấu trúc**:
```csharp
public class UIAutomatedTestCase : ScriptableObject
{
    public string description;           // Mô tả test case
    public bool stopOnError;             // Dừng khi có lỗi
    public List<TestStepBase> steps;     // Danh sách steps
}
```

**Chức năng**:
- Chạy tuần tự các steps trong list
- Enforce timeout cho mỗi step (per-step timeout)
- Dừng hoặc tiếp tục khi có lỗi (tùy stopOnError)
- Yield coroutines cho từng step

### 3. TestStepBase

**File**: `TestStepBase.cs`

**Mục đích**: Abstract base class cho tất cả test steps.

**Cấu trúc**:
```csharp
public abstract class TestStepBase : ScriptableObject
{
    public string note;                  // Ghi chú cho step
    public float timeoutSeconds;         // Timeout riêng (0 = dùng default)
    
    public abstract IEnumerator Execute(MonoBehaviour host, UITest uiTest);
}
```

**Các implementations**:
- **ActionStepSO**: Thực hiện actions (Press, Input, Assert, etc.)
- **WaitForConditionStepSO**: Chờ điều kiện thỏa mãn
- **WaitTimeStepSO**: Chờ một khoảng thời gian

### 4. UITestRunner

**File**: `UITestRunner.cs`

**Mục đích**: MonoBehaviour runtime component để chạy test cases trong Play mode.

**Chức năng**:
- Singleton pattern (Instance)
- Chạy test case được gán từ UITestRunnerWindow hoặc code
- Track trạng thái: CurrentStepIndex, IsRunning, HasFailed
- Tự động cleanup sau khi test hoàn thành

**Static properties**:
- `CurrentTestCase`: Test case đang chạy
- `CurrentStepIndex`: Step index hiện tại
- `IsRunning`: Test có đang chạy không
- `HasFailed`: Test có thất bại không
- `FailureMessage`: Message lỗi nếu có
- `StepResults`: Kết quả từng step (list of bool)

### 5. UITestRunnerWindow

**File**: `Editor/UITestRunnerWindow.cs`

**Mục đích**: Editor Window để chạy và monitor tests trong Unity Editor.

**Chức năng**:
- UI để chọn và chạy test cases
- Real-time monitoring với màu sắc:
  - Vàng: Step đang chạy
  - Xanh: Step pass
  - Đỏ: Step fail
- Hiển thị timeout cho mỗi step
- Tự động chuyển sang Play mode khi cần
- Stop test button

### 6. UITestManager

**File**: `UITestManager.cs`

**Mục đích**: Component để chạy batch tests (nhiều test cases tuần tự).

**Cấu hình**:
- `testCases`: List các test cases
- `autoRunOnStart`: Tự động chạy khi Start
- `logResults`: Log kết quả ra console
- Context menu: "Run All Tests"

## Extension Components

### TestActionBase

**File**: `TestActionBase.cs`

**Mục đích**: Base class cho custom actions, được sử dụng bởi ActionStepSO.

**Cách dùng**:
```csharp
[CreateAssetMenu(menuName = "UI Tests/Actions/My Action")]
public class MyActionSO : TestActionSO
{
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        // Custom logic here
        yield return null;
    }
}
```

### TestConditionBase

**File**: `TestConditionBase.cs`

**Mục đích**: Base class cho custom conditions, được sử dụng bởi WaitForConditionStepSO.

**Cách dùng**:
```csharp
[CreateAssetMenu(menuName = "UI Tests/Conditions/My Condition")]
public class MyConditionSO : TestCondition
{
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        // Return true when condition is met
        return someCondition;
    }
}
```

### ObjectID Component

**File**: `Object ID/ObjectID.cs`

**Mục đích**: Component để đánh dấu GameObjects với ID, cho phép lookup ổn định.

**Cách dùng**:
1. Add component `ObjectID` vào GameObject
2. Set field `Id` = "MyUniqueId"
3. Trong test steps, dùng path: `id:MyUniqueId`

**Ưu điểm**: Không phụ thuộc vào hierarchy structure, ổn định khi refactor UI.

## Flow thực thi Test Case

```
1. User chọn test case trong UITestRunnerWindow
   ↓
2. Click "Run Test"
   ↓
3. UITestRunnerWindow calls UITestRunner.SetTestCase()
   ↓
4. Enter Play mode (nếu chưa)
   ↓
5. UITestRunner.Start() → StartCoroutine(RunTest())
   ↓
6. For each step in testCase.steps:
   ├─ Set CurrentStepIndex
   ├─ Call step.Execute(host, uiTest)
   ├─ Enforce timeoutSeconds
   ├─ Track result in StepResults
   └─ If error and stopOnError: break
   ↓
7. Test complete: Update HasFailed, FailureMessage
   ↓
8. UITestRunner self-destructs (Destroy GameObject)
   ↓
9. UITestRunnerWindow updates UI (Success/Failed)
```

## Design Patterns

### 1. ScriptableObject Architecture
- Test cases và steps là assets có thể tái sử dụng
- Không cần compile code để tạo tests
- Dễ version control và collaboration

### 2. Strategy Pattern
- TestStepBase là interface chung
- Các concrete steps (Action, WaitCondition, WaitTime) implement khác nhau
- Dễ thêm step types mới

### 3. Singleton Pattern
- UITestRunner sử dụng singleton để dễ access từ Editor
- Đảm bảo chỉ có một runner active

### 4. Command Pattern
- Mỗi TestStepBase là một command
- Test case là sequence of commands
- Hỗ trợ undo (thông qua replay), logging, timeout

### 5. Template Method Pattern
- TestStepBase.Execute() là template
- Subclasses override để implement chi tiết
- UIAutomatedTestCase.Run() handle timeout enforcement

## Thread Safety và Coroutines

Framework chạy hoàn toàn trên Unity main thread thông qua coroutines:
- Không có threading issues
- Tất cả UI interactions an toàn
- WaitFor sử dụng unscaled time để không bị ảnh hưởng bởi Time.timeScale

## Performance Considerations

- **WaitIntervalFrames**: Điều chỉnh để balance giữa responsiveness và performance
- **Timeout values**: Set hợp lý để tránh false positives
- **Object lookup**: ObjectID lookup nhanh hơn hierarchy path trong scenes phức tạp
- **Coroutine overhead**: Minimal, phù hợp cho UI testing

## Extensibility Points

Framework cung cấp nhiều điểm mở rộng:

1. **Custom Test Steps**: Kế thừa TestStepBase
2. **Custom Actions**: Kế thừa TestActionSO cho ActionStepSO
3. **Custom Conditions**: Kế thừa TestCondition cho WaitForConditionStepSO
4. **UITest extensions**: Thêm partial class hoặc extension methods
5. **Custom test runners**: Implement logic riêng dựa trên UITestRunner pattern

Xem [[Custom Test Steps]], [[Custom Actions]], và [[Custom Conditions]] để biết chi tiết.
