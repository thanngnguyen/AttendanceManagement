# Attendance Management System

Hệ thống quản lý lớp học và điểm danh trực tuyến với công nghệ GPS, được xây dựng bằng ASP.NET Core 8.0.

## Tính Năng Chính

### Điểm Danh GPS
- Sinh viên bắt buộc phải bật định vị và ở trong phạm vi lớp học để điểm danh
- Tính toán khoảng cách chính xác sử dụng công thức Haversine
- Tự động đánh dấu vi phạm nếu ngoài phạm vi cho phép
- Lưu lại tọa độ và thời gian điểm danh chính xác

### Phát Hiện Gian Lận
Hệ thống tự động phát hiện:
- Điểm danh ngoài phạm vi cho phép (> 100m)
- Nhiều tài khoản điểm danh từ cùng thiết bị
- Nhiều tài khoản điểm danh từ cùng IP
- Vị trí bất thường hoặc vô lý

### Quản Lý Lớp Học
- Tạo và quản lý nhiều lớp học
- Chia sẻ mã lớp cho sinh viên tham gia
- Theo dõi danh sách thành viên
- Thống kê chi tiết về điểm danh

### Bài Tập và Chấm Điểm
- Tạo bài tập với hạn nộp
- Nộp bài trực tuyến (text hoặc file)
- Chấm điểm và phản hồi
- Theo dõi tiến độ nộp bài

### Xin Nghỉ Phép
- Sinh viên gửi đơn xin nghỉ kèm minh chứng
- Giáo viên duyệt/từ chối đơn
- Ghi chú từ giáo viên

### Bảng Tin và Thảo Luận
- Giáo viên đăng thông báo, bài giảng
- Trao đổi và thảo luận trong lớp
- Bình luận, chia sẻ tài liệu

---

## Công Nghệ Sử Dụng

| Thành Phần | Công Nghệ |
|-----------|-----------|
| Backend | ASP.NET Core 8.0 MVC |
| Database | SQL Server 2022 / LocalDB |
| ORM | Entity Framework Core |
| Authentication | ASP.NET Core Identity |
| Frontend | Bootstrap 5, jQuery, Font Awesome |
| Geolocation | HTML5 Geolocation API |
| Language | C# 12.0 |

---

## Yêu Cầu Hệ Thống

- .NET 8 SDK [Tải tại đây](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- SQL Server 2022 hoặc SQL Server LocalDB
- Visual Studio 2022 hoặc VS Code
- Git (để clone repository)

---

## Cài Đặt và Chạy

### 1. Clone Repository

```bash
git clone https://github.com/thanngnguyen/AttendanceManagement.git
cd AttendanceManagement
```

### 2. Cấu Hình Database

Mở file `appsettings.json` và cập nhật connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AttendanceManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  },
  "AppSettings": {
    "TestMode": false
  }
}
```

### 3. Chạy Migration

**Cách 1: Package Manager Console (Visual Studio)**
```bash
Add-Migration InitialCreate
Update-Database
```

**Cách 2: .NET CLI**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Chạy Ứng Dụng

```bash
dotnet run
```

Hoặc nhấn **F5** trong Visual Studio.

Ứng dụng sẽ chạy tại: `https://localhost:7001`

---

## Tài Khoản Demo

Hệ thống tự động tạo các tài khoản demo khi khởi động:

### Admin
- Email: `admin@attendance.com`
- Password: `Admin@123`
- Quyền: Quản lý toàn bộ hệ thống

### Giáo Viên
- Email: `teacher@attendance.com`
- Password: `Teacher@123`
- Quyền: Tạo lớp, tạo phiên điểm danh, chấm điểm

### Sinh Viên
- Email: `student@attendance.com`
- Password: `Student@123`
- Quyền: Tham gia lớp, điểm danh, nộp bài

---

## Hướng Dẫn Sử Dụng

### Dành cho Giáo Viên

#### 1. Tạo Lớp Học
1. Đăng nhập với tài khoản giáo viên
2. Click "Tạo Lớp Học" trên trang chủ
3. Điền thông tin:
   - Tên lớp
   - Mã lớp (tự động)
   - Mô tả
   - Vị trí GPS (dùng nút "Lấy vị trí hiện tại")
   - Khoảng cách cho phép (mặc định 100m)
4. Click "Tạo"

#### 2. Tạo Phiên Điểm Danh
1. Vào lớp học
2. Click "Tạo Phiên Điểm Danh"
3. Điền thông tin:
   - Tên phiên
   - Thời gian bắt đầu/kết thúc
   - Vị trí và khoảng cách cho phép
4. Click "Tạo"

#### 3. Theo Dõi Điểm Danh
1. Click vào phiên điểm danh
2. Xem danh sách sinh viên:
   - Đã điểm danh
   - Đi muộn
   - Vắng
3. Xem Vi Phạm (ngoài phạm vi, trùng thiết bị)
4. Duyệt Đơn Xin Nghỉ

#### 4. Quản Lý Bài Tập
1. Click tab "Bài Tập"
2. Click "Tạo Bài Tập"
3. Điền:
   - Tiêu đề
   - Mô tả
   - Hạn nộp
   - Điểm tối đa
4. Xem bài nộp và chấm điểm

### Dành cho Sinh Viên

#### 1. Tham Gia Lớp Học
1. Đăng nhập với tài khoản sinh viên
2. Click "Tham Gia Lớp"
3. Nhập Mã Lớp do giáo viên cung cấp
4. Click "Tham Gia"

#### 2. Điểm Danh (QUAN TRỌNG)
1. **Bật Định Vị trên thiết bị TRƯỚC**
2. Vào lớp học > Tab "Điểm Danh"
3. Click vào phiên điểm danh đang diễn ra
4. Click "Điểm Danh Ngay"
5. **Cho phép truy cập vị trí** khi trình duyệt yêu cầu
6. Chờ tính toán khoảng cách
7. Xác nhận điểm danh

> Lưu ý: Nếu khoảng cách > 100m sẽ bị đánh dấu vi phạm

#### 3. Xin Nghỉ Phép
1. Click "Xin Nghỉ Phép"
2. Điền:
   - Lý do xin nghỉ
   - Đính kèm minh chứng (nếu có)
3. Click "Gửi Đơn"
4. Chờ giáo viên duyệt

#### 4. Nộp Bài Tập
1. Tab "Bài Tập" > Click bài tập cần nộp
2. Click "Nộp Bài"
3. Điền nội dung hoặc tải lên file
4. Click "Nộp"

---

## Cấu Trúc Database

```
AspNetUsers
  ├── Enrollments (tham gia lớp)
  ├── AttendanceRecords (điểm danh)
  ├── LeaveRequests (xin nghỉ)
  ├── Submissions (nộp bài)
  ├── Posts (bài viết)
  └── Comments (bình luận)

Classes
  ├── AttendanceSlots (phiên điểm danh)
  ├── Assignments (bài tập)
  ├── ClassSessions (buổi học)
  ├── ClassMaterials (tài liệu)
  ├── CalendarEvents (sự kiện)
  └── Posts (bài viết)

AttendanceRecords
  └── AttendanceFlags (vi phạm)
```

---

## API Endpoints Chính

### Account
```
POST   /Account/Register        - Đăng ký
POST   /Account/Login           - Đăng nhập
POST   /Account/Logout          - Đăng xuất
GET    /Account/Profile         - Xem hồ sơ
POST   /Account/EditProfile     - Sửa hồ sơ
POST   /Account/ChangePassword  - Đổi mật khẩu
```

### Class
```
GET    /Class/Index             - Danh sách lớp
GET    /Class/Create            - Form tạo lớp
POST   /Class/Create            - Tạo lớp
GET    /Class/Detail/{id}       - Chi tiết lớp
POST   /Class/Join              - Tham gia lớp
GET    /Class/Members/{id}      - Danh sách thành viên
```

### Attendance
```
POST   /Attendance/CreateSlot         - Tạo phiên
GET    /Attendance/SlotDetail/{id}    - Chi tiết phiên
GET    /Attendance/CheckIn/{id}       - Form điểm danh
POST   /Attendance/CheckIn            - Gửi điểm danh
GET    /Attendance/RequestLeave/{id}  - Form xin nghỉ
POST   /Attendance/RequestLeave       - Gửi đơn xin nghỉ
POST   /Attendance/ReviewLeaveRequest - Duyệt đơn
```

### Assignment
```
GET    /Assignment/Create             - Form tạo bài
POST   /Assignment/Create             - Tạo bài tập
GET    /Assignment/Detail/{id}        - Chi tiết bài
GET    /Assignment/Submit/{id}        - Form nộp bài
POST   /Assignment/Submit             - Nộp bài
POST   /Assignment/Grade/{id}         - Chấm điểm
```

---

## Cấu Hình Nâng Cao

### Test Mode (Tăng phạm vi điểm danh)

**appsettings.json:**
```json
{
  "AppSettings": {
    "TestMode": true,
    "TestModeDistanceMeters": 5000
  }
}
```

Khi bật, khoảng cách cho phép sẽ tự động tăng lên 5000m.

### Thay Đổi Timezone

Mặc định: **SE Asia Standard Time (UTC+7)** - Việt Nam

Để đổi, sửa trong `Helpers/DateTimeHelper.cs`:
```csharp
TimeZoneInfo.FindSystemTimeZoneById("Your-Timezone-ID")
```

### Thay Đổi Khoảng Cách Mặc Định

Trong `Models/Class.cs`:
```csharp
public int AllowedDistanceMeters { get; set; } = 100; // Đổi 100 thành giá trị mong muốn
```

---

## Bảo Mật

Các biện pháp bảo mật:
- Mật khẩu hash bằng ASP.NET Core Identity
- Anti-Forgery Token cho tất cả form
- Role-based Access Control (RBAC)
- HTTPS required cho Geolocation
- Input validation để tránh SQL Injection, XSS
- CORS policy được cấu hình

---

## Troubleshooting

### Lỗi không lấy được vị trí GPS

| Vấn Đề | Giải Pháp |
|--------|----------|
| Trình duyệt không hỗ trợ | Dùng Chrome, Firefox, Safari, Edge |
| Chưa cho phép | Click khóa trên thanh URL → Cho phép Location |
| Không bật định vị thiết bị | Bật GPS trên điện thoại/máy tính |
| Ở trong nhà | Ra ngoài trời để tín hiệu tốt hơn |
| Sử dụng HTTP (không HTTPS) | Geolocation yêu cầu HTTPS |

### Lỗi Migration

```bash
# Reset database
dotnet ef database drop --force
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Lỗi Connection String

```bash
# Kiểm tra SQL Server
sqlcmd -S (localdb)\mssqllocaldb
```

Nếu lỗi, cài lại SQL Server LocalDB:
```bash
# Gỡ cài
sqllocaldb stop mssqllocaldb
sqllocaldb delete mssqllocaldb

# Cài lại
sqllocaldb create mssqllocaldb
sqllocaldb start mssqllocaldb
```

---

## Cải Tiến Trong Tương Lai

- Thông báo real-time (SignalR)
- Mobile app (Xamarin/MAUI)
- Tích hợp video call (Zoom/Teams)
- Dashboard và báo cáo nâng cao
- AI phát hiện gian lận
- Đa ngôn ngữ (i18n)
- Export báo cáo Excel/PDF

---

## License

MIT License - Mã nguồn mở, tự do sử dụng và phát triển

---

## Tác Giả

Thắng Nguyễn - Developer

Email: thang.nguyen@example.com
GitHub: [@thanngnguyen](https://github.com/thanngnguyen)
Repository: [AttendanceManagement](https://github.com/thanngnguyen/AttendanceManagement)

---

## Hỗ Trợ và Phản Hồi

- Issues: [GitHub Issues](https://github.com/thanngnguyen/AttendanceManagement/issues)
- Discussions: [GitHub Discussions](https://github.com/thanngnguyen/AttendanceManagement/discussions)
- Email: thang.nguyen@example.com

---

## Cảm Ơn

- Bootstrap, jQuery, Font Awesome
- Microsoft .NET Foundation
- Entity Framework Core
- ASP.NET Core Community

---

**Chúc bạn sử dụng hệ thống vui vẻ!**
