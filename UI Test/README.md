# UI Test Framework

Framework kiểm thử UI tự động cho Unity, hỗ trợ cấu hình test cases dạng ScriptableObject với các bước (steps) tuần tự và timeout per-step.

## 📚 Documentation

**Tài liệu Wiki đầy đủ:** Xem thư mục [`wiki/`](../wiki/) cho tài liệu chi tiết:

- **[Home](../wiki/Home.md)** - Trang chủ với tổng quan
- **[Getting Started](../wiki/Getting-Started.md)** - Hướng dẫn bắt đầu
- **[Architecture](../wiki/Architecture.md)** - Kiến trúc hệ thống
- **[Test Steps](../wiki/Test-Steps.md)** - Các loại test steps
- **[UITestRunner Window](../wiki/UITestRunner-Window.md)** - Hướng dẫn Editor Window
- **[Object Lookup](../wiki/Object-Lookup.md)** - Tìm kiếm UI elements
- **[Best Practices](../wiki/Best-Practices.md)** - Thực hành tốt nhất
- **[Custom Test Steps](../wiki/Custom-Test-Steps.md)** - Tạo custom steps
- **[Custom Actions](../wiki/Custom-Actions.md)** - Tạo custom actions
- **[Custom Conditions](../wiki/Custom-Conditions.md)** - Tạo custom conditions
- **[Examples and Tutorials](../wiki/Examples-And-Tutorials.md)** - Ví dụ thực tế
- **[Troubleshooting](../wiki/Troubleshooting.md)** - Khắc phục sự cố
- **[API Reference](../wiki/API-Reference.md)** - Tài liệu API

---

## Kiến trúc

### Core Components

- **UITest** (`UITest.cs`): Helper class cung cấp các phương thức tương tác UI (Press, AssertLabel, InputText, etc.) và điều kiện chờ (WaitFor + Conditions).
- **UITestException** (`UITestException.cs`): Exception class cho UI test errors.
- **UITestScenarios** (`UITestScenarios.cs`): Ví dụ sử dụng UITest.

### Test Steps System

- **TestStepBase** (`TestStepBase.cs`): Abstract base class cho tất cả test steps, hỗ trợ timeout per-step (`timeoutSeconds`).
- **UIAutomatedTestCase** (`UIAutomatedTestCase.cs`): ScriptableObject chứa danh sách các bước test tuần tự, chạy với timeout enforcement.
- **UITestManager** (`UITestManager.cs`): MonoBehaviour quản lý và chạy nhiều test cases tuần tự.
- **UITestRunner** (`UITestRunner.cs`): Runtime component để chạy test case trong Play mode.
- **UITestRunnerWindow** (`Editor/UITestRunnerWindow.cs`): Editor Window để chạy và monitor test cases trực tiếp trong Editor.

### Custom Actions & Conditions

- **TestActionBase** (`SO Action/TestActionBase.cs`): Base class cho custom actions.
- **TestConditionBase** (`SO Action/TestConditionBase.cs`): Base class cho custom conditions.

### Object Lookup

- **ObjectID** (`Object ID/ObjectID.cs`): Component để lookup GameObjects bằng ID thay vì hierarchy path.

### Built-in Steps

- **ActionStepSO** (`Steps/ActionStepSO.cs`): Thực hiện hành động UI (Press, Assert, Input, etc.).
- **WaitForConditionStepSO** (`Steps/WaitForConditionStepSO.cs`): Chờ điều kiện được thỏa mãn.
- **WaitTimeStepSO** (`Steps/WaitTimeStepSO.cs`): Chờ thời gian cố định.

### Examples

- **SOExamples/**: Ví dụ custom actions và conditions.
- **Test Demo/**: Demo test cases.

### Test Steps (Các loại bước test - ScriptableObject)

Mỗi test case bao gồm một chuỗi các `TestStepBase` ScriptableObject tuần tự. Các step có thể tái sử dụng giữa nhiều test cases và hỗ trợ timeout per-step.

#### 1. ActionStepSO
Thực hiện một hành động UI (nhấn button, nhập text, assert label, etc.)

**Tạo asset:**
```
Assets → Create → UI Tests → Steps → Action Step
```

**Inline action types:**
- Press
- AssertLabel
- LoadScene
- InputText
- SetToggle
- WaitSeconds
- DragAndDrop
- RaycastClick
- SelectDropdown
- SetSlider
- Hover
- Hold

**Hoặc sử dụng custom action:**
- Gán `TestActionSO` vào field `customActionSO` để sử dụng custom action logic.

**Timeout:** Có thể set `timeoutSeconds` cho từng step (0 = dùng UITest.WaitTimeout).

#### 2. WaitForConditionStepSO
Chờ đến khi một hoặc nhiều điều kiện được thỏa mãn.

**Tạo asset:**
```
Assets → Create → UI Tests → Steps → Wait For Condition
```

**Inline conditions:**
- ObjectAppeared: Object xuất hiện và active
- LabelTextEquals: Label có text mong đợi
- SelectableInteractable: Selectable component có thể tương tác

**ScriptableObject conditions:**
- Gán `TestCondition` SO vào list `preconditionSOs`

**Cấu hình:**
- `preconditionMode`: All (tất cả phải đúng) hoặc Any (một trong số đó đúng)
- `timeout`: Thời gian chờ tối đa (giây)

#### 3. WaitTimeStepSO
Chờ một khoảng thời gian cố định (unscaled time).

**Tạo asset:**
```
Assets → Create → UI Tests → Steps → Wait Time
```

**Cấu hình:**
- `seconds`: Số giây chờ (có MinValue để đảm bảo >= 0)

### UITestRunner Window

Công cụ Editor Window để chạy và theo dõi test cases trực tiếp trong Unity Editor.

**Mở window:**
```
Tools → UI Test Runner
```

**Cách sử dụng:**
1. Kéo thả `UIAutomatedTestCase` vào field "Test Case"
2. Nhấn "Run Test" - sẽ tự động chuyển sang Play mode và chạy test
3. Theo dõi tiến trình trong danh sách steps:
   - **Vàng**: Step đang thực hiện (hiển thị timeout nếu có)
   - **Xanh**: Step thành công
   - **Đỏ**: Step thất bại
4. Nếu tất cả steps pass: Hiển thị "SUCCESS"
5. Nếu có step fail: Hiển thị "FAILED" với thông báo lỗi
6. Có thể dừng test bằng nút "Stop Test"

**Lưu ý:** UITestRunner component sẽ tự động được tạo trong scene khi chạy test và tự động xóa khi test hoàn thành (pass hoặc fail).

### UITestRunner Window — hướng dẫn chi tiết

Quick walkthrough:

- Mở `Tools → UI Test Runner`.
- Kéo thả một asset `UIAutomatedTestCase` từ Project pane vào ô "Test Case" ở trên cùng.
- Nhấn "Run Test". Nếu Editor chưa ở Play mode, cửa sổ sẽ chuyển sang Play mode tự động và bắt đầu test khi Play mode đã sẵn sàng.

Controls & behavior:

- Run Test: bắt đầu chạy test case đã chọn. Tự động tạo `GameObject` chứa `UITestRunner` nếu chưa có trong scene.
- Stop Test: dừng test hiện tại (chỉ hiện khi đang chạy). `UITestRunner` sẽ reset trạng thái.

What the window shows:

- Header: tên test case đang chạy.
- Status line:
    - "Running step N..." khi test đang chạy.
    - "SUCCESS: All steps passed!" khi test kết thúc thành công.
    - "FAILED: <message>" khi có step thất bại — message chứa exception hoặc thông báo timeout.
- Steps list: hiển thị danh sách step theo thứ tự. Mỗi dòng có:
    - Số thứ tự và tên step (note hoặc ToString())
    - Nếu step có `timeoutSeconds > 0` sẽ hiển thị `[timeout: Ns]` bên cạnh tên.
    - Màu:
        - Vàng (Bold): step đang chạy
        - Xanh: đã pass
        - Đỏ: đã fail

Play-mode nuance:

- Khi nhấn "Run Test" từ Editor window, nếu Editor chưa ở Play mode, hệ thống sẽ gọi `EditorApplication.EnterPlaymode()` và delay bắt đầu test cho đến khi Play mode sẵn sàng. Test case được đăng ký với `UITestRunner.SetTestCase(...)` trước khi component được tạo.
- `UITestRunner` chạy các step trên GameObject runtime. Khi test hoàn thành (pass hoặc fail), `UITestRunner` sẽ tự động xóa GameObject của nó để dọn dẹp scene.

Interpreting failures:

- Nếu một step throws exception trong quá trình chạy, test sẽ được đánh dấu failed và message lỗi sẽ hiển thị trong header.
- Nếu một step vượt quá `timeoutSeconds` (hoặc `UITest.WaitTimeout` nếu step mặc định), helper sẽ ném `TimeoutException` và đánh dấu step failed — cửa sổ sẽ hiển thị message tương ứng.

Tips & advanced usage:

- Nếu bạn muốn debug tương tác cụ thể, đặt giá trị `timeoutSeconds` dài hơn để tránh false-positive timeouts khi scene load chậm.
- Bạn có thể tạo test case in-memory (runtime) hoặc lưu thành asset và kéo vào Runner để chạy nhiều lần.
- Sử dụng `UITestManager` cho batch runs (chạy nhiều test cases theo danh sách) khi cần chạy trên CI hoặc test suite lớn.

Troubleshooting:

- Nếu Runner không bắt đầu ở Play mode: kiểm tra console để xem có lỗi xảy ra trong `OnEnable` hoặc khi tạo `UITestRunner`.
- Nếu `UITestRunner` không tự xóa: có thể test treo trong một step (kiểm tra step timeout và logs).
- Nếu step không tìm thấy target object khi dùng `id:...`: kiểm tra component `ObjectID` có tồn tại và `Id` chính xác (README đã có phần mô tả tiền tố `id:`).


## Hướng dẫn sử dụng

### 1. Tạo Test Case Asset

```
Assets → Create → UI Tests → Automated Test Case
```

### 2. Tạo các Test Step assets

Tạo các step asset có thể tái sử dụng:

**Ví dụ tạo steps:**

1. **WaitForConditionStepSO** - "Wait For MainMenu"
   - Assets → Create → UI Tests → Steps → Wait For Condition
   - Thêm condition: ObjectAppeared, path = "Canvas/MainMenu"
   - `timeoutSeconds` = 5 (timeout riêng cho step này)
   - Lưu asset với tên "WaitForMainMenu"

2. **WaitTimeStepSO** - "Wait 0.5s"
   - Assets → Create → UI Tests → Steps → Wait Time
   - seconds = 0.5
   - Lưu asset với tên "Wait_05s"

3. **ActionStepSO** - "Press Play Button"
   - Assets → Create → UI Tests → Steps → Action Step
   - ActionType = Press, path = "id:PlayButton"
   - `timeoutSeconds` = 2 (timeout cho action này)
   - Lưu asset với tên "PressPlayButton"

4. **WaitForConditionStepSO** - "Wait Game Ready"
   - Assets → Create → UI Tests → Steps → Wait For Condition
   - Thêm condition: LabelTextEquals, path = "Canvas/StatusLabel", expectedText = "Ready"
   - `timeoutSeconds` = 10
   - Lưu asset với tên "WaitGameReady"

5. **ActionStepSO** - "Assert Game Started"
   - Assets → Create → UI Tests → Steps → Action Step
   - ActionType = AssertLabel, path = "Canvas/Title", text = "Game Started"
   - `timeoutSeconds` = 1
   - Lưu asset với tên "AssertGameStarted"

### 3. Gán steps vào Test Case

Trong Inspector của Test Case asset, kéo thả các step assets vào list `steps` theo thứ tự mong muốn:

```
Steps:
  [0] WaitForMainMenu
  [1] Wait_05s
  [2] PressPlayButton
  [3] WaitGameReady
  [4] AssertGameStarted
```

### 4. Chạy Test Case

**Cách 1: UITestRunner Window (Khuyến nghị)**
- Tools → UI Test Runner
- Kéo thả test case vào field
- Nhấn "Run Test"
- Theo dõi real-time với timeout hiển thị

**Cách 2: UITestManager**
- Tạo GameObject trong scene
- Add component `UITestManager`
- Gán test case assets vào list `testCases`
- Chọn `autoRunOnStart = true` để tự động chạy khi Start
- Hoặc right-click component → "Run All Tests"

**Cách 3: Code**
```csharp
public class MyTestRunner : MonoBehaviour
{
    public UIAutomatedTestCase testCase;
    
    IEnumerator Start()
    {
        var uiTest = new UITest();
        yield return StartCoroutine(testCase.Run(this, uiTest));
    }
}
```

## Object Lookup

Hỗ trợ hai cách tìm GameObject:

1. **Hierarchy path**: `"Canvas/PlayButton"`
2. **ObjectID**: `"id:PlayButton"` (tìm GameObject có component `ObjectID` với Id = "PlayButton")

Lưu ý về tiền tố `id:`:
- Tiền tố phải có dạng `id:...` nhưng phần tiền tố là không phân biệt chữ hoa/thường — ví dụ `id:PlayButton`, `ID:PlayButton` đều hợp lệ.
- Phần sau dấu `:` là giá trị Id so khớp chính xác (so sánh theo `StringComparison.Ordinal` trong code). Tức là `id:PlayButton` khác `id:playbutton` nếu Id lưu trong component là `PlayButton`.
- Cơ chế tìm kiếm dùng `ObjectID` component (tìm tất cả ObjectID trong scene và so sánh `Id`), vì vậy đảm bảo GameObject cần có component `ObjectID` và trường `Id` được thiết lập.

Ví dụ:
- `"Canvas/PlayButton"` tìm theo hierarchy path (dễ vỡ khi di chuyển trong hierarchy)
- `"id:PlayButton"` tìm theo ObjectID (ổn định hơn khi UI thay đổi cấu trúc)

## Tùy chỉnh và mở rộng

### Tạo Custom Test Step

Kế thừa `TestStepBase`:

```csharp
[CreateAssetMenu(menuName = "UI Tests/Steps/My Custom Step")]
public class MyCustomStepSO : TestStepBase
{
    public string myParameter;
    public float delay;
    
    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        Debug.Log($"Running custom step: {myParameter}");
        yield return new WaitForSeconds(delay);
        // Your custom logic here
    }
    
    public override string ToString()
    {
        return string.IsNullOrEmpty(note) ? $"Custom: {myParameter}" : note;
    }
}
```

### Tạo Custom Action (cho ActionStepSO)

Kế thừa `TestActionSO`:

```csharp
[CreateAssetMenu(menuName = "UI Tests/Actions/My Custom Action")]
public class MyCustomActionSO : TestActionSO
{
    public string myParameter;
    
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        Debug.Log($"Running custom action: {myParameter}");
        // Your logic here
        yield return null;
    }
}
```

### Tạo Custom Condition (cho WaitForConditionStepSO)

Kế thừa `TestCondition`:

```csharp
[CreateAssetMenu(menuName = "UI Tests/Conditions/My Condition")]
public class MyConditionSO : TestCondition
{
    public string checkValue;
    
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        // Your condition logic
        return MyManager.Instance.IsReady;
    }
}
```

## Best Practices

1. **Sử dụng ObjectID** thay vì hierarchy path khi có thể (ổn định hơn khi UI thay đổi cấu trúc)
2. **Tái sử dụng step assets**: Tạo thư viện các step assets phổ biến và tái sử dụng trong nhiều test cases
3. **Đặt tên rõ ràng**: Đặt tên step assets mô tả hành động (vd: "PressPlayButton", "WaitForMenuLoad")
4. **Sử dụng note field** để ghi chú mục đích của step asset
5. **Thiết lập timeout per-step**: Set `timeoutSeconds` cho từng step để tránh test bị treo quá lâu
6. **Tách logic phức tạp** thành custom TestStepBase hoặc TestActionSO
7. **Kiểm tra stopOnError**: true để dừng khi lỗi, false để tiếp tục
8. **Tổ chức thư mục**: Tạo folder riêng cho Steps, Actions, Conditions để dễ quản lý
9. **Sử dụng UITestRunnerWindow** để debug và monitor test real-time

## Examples

### Built-in Step Types
- `ActionStepSO.cs` - Step thực hiện các hành động UI cơ bản
- `WaitForConditionStepSO.cs` - Step chờ điều kiện
- `WaitTimeStepSO.cs` - Step chờ thời gian

### Built-in Custom Actions/Conditions
Xem folder `SOExamples/` để tham khảo:
- `PressActionSO.cs` - Custom action example
- `ObjectAppearedConditionSO.cs` - Custom condition example
- `DebugLogActionSO.cs` - Debug action example

### Demo Test Cases
Xem folder `Test Demo/` để tham khảo các test case hoàn chỉnh.

## Odin Inspector Integration

Framework này sử dụng Odin Inspector để cải thiện giao diện editor với các tính năng:

- **[EnumToggleButtons]**: Hiển thị enum dưới dạng toggle buttons cho dễ chọn (ActionType, ConditionType, ConditionMatchMode)
- **[ShowIf]**: Ẩn/hiện các field dựa trên giá trị enum để tránh nhầm lẫn
  - Trong `ActionStepSO`: Chỉ hiển thị `path` khi không phải WaitSeconds, `text` chỉ cho AssertLabel/InputText, etc.
  - Trong `WaitForConditionStepSO`: `expectedText` chỉ hiển thị khi conditionType là LabelTextEquals
- **[MinValue]**: Đảm bảo giá trị không âm (seconds trong WaitTimeStepSO)

**Lưu ý**: Nếu gặp lỗi MissingMethodException với Odin, hãy update plugin hoặc disable "Automatic Height Adjustment" trong Odin Preferences.

## Troubleshooting

- **MissingMethodException (Odin)**: Update Odin Inspector plugin hoặc disable automatic height adjustment trong Odin preferences
- **Timeout khi chờ condition**: Tăng `timeout` hoặc `timeoutSeconds` trên step, hoặc kiểm tra điều kiện có đúng không
- **Step timeout fail**: Step bị coi là fail nếu vượt quá `timeoutSeconds` (hoặc UITest.WaitTimeout nếu = 0)
- **ObjectID không tìm thấy**: Đảm bảo GameObject có component `ObjectID` và Id khớp với giá trị tìm kiếm
- **UITestRunner không tự xóa**: Kiểm tra xem test có thực sự hoàn thành không (có thể bị treo trong step)
