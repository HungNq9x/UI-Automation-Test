# Hướng dẫn bắt đầu

Hướng dẫn này sẽ giúp bạn bắt đầu sử dụng UI Test Framework để tạo và chạy test cases tự động trong Unity.

## Yêu cầu hệ thống

- Unity 2019.4 hoặc mới hơn
- Hỗ trợ Legacy Unity UI hoặc TextMeshPro
- (Tùy chọn) Odin Inspector để cải thiện Editor UI

## Cài đặt

1. Import folder `UI Test` vào project Unity của bạn
2. Framework sẽ tự động được Unity nhận diện
3. Kiểm tra cài đặt bằng cách mở `Tools → UI Test Runner`

## Tạo Test Case đầu tiên

### Bước 1: Tạo Test Case Asset

1. Trong Project window, chuột phải chọn:
   ```
   Create → UI Tests → Automated Test Case
   ```
2. Đặt tên cho test case, ví dụ: `LoginTest`
3. Trong Inspector, điền description cho test case (tùy chọn)

### Bước 2: Tạo Test Steps

Tạo các step assets cho test case. Ví dụ test một flow đăng nhập:

#### Step 1: Chờ màn hình đăng nhập xuất hiện

```
Create → UI Tests → Steps → Wait For Condition
```

Cấu hình:
- **Preconditions**: Thêm một condition
  - Type: `ObjectAppeared`
  - Path: `Canvas/LoginPanel`
- **Timeout**: `5` giây
- Lưu với tên: `WaitForLoginScreen`

#### Step 2: Nhập username

```
Create → UI Tests → Steps → Action Step
```

Cấu hình:
- **Action Type**: `InputText`
- **Path**: `Canvas/LoginPanel/UsernameField`
- **Text**: `testuser@example.com`
- **Timeout Seconds**: `2`
- Lưu với tên: `InputUsername`

#### Step 3: Nhập password

```
Create → UI Tests → Steps → Action Step
```

Cấu hình:
- **Action Type**: `InputText`
- **Path**: `Canvas/LoginPanel/PasswordField`
- **Text**: `password123`
- **Timeout Seconds**: `2`
- Lưu với tên: `InputPassword`

#### Step 4: Nhấn nút Login

```
Create → UI Tests → Steps → Action Step
```

Cấu hình:
- **Action Type**: `Press`
- **Path**: `id:LoginButton`  *(sử dụng ObjectID)*
- **Timeout Seconds**: `2`
- Lưu với tên: `PressLoginButton`

#### Step 5: Chờ đăng nhập thành công

```
Create → UI Tests → Steps → Wait For Condition
```

Cấu hình:
- **Preconditions**: Thêm một condition
  - Type: `LabelTextEquals`
  - Path: `Canvas/WelcomeLabel`
  - Expected Text: `Welcome!`
- **Timeout**: `10` giây
- Lưu với tên: `WaitForWelcome`

### Bước 3: Gán Steps vào Test Case

1. Mở test case `LoginTest` trong Inspector
2. Trong phần **Steps**, click nút `+` để thêm step
3. Kéo thả các step assets theo thứ tự:
   - `[0]` WaitForLoginScreen
   - `[1]` InputUsername
   - `[2]` InputPassword
   - `[3]` PressLoginButton
   - `[4]` WaitForWelcome
4. Bật tùy chọn **Stop On Error** để test dừng khi có lỗi

### Bước 4: Chạy Test

#### Cách 1: Sử dụng UI Test Runner Window (Khuyến nghị)

1. Mở `Tools → UI Test Runner`
2. Kéo thả `LoginTest` asset vào field "Test Case"
3. Nhấn **Run Test**
4. Editor sẽ tự động chuyển sang Play mode và chạy test
5. Theo dõi tiến trình trong window:
   - Màu vàng = Step đang chạy
   - Màu xanh = Step thành công
   - Màu đỏ = Step thất bại

#### Cách 2: Sử dụng UITestManager Component

1. Tạo empty GameObject trong scene test của bạn
2. Add component `UITestManager`
3. Trong Inspector, thêm `LoginTest` vào list **Test Cases**
4. Bật **Auto Run On Start** để tự động chạy khi scene start
5. Enter Play mode để chạy test

#### Cách 3: Chạy từ code

```csharp
using System.Collections;
using UnityEngine;

public class LoginTestRunner : MonoBehaviour
{
    public UIAutomatedTestCase loginTest;
    
    IEnumerator Start()
    {
        var uiTest = new UITest();
        uiTest.WaitTimeout = 15f; // Set timeout mặc định
        
        Debug.Log("Starting login test...");
        yield return StartCoroutine(loginTest.Run(this, uiTest));
        Debug.Log("Login test completed!");
    }
}
```

## Sử dụng ObjectID

Để tìm kiếm UI elements ổn định hơn khi cấu trúc hierarchy thay đổi:

1. Chọn GameObject muốn đánh dấu (ví dụ: LoginButton)
2. Add component `ObjectID`
3. Đặt **Id** field = `LoginButton`
4. Trong test steps, sử dụng path: `id:LoginButton`

**Lợi ích**: Khi bạn di chuyển hoặc tái cấu trúc UI, test vẫn hoạt động mà không cần sửa path.

## Xem kết quả

Khi test chạy:

### Thành công ✅
- UITestRunner Window hiển thị: **"SUCCESS: All steps passed!"**
- Tất cả steps có màu xanh
- Console log: Step-by-step execution

### Thất bại ❌
- UITestRunner Window hiển thị: **"FAILED: [error message]"**
- Step thất bại có màu đỏ
- Console log chi tiết lỗi và stack trace
- Test dừng lại (nếu Stop On Error = true)

## Các bước tiếp theo

- Đọc [[Best Practices]] để viết tests tốt hơn
- Tìm hiểu [[Test Steps]] để biết tất cả loại steps có sẵn
- Xem [[Examples And Tutorials]] để học từ ví dụ thực tế
- Tìm hiểu [[Custom Test Steps]] để tạo logic test riêng

## Troubleshooting

### Test không chạy
- Kiểm tra test case có steps không rỗng
- Đảm bảo các step assets tồn tại và không bị missing reference

### Step timeout
- Tăng `timeoutSeconds` trên step đó
- Kiểm tra path hoặc id có đúng không
- Xem [[Troubleshooting]] để biết thêm chi tiết

### GameObject không tìm thấy
- Kiểm tra hierarchy path chính xác (case-sensitive)
- Nếu dùng ObjectID, kiểm tra component đã được add và Id đúng
- Đảm bảo GameObject active trong scene
