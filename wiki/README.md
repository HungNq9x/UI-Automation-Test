# Wiki Documentation

Đây là thư mục chứa tài liệu Wiki cho UI Test Framework.

## Cách sử dụng

Các file markdown trong thư mục này có thể được:

1. **Upload lên GitHub Wiki**
   - Vào repository Settings → Features → Enable Wiki
   - Clone wiki repository: `git clone https://github.com/HungNq9x/UI-Automation-Test.wiki.git`
   - Copy các file markdown vào wiki repo
   - Commit và push

2. **Xem trực tiếp trên GitHub**
   - Browse các file .md trong thư mục này
   - GitHub tự động render markdown

3. **Sử dụng local**
   - Mở các file .md bằng Markdown viewer
   - Dùng trong IDE (VS Code, Rider, etc.)

## Cấu trúc Wiki

- **Home.md** - Trang chủ với tổng quan và quick links
- **Getting-Started.md** - Hướng dẫn bắt đầu cho người mới
- **Architecture.md** - Kiến trúc hệ thống chi tiết
- **Test-Steps.md** - Tài liệu về các loại test steps
- **UITestRunner-Window.md** - Hướng dẫn sử dụng Editor Window
- **Object-Lookup.md** - Cách tìm kiếm UI elements
- **Best-Practices.md** - Các thực hành tốt nhất
- **Custom-Test-Steps.md** - Tạo custom test steps
- **Troubleshooting.md** - Khắc phục sự cố thường gặp

## Đóng góp

Để cập nhật hoặc thêm tài liệu:

1. Tạo hoặc edit file .md trong thư mục này
2. Sử dụng Markdown syntax
3. Link giữa các pages bằng `[[Page Title|File-Name]]`
4. Commit và push changes

## Note về Wiki Links

Trong GitHub Wiki, links sử dụng format:
```markdown
[[Tên Trang|File-Name]]
```

Ví dụ:
```markdown
[[Getting Started|Getting-Started]]
[[Test Steps|Test-Steps]]
```

Nếu xem local, có thể cần adjust links thành relative paths:
```markdown
[Getting Started](./Getting-Started.md)
[Test Steps](./Test-Steps.md)
```
