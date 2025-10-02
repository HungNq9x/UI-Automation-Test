# Test Steps - Các bước kiểm thử

Document này mô tả chi tiết các loại Test Steps có sẵn trong framework.

## Tổng quan

Test Steps là các ScriptableObject đại diện cho một hành động hoặc điều kiện chờ trong test case. Mỗi step có thể:
- Được tạo và lưu thành asset để tái sử dụng
- Có timeout riêng (`timeoutSeconds`)
- Có note/description để dễ đọc
- Được sắp xếp tuần tự trong test case

## Base Class: TestStepBase

Tất cả test steps đều kế thừa từ `TestStepBase`:

```csharp
public abstract class TestStepBase : ScriptableObject
{
    [TextArea]
    public string note;              // Mô tả step (hiển thị trong test runner)
    
    [Tooltip("Optional per-step timeout in seconds. 0 means use UITest.WaitTimeout.")]
    public float timeoutSeconds = 0f; // Timeout riêng cho step này
    
    public abstract IEnumerator Execute(MonoBehaviour host, UITest uiTest);
}
```

## 1. ActionStepSO

### Mô tả
Thực hiện một hành động UI như nhấn button, nhập text, assert label, drag and drop, v.v.

### Tạo Asset
```
Assets → Create → UI Tests → Steps → Action Step
```

### Properties

| Property | Type | Mô tả |
|----------|------|-------|
| `actionType` | ActionType enum | Loại hành động cần thực hiện |
| `path` | string | Hierarchy path hoặc `id:ObjectID` |
| `path2` | string | Path thứ hai (cho DragAndDrop) |
| `text` | string | Text input (cho AssertLabel, InputText) |
| `floatValue` | float | Giá trị số (cho WaitSeconds, SetSlider, Hold) |
| `intValue` | int | Giá trị int (cho SelectDropdown) |
| `boolValue` | bool | Giá trị bool (cho SetToggle) |
| `customActionSO` | TestActionSO | Custom action (nếu muốn dùng thay vì inline) |

### Action Types

#### Press
Nhấn một button hoặc UI element có thể tương tác.

**Cấu hình**:
- `path`: Đường dẫn đến button/element

**Ví dụ**:
```
actionType = Press
path = "Canvas/MainMenu/PlayButton"
timeoutSeconds = 2
```

#### AssertLabel
Kiểm tra text của một Label (Text hoặc TextMeshPro).

**Cấu hình**:
- `path`: Đường dẫn đến Label
- `text`: Text mong đợi

**Ví dụ**:
```
actionType = AssertLabel
path = "Canvas/StatusLabel"
text = "Ready"
timeoutSeconds = 1
```

#### LoadScene
Load một scene mới.

**Cấu hình**:
- `path`: Tên scene hoặc build index (dạng số)

**Ví dụ**:
```
actionType = LoadScene
path = "GameScene"
timeoutSeconds = 10
```

#### InputText
Nhập text vào InputField hoặc TMP_InputField.

**Cấu hình**:
- `path`: Đường dẫn đến InputField
- `text`: Text cần nhập

**Ví dụ**:
```
actionType = InputText
path = "Canvas/LoginPanel/UsernameField"
text = "testuser@example.com"
timeoutSeconds = 2
```

#### SetToggle
Bật/tắt một Toggle component.

**Cấu hình**:
- `path`: Đường dẫn đến Toggle
- `boolValue`: true = bật, false = tắt

**Ví dụ**:
```
actionType = SetToggle
path = "Canvas/Settings/SoundToggle"
boolValue = true
```

#### WaitSeconds
Chờ một khoảng thời gian (giống WaitTimeStepSO nhưng inline).

**Cấu hình**:
- `floatValue`: Số giây chờ

**Ví dụ**:
```
actionType = WaitSeconds
floatValue = 2.5
```

#### DragAndDrop
Kéo một object từ vị trí này sang vị trí khác.

**Cấu hình**:
- `path`: Object nguồn (kéo từ đây)
- `path2`: Object đích (thả vào đây)

**Ví dụ**:
```
actionType = DragAndDrop
path = "Canvas/Inventory/Item1"
path2 = "Canvas/EquipmentSlot"
timeoutSeconds = 3
```

#### RaycastClick
Click vào vị trí sử dụng raycast (hữu ích cho 3D UI).

**Cấu hình**:
- `path`: Đường dẫn đến object

**Ví dụ**:
```
actionType = RaycastClick
path = "Canvas/WorldMap/Location5"
```

#### SelectDropdown
Chọn option trong Dropdown.

**Cấu hình**:
- `path`: Đường dẫn đến Dropdown
- `intValue`: Index của option (bắt đầu từ 0)

**Ví dụ**:
```
actionType = SelectDropdown
path = "Canvas/Settings/QualityDropdown"
intValue = 2  // Chọn option thứ 3
```

#### SetSlider
Đặt giá trị cho Slider.

**Cấu hình**:
- `path`: Đường dẫn đến Slider
- `floatValue`: Giá trị cần set

**Ví dụ**:
```
actionType = SetSlider
path = "Canvas/Settings/VolumeSlider"
floatValue = 0.75
```

#### Hover
Di chuột hover lên một element (trigger PointerEnter event).

**Cấu hình**:
- `path`: Đường dẫn đến element

**Ví dụ**:
```
actionType = Hover
path = "Canvas/Tooltip/HelpIcon"
```

#### Hold
Giữ nhấn một button trong khoảng thời gian.

**Cấu hình**:
- `path`: Đường dẫn đến button
- `floatValue`: Thời gian giữ (giây)

**Ví dụ**:
```
actionType = Hold
path = "Canvas/ChargeButton"
floatValue = 3.0
```

### Custom Actions

Thay vì dùng inline action types, bạn có thể gán một `TestActionSO` custom:

```csharp
[CreateAssetMenu(menuName = "UI Tests/Actions/Play Sound")]
public class PlaySoundActionSO : TestActionSO
{
    public AudioClip clip;
    
    public override IEnumerator Run(MonoBehaviour host, UITest uiTest)
    {
        AudioSource.PlayClipAtPoint(clip, Vector3.zero);
        yield return null;
    }
}
```

Sau đó trong ActionStepSO, gán asset vào field `customActionSO`.

## 2. WaitForConditionStepSO

### Mô tả
Chờ đến khi một hoặc nhiều điều kiện được thỏa mãn (hoặc timeout).

### Tạo Asset
```
Assets → Create → UI Tests → Steps → Wait For Condition
```

### Properties

| Property | Type | Mô tả |
|----------|------|-------|
| `preconditions` | List<ConditionSpec> | Danh sách điều kiện inline |
| `preconditionSOs` | List<TestCondition> | Danh sách custom condition assets |
| `preconditionMode` | ConditionMatchMode | All = tất cả phải đúng, Any = một trong số đó đúng |
| `timeout` | float | Timeout riêng cho step này (giây) |

### Inline Condition Types

#### ObjectAppeared
Kiểm tra một GameObject xuất hiện và active.

**Cấu hình**:
- `conditionType` = ObjectAppeared
- `path`: Đường dẫn đến object

**Ví dụ**:
```
conditionType = ObjectAppeared
path = "Canvas/MainMenu"
```

#### LabelTextEquals
Kiểm tra label có text mong đợi.

**Cấu hình**:
- `conditionType` = LabelTextEquals
- `path`: Đường dẫn đến label
- `expectedText`: Text mong đợi

**Ví dụ**:
```
conditionType = LabelTextEquals
path = "Canvas/StatusLabel"
expectedText = "Connected"
```

#### SelectableInteractable
Kiểm tra Selectable component có thể tương tác (enabled và interactable).

**Cấu hình**:
- `conditionType` = SelectableInteractable
- `path`: Đường dẫn đến UI element

**Ví dụ**:
```
conditionType = SelectableInteractable
path = "Canvas/SubmitButton"
```

### Condition Match Mode

- **All**: Tất cả conditions phải true (AND logic)
- **Any**: Một trong các conditions true (OR logic)

### Custom Conditions

Tạo custom condition bằng cách kế thừa `TestCondition`:

```csharp
[CreateAssetMenu(menuName = "UI Tests/Conditions/Player Ready")]
public class PlayerReadyConditionSO : TestCondition
{
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        return PlayerManager.Instance != null && 
               PlayerManager.Instance.IsReady;
    }
}
```

Sau đó thêm asset vào list `preconditionSOs`.

### Ví dụ đầy đủ

```
Preconditions (inline):
  [0] ObjectAppeared: "Canvas/LoadingScreen"
  [1] LabelTextEquals: "Canvas/LoadingScreen/ProgressText" = "100%"
  
Precondition SOs:
  [0] DataLoadedConditionSO
  
Precondition Mode: All
Timeout: 15 seconds
```

Condition này chờ đến khi:
- Loading screen xuất hiện
- Progress text = "100%"
- Data đã load xong (custom condition)

## 3. WaitTimeStepSO

### Mô tả
Chờ một khoảng thời gian cố định (unscaled time).

### Tạo Asset
```
Assets → Create → UI Tests → Steps → Wait Time
```

### Properties

| Property | Type | Mô tả |
|----------|------|-------|
| `seconds` | float | Số giây chờ (>= 0) |

### Ví dụ

```
seconds = 1.5
note = "Wait for animation to complete"
```

**Lưu ý**: WaitTimeStepSO sử dụng unscaled time, không bị ảnh hưởng bởi `Time.timeScale`.

## Best Practices cho Test Steps

### 1. Đặt tên rõ ràng
```
✅ Tốt: "WaitForMainMenu", "PressPlayButton", "AssertWelcomeMessage"
❌ Không tốt: "Step1", "Wait", "Action"
```

### 2. Sử dụng note field
```
note = "Wait for loading screen to disappear"
note = "Press the Play button to start game"
```

### 3. Set timeout hợp lý
```
// Fast actions
timeoutSeconds = 1-2

// UI transitions
timeoutSeconds = 5

// Scene loading
timeoutSeconds = 10-15

// Network operations
timeoutSeconds = 30
```

### 4. Tái sử dụng steps
Tạo library của các common steps:
```
Common/
├── WaitForMainMenu.asset
├── PressBackButton.asset
├── WaitHalfSecond.asset
└── AssertNoErrors.asset
```

### 5. Tổ chức thư mục
```
Tests/
├── LoginFlow/
│   ├── LoginTestCase.asset
│   ├── Steps/
│   │   ├── InputUsername.asset
│   │   ├── InputPassword.asset
│   │   └── PressLogin.asset
└── GameplayFlow/
    ├── ...
```

### 6. Sử dụng ObjectID cho stability
```
✅ Tốt: "id:PlayButton"
❌ Rủi ro: "Canvas/MainMenu/ButtonPanel/Row1/PlayButton"
```

### 7. Combine conditions hiệu quả
```
// Thay vì nhiều WaitForConditionSteps:
// Step1: Wait for object
// Step2: Wait for text
// Step3: Wait for interactable

// Gộp thành một:
WaitForConditionStep:
  - ObjectAppeared
  - LabelTextEquals
  - SelectableInteractable
  Mode: All
```

## Troubleshooting Steps

### Step timeout
- Tăng `timeoutSeconds`
- Kiểm tra condition logic
- Check console logs để biết step đang chờ gì

### GameObject not found
- Verify path (case-sensitive)
- Check hierarchy structure
- Ensure object is active
- If using ObjectID, verify component exists and Id matches

### Action không có hiệu ứng
- Check element có interactable không
- Verify event system hoạt động
- Check button/element listeners

### Condition never satisfied
- Debug condition logic
- Add debug logs trong custom conditions
- Verify expected values
- Check timing (có thể cần thêm wait trước condition)

## Xem thêm

- [[Custom Test Steps]] - Tạo step types mới
- [[Custom Actions]] - Tạo custom actions
- [[Custom Conditions]] - Tạo custom conditions
- [[Best Practices]] - Tips viết tests tốt hơn
