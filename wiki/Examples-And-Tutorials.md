# Ví dụ và Tutorials

Tổng hợp các ví dụ thực tế và tutorials để học cách sử dụng UI Test Framework.

## Table of Contents

- [Tutorial 1: Login Test](#tutorial-1-login-test)
- [Tutorial 2: Shop Purchase Flow](#tutorial-2-shop-purchase-flow)
- [Tutorial 3: Settings Menu Test](#tutorial-3-settings-menu-test)
- [Tutorial 4: Form Validation](#tutorial-4-form-validation)
- [Tutorial 5: Game Flow Test](#tutorial-5-game-flow-test)
- [Code Examples](#code-examples)

---

## Tutorial 1: Login Test

### Mô tả
Test flow đăng nhập cơ bản: nhập username, password, và verify login thành công.

### Setup Scene

```
Canvas
├── LoginPanel
│   ├── UsernameField (InputField)
│   ├── PasswordField (InputField)
│   └── LoginButton (Button)
└── HomeScreen
    └── WelcomeLabel (Text)
```

### Thêm ObjectIDs

1. Select `UsernameField` → Add Component → ObjectID → Set Id = "UsernameField"
2. Select `PasswordField` → Add Component → ObjectID → Set Id = "PasswordField"
3. Select `LoginButton` → Add Component → ObjectID → Set Id = "LoginButton"
4. Select `HomeScreen` → Add Component → ObjectID → Set Id = "HomeScreen"

### Tạo Test Steps

#### Step 1: Wait for Login Panel
```
Create → UI Tests → Steps → Wait For Condition
Name: WaitForLoginPanel

Preconditions:
  [0] ConditionType: ObjectAppeared
      Path: "id:UsernameField"
  [1] ConditionType: ObjectAppeared
      Path: "id:LoginButton"

Precondition Mode: All
Timeout: 5
```

#### Step 2: Input Username
```
Create → UI Tests → Steps → Action Step
Name: InputUsername

Action Type: InputText
Path: "id:UsernameField"
Text: "testuser@example.com"
Timeout Seconds: 2
```

#### Step 3: Input Password
```
Create → UI Tests → Steps → Action Step
Name: InputPassword

Action Type: InputText
Path: "id:PasswordField"
Text: "password123"
Timeout Seconds: 2
```

#### Step 4: Press Login
```
Create → UI Tests → Steps → Action Step
Name: PressLogin

Action Type: Press
Path: "id:LoginButton"
Timeout Seconds: 2
```

#### Step 5: Wait for Home Screen
```
Create → UI Tests → Steps → Wait For Condition
Name: WaitForHomeScreen

Preconditions:
  [0] ConditionType: ObjectAppeared
      Path: "id:HomeScreen"

Timeout: 10
```

### Tạo Test Case

```
Create → UI Tests → Automated Test Case
Name: LoginTest

Description: "Test basic login flow with valid credentials"
Stop On Error: true

Steps:
  [0] WaitForLoginPanel
  [1] InputUsername
  [2] InputPassword
  [3] PressLogin
  [4] WaitForHomeScreen
```

### Chạy Test

1. `Tools → UI Test Runner`
2. Kéo `LoginTest` vào field
3. Click "Run Test"
4. Observe results

---

## Tutorial 2: Shop Purchase Flow

### Mô tả
Test flow mua hàng: navigate đến shop, chọn item, verify giá, purchase.

### Test Case Structure

```
PurchaseTest
├── 1. Navigate to Shop
├── 2. Wait for Shop to Load
├── 3. Select Item
├── 4. Verify Price
├── 5. Click Buy Button
├── 6. Confirm Purchase
└── 7. Verify Purchase Success
```

### Step Details

#### Step 1: Navigate to Shop
```
Action Step - Press
Path: "id:ShopButton"
Note: "Open shop from main menu"
```

#### Step 2: Wait for Shop
```
Wait For Condition
Preconditions:
  - ObjectAppeared: "id:ShopPanel"
  - SelectableInteractable: "id:Item_Sword"
Mode: All
Timeout: 5
```

#### Step 3: Select Item
```
Action Step - Press
Path: "id:Item_Sword"
Note: "Select sword item"
```

#### Step 4: Verify Price
```
Action Step - AssertLabel
Path: "id:ItemPriceLabel"
Text: "100"
Note: "Verify sword costs 100 gold"
```

#### Step 5: Click Buy
```
Action Step - Press
Path: "id:BuyButton"
```

#### Step 6: Confirm Purchase
```
Wait For Condition
Preconditions:
  - ObjectAppeared: "id:ConfirmDialog"
Mode: Any
Timeout: 3

Action Step - Press
Path: "id:ConfirmYesButton"
```

#### Step 7: Verify Success
```
Wait For Condition
Preconditions:
  - LabelTextEquals: "id:StatusMessage" = "Purchase Successful"
Timeout: 5
```

---

## Tutorial 3: Settings Menu Test

### Mô tả
Test settings menu: toggle sound, adjust volume, change quality.

### Custom Step: Apply Settings

```csharp
[CreateAssetMenu(menuName = "UI Tests/Steps/Apply Settings")]
public class ApplySettingsStepSO : TestStepBase
{
    public bool soundEnabled = true;
    public float volume = 0.75f;
    public int qualityLevel = 2;
    
    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        // Open settings
        yield return uiTest.Press(host, "id:SettingsButton");
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Toggle sound
        var soundToggle = UITest.FindByPathOrId("id:SoundToggle");
        var toggle = soundToggle?.GetComponent<Toggle>();
        if (toggle != null && toggle.isOn != soundEnabled)
        {
            yield return uiTest.Press(host, "id:SoundToggle");
        }
        
        // Set volume
        yield return uiTest.SetSlider(host, "id:VolumeSlider", volume);
        
        // Set quality
        yield return uiTest.SelectDropdown(host, "id:QualityDropdown", qualityLevel);
        
        // Apply
        yield return uiTest.Press(host, "id:ApplyButton");
        
        Debug.Log($"Settings applied: Sound={soundEnabled}, Volume={volume}, Quality={qualityLevel}");
    }
}
```

### Test Case

```
SettingsTest
├── 1. Apply Settings (Custom Step)
│   ├── soundEnabled: true
│   ├── volume: 0.75
│   └── qualityLevel: 2
├── 2. Close Settings
└── 3. Verify Settings Persisted
```

---

## Tutorial 4: Form Validation

### Mô tả
Test form validation với invalid inputs.

### Test Case: Invalid Email

```
InvalidEmailTest
├── 1. Wait for Registration Form
├── 2. Input Invalid Email
├── 3. Input Valid Password
├── 4. Press Submit
└── 5. Assert Error Message
```

#### Step 2: Input Invalid Email
```
Action Step - InputText
Path: "id:EmailField"
Text: "notanemail"
Note: "Enter invalid email format"
```

#### Step 5: Assert Error
```
Wait For Condition
Preconditions:
  - LabelTextEquals: "id:ErrorLabel" = "Invalid email format"
Timeout: 3
```

### Test Case: Password Too Short

```
ShortPasswordTest
├── 1. Input Valid Email
├── 2. Input Short Password (< 8 chars)
├── 3. Press Submit
└── 4. Assert Password Error
```

### Custom Condition: Form Has Errors

```csharp
[CreateAssetMenu(menuName = "UI Tests/Conditions/Form Has Errors")]
public class FormHasErrorsConditionSO : TestCondition
{
    public string formPath = "id:RegistrationForm";
    
    public override bool Evaluate(MonoBehaviour host, UITest uiTest)
    {
        var form = UITest.FindByPathOrId(formPath);
        if (form == null) return false;
        
        // Look for any active error labels
        var errorLabels = form.GetComponentsInChildren<Text>()
            .Where(t => t.name.Contains("Error") && t.gameObject.activeInHierarchy)
            .ToArray();
            
        return errorLabels.Length > 0;
    }
}
```

---

## Tutorial 5: Game Flow Test

### Mô tả
End-to-end test: Main menu → Start game → Play → Game over → Return to menu.

### Test Case Structure

```
CompleteGameFlowTest
├── 1. Wait for Main Menu
├── 2. Start Game
├── 3. Wait for Game Start
├── 4. Play Until Game Over (Custom Step)
├── 5. Verify Game Over Screen
├── 6. Return to Main Menu
└── 7. Verify Back at Main Menu
```

### Custom Step: Play Until Game Over

```csharp
[CreateAssetMenu(menuName = "UI Tests/Steps/Play Until Game Over")]
public class PlayUntilGameOverStepSO : TestStepBase
{
    public float maxPlayTime = 60f;
    public float actionInterval = 1f;
    
    public override IEnumerator Execute(MonoBehaviour host, UITest uiTest)
    {
        Debug.Log("[PlayGame] Starting gameplay...");
        
        float elapsed = 0f;
        
        while (elapsed < maxPlayTime)
        {
            // Check if game over
            var gameOverScreen = UITest.FindByPathOrId("id:GameOverScreen");
            if (gameOverScreen != null && gameOverScreen.activeInHierarchy)
            {
                Debug.Log("[PlayGame] Game over detected!");
                yield break;
            }
            
            // Simulate player action (e.g., click to shoot)
            var fireButton = UITest.FindByPathOrId("id:FireButton");
            if (fireButton != null && fireButton.activeInHierarchy)
            {
                yield return uiTest.Press(host, "id:FireButton");
            }
            
            // Wait before next action
            yield return new WaitForSecondsRealtime(actionInterval);
            elapsed += actionInterval;
        }
        
        throw new System.TimeoutException(
            $"Game did not end within {maxPlayTime} seconds"
        );
    }
}
```

---

## Code Examples

### Example 1: Programmatic Test Creation

```csharp
using System.Collections;
using UnityEngine;

public class ProgrammaticTestExample : MonoBehaviour
{
    IEnumerator Start()
    {
        // Create UITest instance
        var uiTest = new UITest();
        uiTest.WaitTimeout = 15f;
        
        Debug.Log("=== Starting Programmatic Test ===");
        
        // Wait for main menu
        Debug.Log("Waiting for main menu...");
        yield return uiTest.WaitFor(this, 
            new UITest.ObjectAppeared("id:MainMenu"));
        
        // Press play button
        Debug.Log("Pressing play button...");
        yield return uiTest.Press(this, "id:PlayButton");
        
        // Wait for game to load
        Debug.Log("Waiting for game to load...");
        yield return uiTest.WaitFor(this,
            new UITest.LabelTextAppeared("id:StatusLabel", "Ready"));
        
        // Assert game started
        Debug.Log("Verifying game started...");
        yield return uiTest.AssertLabel(this, "id:TitleLabel", "Game Started");
        
        Debug.Log("=== Test Passed ===");
    }
}
```

### Example 2: Custom Test Runner

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTestRunner : MonoBehaviour
{
    public List<UIAutomatedTestCase> testCases;
    public bool runOnStart = true;
    
    void Start()
    {
        if (runOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }
    
    IEnumerator RunAllTests()
    {
        int passed = 0;
        int failed = 0;
        
        Debug.Log($"Running {testCases.Count} test cases...");
        
        foreach (var testCase in testCases)
        {
            Debug.Log($"--- Running: {testCase.name} ---");
            
            var uiTest = new UITest();
            bool success = true;
            
            // Run test and catch exceptions
            var enumerator = testCase.Run(this, uiTest);
            while (true)
            {
                object current = null;
                bool moveNext;
                
                try
                {
                    moveNext = enumerator.MoveNext();
                    if (moveNext) current = enumerator.Current;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Test failed: {ex.Message}");
                    success = false;
                    break;
                }
                
                if (!moveNext) break;
                yield return current;
            }
            
            if (success)
            {
                Debug.Log($"✓ {testCase.name} PASSED");
                passed++;
            }
            else
            {
                Debug.LogError($"✗ {testCase.name} FAILED");
                failed++;
            }
            
            // Wait between tests
            yield return new WaitForSecondsRealtime(1f);
        }
        
        Debug.Log($"=== Test Summary ===");
        Debug.Log($"Passed: {passed}");
        Debug.Log($"Failed: {failed}");
        Debug.Log($"Total: {testCases.Count}");
    }
}
```

### Example 3: Conditional Test Flow

```csharp
IEnumerator ConditionalFlowExample()
{
    var uiTest = new UITest();
    
    // Check if tutorial is active
    var tutorialPanel = UITest.FindByPathOrId("id:TutorialPanel");
    
    if (tutorialPanel != null && tutorialPanel.activeInHierarchy)
    {
        Debug.Log("Tutorial detected, completing it...");
        
        // Complete tutorial
        yield return uiTest.Press(this, "id:TutorialNextButton");
        yield return new WaitForSecondsRealtime(0.5f);
        yield return uiTest.Press(this, "id:TutorialNextButton");
        yield return new WaitForSecondsRealtime(0.5f);
        yield return uiTest.Press(this, "id:TutorialCompleteButton");
    }
    else
    {
        Debug.Log("No tutorial, proceeding to main menu...");
    }
    
    // Continue with main test flow
    yield return uiTest.Press(this, "id:StartButton");
}
```

### Example 4: Data-Driven Testing

```csharp
[System.Serializable]
public class LoginTestData
{
    public string username;
    public string password;
    public bool shouldSucceed;
    public string expectedMessage;
}

public class DataDrivenLoginTest : MonoBehaviour
{
    public List<LoginTestData> testData;
    
    IEnumerator Start()
    {
        var uiTest = new UITest();
        
        foreach (var data in testData)
        {
            Debug.Log($"Testing login: {data.username}");
            
            // Input credentials
            yield return uiTest.InputText(this, "id:Username", data.username);
            yield return uiTest.InputText(this, "id:Password", data.password);
            
            // Press login
            yield return uiTest.Press(this, "id:LoginButton");
            
            // Wait for result
            yield return new WaitForSecondsRealtime(1f);
            
            // Verify message
            yield return uiTest.AssertLabel(this, "id:MessageLabel", data.expectedMessage);
            
            Debug.Log($"Test passed for: {data.username}");
            
            // Reset for next test
            yield return uiTest.Press(this, "id:LogoutButton");
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
}
```

---

## Tips and Tricks

### Tip 1: Debug Screenshots

Thêm screenshot steps để debug:
```
Step 1: Action
Step 2: Screenshot (Custom)
Step 3: Wait
Step 4: Screenshot (Custom)
```

### Tip 2: Wait Patterns

```csharp
// Wait với timeout custom
float prevTimeout = uiTest.WaitTimeout;
uiTest.WaitTimeout = 30f;
yield return uiTest.WaitFor(this, condition);
uiTest.WaitTimeout = prevTimeout;

// Wait với retry
for (int i = 0; i < 3; i++)
{
    try
    {
        yield return uiTest.SomeAction(this);
        break; // Success
    }
    catch
    {
        if (i == 2) throw; // Last attempt
        yield return new WaitForSecondsRealtime(1f);
    }
}
```

### Tip 3: Reusable Test Helpers

```csharp
public static class TestHelpers
{
    public static IEnumerator Login(MonoBehaviour host, string user, string pass)
    {
        var uiTest = new UITest();
        yield return uiTest.InputText(host, "id:Username", user);
        yield return uiTest.InputText(host, "id:Password", pass);
        yield return uiTest.Press(host, "id:LoginButton");
    }
    
    public static IEnumerator NavigateToShop(MonoBehaviour host)
    {
        var uiTest = new UITest();
        yield return uiTest.Press(host, "id:ShopButton");
        yield return uiTest.WaitFor(host, 
            new UITest.ObjectAppeared("id:ShopPanel"));
    }
}
```

---

## Repository Examples

Xem thêm examples trong repository:

### Built-in Examples
- `SOExamples/` - Custom action và condition examples
- `Test Demo/` - Demo test cases và runtime examples

### Demo Script
- `UITestRuntimeDemo.cs` - Programmatic test example

---

## Xem thêm

- [[Getting Started]] - Basic setup
- [[Test Steps]] - Available steps
- [[Custom Test Steps]] - Creating custom steps
- [[Best Practices]] - Recommendations
- [[API Reference]] - Complete API documentation
