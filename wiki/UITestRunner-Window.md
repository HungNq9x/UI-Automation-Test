# UITestRunner Window - Hướng dẫn chi tiết

UITestRunner Window là công cụ Editor mạnh mẽ cho phép bạn chạy và monitor UI tests trực tiếp trong Unity Editor với real-time feedback.

## Mở Window

```
Tools → UI Test Runner
```

hoặc

```
Window → UI Test Runner
```

Window sẽ dock được và có thể đặt cạnh Inspector hoặc console để thuận tiện theo dõi.

## Giao diện Window

```
┌─────────────────────────────────────┐
│ UI Test Runner                   [X]│
├─────────────────────────────────────┤
│                                     │
│ Test Case: [Drag Test Case Here ]  │
│                                     │
│ [ Run Test ]  [ Stop Test ]        │
│                                     │
├─────────────────────────────────────┤
│ Status: Idle                        │
├─────────────────────────────────────┤
│ Steps:                              │
│ ┌─────────────────────────────────┐ │
│ │ 1. Wait For Main Menu           │ │
│ │ 2. Press Play Button [timeout:2]│ │
│ │ 3. Wait For Game Ready          │ │
│ │ 4. Assert Game Started          │ │
│ └─────────────────────────────────┘ │
└─────────────────────────────────────┘
```

## Các thành phần

### 1. Test Case Field
- **Drag & Drop**: Kéo UIAutomatedTestCase asset từ Project vào đây
- **Clear**: Bấm X để clear selection
- **Hiển thị**: Tên test case hiện tại

### 2. Control Buttons

#### Run Test Button
- **Hiển thị khi**: Không có test đang chạy
- **Chức năng**: 
  - Start test case đã chọn
  - Tự động enter Play mode nếu cần
  - Tạo UITestRunner GameObject
- **Shortcut**: Có thể map keyboard shortcut

#### Stop Test Button
- **Hiển thị khi**: Test đang chạy
- **Chức năng**: 
  - Dừng test ngay lập tức
  - Cleanup UITestRunner
  - Exit Play mode (tùy chọn)

### 3. Status Display

Hiển thị trạng thái hiện tại:
- **"Idle"**: Không có test chạy
- **"Running step N/M..."**: Đang chạy step N trong tổng M steps
- **"SUCCESS: All steps passed!"**: Tất cả steps thành công (màu xanh)
- **"FAILED: [error message]"**: Test thất bại với error message (màu đỏ)

### 4. Steps List

Danh sách tất cả steps trong test case với:
- **Số thứ tự**: 1, 2, 3, ...
- **Tên step**: Note hoặc ToString() của step
- **Timeout**: Hiển thị `[timeout: Xs]` nếu step có timeout riêng
- **Màu sắc**:
  - Trắng/Xám: Chưa chạy
  - **Vàng (Bold)**: Đang chạy
  - **Xanh**: Pass ✓
  - **Đỏ**: Fail ✗

## Workflow sử dụng

### Workflow cơ bản

1. **Chuẩn bị Test Case**
   - Tạo UIAutomatedTestCase asset
   - Thêm các steps cần thiết

2. **Load Test**
   - Mở UITestRunner Window
   - Kéo test case asset vào field

3. **Run Test**
   - Click "Run Test"
   - Editor tự động enter Play mode
   - Test bắt đầu chạy

4. **Monitor Progress**
   - Theo dõi status updates
   - Xem step nào đang chạy (màu vàng)
   - Check timeout hiển thị

5. **Review Results**
   - Xem steps pass/fail
   - Đọc error messages nếu có
   - Check console logs cho chi tiết

### Quick Test Iteration

```
1. Write test case
2. Run test → Observe failure
3. Fix issue
4. Run test again (Editor còn ở Play mode)
5. Repeat until pass
```

**Tip**: Không cần exit Play mode giữa các lần chạy test nếu muốn iterate nhanh.

## Play Mode Behavior

### Auto Play Mode Entry

Khi click "Run Test" ở Edit mode:
1. Window calls `EditorApplication.EnterPlaymode()`
2. Đợi Play mode sẵn sàng
3. UITestRunner được tạo trong scene
4. Test bắt đầu execute

### Manual Play Mode

Bạn cũng có thể:
1. Enter Play mode trước
2. Sau đó mới chọn test case và run
3. Linh hoạt hơn cho debugging

### Stay in Play Mode

Window có thể configure để:
- Stay in Play mode sau test complete (để inspect)
- Auto exit Play mode sau test complete (cho CI)

## Monitoring Features

### Real-time Step Updates

Window update liên tục khi test chạy:
```
Status: Running step 2/5...
Steps:
  1. Wait For Main Menu        [✓]
  2. Press Play Button         [← RUNNING]
  3. Wait For Game Ready       [ ]
  4. Assert Game Started       [ ]
  5. Verify Score Display      [ ]
```

### Timeout Visibility

Steps với timeout riêng hiển thị rõ ràng:
```
1. Wait For Main Menu [timeout: 5s]
2. Press Play Button [timeout: 2s]
3. Wait For Game Ready [timeout: 10s]
```

Giúp bạn biết step nào có thể timeout và cần bao lâu.

### Error Messages

Khi step fail, message chi tiết hiển thị:
```
Status: FAILED: Step timed out after 5 seconds: WaitForMainMenu

// hoặc

Status: FAILED: Đối tượng label Canvas/StatusLabel không tồn tại
```

## Advanced Usage

### Running Multiple Tests

Để chạy nhiều tests:
1. Chạy test đầu tiên
2. Đợi complete
3. Chọn test case khác
4. Click "Run Test" again

**Note**: Có thể dùng UITestManager để batch run nhiều tests tự động.

### Debugging với Window

#### Pause and Inspect
```
// Thêm WaitTime steps để pause
1. Login
2. WaitTime: 5s  ← Inspect UI tại đây
3. Navigate
```

#### Screenshot on Failure
```csharp
// Add to test setup
if (UITestRunner.HasFailed)
{
    ScreenCapture.CaptureScreenshot($"failure_{Time.time}.png");
}
```

#### Step-by-Step Execution

Không có built-in step-by-step debugger, nhưng có thể:
1. Run đến step fail
2. Comment out steps sau đó
3. Re-run để focus vào issue
4. Iterate

### Integration với Version Control

Window state không được save, nhưng:
- Test case assets được version control
- Window config có thể code vào EditorPrefs

```csharp
// Save last test case
EditorPrefs.SetString("LastTestCase", AssetDatabase.GetAssetPath(testCase));

// Load on window open
string lastPath = EditorPrefs.GetString("LastTestCase");
selectedTestCase = AssetDatabase.LoadAssetAtPath<UIAutomatedTestCase>(lastPath);
```

## Performance Tips

### Window Update Rate

Window polls UITestRunner mỗi frame trong `OnEditorUpdate()`:
- Rất responsive
- Overhead minimal

Nếu cần optimize:
```csharp
// Modify UITestRunnerWindow.cs
float lastUpdateTime;
void OnEditorUpdate()
{
    if (Time.realtimeSinceStartup - lastUpdateTime < 0.1f) return; // 10 FPS
    lastUpdateTime = Time.realtimeSinceStartup;
    
    // Update logic...
}
```

### Large Test Cases

Với test cases có nhiều steps (50+):
- Scroll view tự động
- Performance vẫn tốt
- Consider breaking thành smaller tests

## Customization

### Extending Window

Bạn có thể extend UITestRunnerWindow:

```csharp
public class MyTestRunnerWindow : UITestRunnerWindow
{
    [MenuItem("Tools/My Test Runner")]
    static void OpenMyWindow()
    {
        GetWindow<MyTestRunnerWindow>("My Tests");
    }
    
    protected override void OnGUI()
    {
        base.OnGUI();
        
        // Add custom UI
        if (GUILayout.Button("Custom Action"))
        {
            // Your logic
        }
    }
}
```

### Custom Styling

```csharp
// Modify colors
var passColor = new Color(0.5f, 1f, 0.5f); // Green
var failColor = new Color(1f, 0.5f, 0.5f); // Red
var runningColor = new Color(1f, 1f, 0.5f); // Yellow
```

## Keyboard Shortcuts

Có thể setup shortcuts cho:
```csharp
[MenuItem("Tools/UI Test Runner _F5")] // F5 to open
[MenuItem("Tools/Run Test _F6")]       // F6 to run
```

## Troubleshooting Window

### Window không update

**Giải pháp**:
1. Close và reopen window
2. Check console cho errors
3. Verify UITestRunner tồn tại trong scene

### Play mode không tự động enter

**Kiểm tra**:
- Có compile errors không?
- Editor có đang compile code không?

**Giải pháp**:
- Fix all errors
- Wait for compilation
- Retry

### Steps không hiển thị màu

**Nguyên nhân**: UITestRunner.StepResults không sync

**Giải pháp**:
- Check UITestRunner.cs logic
- Verify Step tracking code
- Restart Editor

### Window bị freeze

**Giải pháp**:
- Force close window (dock to new position)
- Restart Unity
- Check for infinite loops trong tests

## Best Practices

### ✅ Do's

- Keep window visible khi run tests
- Monitor step progress real-time
- Check timeout values displayed
- Read error messages carefully
- Use window cho quick iterations

### ❌ Don'ts

- Đừng close window khi test đang chạy (có thể orphan UITestRunner)
- Đừng spam "Run Test" button
- Đừng rely solely on window (check Console too)
- Đừng ignore timeout warnings

## Comparison với Other Methods

| Feature | UITestRunner Window | UITestManager | Code-based |
|---------|-------------------|---------------|------------|
| Ease of Use | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ |
| Real-time Monitoring | ✅ | ❌ | ❌ |
| Quick Iteration | ✅ | ❌ | ⭐ |
| Batch Testing | ❌ | ✅ | ✅ |
| CI/CD | ❌ | ✅ | ✅ |
| Debugging | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐ |

**Khuyến nghị**: 
- Development: UITestRunner Window
- CI/CD: UITestManager hoặc Code-based

## FAQ

**Q: Có thể run nhiều tests cùng lúc không?**  
A: Không, chỉ một test tại một thời điểm.

**Q: Window có save test case selection không?**  
A: Không mặc định, nhưng có thể implement với EditorPrefs.

**Q: Có thể run test ở Edit mode không?**  
A: Không, tests cần Play mode để chạy.

**Q: Window có hoạt động với Test Runner package không?**  
A: Đây là separate system, không integrate với Unity Test Runner.

**Q: Có thể customize window layout không?**  
A: Có, bằng cách modify source code `UITestRunnerWindow.cs`.

## Xem thêm

- [[Getting Started]] - Setup và first test
- [[Test Steps]] - Available step types
- [[Best Practices]] - Tips and recommendations
- [[Troubleshooting]] - Common issues
- [[Architecture]] - How it works under the hood
