# H? th?ng Qu?n lý L?p h?c & ?i?m danh

## Mô t?
H? th?ng qu?n lý l?p h?c t??ng t? Google Classroom v?i các tính n?ng ??c bi?t:
- **?i?m danh v?i ??nh v? GPS**: Sinh viên ph?i b?t ??nh v? và ? trong ph?m vi l?p h?c ?? ?i?m danh
- **Ch?ng gian l?n**: H? th?ng t? ??ng phát hi?n ?i?m danh h?, s? d?ng chung thi?t b?/IP
- **Qu?n lý bài t?p**: T?o, n?p và ch?m ?i?m bài t?p tr?c tuy?n
- **Xin ngh? phép**: Sinh viên có th? g?i ??n xin ngh? phép kèm minh ch?ng
- **B?ng tin**: Giáo viên và sinh viên trao ??i, thông báo trong l?p h?c

## Công ngh?
- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server (Code First approach)
- **ORM**: Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, jQuery, Font Awesome
- **Geolocation**: HTML5 Geolocation API

## Cài ??t

### 1. Yêu c?u h? th?ng
- .NET 8 SDK
- SQL Server 2019 ho?c m?i h?n (ho?c SQL Server LocalDB)
- Visual Studio 2022 ho?c VS Code

### 2. C?u hình Database
M? file `appsettings.json` và c?p nh?t connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AttendanceManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

### 3. Ch?y Migration

M? Package Manager Console trong Visual Studio và ch?y:

```bash
Add-Migration InitialCreate
Update-Database
```

Ho?c s? d?ng .NET CLI:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Ch?y ?ng d?ng

```bash
dotnet run
```

Ho?c nh?n F5 trong Visual Studio.

## Tài kho?n m?u

H? th?ng t? ??ng t?o các tài kho?n demo khi kh?i ??ng:

### Admin
- Email: `admin@attendance.com`
- Password: `Admin@123`

### Giáo viên
- Email: `teacher@attendance.com`
- Password: `Teacher@123`

### Sinh viên
- Email: `student@attendance.com`
- Password: `Student@123`

## H??ng d?n s? d?ng

### Dành cho Giáo viên

1. **T?o l?p h?c**
   - ??ng nh?p v?i tài kho?n giáo viên
   - Click "T?o l?p h?c"
   - ?i?n thông tin l?p h?c và v? trí GPS (có th? dùng nút "L?y v? trí hi?n t?i")
   - H? th?ng s? t?o mã l?p t? ??ng

2. **T?o phiên ?i?m danh**
   - Vào l?p h?c
   - Click tab "?i?m danh" > "T?o phiên ?i?m danh"
   - ??t th?i gian b?t ??u/k?t thúc
   - C?u hình v? trí và kho?ng cách cho phép

3. **Theo dõi ?i?m danh**
   - Click vào phiên ?i?m danh
   - Xem danh sách sinh viên ?ã ?i?m danh
   - Xem các vi ph?m (ngoài ph?m vi, trùng thi?t b?/IP)
   - Duy?t ??n xin ngh? phép

4. **Qu?n lý bài t?p**
   - T?o bài t?p v?i h?n n?p
   - Xem danh sách sinh viên n?p bài
   - Ch?m ?i?m và ph?n h?i

### Dành cho Sinh viên

1. **Tham gia l?p h?c**
   - ??ng nh?p v?i tài kho?n sinh viên
   - Click "Tham gia l?p"
   - Nh?p mã l?p do giáo viên cung c?p

2. **?i?m danh**
   - **Quan tr?ng**: B?t ??nh v? trên thi?t b?
   - Vào l?p h?c > Tab "?i?m danh"
   - Click vào phiên ?i?m danh ?ang di?n ra
   - Click "?i?m danh ngay"
   - Cho phép truy c?p v? trí khi ???c yêu c?u
   - Ki?m tra kho?ng cách và xác nh?n ?i?m danh

3. **Xin ngh? phép**
   - Vào phiên ?i?m danh
   - Click "Xin ngh? phép"
   - ?i?n thông tin và lý do
   - ?ính kèm minh ch?ng (n?u có)

4. **N?p bài t?p**
   - Vào l?p h?c > Tab "Bài t?p"
   - Click vào bài t?p c?n n?p
   - Click "N?p bài"
   - ?i?n n?i dung ho?c ?ính kèm file

## Tính n?ng chính

### 1. ?i?m danh v?i GPS
- Sinh viên b?t bu?c ph?i b?t ??nh v?
- Tính toán kho?ng cách t? v? trí l?p h?c
- T? ??ng ?ánh d?u vi ph?m n?u ngoài ph?m vi cho phép
- L?u th?i gian ?i?m danh chính xác

### 2. Phát hi?n gian l?n
H? th?ng t? ??ng phát hi?n:
- ?i?m danh ngoài ph?m vi cho phép
- Nhi?u tài kho?n ?i?m danh t? cùng thi?t b?
- Nhi?u tài kho?n ?i?m danh t? cùng IP
- V? trí ?áng ng?

### 3. Qu?n lý l?p h?c
- T?o và qu?n lý nhi?u l?p h?c
- Chia s? mã l?p cho sinh viên
- Qu?n lý danh sách thành viên
- Th?ng kê ?i?m danh, bài t?p

### 4. Bài t?p & Ch?m ?i?m
- T?o bài t?p v?i h?n n?p
- N?p bài tr?c tuy?n
- Ch?m ?i?m và ph?n h?i
- Th?ng kê t? l? n?p bài

### 5. B?ng tin
- ??ng thông báo, th?o lu?n
- Bình lu?n và t??ng tác
- ?ính kèm tài li?u

## C?u trúc Database

### Các b?ng chính:
- **AspNetUsers**: Ng??i dùng (Student, Teacher, Admin)
- **Classes**: L?p h?c
- **Enrollments**: ??ng ký l?p h?c
- **AttendanceSlots**: Phiên ?i?m danh
- **AttendanceRecords**: B?n ghi ?i?m danh
- **AttendanceFlags**: C? vi ph?m ?i?m danh
- **LeaveRequests**: ??n xin ngh?
- **Assignments**: Bài t?p
- **Submissions**: Bài n?p
- **Posts**: Bài ??ng
- **Comments**: Bình lu?n

## API Endpoints

### Account
- POST `/Account/Login` - ??ng nh?p
- POST `/Account/Register` - ??ng ký
- POST `/Account/Logout` - ??ng xu?t

### Class
- GET `/Class/Index` - Danh sách l?p h?c
- GET `/Class/Create` - Form t?o l?p
- POST `/Class/Create` - T?o l?p h?c
- GET `/Class/Detail/{id}` - Chi ti?t l?p h?c
- POST `/Class/Join` - Tham gia l?p

### Attendance
- POST `/Attendance/CreateSlot` - T?o phiên ?i?m danh
- GET `/Attendance/SlotDetail/{id}` - Chi ti?t phiên
- POST `/Attendance/CheckIn` - ?i?m danh
- POST `/Attendance/RequestLeave` - Xin ngh? phép
- POST `/Attendance/ReviewLeaveRequest` - Duy?t ??n

### Assignment
- POST `/Assignment/Create` - T?o bài t?p
- GET `/Assignment/Detail/{id}` - Chi ti?t bài t?p
- POST `/Assignment/Submit` - N?p bài
- POST `/Assignment/Grade` - Ch?m ?i?m

## L?u ý b?o m?t

- M?t kh?u ???c hash b?ng ASP.NET Core Identity
- S? d?ng Anti-Forgery Token cho các form
- Authorization v?i Role-based access control
- HTTPS required trong production
- Validate input ?? tránh SQL Injection, XSS

## Tùy ch?nh

### Thay ??i kho?ng cách cho phép m?c ??nh
Trong `Class.cs` và `AttendanceSlot.cs`:
```csharp
public int AllowedDistanceMeters { get; set; } = 100; // ??i 100 thành giá tr? mong mu?n
```

### Thay ??i th?i gian session
Trong `Program.cs`:
```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // ??i 30 thành giá tr? mong mu?n
});
```

## Troubleshooting

### L?i không l?y ???c v? trí
- ??m b?o trình duy?t h? tr? Geolocation API
- Cho phép truy c?p v? trí khi ???c h?i
- S? d?ng HTTPS (required cho Geolocation)

### L?i migration
```bash
dotnet ef database drop
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### L?i connection string
- Ki?m tra SQL Server ?ang ch?y
- Ki?m tra tên server trong connection string
- Th? dùng SQL Server Object Explorer ?? test k?t n?i

## License
MIT License

## Tác gi?
Developed for classroom management and attendance tracking
