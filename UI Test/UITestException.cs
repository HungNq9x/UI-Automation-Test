using System;

/// <summary>
/// Ngoại lệ riêng cho các lỗi trong quá trình kiểm thử UI.
/// Dùng để phân biệt lỗi test với lỗi hệ thống khác.
/// </summary>
public class UITestException : Exception
{
    public UITestException() { }
    public UITestException(string message) : base(message) { }
    public UITestException(string message, Exception inner) : base(message, inner) { }
}
