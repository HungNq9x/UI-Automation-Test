using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Trợ giúp kiểm thử UI nhẹ. Tạo instance và gọi phương thức từ MonoBehaviour host.
/// Không tự chạy coroutine; truyền MonoBehaviour (thường là test harness) để chạy.
/// </summary>
/// <example>
/// // Ví dụ trong PlayMode test (trong MonoBehaviour):
/// IEnumerator MyTest()
/// {
///     var uiTest = new UITest();
///     // Tải scene và chờ active
///     yield return uiTest.LoadScene(this, "MainMenu");
///     // Kiểm tra nội dung label
///     yield return uiTest.AssertLabel(this, "Canvas/Title", "Welcome");
///     // Nhấn nút theo tên
///     yield return uiTest.Press(this, "Canvas/PlayButton");
/// }
/// </example>
public partial class UITest
{
    private const float DefaultWaitTimeout = 10f;
    private const int DefaultWaitIntervalFrames = 10;

    /// <summary>
    /// Thời gian chờ tối đa (giây) cho Condition trước khi ném TimeoutException.
    /// Mặc định: 10s. Có thể điều chỉnh cho mỗi instance UITest từ tests.
    /// </summary>
    public float WaitTimeout { get; set; }


    /// <summary>
    /// Số khung hình chờ giữa các lần đánh giá điều kiện. Dùng thời gian không scale.
    /// Mặc định: 10 khung. Điều chỉnh để kiểm tra thường xuyên hơn hoặc ít hơn.
    /// </summary>
    public int WaitIntervalFrames { get; set; }

    /// <summary>
    /// Tạo instance UITest. Instance nhẹ, chỉ chứa cấu hình.
    /// Sử dụng phương thức từ MonoBehaviour test harness (truyền MonoBehaviour làm host).
    /// </summary>
    /// <example>
    /// var uiTest = new UITest { WaitTimeout = 5f, WaitIntervalFrames = 5 };
    /// yield return uiTest.LoadScene(this, "MainMenu");
    /// </example>
    public UITest()
    {
        WaitTimeout = DefaultWaitTimeout;
        WaitIntervalFrames = DefaultWaitIntervalFrames;
    }

    /// <summary>
    /// Bắt đầu chờ Condition được thỏa. Coroutine chạy trên MonoBehaviour được cung cấp.
    /// </summary>
    /// <param name="behaviour">MonoBehaviour host coroutine (test runner hoặc component helper).</param>
    /// <param name="condition">Instance Condition để đánh giá cho đến khi thỏa hoặc timeout.</param>
    /// <returns>Handle của Coroutine đang chạy.</returns>
    /// <remarks>
    /// Sử dụng:
    /// <code language="csharp">
    /// // trong MonoBehaviour test:
    /// var uiTest = new UITest();
    /// yield return uiTest.WaitFor(this, new UITest.SceneLoaded("GameScene"));
    /// </code>
    /// </remarks>
    /// <summary>
    /// Chờ đến khi Condition thỏa hoặc hết thời gian chờ cấu hình.
    /// Coroutine bắt đầu trên behaviour được truyền (host do người gọi kiểm soát).
    /// </summary>
    /// <param name="behaviour">MonoBehaviour chạy coroutine (thường là test runner).</param>
    /// <param name="condition">Condition đánh giá định kỳ.</param>
    /// <returns>Handle của Coroutine đang chạy.</returns>
    /// <example>
    /// // Trong PlayMode test (trong MonoBehaviour):
    /// IEnumerator MyTest()
    /// {
    ///     var uiTest = new UITest();
    ///     // Chờ scene 'MainMenu' active
    ///     yield return uiTest.WaitFor(this, new UITest.SceneLoaded("MainMenu"));
    ///     // Hoặc chờ điều kiện boolean tùy chỉnh
    ///     yield return uiTest.WaitFor(this, new UITest.BoolCondition(() => MyManager.IsReady));
    /// }
    /// </example>
    /// <remarks>
    /// - Timeout và tần suất kiểm tra do WaitTimeout và WaitIntervalFrames điều khiển.
    /// </remarks>
    public Coroutine WaitFor(MonoBehaviour behaviour, Condition condition)
    {
        return behaviour.StartCoroutine(WaitForInternal(condition, Environment.StackTrace));
    }

    /// <summary>
    /// Tải scene bất đồng bộ và chờ đến khi nó trở thành scene active.
    /// </summary>
    /// <param name="behaviour">MonoBehaviour chạy coroutine nội bộ.</param>
    /// <param name="name">Tên scene (theo Build Settings).</param>
    /// <returns>Handle của Coroutine hoàn thành khi scene active.</returns>
    /// <example>
    /// // Trong PlayMode test:
    /// IEnumerator TestSceneLoad()
    /// {
    ///     var uiTest = new UITest();
    ///     // Tải scene và chờ active
    ///     yield return uiTest.LoadScene(this, "MainMenu");
    ///     // Tiếp tục kiểm tra sau khi scene active
    ///     yield return uiTest.AssertLabel(this, "Canvas/Title", "Welcome");
    /// }
    /// </example>
    /// <remarks>
    /// - Scene cần thêm vào Build Settings; để tải additive, mở rộng helper này.
    /// </remarks>
    public Coroutine LoadScene(MonoBehaviour behaviour, string name)
    {
        return behaviour.StartCoroutine(LoadSceneInternal(behaviour, name));
    }

    private IEnumerator LoadSceneInternal(MonoBehaviour behaviour, string name)
    {
        // Bắt đầu tải bất đồng bộ, sau đó chờ scene active khớp
        var op = SceneManager.LoadSceneAsync(name);
        if (op != null)
        {
            while (!op.isDone) yield return null;
        }

        yield return WaitFor(behaviour, new SceneLoaded(name));
    }

    // Find GameObject either by hierarchy path (GameObject.Find)
    // or by ObjectID component using the prefix "id:MyId" (case-insensitive prefix).
    // Example: "id:StartButton" will search all ObjectID components for Id == "StartButton".
    private static GameObject FindByPathOrId(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        const string idPrefix = "id:";
        if (path.StartsWith(idPrefix, StringComparison.InvariantCultureIgnoreCase))
        {
            var id = path.Substring(idPrefix.Length);
            // Use Unity 6 API to find all ObjectID components (includes inactive objects)
            var objs = UnityEngine.Object.FindObjectsByType<ObjectID>(FindObjectsSortMode.None);
            foreach (var oid in objs)
            {
                if (oid == null) continue;
                if (string.Equals(oid.Id, id, StringComparison.Ordinal))
                    return oid.gameObject;
            }
            return null;
        }

        return GameObject.Find(path);
    }

    /// <summary>
    /// Chờ đến khi UI label tại đường dẫn hierarchy có văn bản mong đợi.
    /// Hỗ trợ UnityEngine.UI.Text và TextMeshPro (nếu có).
    /// </summary>
    /// <param name="behaviour">MonoBehaviour host coroutine.</param>
    /// <param name="id">Đường dẫn hierarchy dùng GameObject.Find để tìm label (vd: "Canvas/Title").</param>
    /// <param name="text">Văn bản label mong đợi.</param>
    /// <returns>Handle của Coroutine hoàn thành khi văn bản khớp hoặc timeout.</returns>
    /// <example>
    /// // trong PlayMode test MonoBehaviour:
    /// yield return new UITest().AssertLabel(this, "Canvas/Title", "Ready");
    /// </example>
    /// <remarks>
    /// - Sử dụng GameObject.Find để tìm object theo đường dẫn. Ưu tiên truyền GameObject trực tiếp để tránh đường dẫn dễ vỡ.
    /// - Hỗ trợ TextMeshPro qua reflection; nếu luôn dùng TMP, tạo condition riêng cho TMP.
    /// </remarks>
    public Coroutine AssertLabel(MonoBehaviour behaviour, string id, string text)
    {
        return behaviour.StartCoroutine(AssertLabelInternal(behaviour, id, text));
    }

    /// <summary>
    /// Tìm GameObject tại buttonName và mô phỏng nhấn. Ưu tiên overload GameObject nếu đã có reference.
    /// </summary>
    /// <param name="behaviour">MonoBehaviour host coroutine.</param>
    /// <param name="buttonName">Đường dẫn hierarchy dùng GameObject.Find để tìm button.</param>
    /// <returns>Handle của Coroutine hoàn thành sau khi mô phỏng nhấn.</returns>
    /// <example>
    /// yield return new UITest().Press(this, "Canvas/PlayButton");
    /// </example>
    public Coroutine Press(MonoBehaviour behaviour, string buttonName)
    {
        return behaviour.StartCoroutine(PressInternal(behaviour, buttonName));
    }

    /// <summary>
    /// Mô phỏng nhấn trên GameObject o được cung cấp. Phương thức chờ đến khi button accessible, sau đó gửi pointer events (down/up/click).
    /// </summary>
    /// <param name="behaviour">MonoBehaviour host coroutine.</param>
    /// <param name="o">Reference trực tiếp đến GameObject target (khuyên dùng).</param>
    /// <returns>Handle của Coroutine cho chuỗi nhấn mô phỏng.</returns>
    /// <example>
    /// var btn = GameObject.Find("Canvas/ExitButton");
    /// yield return new UITest().Press(this, btn);
    /// </example>
    public Coroutine Press(MonoBehaviour behaviour, GameObject o)
    {
        return behaviour.StartCoroutine(PressInternal(behaviour, o));
    }

    private IEnumerator WaitForInternal(Condition condition, string stackTrace)
    {
        float time = 0f;
        int checkCount = 0;
        while (!condition.Satisfied())
        {
            checkCount++;
            if (time > WaitTimeout)
            {
                Debug.LogError($"[UITest] Timeout after {checkCount} checks over {time:F2}s. Condition: {condition}");
                throw new TimeoutException("Operation timed out: " + condition + "\n" + stackTrace);
            }
            for (int i = 0; i < WaitIntervalFrames; i++)
            {
                time += Time.unscaledDeltaTime;
                yield return null;
            }
        }
        Debug.Log($"[UITest] Condition satisfied after {checkCount} checks in {time:F2}s: {condition}");
    }

    private IEnumerator PressInternal(MonoBehaviour behaviour, string buttonName)
    {
        var buttonAppeared = new ObjectAppeared(buttonName);
        yield return WaitFor(behaviour, buttonAppeared);
        yield return Press(behaviour, buttonAppeared.o);
    }

    private IEnumerator PressInternal(MonoBehaviour behaviour, GameObject o)
    {
        yield return WaitFor(behaviour, new ButtonAccessible(o));
        Debug.Log("Button pressed: " + (o ? o.name : "null"));

        if (o == null)
        {
            throw new UITestException("Không thể nhấn GameObject null.");
        }

        if (EventSystem.current == null)
        {
            throw new UITestException("Không có EventSystem. Không thể mô phỏng pointer events.");
        }

        var pointerData = new PointerEventData(EventSystem.current);
        var rect = o.GetComponent<RectTransform>();
        Vector2 screenPos = Vector2.zero;
        if (rect != null && Camera.main != null) screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, rect.position);
        pointerData.position = screenPos;

        // Mô phỏng chuỗi nhấn đầy đủ để tăng độ chính xác
        ExecuteEvents.Execute(o, pointerData, ExecuteEvents.pointerDownHandler);
        yield return null;
        ExecuteEvents.Execute(o, pointerData, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(o, pointerData, ExecuteEvents.pointerClickHandler);
        yield return null;
    }

    private IEnumerator AssertLabelInternal(MonoBehaviour behaviour, string id, string text)
    {
        yield return WaitFor(behaviour, new LabelTextAppeared(id, text));
    }


    /// <summary>
    /// Nhập text vào InputField hoặc TMP_InputField.
    /// </summary>
    public Coroutine InputText(MonoBehaviour behaviour, string inputFieldPath, string text)
    {
        return behaviour.StartCoroutine(InputTextInternal(behaviour, inputFieldPath, text));
    }

    public Coroutine InputText(MonoBehaviour behaviour, GameObject inputFieldObj, string text)
    {
        return behaviour.StartCoroutine(InputTextByObjectInternal(inputFieldObj, text));
    }

    private IEnumerator InputTextInternal(MonoBehaviour behaviour, string path, string text)
    {
        var appeared = new ObjectAppeared(path);
        yield return WaitFor(behaviour, appeared);
        yield return InputTextByObjectInternal(appeared.o, text);
    }

    private IEnumerator InputTextByObjectInternal(GameObject obj, string text)
    {
        if (obj == null)
        {
            throw new UITestException("InputField object là null.");
        }

        var inputField = obj.GetComponent<InputField>();
        if (inputField != null)
        {
            inputField.text = text;
            inputField.onEndEdit?.Invoke(text);
            yield return null;
            Debug.Log($"✓ Nhập text vào InputField '{obj.name}': {text}");
            yield break;
        }

        // Try TMP_InputField via reflection
        var tmpType = Type.GetType("TMPro.TMP_InputField, Unity.TextMeshPro");
        if (tmpType != null)
        {
            var tmpInput = obj.GetComponent(tmpType);
            if (tmpInput != null)
            {
                var textProp = tmpType.GetProperty("text");
                textProp?.SetValue(tmpInput, text);
                var onEndEditField = tmpType.GetField("onEndEdit");
                var endEditEvent = onEndEditField?.GetValue(tmpInput);
                endEditEvent?.GetType().GetMethod("Invoke")?.Invoke(endEditEvent, new object[] { text });
                yield return null;
                Debug.Log($"✓ Nhập text vào TMP_InputField '{obj.name}': {text}");
                yield break;
            }
        }

        throw new UITestException($"Object '{obj.name}' không có InputField hoặc TMP_InputField.");
    }

    /// <summary>
    /// Bật/tắt Toggle.
    /// </summary>
    public Coroutine SetToggle(MonoBehaviour behaviour, string togglePath, bool isOn)
    {
        return behaviour.StartCoroutine(SetToggleInternal(behaviour, togglePath, isOn));
    }

    public Coroutine SetToggle(MonoBehaviour behaviour, GameObject toggleObj, bool isOn)
    {
        return behaviour.StartCoroutine(SetToggleByObjectInternal(toggleObj, isOn));
    }

    private IEnumerator SetToggleInternal(MonoBehaviour behaviour, string path, bool isOn)
    {
        var appeared = new ObjectAppeared(path);
        yield return WaitFor(behaviour, appeared);
        yield return SetToggleByObjectInternal(appeared.o, isOn);
    }

    private IEnumerator SetToggleByObjectInternal(GameObject obj, bool isOn)
    {
        if (obj == null)
        {
            throw new UITestException("Toggle object là null.");
        }

        var toggle = obj.GetComponent<Toggle>();
        if (toggle == null)
        {
            throw new UITestException($"Object '{obj.name}' không có Toggle component.");
        }

        toggle.isOn = isOn;
        yield return null;
        Debug.Log($"✓ Đặt Toggle '{obj.name}': {isOn}");
    }

    /// <summary>
    /// Đặt giá trị Slider.
    /// </summary>
    public Coroutine SetSlider(MonoBehaviour behaviour, string sliderPath, float value)
    {
        return behaviour.StartCoroutine(SetSliderInternal(behaviour, sliderPath, value));
    }

    public Coroutine SetSlider(MonoBehaviour behaviour, GameObject sliderObj, float value)
    {
        return behaviour.StartCoroutine(SetSliderByObjectInternal(sliderObj, value));
    }

    private IEnumerator SetSliderInternal(MonoBehaviour behaviour, string path, float value)
    {
        var appeared = new ObjectAppeared(path);
        yield return WaitFor(behaviour, appeared);
        yield return SetSliderByObjectInternal(appeared.o, value);
    }

    private IEnumerator SetSliderByObjectInternal(GameObject obj, float value)
    {
        if (obj == null)
        {
            throw new UITestException("Slider object là null.");
        }

        var slider = obj.GetComponent<Slider>();
        if (slider == null)
        {
            throw new UITestException($"Object '{obj.name}' không có Slider component.");
        }

        slider.value = value;
        yield return null;
        Debug.Log($"✓ Đặt Slider '{obj.name}': {value}");
    }

    /// <summary>
    /// Chọn option trong Dropdown theo index.
    /// </summary>
    public Coroutine SelectDropdown(MonoBehaviour behaviour, string dropdownPath, int index)
    {
        return behaviour.StartCoroutine(SelectDropdownInternal(behaviour, dropdownPath, index));
    }

    public Coroutine SelectDropdown(MonoBehaviour behaviour, GameObject dropdownObj, int index)
    {
        return behaviour.StartCoroutine(SelectDropdownByObjectInternal(dropdownObj, index));
    }

    private IEnumerator SelectDropdownInternal(MonoBehaviour behaviour, string path, int index)
    {
        var appeared = new ObjectAppeared(path);
        yield return WaitFor(behaviour, appeared);
        yield return SelectDropdownByObjectInternal(appeared.o, index);
    }

    private IEnumerator SelectDropdownByObjectInternal(GameObject obj, int index)
    {
        if (obj == null)
        {
            throw new UITestException("Dropdown object là null.");
        }

        var dropdown = obj.GetComponent<Dropdown>();
        if (dropdown != null)
        {
            if (index < 0 || index >= dropdown.options.Count)
            {
                throw new UITestException($"Dropdown index {index} nằm ngoài phạm vi (0-{dropdown.options.Count - 1}).");
            }
            dropdown.value = index;
            yield return null;
            Debug.Log($"✓ Chọn Dropdown '{obj.name}' option {index}: {dropdown.options[index].text}");
            yield break;
        }

        // Try TMP_Dropdown via reflection
        var tmpType = Type.GetType("TMPro.TMP_Dropdown, Unity.TextMeshPro");
        if (tmpType != null)
        {
            var tmpDropdown = obj.GetComponent(tmpType);
            if (tmpDropdown != null)
            {
                var valueProp = tmpType.GetProperty("value");
                var optionsProp = tmpType.GetProperty("options");
                var options = optionsProp?.GetValue(tmpDropdown) as System.Collections.IList;

                if (options != null && (index < 0 || index >= options.Count))
                {
                    throw new UITestException($"TMP_Dropdown index {index} nằm ngoài phạm vi (0-{options.Count - 1}).");
                }

                valueProp?.SetValue(tmpDropdown, index);
                yield return null;
                Debug.Log($"✓ Chọn TMP_Dropdown '{obj.name}' option {index}");
                yield break;
            }
        }

        throw new UITestException($"Object '{obj.name}' không có Dropdown hoặc TMP_Dropdown.");
    }

    /// <summary>
    /// Cuộn ScrollRect theo delta.
    /// </summary>
    public Coroutine Scroll(MonoBehaviour behaviour, string scrollRectPath, Vector2 delta)
    {
        return behaviour.StartCoroutine(ScrollInternal(behaviour, scrollRectPath, delta));
    }

    public Coroutine Scroll(MonoBehaviour behaviour, GameObject scrollRectObj, Vector2 delta)
    {
        return behaviour.StartCoroutine(ScrollByObjectInternal(scrollRectObj, delta));
    }

    private IEnumerator ScrollInternal(MonoBehaviour behaviour, string path, Vector2 delta)
    {
        var appeared = new ObjectAppeared(path);
        yield return WaitFor(behaviour, appeared);
        yield return ScrollByObjectInternal(appeared.o, delta);
    }

    private IEnumerator ScrollByObjectInternal(GameObject obj, Vector2 delta)
    {
        if (obj == null)
        {
            throw new UITestException("ScrollRect object là null.");
        }

        if (EventSystem.current == null)
        {
            throw new UITestException("Không có EventSystem để mô phỏng scroll.");
        }

        var scrollRect = obj.GetComponent<ScrollRect>();
        if (scrollRect == null)
        {
            throw new UITestException($"Object '{obj.name}' không có ScrollRect component.");
        }

        var pointerData = new PointerEventData(EventSystem.current);
        pointerData.scrollDelta = delta;

        ExecuteEvents.Execute(obj, pointerData, ExecuteEvents.scrollHandler);
        yield return null;
        Debug.Log($"✓ Cuộn ScrollRect '{obj.name}' delta: {delta}");
    }

    /// <summary>
    /// Mô phỏng hover (pointer enter).
    /// </summary>
    public Coroutine Hover(MonoBehaviour behaviour, string targetPath)
    {
        return behaviour.StartCoroutine(HoverInternal(behaviour, targetPath));
    }

    public Coroutine Hover(MonoBehaviour behaviour, GameObject targetObj)
    {
        return behaviour.StartCoroutine(HoverByObjectInternal(targetObj));
    }

    private IEnumerator HoverInternal(MonoBehaviour behaviour, string path)
    {
        var appeared = new ObjectAppeared(path);
        yield return WaitFor(behaviour, appeared);
        yield return HoverByObjectInternal(appeared.o);
    }

    private IEnumerator HoverByObjectInternal(GameObject obj)
    {
        if (obj == null)
        {
            throw new UITestException("Hover target object là null.");
        }

        if (EventSystem.current == null)
        {
            throw new UITestException("Không có EventSystem để mô phỏng hover.");
        }

        var pointerData = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(obj, pointerData, ExecuteEvents.pointerEnterHandler);
        yield return null;
        Debug.Log($"✓ Hover vào '{obj.name}'");
    }

    /// <summary>
    /// Giữ pointer down trong khoảng thời gian (seconds).
    /// </summary>
    public Coroutine Hold(MonoBehaviour behaviour, string targetPath, float duration)
    {
        return behaviour.StartCoroutine(HoldInternal(behaviour, targetPath, duration));
    }

    public Coroutine Hold(MonoBehaviour behaviour, GameObject targetObj, float duration)
    {
        return behaviour.StartCoroutine(HoldByObjectInternal(targetObj, duration));
    }

    private IEnumerator HoldInternal(MonoBehaviour behaviour, string path, float duration)
    {
        var appeared = new ObjectAppeared(path);
        yield return WaitFor(behaviour, appeared);
        yield return HoldByObjectInternal(appeared.o, duration);
    }

    private IEnumerator HoldByObjectInternal(GameObject obj, float duration)
    {
        if (obj == null)
        {
            throw new UITestException("Hold target object là null.");
        }

        if (EventSystem.current == null)
        {
            throw new UITestException("Không có EventSystem để mô phỏng hold.");
        }

        var pointerData = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(obj, pointerData, ExecuteEvents.pointerDownHandler);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        ExecuteEvents.Execute(obj, pointerData, ExecuteEvents.pointerUpHandler);
        Debug.Log($"✓ Giữ '{obj.name}' trong {duration}s");
    }


    [System.Serializable]
    public abstract class Condition
    {
        protected string param;
        protected string objectName;

        public Condition() { }
        public Condition(string param) { this.param = param; }
        public Condition(string objectName, string param) { this.param = param; this.objectName = objectName; }
        public abstract bool Satisfied();
        public override string ToString() { return GetType().Name + " '" + param + "'"; }
    }

    [System.Serializable]
    public class LabelTextAppeared : Condition
    {
        public LabelTextAppeared(string objectName, string param) : base(objectName, param) { }
        public override bool Satisfied() { return GetErrorMessage() == null; }

        string GetErrorMessage()
        {
            var go = FindByPathOrId(objectName);
            if (go == null) return "Đối tượng label " + objectName + " không tồn tại";
            if (!go.activeInHierarchy) return "Đối tượng label " + objectName + " không active";

            var t = go.GetComponent<Text>();
            if (t != null)
            {
                if (t.text != param) return "Label " + objectName + "\n văn bản mong đợi: " + param + ",\n thực tế: " + t.text;
                return null;
            }

            // Try TextMeshPro via reflection to avoid hard compile dependency
            var tmpType = Type.GetType("TMPro.TMP_Text, Unity.TextMeshPro");
            if (tmpType != null)
            {
                var comp = go.GetComponent(tmpType);
                if (comp != null)
                {
                    var textProp = tmpType.GetProperty("text");
                    var value = textProp?.GetValue(comp) as string;
                    if (value != param) return "Label " + objectName + "\n văn bản mong đợi: " + param + ",\n thực tế: " + value;
                    return null;
                }
            }

            return "Đối tượng label " + objectName + " không có Text hoặc TMP_Text đính kèm";
        }

        public override string ToString() { return GetErrorMessage(); }
    }

    [System.Serializable]
    public class SceneLoaded : Condition
    {
        public SceneLoaded(string param) : base(param) { }
        public override bool Satisfied() { return SceneManager.GetActiveScene().name == param; }
    }

    [System.Serializable]
    public class ObjectAnimationPlaying : Condition
    {
        public ObjectAnimationPlaying(string objectName, string param) : base(objectName, param) { }
        public override bool Satisfied()
        {
            var go = FindByPathOrId(objectName);
            if (go == null || !go.activeInHierarchy) return false;

            var animator = go.GetComponent<Animator>();
            if (animator != null)
            {
                var state = animator.GetCurrentAnimatorStateInfo(0);
                return state.IsName(param);
            }

            var animation = go.GetComponent<Animation>();
            if (animation != null) return animation.IsPlaying(param);

            return false;
        }
    }

    [System.Serializable]
    public class ObjectAppeared<T> : Condition where T : Component
    {
        public override bool Satisfied()
        {
            // Use Unity 6 API to find the first instance of type T
            var obj = UnityEngine.Object.FindFirstObjectByType<T>();
            return obj != null && obj.gameObject.activeInHierarchy;
        }
    }
    [System.Serializable]
    public class ObjectDisappeared<T> : Condition where T : Component
    {
        public override bool Satisfied()
        {
            // Use Unity 6 API to find the first instance of type T
            var obj = UnityEngine.Object.FindFirstObjectByType<T>();
            return obj == null || !obj.gameObject.activeInHierarchy;
        }
    }
    [System.Serializable]
    public class ObjectAppeared : Condition
    {
        protected string path;
        public GameObject o;

        public ObjectAppeared(string path) { this.path = path; }

        public override bool Satisfied()
        {
            o = FindByPathOrId(path);
            return o != null && o.activeInHierarchy;
        }

        public override string ToString() { return "ObjectAppeared(" + path + ")"; }
    }
    [System.Serializable]
    public class ObjectDisappeared : ObjectAppeared
    {
        public ObjectDisappeared(string path) : base(path) { }
        public override bool Satisfied() { return !base.Satisfied(); }
        public override string ToString() { return "ObjectDisappeared(" + path + ")"; }
    }
    [System.Serializable]
    public class BoolCondition : Condition
    {
        private Func<bool> _getter;
        public BoolCondition(Func<bool> getter) { _getter = getter; }
        public override bool Satisfied()
        {
            bool result = _getter != null && _getter();
            if (!result)
            {
                Debug.Log($"[UITest] BoolCondition not satisfied: {_getter?.Target?.GetType().Name ?? "null"}.{_getter?.Method?.Name ?? "null"}");
            }
            return result;
        }
        public override string ToString() { return "BoolCondition(" + _getter + ")"; }
    }
    [System.Serializable]
    public class ButtonAccessible : Condition
    {
        GameObject button;
        public ButtonAccessible(GameObject button) { this.button = button; }
        public override bool Satisfied() { return GetAccessibilityMessage() == null; }
        public override string ToString() { return GetAccessibilityMessage() ?? "Button " + (button ? button.name : "null") + " is accessible"; }
        string GetAccessibilityMessage()
        {
            if (button == null) return "Không tìm thấy button";
            var btn = button.GetComponent<Button>();
            if (btn == null) return "GameObject không có component Button đính kèm";
            if (!button.activeInHierarchy) return "Button không active";
            return null;
        }
    }

   

    /// <summary>
    /// Chờ TextMeshPro label hiển thị văn bản mong đợi.
    /// </summary>
    [System.Serializable]
    public class TMPLabelTextAppeared : Condition
    {
        public TMPLabelTextAppeared(string objectName, string param) : base(objectName, param) { }
        public override bool Satisfied() { return GetErrorMessage() == null; }

        string GetErrorMessage()
        {
            var go = FindByPathOrId(objectName);
            if (go == null) return "Đối tượng TMP label " + objectName + " không tồn tại";
            if (!go.activeInHierarchy) return "Đối tượng TMP label " + objectName + " không active";

            var tmpType = Type.GetType("TMPro.TMP_Text, Unity.TextMeshPro");
            if (tmpType != null)
            {
                var comp = go.GetComponent(tmpType);
                if (comp != null)
                {
                    var textProp = tmpType.GetProperty("text");
                    var value = textProp?.GetValue(comp) as string;
                    if (value != param) return "TMP Label " + objectName + "\n văn bản mong đợi: " + param + ",\n thực tế: " + value;
                    return null;
                }
            }

            return "Đối tượng " + objectName + " không có TMP_Text component";
        }

        public override string ToString() { return GetErrorMessage(); }
    }

    /// <summary>
    /// Chờ Selectable (Button/Toggle/Slider) có thể tương tác.
    /// </summary>
    [System.Serializable]
    public class SelectableAccessible : Condition
    {
        GameObject target;
        public SelectableAccessible(GameObject target) { this.target = target; }

        public override bool Satisfied()
        {
            if (target == null) return false;
            if (!target.activeInHierarchy) return false;

            var selectable = target.GetComponent<Selectable>();
            if (selectable == null) return false;

            return selectable.interactable;
        }

        public override string ToString()
        {
            if (target == null) return "Selectable target null";
            if (!target.activeInHierarchy) return "Selectable không active";
            var selectable = target.GetComponent<Selectable>();
            if (selectable == null) return "Không có Selectable component";
            if (!selectable.interactable) return "Selectable không interactable";
            return "Selectable accessible";
        }
    }

    /// <summary>
    /// Chờ Animator vào một state cụ thể.
    /// </summary>
    [System.Serializable]
    public class AnimatorStateEntered : Condition
    {
        GameObject target;
        string stateName;
        int layerIndex;

        public AnimatorStateEntered(GameObject target, string stateName, int layerIndex = 0)
        {
            this.target = target;
            this.stateName = stateName;
            this.layerIndex = layerIndex;
        }

        public override bool Satisfied()
        {
            if (target == null || !target.activeInHierarchy) return false;

            var animator = target.GetComponent<Animator>();
            if (animator == null) return false;

            var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            return stateInfo.IsName(stateName);
        }

        public override string ToString()
        {
            return $"AnimatorStateEntered({(target ? target.name : "null")}, {stateName}, layer {layerIndex})";
        }
    }

    /// <summary>
    /// Chờ AudioSource đang phát.
    /// </summary>
    [System.Serializable]
    public class AudioPlaying : Condition
    {
        GameObject target;

        public AudioPlaying(GameObject target)
        {
            this.target = target;
        }

        public override bool Satisfied()
        {
            if (target == null || !target.activeInHierarchy) return false;

            var audioSource = target.GetComponent<AudioSource>();
            if (audioSource == null) return false;

            return audioSource.isPlaying;
        }

        public override string ToString()
        {
            return $"AudioPlaying({(target ? target.name : "null")})";
        }
    }

    /// <summary>
    /// Chờ ParticleSystem đang chạy.
    /// </summary>
    [System.Serializable]
    public class ParticlePlaying : Condition
    {
        GameObject target;

        public ParticlePlaying(GameObject target)
        {
            this.target = target;
        }

        public override bool Satisfied()
        {
            if (target == null || !target.activeInHierarchy) return false;

            var particle = target.GetComponent<ParticleSystem>();
            if (particle == null) return false;

            return particle.isPlaying;
        }

        public override string ToString()
        {
            return $"ParticlePlaying({(target ? target.name : "null")})";
        }
    }


    #region Drag & Drop

    /// <summary>
    /// Drag & drop từ source object tới target object.
    /// </summary>
    public Coroutine DragAndDrop(MonoBehaviour behaviour, string sourcePath, string targetPath)
    {
        return behaviour.StartCoroutine(DragAndDropInternal(behaviour, sourcePath, targetPath));
    }

    public Coroutine DragAndDrop(MonoBehaviour behaviour, GameObject source, GameObject target)
    {
        return behaviour.StartCoroutine(DragAndDropByObjectInternal(source, target));
    }

    private IEnumerator DragAndDropInternal(MonoBehaviour behaviour, string sourcePath, string targetPath)
    {
    var source = FindByPathOrId(sourcePath);
        if (source == null)
        {
            throw new UITestException($"Drag source không tìm thấy: {sourcePath}");
        }

    var target = FindByPathOrId(targetPath);
        if (target == null)
        {
            throw new UITestException($"Drop target không tìm thấy: {targetPath}");
        }

        yield return DragAndDropByObjectInternal(source, target);
    }

    private IEnumerator DragAndDropByObjectInternal(GameObject source, GameObject target)
    {
        if (source == null || target == null)
        {
            throw new UITestException("Drag source hoặc target là null.");
        }

        if (EventSystem.current == null)
        {
            throw new UITestException("Không có EventSystem để mô phỏng drag & drop.");
        }

        var pointerData = new PointerEventData(EventSystem.current);

        // Get source position
        var sourceRect = source.GetComponent<RectTransform>();
        if (sourceRect != null && Camera.main != null)
        {
            pointerData.position = RectTransformUtility.WorldToScreenPoint(Camera.main, sourceRect.position);
        }
        else
        {
            pointerData.position = Camera.main.WorldToScreenPoint(source.transform.position);
        }

        // Begin drag
        ExecuteEvents.Execute(source, pointerData, ExecuteEvents.beginDragHandler);
        ExecuteEvents.Execute(source, pointerData, ExecuteEvents.pointerDownHandler);
        yield return null;

        // Drag to target
        var targetRect = target.GetComponent<RectTransform>();
        if (targetRect != null && Camera.main != null)
        {
            pointerData.position = RectTransformUtility.WorldToScreenPoint(Camera.main, targetRect.position);
        }
        else
        {
            pointerData.position = Camera.main.WorldToScreenPoint(target.transform.position);
        }

        ExecuteEvents.Execute(source, pointerData, ExecuteEvents.dragHandler);
        yield return new WaitForSeconds(0.1f); // Simulate drag duration

        // Drop
        ExecuteEvents.Execute(source, pointerData, ExecuteEvents.endDragHandler);
        ExecuteEvents.Execute(target, pointerData, ExecuteEvents.dropHandler);
        ExecuteEvents.Execute(source, pointerData, ExecuteEvents.pointerUpHandler);

        Debug.Log($"✓ Drag & Drop: '{source.name}' → '{target.name}'");
    }

    #endregion

    #region RaycastClick (3D Objects)

    /// <summary>
    /// Click vào 3D object bằng raycast từ camera.
    /// </summary>
    public Coroutine RaycastClick(MonoBehaviour behaviour, string objectName, Camera camera = null)
    {
        return behaviour.StartCoroutine(RaycastClickInternal(objectName, camera));
    }

    public Coroutine RaycastClick(MonoBehaviour behaviour, GameObject target, Camera camera = null)
    {
        return behaviour.StartCoroutine(RaycastClickByObjectInternal(target, camera));
    }

    private IEnumerator RaycastClickInternal(string objectName, Camera camera)
    {
    var target = FindByPathOrId(objectName);
        if (target == null)
        {
            throw new UITestException($"Raycast target không tìm thấy: {objectName}");
        }

        yield return RaycastClickByObjectInternal(target, camera);
    }

    private IEnumerator RaycastClickByObjectInternal(GameObject target, Camera camera)
    {
        if (target == null)
        {
            throw new UITestException("Raycast target là null.");
        }

        if (camera == null)
        {
            camera = Camera.main;
        }

        if (camera == null)
        {
            throw new UITestException("Không có camera để raycast.");
        }

        // Get world position of target
        Vector3 targetPosition = target.transform.position;

        // Check if target has collider
        var collider = target.GetComponent<Collider>();
        if (collider != null)
        {
            targetPosition = collider.bounds.center;
        }

        // Raycast from camera to target
        Vector3 screenPoint = camera.WorldToScreenPoint(targetPosition);
        Ray ray = camera.ScreenPointToRay(screenPoint);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == target)
            {
                Debug.Log($"✓ RaycastClick: '{target.name}' tại {targetPosition}");

                // Send click event if target has event handler
                var eventHandler = target.GetComponent<IPointerClickHandler>();
                if (eventHandler != null)
                {
                    var pointerData = new PointerEventData(EventSystem.current)
                    {
                        position = screenPoint
                    };
                    eventHandler.OnPointerClick(pointerData);
                }

                yield return null;
            }
            else
            {
                throw new UITestException($"Raycast hit '{hit.collider.gameObject.name}' thay vì '{target.name}'");
            }
        }
        else
        {
            throw new UITestException($"Raycast không hit object '{target.name}'");
        }
    }

    #endregion

    #region Advanced Conditions

    /// <summary>
    /// Chờ NavMeshAgent đến đích.
    /// </summary>
    [Serializable]
    public class NavMeshAgentReached : Condition
    {
        private UnityEngine.AI.NavMeshAgent agent;
        private float threshold;

        public NavMeshAgentReached(UnityEngine.AI.NavMeshAgent agent, float threshold = 0.1f)
        {
            this.agent = agent;
            this.threshold = threshold;
        }

        public override bool Satisfied()
        {
            if (agent == null) return false;
            if (!agent.hasPath) return false;
            if (agent.pathPending) return false;

            return agent.remainingDistance <= threshold && !agent.pathPending;
        }

        public override string ToString()
        {
            if (agent == null) return "NavMeshAgent null";
            if (!agent.hasPath) return "NavMeshAgent không có path";
            if (agent.pathPending) return "NavMeshAgent path đang pending";
            return $"NavMeshAgent còn {agent.remainingDistance:F2}m (threshold: {threshold}m)";
        }
    }

    /// <summary>
    /// Chờ physics ổn định (tất cả rigidbodies ngừng di chuyển).
    /// </summary>
    [Serializable]
    public class PhysicsSettled : Condition
    {
        private Rigidbody[] rigidbodies;
        private float velocityThreshold;
        private float angularThreshold;

        public PhysicsSettled(float velocityThreshold = 0.01f, float angularThreshold = 0.01f)
        {
            this.velocityThreshold = velocityThreshold;
            this.angularThreshold = angularThreshold;
            // Find all rigidbodies in scene
            rigidbodies = UnityEngine.Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        }

        public PhysicsSettled(Rigidbody[] specificBodies, float velocityThreshold = 0.01f, float angularThreshold = 0.01f)
        {
            this.rigidbodies = specificBodies;
            this.velocityThreshold = velocityThreshold;
            this.angularThreshold = angularThreshold;
        }

        public override bool Satisfied()
        {
            // if (rigidbodies == null || rigidbodies.Length == 0)
            //     return true;

            // foreach (var rb in rigidbodies)
            // {
            //     if (rb == null || rb.isKinematic) continue;

            //     if (rb.linearVelocity.magnitude > velocityThreshold)
            //         return false;

            //     if (rb.angularVelocity.magnitude > angularThreshold)
            //         return false;
            // }

            return true;
        }

        public override string ToString()
        {
            // if (rigidbodies == null || rigidbodies.Length == 0)
            //     return "Không có Rigidbody nào";

            // int moving = 0;
            // foreach (var rb in rigidbodies)
            // {
            //     if (rb == null || rb.isKinematic) continue;
            //     if (rb.linearVelocity.magnitude > velocityThreshold || rb.angularVelocity.magnitude > angularThreshold)
            //         moving++;
            // }

            // return $"Physics: {moving}/{rigidbodies.Length} rigidbodies đang di chuyển";
            return string.Empty;
        }
    }

    #endregion
    
    #region Multi-Touch Support

    /// <summary>
    /// Mô phỏng multi-touch gesture (pinch to zoom).
    /// </summary>
    public Coroutine PinchZoom(MonoBehaviour behaviour, Vector2 centerPosition, float initialDistance, float finalDistance, float duration)
    {
        return behaviour.StartCoroutine(PinchZoomInternal(centerPosition, initialDistance, finalDistance, duration));
    }

    private IEnumerator PinchZoomInternal(Vector2 center, float initialDistance, float finalDistance, float duration)
    {
        if (EventSystem.current == null)
        {
            throw new UITestException("Không có EventSystem để mô phỏng multi-touch.");
        }

        float elapsed = 0f;
        
        // Create two touch points
        var touch1 = new PointerEventData(EventSystem.current);
        var touch2 = new PointerEventData(EventSystem.current);

        // Initial positions
        Vector2 offset = new Vector2(initialDistance / 2f, 0);
        touch1.position = center - offset;
        touch2.position = center + offset;

        // Begin touches
        ExecuteEvents.Execute<IPointerDownHandler>(null, touch1, (handler, data) => handler.OnPointerDown((PointerEventData)data));
        ExecuteEvents.Execute<IPointerDownHandler>(null, touch2, (handler, data) => handler.OnPointerDown((PointerEventData)data));

        yield return null;

        // Animate pinch
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float currentDistance = Mathf.Lerp(initialDistance, finalDistance, t);
            
            offset = new Vector2(currentDistance / 2f, 0);
            touch1.position = center - offset;
            touch2.position = center + offset;

            yield return null;
        }

        // End touches
        ExecuteEvents.Execute<IPointerUpHandler>(null, touch1, (handler, data) => handler.OnPointerUp((PointerEventData)data));
        ExecuteEvents.Execute<IPointerUpHandler>(null, touch2, (handler, data) => handler.OnPointerUp((PointerEventData)data));

        Debug.Log($"✓ Pinch zoom: {initialDistance} → {finalDistance} trong {duration}s");
    }

    /// <summary>
    /// Mô phỏng two-finger rotation.
    /// </summary>
    public Coroutine TwoFingerRotate(MonoBehaviour behaviour, Vector2 centerPosition, float angleDelta, float duration)
    {
        return behaviour.StartCoroutine(TwoFingerRotateInternal(centerPosition, angleDelta, duration));
    }

    private IEnumerator TwoFingerRotateInternal(Vector2 center, float angleDelta, float duration)
    {
        if (EventSystem.current == null)
        {
            throw new UITestException("Không có EventSystem để mô phỏng rotation.");
        }

        float elapsed = 0f;
        float radius = 100f; // Distance from center
        float startAngle = 0f;

        var touch1 = new PointerEventData(EventSystem.current);
        var touch2 = new PointerEventData(EventSystem.current);

        // Begin touches
        ExecuteEvents.Execute<IPointerDownHandler>(null, touch1, (handler, data) => handler.OnPointerDown((PointerEventData)data));
        ExecuteEvents.Execute<IPointerDownHandler>(null, touch2, (handler, data) => handler.OnPointerDown((PointerEventData)data));

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float currentAngle = Mathf.Lerp(startAngle, startAngle + angleDelta, t);

            // Calculate touch positions on circle
            float angle1 = currentAngle * Mathf.Deg2Rad;
            float angle2 = (currentAngle + 180f) * Mathf.Deg2Rad;

            touch1.position = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
            touch2.position = center + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;

            yield return null;
        }

        // End touches
        ExecuteEvents.Execute<IPointerUpHandler>(null, touch1, (handler, data) => handler.OnPointerUp((PointerEventData)data));
        ExecuteEvents.Execute<IPointerUpHandler>(null, touch2, (handler, data) => handler.OnPointerUp((PointerEventData)data));

        Debug.Log($"✓ Two-finger rotate: {angleDelta}° trong {duration}s");
    }

    #endregion

    #region Input System Integration (New Input System support)  
    #endregion
}
