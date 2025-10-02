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
