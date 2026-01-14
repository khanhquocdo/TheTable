# 🚀 Hướng Dẫn Setup Nhanh - Ammo System

## ⚡ Setup Nhanh (5 phút)

### **Bước 1: Tạo ScriptableObjects**

1. **Tạo AmmoData:**
   - Right-click trong Project → `Create → Weapon → Ammo Data`
   - Đặt tên: `PistolAmmoData`
   - Cấu hình:
     - Ammo Type: `Pistol`
     - Max Magazine Size: `30`
     - Max Reserve Ammo: `120`
     - Reload Time: `2.0`

2. **Tạo WeaponAmmoData:**
   - Right-click → `Create → Weapon → Weapon Ammo Data`
   - Đặt tên: `PistolWeaponAmmoData`
   - Cấu hình:
     - Ammo Type: `Pistol`
     - Initial Magazine Ammo: `30`
     - Initial Reserve Ammo: `90`
     - Fire Mode: `Auto`

### **Bước 2: Setup AmmoController**

1. Chọn **Player GameObject** trong scene
2. Add Component → `AmmoController`
3. Trong Inspector:
   - Gán `PistolAmmoData` vào mảng **Ammo Data List**

### **Bước 3: Setup GunWeapon**

1. Tìm script khởi tạo `GunWeapon` (thường trong `PlayerEquipmentSetup.cs`)
2. Gán `PistolWeaponAmmoData` vào field `weaponAmmoData` của `GunWeapon`

### **Bước 4: Tạo AmmoPickup Prefab**

1. Tạo GameObject mới → Đặt tên: `AmmoPickup_Pistol`
2. Add Component:
   - `SpriteRenderer` (gán sprite đạn)
   - `CircleCollider2D` → Check **Is Trigger**
   - `AmmoPickup`
3. Cấu hình `AmmoPickup`:
   - Ammo Type: `Pistol`
   - Ammo Amount: `30`
   - Auto Pickup: `true`
4. Save thành Prefab

### **Bước 5: Setup UI (Optional)**

1. Tạo Canvas nếu chưa có
2. Tạo 2 TextMeshProUGUI:
   - `MagazineAmmoText` (hiển thị đạn trong băng)
   - `ReserveAmmoText` (hiển thị đạn dự trữ)
3. Tạo GameObject → Add Component `AmmoUI`
4. Gán các Text vào `AmmoUI` component

---

## ✅ Kiểm Tra

Sau khi setup, kiểm tra:

- [ ] Player có thể bắn và đạn bị trừ
- [ ] Nhấn **R** để reload
- [ ] Nhặt `AmmoPickup` để cộng đạn
- [ ] UI hiển thị đúng số đạn
- [ ] Không thể bắn khi hết đạn trong băng
- [ ] Reload tự động hủy khi bắn

---

## 🎮 Test Checklist

- [ ] Bắn hết đạn trong băng → Không thể bắn tiếp
- [ ] Nhấn R khi có đạn dự trữ → Reload thành công
- [ ] Bắn khi đang reload → Reload bị hủy
- [ ] Đổi vũ khí khi đang reload → Reload bị hủy
- [ ] Nhặt AmmoPickup → Đạn được cộng vào Reserve
- [ ] Nhặt đạn khi Reserve đầy → Không vượt quá Max Reserve
- [ ] UI cập nhật khi ammo thay đổi
- [ ] UI hiển thị "RELOADING..." khi đang reload
- [ ] UI hiển thị "OUT OF AMMO" khi hết đạn

---

## 🔧 Troubleshooting

### **Lỗi Compile:**
- Đảm bảo tất cả file `.cs` đã được Unity import
- Kiểm tra Console để xem lỗi cụ thể
- Đảm bảo không có duplicate class names

### **AmmoController không hoạt động:**
- Kiểm tra có đúng một `AmmoController` trong scene
- Kiểm tra `Ammo Data List` đã được gán chưa
- Kiểm tra `AmmoController` có được khởi tạo trước khi sử dụng

### **GunWeapon không bắn:**
- Kiểm tra `weaponAmmoData` đã được gán chưa
- Kiểm tra `AmmoController.Instance` có tồn tại không
- Kiểm tra `CanUse()` có return false không

### **UI không cập nhật:**
- Kiểm tra `AmmoUI` đã subscribe events chưa
- Kiểm tra `EquipmentSystem.Instance` có tồn tại không
- Kiểm tra weapon có implement `IShootableWeapon` không

---

## 📚 Tài Liệu Chi Tiết

Xem file `README_AMMO_SYSTEM.md` để biết thêm chi tiết về:
- Kiến trúc hệ thống
- API Reference
- Mở rộng hệ thống
- Ví dụ code

---

**Chúc bạn setup thành công! 🎉**
