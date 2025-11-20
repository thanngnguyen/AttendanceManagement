# ?? Attendance Management System

H? th?ng qu?n lý l?p h?c và ?i?m danh tr?c tuy?n v?i công ngh? GPS, ???c xây d?ng b?ng ASP.NET Core 8.0.

## ? Tính N?ng Chính

### ?? ?i?m Danh GPS
- Sinh viên b?t bu?c ph?i b?t ??nh v? và ? trong ph?m vi l?p h?c ?? ?i?m danh
- Tính toán kho?ng cách chính xác s? d?ng công th?c Haversine
- T? ??ng ?ánh d?u vi ph?m n?u ngoài ph?m vi cho phép
- L?u l?i t?a ?? và th?i gian ?i?m danh chính xác

### ??? Phát Hi?n Gian L?n
H? th?ng t? ??ng phát hi?n:
- ? ?i?m danh ngoài ph?m vi cho phép (> 100m)
- ? Nhi?u tài kho?n ?i?m danh t? cùng thi?t b?
- ? Nhi?u tài kho?n ?i?m danh t? cùng IP
- ? V? trí b?t th??ng ho?c vô lý

### ?? Qu?n Lý L?p H?c
- T?o và qu?n lý nhi?u l?p h?c
- Chia s? mã l?p cho sinh viên tham gia
- Theo dõi danh sách thành viên
- Th?ng kê chi ti?t v? ?i?m danh

### ?? Bài T?p & Ch?m ?i?m
- T?o bài t?p v?i h?n n?p
- N?p bài tr?c tuy?n (text ho?c file)
- Ch?m ?i?m và ph?n h?i
- Theo dõi ti?n ?? n?p bài

### ?? Xin Ngh? Phép
- Sinh viên g?i ??n xin ngh? kèm minh ch?ng
- Giáo viên duy?t/t? ch?i ??n
- Ghi chú t? giáo viên

### ?? B?ng Tin & Th?o Lu?n
- Giáo viên ??ng thông báo, bài gi?ng
- Trao ??i và th?o lu?n trong l?p
- Bình lu?n, chia s? tài li?u

---

## ??? Công Ngh? S? D?ng

| Thành Ph?n | Công Ngh? |
|-----------|-----------|
| **Backend** | ASP.NET Core 8.0 MVC |
| **Database** | SQL Server 2019+ / LocalDB |
| **ORM** | Entity Framework Core |
| **Authentication** | ASP.NET Core Identity |
| **Frontend** | Bootstrap 5, jQuery, Font Awesome |
| **Geolocation** | HTML5 Geolocation API |
| **Language** | C# 12.0 |

---

## ?? Yêu C?u H? Th?ng

- ? .NET 8 SDK [T?i t?i ?ây](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- ? SQL Server 2019+ ho?c SQL Server LocalDB
- ? Visual Studio 2022 ho?c VS Code
- ? Git (?? clone repository)

---

## ?? Cài ??t & Ch?y

### 1?? Clone Repository

```bash
git clone https://github.com/thanngnguyen/AttendanceManagement.git
cd AttendanceManagement
```

### 2?? C?u Hình Database

M? file `appsettings.json` và c?p nh?t connection string:

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

### 3?? Ch?y Migration

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

### 4?? Ch?y ?ng D?ng

```bash
dotnet run
```

Ho?c nh?n **F5** trong Visual Studio.

?ng d?ng s? ch?y t?i: `https://localhost:7001`

---

## ?? Tài Kho?n Demo

H? th?ng t? ??ng t?o các tài kho?n demo khi kh?i ??ng:

### ?? Admin
- **Email:** `admin@attendance.com`
- **Password:** `Admin@123`
- **Quy?n:** Qu?n lý toàn b? h? th?ng

### ????? Giáo Viên
- **Email:** `teacher@attendance.com`
- **Password:** `Teacher@123`
- **Quy?n:** T?o l?p, t?o phiên ?i?m danh, ch?m ?i?m

### ????? Sinh Viên
- **Email:** `student@attendance.com`
- **Password:** `Student@123`
- **Quy?n:** Tham gia l?p, ?i?m danh, n?p bài

---

## ?? H??ng D?n S? D?ng

### ????? Dành cho Giáo Viên

#### 1. T?o L?p H?c
1. ??ng nh?p v?i tài kho?n giáo viên
2. Click **"T?o L?p H?c"** trên trang ch?
3. ?i?n thông tin:
   - Tên l?p
   - Mã l?p (t? ??ng)
   - Mô t?
   - V? trí GPS (dùng nút "L?y v? trí hi?n t?i")
   - Kho?ng cách cho phép (m?c ??nh 100m)
4. Click **"T?o"**

#### 2. T?o Phiên ?i?m Danh
1. Vào l?p h?c
2. Click **"T?o Phiên ?i?m Danh"**
3. ?i?n thông tin:
   - Tên phiên
   - Th?i gian b?t ??u/k?t thúc
   - V? trí và kho?ng cách cho phép
4. Click **"T?o"**

#### 3. Theo Dõi ?i?m Danh
1. Click vào phiên ?i?m danh
2. Xem danh sách sinh viên:
   - ? ?ã ?i?m danh
   - ? ?i mu?n
   - ?? V?ng
3. Xem **Vi Ph?m** (ngoài ph?m vi, trùng thi?t b?)
4. Duy?t **??n Xin Ngh?**

#### 4. Qu?n Lý Bài T?p
1. Click tab **"Bài T?p"**
2. Click **"T?o Bài T?p"**
3. ?i?n:
   - Tiêu ??
   - Mô t?
   - H?n n?p
   - ?i?m t?i ?a
4. Xem bài n?p và ch?m ?i?m

### ????? Dành cho Sinh Viên

#### 1. Tham Gia L?p H?c
1. ??ng nh?p v?i tài kho?n sinh viên
2. Click **"Tham Gia L?p"**
3. Nh?p **Mã L?p** do giáo viên cung c?p
4. Click **"Tham Gia"**

#### 2. ?i?m Danh ? QUAN TR?NG
1. **?? B?t ??nh V? trên thi?t b? TR??C**
2. Vào l?p h?c > Tab **"?i?m Danh"**
3. Click vào phiên ?i?m danh ?ang di?n ra
4. Click **"?i?m Danh Ngay"**
5. **Cho phép truy c?p v? trí** khi trình duy?t yêu c?u
6. Ch? tính toán kho?ng cách
7. Xác nh?n ?i?m danh

> ?? **L?u ý:** N?u kho?ng cách > 100m s? b? ?ánh d?u vi ph?m

#### 3. Xin Ngh? Phép
1. Click **"Xin Ngh? Phép"**
2. ?i?n:
   - Lý do xin ngh?
   - ?ính kèm minh ch?ng (n?u có)
3. Click **"G?i ??n"**
4. Ch? giáo viên duy?t

#### 4. N?p Bài T?p
1. Tab **"Bài T?p"** > Click bài t?p c?n n?p
2. Click **"N?p Bài"**
3. ?i?n n?i dung ho?c t?i lên file
4. Click **"N?p"**

---

## ?? C?u Trúc Database

```
AspNetUsers
  ??? Enrollments (tham gia l?p)
  ??? AttendanceRecords (?i?m danh)
  ??? LeaveRequests (xin ngh?)
  ??? Submissions (n?p bài)
  ??? Posts (bài vi?t)
  ??? Comments (bình lu?n)

Classes
  ??? AttendanceSlots (phiên ?i?m danh)
  ??? Assignments (bài t?p)
  ??? ClassSessions (bu?i h?c)
  ??? ClassMaterials (tài li?u)
  ??? CalendarEvents (s? ki?n)
  ??? Posts (bài vi?t)

AttendanceRecords
  ??? AttendanceFlags (vi ph?m)
```

---

## ?? API Endpoints Chính

### ?? Account
```
POST   /Account/Register        - ??ng ký
POST   /Account/Login           - ??ng nh?p
POST   /Account/Logout          - ??ng xu?t
GET    /Account/Profile         - Xem h? s?
POST   /Account/EditProfile     - S?a h? s?
POST   /Account/ChangePassword  - ??i m?t kh?u
```

### ?? Class
```
GET    /Class/Index             - Danh sách l?p
GET    /Class/Create            - Form t?o l?p
POST   /Class/Create            - T?o l?p
GET    /Class/Detail/{id}       - Chi ti?t l?p
POST   /Class/Join              - Tham gia l?p
GET    /Class/Members/{id}      - Danh sách thành viên
```

### ?? Attendance
```
POST   /Attendance/CreateSlot         - T?o phiên
GET    /Attendance/SlotDetail/{id}    - Chi ti?t phiên
GET    /Attendance/CheckIn/{id}       - Form ?i?m danh
POST   /Attendance/CheckIn            - G?i ?i?m danh
GET    /Attendance/RequestLeave/{id}  - Form xin ngh?
POST   /Attendance/RequestLeave       - G?i ??n xin ngh?
POST   /Attendance/ReviewLeaveRequest - Duy?t ??n
```

### ?? Assignment
```
GET    /Assignment/Create             - Form t?o bài
POST   /Assignment/Create             - T?o bài t?p
GET    /Assignment/Detail/{id}        - Chi ti?t bài
GET    /Assignment/Submit/{id}        - Form n?p bài
POST   /Assignment/Submit             - N?p bài
POST   /Assignment/Grade/{id}         - Ch?m ?i?m
```

---

## ?? C?u Hình Nâng Cao

### Test Mode (T?ng ph?m vi ?i?m danh)

**appsettings.json:**
```json
{
  "AppSettings": {
    "TestMode": true,
    "TestModeDistanceMeters": 5000
  }
}
```

Khi b?t, kho?ng cách cho phép s? t? ??ng t?ng lên 5000m.

### Thay ??i Timezone

M?c ??nh: **SE Asia Standard Time (UTC+7)** - Vi?t Nam

?? ??i, s?a trong `Helpers/DateTimeHelper.cs`:
```csharp
TimeZoneInfo.FindSystemTimeZoneById("Your-Timezone-ID")
```

### Thay ??i Kho?ng Cách M?c ??nh

Trong `Models/Class.cs`:
```csharp
public int AllowedDistanceMeters { get; set; } = 100; // ??i 100 thành giá tr? mong mu?n
```

---

## ?? B?o M?t

? **Các bi?n pháp b?o m?t:**
- M?t kh?u hash b?ng **ASP.NET Core Identity**
- **Anti-Forgery Token** cho t?t c? form
- **Role-based Access Control (RBAC)**
- **HTTPS required** cho Geolocation
- **Input validation** ?? tránh SQL Injection, XSS
- **CORS policy** ???c c?u hình

---

## ?? Troubleshooting

### ? L?i không l?y ???c v? trí GPS

**Nguyên nhân & Cách Kh?c Ph?c:**

| V?n ?? | Gi?i Pháp |
|--------|----------|
| Trình duy?t không h? tr? | Dùng Chrome, Firefox, Safari, Edge |
| Ch?a cho phép | Click ?? trên thanh URL ? Cho phép Location |
| Không b?t ??nh v? thi?t b? | B?t GPS trên ?i?n tho?i/máy tính |
| ? trong nhà | Ra ngoài tr?i ?? tín hi?u t?t h?n |
| S? d?ng HTTP (không HTTPS) | Geolocation yêu c?u HTTPS |

### ? L?i Migration

```bash
# Reset database
dotnet ef database drop --force
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### ? L?i Connection String

```bash
# Ki?m tra SQL Server
sqlcmd -S (localdb)\mssqllocaldb
```

N?u l?i, cài l?i SQL Server LocalDB:
```bash
# G? cài
sqllocaldb stop mssqllocaldb
sqllocaldb delete mssqllocaldb

# Cài l?i
sqllocaldb create mssqllocaldb
sqllocaldb start mssqllocaldb
```

---

## ?? C?i Ti?n Trong T??ng Lai

- [ ] ?? Thông báo real-time (SignalR)
- [ ] ?? Mobile app (Xamarin/MAUI)
- [ ] ?? Tích h?p video call (Zoom/Teams)
- [ ] ?? Dashboard & báo cáo nâng cao
- [ ] ?? AI phát hi?n gian l?n
- [ ] ?? ?a ngôn ng? (i18n)
- [ ] ?? Export báo cáo Excel/PDF

---

## ?? License

MIT License - Mã ngu?n m?, t? do s? d?ng và phát tri?n

---

## ????? Tác Gi?

**Th?ng Nguy?n** - Developer

?? Email: thang.nguyen@example.com  
?? GitHub: [@thanngnguyen](https://github.com/thanngnguyen)  
?? Repository: [AttendanceManagement](https://github.com/thanngnguyen/AttendanceManagement)

---

## ?? H? Tr? & Ph?n H?i

- ?? Issues: [GitHub Issues](https://github.com/thanngnguyen/AttendanceManagement/issues)
- ?? Discussions: [GitHub Discussions](https://github.com/thanngnguyen/AttendanceManagement/discussions)
- ?? Email: thang.nguyen@example.com

---

## ?? C?m ?n

- Bootstrap, jQuery, Font Awesome
- Microsoft .NET Foundation
- Entity Framework Core
- ASP.NET Core Community

---

**Chúc b?n s? d?ng h? th?ng vui v?! ??**
