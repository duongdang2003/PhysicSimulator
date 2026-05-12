# Hướng dẫn cấu hình mô phỏng lực đẩy Archimedes

## Các bước setup trong Unity:

### 1. **Thiết lập Water Plane (Nước)**
- Tạo một Cube, scale nó thành một tấm nước phẳng (ví dụ: 10 x 0.5 x 10)
- Đặt Y position = 0 (đây là chiều cao nước)
- Thêm material xanh với độ trong suốt
- **Ghi chú:** Thay đổi giá trị `waterHeight` trong script nếu nước ở vị trí khác

### 2. **Tạo Object bị thử nghiệm**
- Tạo một Sphere hoặc Cube để đại diện cho vật thử nghiệm
- Thêm **Rigidbody** component
- Đặt vật ở vị trí Y > waterHeight (trên nước) để bắt đầu
- Gán script `ArchimedesSimulation` vào object này

### 3. **Cấu hình ArchimedesSimulation**
Trong Inspector, thiết lập các giá trị:

**Archimedes Variables:**
- Liquid Density: 1000 (kg/m³) - dành cho nước
- Gravity: 9.8 (m/s²)
- Volume DM3: 2 (dm³) - thể tích vật thử nghiệm

**Object Properties:**
- Object Mass: 5 (kg) - khối lượng vật
- Object Density: 500 (kg/m³) - mật độ vật (không bắt buộc cho mô phỏng cơ bản)

**Water:**
- Water Height: 0 (vị trí Y của mặt nước)
- Is Fully Submerged: true (vật hoàn toàn chìm)

### 4. **Tạo UI Panel**
- Tạo **Canvas** nếu chưa có
- Thêm các **InputField** cho:
  - Volume (dm³)
  - Density (kg/m³)
  - Gravity (m/s²)
  - Mass (kg)
- Thêm các **TextMeshPro Text** để hiển thị:
  - Formula
  - Buoyancy Force
  - Weight
  - Net Force
  - Status

### 5. **Gán Script ArchimedesUI**
- Tạo một GameObject trống (hoặc thêm vào Canvas)
- Gán script `ArchimedesUI` vào đó
- Trong Inspector:
  - Kéo Sphere/Cube vào field "Simulation"
  - Kéo các Input Fields vào các field tương ứng
  - Kéo các Text elements vào các field tương ứng

## Công thức tính toán:

```
Lực đẩy Archimedes:  F_A = ρ × g × V
Trọng lực:           F_g = m × g
Lực ròng:            F_net = F_A - F_g
```

Trong đó:
- ρ = mật độ chất lỏng (kg/m³)
- g = gia tốc trọng trường (m/s²)
- V = thể tích vật chìm (m³) = Volume_dm3 × 0.001
- m = khối lượng vật (kg)

## Kết quả mong đợi:

**Ví dụ 1 (theo hình):**
- V = 2 dm³ = 0.002 m³
- ρ = 1000 kg/m³
- g = 9.8 m/s²
- **F_A = 1000 × 9.8 × 0.002 = 19.6 N** ✓

## Thử nghiệm:

1. Nhập các giá trị input theo bài toán
2. Vật sẽ tự động nổi lên nếu F_A > F_g
3. Vật sẽ chìm xuống nếu F_A < F_g
4. Vật cân bằng nếu F_A ≈ F_g
5. Quan sát các giá trị lực trong Panel UI
