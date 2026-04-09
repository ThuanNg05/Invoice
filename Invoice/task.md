Bạn hãy thực hiện **đúng phạm vi, tối thiểu thay đổi, không refactor lan rộng** cho project hiện tại.

## Mục tiêu

Hoàn thiện **logic chọn loại khách trước khi tạo hóa đơn** và **logic chọn giá theo loại khách**.

---

# Yêu cầu chính

## 1) Hiển thị lựa chọn loại khách bằng `ContentDialog`

Trước khi bắt đầu luồng tạo hóa đơn mới / trước khi user chọn sản phẩm, phải hiển thị **`ContentDialog`** cho user chọn **1 trong 2 loại khách**:

* **Khách Lẻ**
* **Khách cố định**

> Đây là bước bắt buộc trước khi vào luồng chọn sản phẩm.

---

## 2) Nghiệp vụ giá theo loại khách

### Nếu user chọn **Khách Lẻ**

* Khi chọn sản phẩm để thêm vào hóa đơn:

  * giá áp dụng cho sản phẩm phải là **`BasePrice`**
* Đây là **quy tắc bắt buộc**
* Không phụ thuộc vào logic giá hiện tại của khách hàng cố định

### Nếu user chọn **Khách cố định**

* **Giữ nguyên 100% logic chọn giá theo sản phẩm hiện tại**
* Không thay đổi cách hệ thống đang xác định giá cho khách cố định
* Không làm ảnh hưởng đến bất kỳ business rule hiện có nào đang hoạt động đúng

---

# Ràng buộc cực kỳ quan trọng

* **Chỉ thay đổi các tệp có liên quan trực tiếp**
* **Không thay đổi các tệp không liên quan**
* **Không refactor lan rộng**
* **Không đổi tên biến/method/cấu trúc lớn nếu không cần**
* Nếu phát hiện bắt buộc phải sửa thêm file ngoài phạm vi trực tiếp:

  1. **DỪNG LẠI**
  2. Liệt kê rõ:

     * tên file
     * lý do cần sửa
     * phần nào cần sửa
  3. **Không tiếp tục code cho đến khi tôi xác nhận**

---

# Phạm vi ưu tiên cần kiểm tra

Ưu tiên phân tích và sửa trong các phần liên quan trực tiếp đến:

* `@Invoice/ViewModels/CreateInvoiceViewModel.cs`
* nơi trigger tạo hóa đơn mới / trước bước chọn sản phẩm
* nơi xác định giá sản phẩm khi add vào invoice line
* nơi đang có logic phân nhánh giá cho khách hàng hiện tại

> Nếu chỉ cần sửa 1 file thì chỉ sửa 1 file.
> Nếu cần thêm file khác để `ContentDialog` hoạt động đúng trong WinUI 3 / MVVM, phải **dừng và báo trước**.

---

# Cách triển khai mong muốn

## A. Trước tiên phân tích code hiện tại

Hãy xác định:

1. Luồng nào đang khởi tạo tạo hóa đơn mới
2. Vị trí phù hợp nhất để show `ContentDialog`
3. Vị trí nào đang quyết định giá của sản phẩm khi add vào invoice
4. Logic giá hiện tại của khách cố định đang nằm ở đâu

---

## B. Thêm state tối thiểu để phân biệt loại khách

Thêm **state/flag tối thiểu cần thiết** để phân biệt:

* `Khách Lẻ`
* `Khách cố định`

Ví dụ:

* `IsWalkInCustomer`
* hoặc enum tương đương

> Chỉ dùng mức tối thiểu, không thiết kế lại kiến trúc nếu chưa cần.

---

## C. Quy tắc áp dụng giá

Khi thêm sản phẩm vào hóa đơn:

### Nếu là `Khách Lẻ`

* giá sản phẩm = `BasePrice`

### Nếu là `Khách cố định`

* giữ nguyên logic hiện tại (không thay đổi)

> Chỉ chèn nhánh điều kiện bao quanh logic cũ, không rewrite lại toàn bộ thuật toán chọn giá nếu không cần.

---

# Output format bắt buộc

## A. Phân tích ngắn gọn

* Luồng hiện tại đang tạo hóa đơn như thế nào
* Giá hiện tại đang được chọn ở đâu
* Điểm nào phù hợp nhất để chèn `ContentDialog`

## B. Danh sách file sẽ chỉnh

* Nếu chỉ cần 1 file:

  * ghi rõ: **Chỉ sửa `@Invoice/ViewModels/CreateInvoiceViewModel.cs`**
* Nếu cần nhiều hơn:

  * **DỪNG LẠI**
  * liệt kê file + lý do
  * **không code tiếp**

## C. Code thay đổi đề xuất

* Chỉ cung cấp **phần code cần sửa/thêm**
* Ghi rõ:

  * vị trí chèn
  * đoạn cũ
  * đoạn mới
* Không dump toàn bộ file nếu không cần

## D. Giải thích sau khi sửa

* Flow khi chọn **Khách Lẻ**
* Flow khi chọn **Khách cố định**
* Vì sao logic cũ của khách cố định không bị ảnh hưởng

## E. Kiểm tra rủi ro

* Các edge case có thể phát sinh
* Nếu còn thiếu context:

  * ghi rõ **“Cần xác nhận thêm trước khi sửa tiếp”**

---

# Lưu ý cuối cùng

* **Không được tự ý suy đoán logic giá cũ nếu chưa thấy code**
* **Không thay đổi logic khách cố định đang hoạt động đúng**
* **Chỉ thêm nhánh riêng cho Khách Lẻ với `BasePrice`**
* **Nếu phải sửa ngoài file liên quan trực tiếp, phải dừng và hỏi lại**
