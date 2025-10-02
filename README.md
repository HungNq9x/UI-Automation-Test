# UI Automation Test Framework


Framework kiểm thử UI tự động cho Unity - Tạo và chạy UI tests mà không cần viết code!

## 🎯 Tổng quan

UI Test Framework là một công cụ mạnh mẽ cho phép bạn tạo và chạy automated UI tests trong Unity sử dụng ScriptableObject. Framework cung cấp:

- ✅ Test cases dạng assets, không cần code
- ✅ Reusable test steps
- ✅ Real-time test monitoring trong Editor
- ✅ Per-step timeout control
- ✅ Custom actions và conditions
- ✅ Flexible object lookup (hierarchy hoặc ObjectID)

## 🚀 Quick Start

### 1. Import Framework

Copy folder `UI Test` vào project Unity của bạn.

### 2. Tạo Test Case đầu tiên

```
Assets → Create → UI Tests → Automated Test Case
```

### 3. Tạo Test Steps

```
Assets → Create → UI Tests → Steps → [Action Step / Wait For Condition / Wait Time]
```

### 4. Chạy Test

```
Tools → UI Test Runner
```

Kéo test case vào window và click "Run Test"!

## 📚 Documentation

Xem **[Wiki Documentation](./wiki/)** để biết chi tiết:

### Hướng dẫn cơ bản
- **[Getting Started](./wiki/Getting-Started.md)** - Tạo test đầu tiên
- **[UITestRunner Window](./wiki/UITestRunner-Window.md)** - Sử dụng Editor Window
- **[Object Lookup](./wiki/Object-Lookup.md)** - Tìm kiếm UI elements

### Kiến trúc
- **[Architecture](./wiki/Architecture.md)** - Tổng quan kiến trúc
- **[Test Steps](./wiki/Test-Steps.md)** - Các loại test steps
- **[API Reference](./wiki/API-Reference.md)** - Tài liệu API

### Mở rộng
- **[Custom Test Steps](./wiki/Custom-Test-Steps.md)** - Tạo step types mới
- **[Custom Actions](./wiki/Custom-Actions.md)** - Tạo custom actions
- **[Custom Conditions](./wiki/Custom-Conditions.md)** - Tạo custom conditions

### Thực hành
- **[Best Practices](./wiki/Best-Practices.md)** - Tips và recommendations
- **[Examples and Tutorials](./wiki/Examples-And-Tutorials.md)** - Ví dụ thực tế
- **[Troubleshooting](./wiki/Troubleshooting.md)** - Giải quyết vấn đề

## 📖 Ví dụ nhanh

### Test Case Asset

```
LoginTest
├── Steps:
│   ├── [0] Wait For Login Screen
│   ├── [1] Input Username
│   ├── [2] Input Password
│   ├── [3] Press Login Button
│   └── [4] Wait For Home Screen
```

### Code Example

```csharp
public class MyTest : MonoBehaviour
{
    public UIAutomatedTestCase testCase;
    
    IEnumerator Start()
    {
        var uiTest = new UITest();
        yield return StartCoroutine(testCase.Run(this, uiTest));
    }
}
```

## 🎨 Features

### Built-in Test Steps

- **ActionStepSO**: Press, InputText, AssertLabel, LoadScene, SetToggle, DragAndDrop, và nhiều hơn
- **WaitForConditionStepSO**: Chờ conditions như ObjectAppeared, LabelTextEquals, SelectableInteractable
- **WaitTimeStepSO**: Chờ một khoảng thời gian cố định

### UI Test Runner Window

- Real-time monitoring
- Step-by-step visualization với colors:
  - 🟡 Yellow: Đang chạy
  - 🟢 Green: Pass
  - 🔴 Red: Fail
- Timeout display cho mỗi step
- Auto Play mode entry

### Object Lookup

```csharp
// Hierarchy path
path = "Canvas/MainMenu/PlayButton"

// ObjectID (recommended)
path = "id:PlayButton"
```

### Extensibility

- Tạo custom TestStepBase
- Tạo custom TestActionSO
- Tạo custom TestCondition
- Tích hợp với game systems

## 🔧 Requirements

- Unity 2019.4 hoặc mới hơn
- Unity UI hoặc TextMeshPro
- (Optional) Odin Inspector cho enhanced Editor UI

## 📂 Project Structure

```
UI Test/
├── Editor/
│   └── UITestRunnerWindow.cs        # Editor Window
├── Steps/
│   ├── ActionStepSO.cs              # Action step
│   ├── WaitForConditionStepSO.cs    # Wait step
│   └── WaitTimeStepSO.cs            # Time wait step
├── SO Action/                        # Action base classes
├── SOExamples/                       # Example custom actions/conditions
├── Test Demo/                        # Demo test cases
├── Object ID/
│   └── ObjectID.cs                  # ObjectID component
├── UITest.cs                        # Core test helper
├── UIAutomatedTestCase.cs           # Test case ScriptableObject
├── UITestRunner.cs                  # Runtime test runner
├── TestStepBase.cs                  # Base class for steps
├── TestActionBase.cs                # Base class for actions
├── TestConditionBase.cs             # Base class for conditions
└── README.md                        # Full documentation
```

## 💡 Use Cases

- ✅ UI flow testing
- ✅ Regression testing
- ✅ Integration testing
- ✅ Smoke testing
- ✅ Acceptance testing
- ✅ CI/CD automated testing

## 🤝 Contributing

Contributions are welcome! Để đóng góp:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📝 License

This project is provided as-is for use in Unity projects.

## 🔗 Links

- **[Full Documentation](./UI%20Test/README.md)** - Tài liệu đầy đủ trong framework
- **[Wiki](./wiki/)** - Wiki documentation
- **[Demo Scripts](./UI%20Test/Test%20Demo/)** - Ví dụ code
- **[Examples](./UI%20Test/SOExamples/)** - Custom action/condition examples

## 📧 Support

Nếu bạn gặp vấn đề:
1. Check [Troubleshooting guide](./wiki/Troubleshooting.md)
2. Review [Examples](./wiki/Examples-And-Tutorials.md)
3. Create an issue trên GitHub

---

Made with ❤️ for Unity UI Testing
=======
[![Unity](https://img.shields.io/badge/Unity-2020.3%2B-black.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Framework kiểm thử UI tự động cho Unity, hỗ trợ cấu hình test cases dạng ScriptableObject với các bước (steps) tuần tự và timeout per-step.

## 📋 Tổng quan

UI Automation Test là một framework kiểm thử tự động được thiết kế đặc biệt cho Unity UI. Framework này cho phép bạn:

- ✅ Tạo test cases dạng ScriptableObject có thể tái sử dụng
- ✅ Xây dựng test scenarios với các bước tuần tự
- ✅ Quản lý timeout cho từng bước test
- ✅ Tương tác với UI elements thông qua hierarchy path hoặc ObjectID
- ✅ Chạy và theo dõi tests real-time trong Unity Editor
- ✅ Mở rộng framework với custom actions và conditions

## 🎯 Tính năng chính

### 1. **ScriptableObject-Based Testing**
Test cases được lưu dưới dạng assets có thể tái sử dụng, dễ dàng chia sẻ và quản lý version control.

### 2. **Step-Based Architecture**
Mỗi test case bao gồm các bước (steps) độc lập:
- **ActionStepSO**: Thực hiện hành động UI (Press, Input, Assert, etc.)
- **WaitForConditionStepSO**: Chờ điều kiện được thỏa mãn
- **WaitTimeStepSO**: Chờ thời gian cố định

### 3. **Timeout Management**
Hỗ trợ timeout riêng cho từng step, tránh test bị treo vô thời hạn.

### 4. **Object Lookup System**
Hai cách tìm GameObjects:
- **Hierarchy Path**: `"Canvas/PlayButton"`
- **ObjectID**: `"id:PlayButton"` (ổn định hơn khi UI thay đổi cấu trúc)

### 5. **Editor Window Integration**
UITestRunner Window cho phép chạy và monitor tests trực tiếp trong Editor với hiển thị trạng thái real-time.

### 6. **Extensibility**
Dễ dàng mở rộng với custom test steps, actions và conditions.

## 🚀 Bắt đầu nhanh

### Cài đặt

1. Clone repository này
2. Copy thư mục `UI Test` vào project Unity của bạn
3. Đảm bảo project sử dụng Unity 2020.3 trở lên

### Tạo Test Case đầu tiên

**Bước 1**: Tạo Test Case Asset
```
Assets → Create → UI Tests → Automated Test Case
```

**Bước 2**: Tạo các Test Steps

Ví dụ tạo step "Press Play Button":
```
Assets → Create → UI Tests → Steps → Action Step
- ActionType: Press
- Path: "Canvas/PlayButton"
- Timeout: 2 seconds
```

**Bước 3**: Gán steps vào Test Case

Trong Inspector của Test Case, kéo thả các step assets vào list `steps`.

**Bước 4**: Chạy Test

Mở `Tools → UI Test Runner`, kéo thả test case vào và nhấn "Run Test".

## 📖 Tài liệu chi tiết

Xem [UI Test/README.md](UI%20Test/README.md) để biết thêm chi tiết về:

- Kiến trúc framework
- Hướng dẫn sử dụng đầy đủ
- Tạo custom test steps, actions, conditions
- Best practices
- Troubleshooting

## 🏗️ Kiến trúc

```
UI Test/
├── UITest.cs                      # Core helper class cho UI interactions
├── UITestException.cs             # Exception handling
├── UITestScenarios.cs             # Example test scenarios
├── UIAutomatedTestCase.cs         # ScriptableObject test case container
├── UITestManager.cs               # Batch test runner
├── UITestRunner.cs                # Runtime test runner component
├── TestStepBase.cs                # Base class cho test steps
├── TestActionBase.cs              # Base class cho custom actions
├── TestConditionBase.cs           # Base class cho custom conditions
├── Steps/
│   ├── ActionStepSO.cs           # UI action step
│   ├── WaitForConditionStepSO.cs # Condition wait step
│   └── WaitTimeStepSO.cs         # Time wait step
├── Editor/
│   └── UITestRunnerWindow.cs     # Editor window for test runner
├── Object ID/
│   └── ObjectID.cs               # Component for stable object lookup
├── SOExamples/                    # Example custom actions/conditions
└── Test Demo/                     # Demo test cases
```

## 💡 Ví dụ sử dụng

### Ví dụ 1: Test Main Menu Flow

```csharp
public static IEnumerator Scenario_MainMenu_Play(MonoBehaviour host)
{
    var t = new UITest();
    
    // Tải scene MainMenu
    yield return t.LoadScene(host, "MainMenu");
    
    // Kiểm tra title hiển thị đúng
    yield return t.AssertLabel(host, "Canvas/Title", "Welcome");
    
    // Nhấn nút Play
    yield return t.Press(host, "Canvas/PlayButton");
    
    // Chờ scene GameScene load
    yield return t.WaitFor(host, new UITest.SceneLoaded("GameScene"));
}
```

### Ví dụ 2: Test Input Fields

```csharp
public static IEnumerator Scenario_InputPlayerName(MonoBehaviour host)
{
    var t = new UITest();
    
    // Nhập tên vào InputField
    yield return t.InputText(host, "Canvas/NameInputField", "Player123");
    
    // Bật toggle "Remember Me"
    yield return t.SetToggle(host, "Canvas/RememberToggle", true);
    
    // Nhấn nút Submit
    yield return t.Press(host, "Canvas/SubmitButton");
}
```

## 🔧 Core Components

### UITest
Helper class cung cấp các phương thức tương tác UI:
- `Press()` - Nhấn button
- `AssertLabel()` - Kiểm tra text label
- `InputText()` - Nhập text
- `SetToggle()` - Bật/tắt toggle
- `WaitFor()` - Chờ điều kiện
- `LoadScene()` - Tải scene
- Và nhiều hơn nữa...

### UIAutomatedTestCase
ScriptableObject container cho test cases với:
- Danh sách steps tuần tự
- Stop on error configuration
- Description field

### UITestRunner
Runtime component để chạy tests với:
- Real-time status tracking
- Step-by-step execution
- Auto cleanup sau khi test hoàn thành

### UITestRunnerWindow
Editor window cung cấp:
- UI để chọn và chạy test cases
- Real-time progress visualization
- Step status với color coding (Yellow: Running, Green: Pass, Red: Fail)
- Timeout display cho mỗi step

## 🎨 Best Practices

1. **Sử dụng ObjectID** thay vì hierarchy path khi có thể
2. **Tái sử dụng step assets** - Tạo thư viện steps phổ biến
3. **Đặt tên rõ ràng** - Đặt tên mô tả hành động (vd: "PressPlayButton")
4. **Sử dụng note field** để ghi chú mục đích của step
5. **Thiết lập timeout per-step** để tránh test bị treo
6. **Tách logic phức tạp** thành custom TestStepBase
7. **Kiểm tra stopOnError** - true để dừng khi lỗi
8. **Tổ chức thư mục** - Tạo folder riêng cho Steps, Actions, Conditions

## 🔌 Mở rộng Framework

### Tạo Custom Test Step

```csharp
[CreateAssetMenu(menuName = "UI Tests/Steps/My Custom Step")]
public class MyCustomStepSO : TestStepBase
{
    public string myParameter;
    
    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        Debug.Log($"Running: {myParameter}");
        // Your custom logic here
        yield return null;
    }
}
```

### Tạo Custom Action

```csharp
[CreateAssetMenu(menuName = "UI Tests/Actions/My Action")]
public class MyCustomActionSO : TestActionSO
{
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        // Your action logic
        yield return null;
    }
}
```

### Tạo Custom Condition

```csharp
[CreateAssetMenu(menuName = "UI Tests/Conditions/My Condition")]
public class MyConditionSO : TestCondition
{
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        // Your condition logic
        return true;
    }
}
```

## 🐛 Troubleshooting

**Timeout khi chờ condition**
- Tăng `timeout` hoặc `timeoutSeconds` trên step
- Kiểm tra điều kiện có đúng không

**ObjectID không tìm thấy**
- Đảm bảo GameObject có component `ObjectID`
- Kiểm tra Id khớp với giá trị tìm kiếm

**UITestRunner không tự xóa**
- Kiểm tra test có hoàn thành không
- Xem logs để tìm step bị treo

## 📝 TODO

- [ ] Add CI/CD integration examples
- [ ] Add more built-in conditions
- [ ] Support for mobile gesture testing
- [ ] Performance testing utilities
- [ ] Test report generation

## 🤝 Đóng góp

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.


## 🙏 Acknowledgments

- Unity Technologies for the Unity Engine
- All contributors to this project

---
