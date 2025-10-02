using System.Collections;
using UnityEngine;

/// <summary>
/// Ví dụ các test scenarios sử dụng UITest framework.
/// </summary>
public static class UITestScenarios
{
    /// <summary>
    /// Ví dụ: Test main menu mở và nhấn Play.
    /// </summary>
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

    /// <summary>
    /// Ví dụ: Test nhập tên người chơi.
    /// </summary>
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

    /// <summary>
    /// Ví dụ: Test settings slider.
    /// </summary>
    public static IEnumerator Scenario_AdjustSettings(MonoBehaviour host)
    {
        var t = new UITest();
        
        // Tải scene Settings
        yield return t.LoadScene(host, "Settings");
        
        // Điều chỉnh volume slider
        yield return t.SetSlider(host, "Canvas/VolumeSlider", 0.75f);
        
        // Chọn quality dropdown
        yield return t.SelectDropdown(host, "Canvas/QualityDropdown", 2);
        
        // Nhấn Save
        yield return t.Press(host, "Canvas/SaveButton");
    }

    /// <summary>
    /// Ví dụ: Test scroll trong danh sách.
    /// </summary>
    public static IEnumerator Scenario_ScrollList(MonoBehaviour host)
    {
        var t = new UITest();
        
        // Chờ ScrollView xuất hiện
        yield return t.WaitFor(host, new UITest.ObjectAppeared("Canvas/ScrollView"));
        
        // Cuộn xuống
        yield return t.Scroll(host, "Canvas/ScrollView", new Vector2(0, -100));
        
        // Chờ item cuối cùng hiển thị
        yield return t.WaitFor(host, new UITest.ObjectAppeared("Canvas/ScrollView/Content/Item_50"));
    }

    /// <summary>
    /// Ví dụ: Test với điều kiện tùy chỉnh.
    /// </summary>
    public static IEnumerator Scenario_CustomCondition(MonoBehaviour host)
    {
        var t = new UITest { WaitTimeout = 15f };
        
        // Chờ một điều kiện boolean tùy chỉnh
        // yield return t.WaitFor(host, new UITest.BoolCondition(() => GameManager.Instance.IsReady));
        
        // Chờ animation play
        var player = GameObject.Find("Player");
        if (player != null)
        {
            yield return t.WaitFor(host, new UITest.AnimatorStateEntered(player, "Idle"));
        }
    }
}
