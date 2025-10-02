# UI Test Framework - Trang chủ

Chào mừng đến với Wiki của **UI Test Framework** - Framework kiểm thử UI tự động cho Unity!

## 📋 Tổng quan

UI Test Framework là một framework mạnh mẽ cho phép bạn tạo và chạy các test cases UI tự động trong Unity. Framework sử dụng ScriptableObject để tạo các bước test có thể tái sử dụng và cấu hình dễ dàng mà không cần viết code.

### Tính năng chính

- ✅ **Test Cases dạng ScriptableObject** - Không cần code, tạo test bằng Unity Editor
- ✅ **Reusable Test Steps** - Tái sử dụng các bước test trong nhiều test cases
- ✅ **Per-Step Timeout** - Kiểm soát timeout cho từng bước test
- ✅ **Object Lookup linh hoạt** - Tìm kiếm theo hierarchy path hoặc ObjectID
- ✅ **UI Test Runner Window** - Monitor và debug test real-time trong Editor
- ✅ **Custom Actions & Conditions** - Dễ dàng mở rộng với logic tùy chỉnh
- ✅ **Batch Testing** - Chạy nhiều test cases tuần tự với UITestManager
- ✅ **Rich Built-in Actions** - Press, Input, Assert, Drag, Hover, và nhiều hơn nữa

## 🚀 Bắt đầu nhanh

1. **Tạo Test Case**: `Assets → Create → UI Tests → Automated Test Case`
2. **Tạo Test Steps**: `Assets → Create → UI Tests → Steps → [Action/Wait/...]`
3. **Gán Steps vào Test Case**: Kéo thả các step assets vào list
4. **Chạy Test**: Mở `Tools → UI Test Runner` và chọn test case

## 📚 Nội dung Wiki

### Hướng dẫn cơ bản
- **[[Hướng dẫn bắt đầu|Getting-Started]]** - Cài đặt và tạo test đầu tiên
- **[[Hướng dẫn sử dụng UITestRunner Window|UITestRunner-Window]]** - Công cụ chạy và monitor test
- **[[Object Lookup|Object-Lookup]]** - Cách tìm kiếm UI elements

### Kiến trúc và Components
- **[[Kiến trúc hệ thống|Architecture]]** - Tổng quan về kiến trúc framework
- **[[Core Components|Core-Components]]** - UITest, UIAutomatedTestCase, UITestRunner
- **[[Test Steps|Test-Steps]]** - ActionStepSO, WaitForConditionStepSO, WaitTimeStepSO

### Mở rộng và tùy chỉnh
- **[[Tạo Custom Test Steps|Custom-Test-Steps]]** - Tạo step types mới
- **[[Tạo Custom Actions|Custom-Actions]]** - Mở rộng ActionStepSO
- **[[Tạo Custom Conditions|Custom-Conditions]]** - Mở rộng WaitForConditionStepSO

### Thực hành và Tips
- **[[Best Practices|Best-Practices]]** - Các thực hành tốt nhất khi viết tests
- **[[Ví dụ và Tutorials|Examples-And-Tutorials]]** - Các ví dụ thực tế
- **[[Troubleshooting|Troubleshooting]]** - Khắc phục các vấn đề thường gặp

### Tham khảo
- **[[API Reference|API-Reference]]** - Chi tiết về các class và method
- **[[Action Types Reference|Action-Types-Reference]]** - Danh sách đầy đủ các action types
- **[[Condition Types Reference|Condition-Types-Reference]]** - Danh sách các condition types

## 🔗 Links hữu ích

- [README chính](../UI%20Test/README.md) - Documentation đầy đủ trong repository
- [Demo Scripts](../UI%20Test/Test%20Demo/) - Ví dụ code và test cases
- [SO Examples](../UI%20Test/SOExamples/) - Ví dụ custom actions và conditions

## 💡 Ví dụ nhanh

```csharp
// Chạy test từ code
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

## 🤝 Đóng góp

Nếu bạn muốn đóng góp hoặc báo lỗi, vui lòng tạo issue trên GitHub repository.

## 📝 Ghi chú

- Framework này yêu cầu Unity 2019.4 trở lên
- Tương thích với cả Legacy UI và TextMeshPro
- Hỗ trợ Odin Inspector để cải thiện UI Editor (không bắt buộc)
