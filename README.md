# Unity 3D Gameplay Prototype

Bài test tuyển dụng vị trí **Intern DEV Unity** của **Trương Công Hiện**, xây dựng một trò chơi vượt chướng ngại vật góc nhìn thứ ba (third-person), sử dụng nhân vật và bản đồ được cung cấp theo đề bài.

## Giới thiệu

Người chơi điều khiển nhân vật **Char9** khám phá hai căn phòng, tương tác với cửa và các nút bấm để mở đường, vượt qua các bậc platform và tránh những khu vực nguy hiểm. Mục tiêu là đến cuối tuyến đường và kích hoạt điểm hoàn thành màn chơi.

## Điều khiển

| Thao tác | Phím / cách thực hiện |
| --- | --- |
| Di chuyển | `W`, `A`, `S`, `D` |
| Nhảy | `Space` |
| Xoay camera | Giữ chuột trái và di chuyển chuột |
| Tương tác | Đến gần đối tượng, nhấp vào biểu tượng tương tác |
| Tạm dừng / tiếp tục | `Tab` hoặc các nút trên giao diện |
| Chơi lại | Nút `Reset` |

## Tính năng đã thực hiện

- **Điều khiển nhân vật TPS:** di chuyển theo hướng camera, xoay nhân vật và nhảy bằng `CharacterController`.
- **Animation:** `Idle`, `Run`, `Jump`, `Interact` và `Death`.
- **Tương tác với môi trường:** mở/đóng cửa, kích hoạt nút bấm và các bậc platform trong phòng thứ hai.
- **Thử thách vượt chướng ngại:** platform di chuyển, sàn sập, bẫy và vùng rơi nguy hiểm.
- **Hệ thống thắng/thua:** phát hiệu ứng chiến thắng (VFX), hiển thị `Win Panel` hoặc `Loss Panel` khi kết thúc lượt chơi.
- **Giao diện:** `Pause`, `Resume`, `Reset` và các màn hình kết quả.
- **Âm thanh:** hiệu ứng bước chân, nhảy, tương tác, mở cửa, thất bại và chiến thắng.
- **Windows build:** cấu hình Player Settings, icon và splash screen cho bản chạy độc lập trên Windows.

## Tải và chạy bản Windows

**[Tải Windows Build trên Google Drive](https://drive.google.com/drive/folders/1QIW4gam_R-hf0ehVZQUQ8CmlXCx61imp?usp=drive_link)**

1. Tải toàn bộ thư mục build từ Google Drive.
2. Giải nén nếu tệp được đóng gói dưới dạng `.zip`.
3. Chạy file `unitybasetest.exe`.
4. Giữ nguyên thư mục `_Data` và các file đi kèm ở cùng vị trí với file `.exe`.

> **Lưu ý:** Không chỉ tải riêng file `.exe`, vì bản build Unity cần các dữ liệu và thư viện đi kèm để hoạt động.

## Mở project bằng Unity

- **Phiên bản Unity:** `2022.3.62f2`.
- **Nền tảng build:** Windows x86_64.

1. Clone repository hoặc tải source code về máy.
2. Mở **Unity Hub** → **Add project from disk** và chọn thư mục project.
3. Mở project bằng phiên bản Unity phù hợp.
4. Mở scene `GameScene`.
5. Nhấn **Play** để chạy thử trong Unity Editor.

## Công nghệ sử dụng

- Unity 3D
- C#
- Unity CharacterController
- Legacy Animation
- Unity Particle System
- Unity UI
- Git & GitHub

## Thông tin bài test

- **Người thực hiện:** Trương Công Hiện
- **Mục đích:** Bài test tuyển dụng vị trí **Intern DEV Unity** — SE7EN Game Studio

**Ghi chú về tài nguyên:** Nhân vật và bản đồ sử dụng các tài nguyên được cung cấp theo đề bài. Không phân phối lại các tài nguyên này cho mục đích khác khi chưa được phép.
