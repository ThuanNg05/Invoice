Tôi sẽ thực hiện một cuộc rà soát toàn diện trên codebase hiện tại để xác định các điểm cần tối ưu hóa về hiệu năng, kiến trúc và trải nghiệm người dùng.

Dưới đây là danh sách các đề xuất tối ưu hóa được phân loại theo mức độ cần thiết:

### 🔴 Mức độ: Bắt buộc (Critical - Cần xử lý ngay)

| File / Thành phần | Vấn đề | Đề xuất tối ưu hóa |
| :--- | :--- | :--- |
| `SupabaseDataService.cs` | **Thiếu cơ chế Retry & Timeout**: Các kết nối Supabase có thể thất bại do mạng chập chờn mà không có cơ chế thử lại. | Triển khai **Polly** hoặc logic retry đơn giản cho các lệnh gọi API quan trọng như `AddInvoice`. |
| `InvoicePdfService.cs` | **Blocking UI**: Việc tạo PDF (QuestPDF) có thể tốn tài nguyên CPU. Nếu chạy trên UI Thread sẽ gây treo ứng dụng. | Đảm bảo tất cả các lệnh gọi tạo PDF đều sử dụng `Task.Run()` (đã có một phần nhưng cần nhất quán). |
| `App.xaml.cs` | **Global Error Handling**: Hiện tại chưa thấy cơ chế bắt lỗi ngoại lệ chưa được xử lý (Unhandled Exceptions) trên toàn ứng dụng. | Thêm sự kiện `UnhandledException` trong `App.xaml.cs` để ghi log và hiển thị thông báo thân thiện thay vì crash. |

### 🟡 Mức độ: Quan trọng (High - Cải thiện hiệu năng & Maintainability)

| File / Thành phần | Vấn đề | Đề xuất tối ưu hóa |
| :--- | :--- | :--- |
| `SupabaseDataService.cs` | **Cache Invalidation**: Cơ chế cache hiện tại dựa trên `TimeSpan` cố định (5 phút). | Chuyển sang cơ chế **Cache-aside** kết hợp với **Supabase Realtime** để invalidate cache ngay khi dữ liệu DB thay đổi thay vì chờ hết hạn. |
| `CreateInvoiceViewModel.cs` | **Logic quá dày (Fat ViewModel)**: ViewModel này đang chứa cả logic tính toán, tạo PDF và quản lý cửa sổ. | Tách logic tính toán hóa đơn thành một `IInvoiceCalculator` hoặc `Domain Service` riêng để dễ unit test. |
| `All XAML Views` | **Compiled Bindings (`x:Bind`)**: Một số chỗ vẫn dùng `Binding`. | Chuyển toàn bộ sang `x:Bind` với `Mode=OneTime` hoặc `OneWay` để tận dụng kiểm tra kiểu dữ liệu lúc compile và tăng tốc độ render UI. |
| `IDataService.cs` | **Interface Segregation**: Interface này quá lớn, chứa mọi thứ từ Khách hàng đến Hóa đơn. | Chia nhỏ thành `ICustomerService`, `IProductService`, `IInvoiceService` để tuân thủ nguyên lý **ISP** trong SOLID. |

### 🔵 Mức độ: Khuyên dùng (Medium - Sạch sẽ & UX)

| File / Thành phần | Vấn đề | Đề xuất tối ưu hóa |
| :--- | :--- | :--- |
| `Models/*.cs` | **Data Annotation Validation**: Hiện tại đang validate thủ công trong code-behind (Page). | Sử dụng `CommunityToolkit.Mvvm` `ObservableValidator` và các DataAnnotations để validate dữ liệu ngay trong Model/ViewModel. |
| `HistoryPage.xaml` | **DataGrid Virtualization**: Với lượng hóa đơn lớn, DataGrid có thể bị chậm. | Đảm bảo `EnableRowVirtualization="True"` và sử dụng `IncrementalLoading` (đã có một phần trong `GetProducts`). |
| `Strings/vi-VN/*.resw` | **Localization Coverage**: Một số thông báo lỗi vẫn đang hardcode bằng tiếng Việt trong code. | Di chuyển tất cả các chuỗi text sang file `Resources.resw` để hỗ trợ đa ngôn ngữ hoàn chỉnh và dễ bảo trì. |
| `appsettings.json` | **Secret Management**: Thông tin Supabase Key đang để trong file json. | Sử dụng **User Secrets** trong quá trình phát triển để tránh vô tình commit key lên Git. |

### 🟢 Mức độ: Thẩm mỹ (Low - Clean Code)

| File / Thành phần | Vấn đề | Đề xuất tối ưu hóa |
| :--- | :--- | :--- |
| `SupabaseDataService.cs` | **Magic Strings**: Các tên bảng và key cache đang được viết trực tiếp. | Gom tất cả tên bảng Supabase vào một class `DbConstants` để tránh gõ sai. |
| `Helpers/StringHelper.cs` | **Tiện ích tập trung**: Một số Page vẫn tự viết logic format chuỗi. | Di chuyển toàn bộ logic parse/format tiền tệ, ngày tháng vào các `ValueConverter` dùng chung trong XAML. |

---

**Ưu tiên tiếp theo:** Nếu bạn đồng ý, tôi có thể bắt đầu bằng việc **tối ưu hóa `SupabaseDataService`** (chia nhỏ Interface và cải thiện cơ chế Cache) vì đây là "trái tim" của ứng dụng. Bạn muốn tôi bắt đầu từ mục nào?