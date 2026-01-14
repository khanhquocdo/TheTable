# 🎯 Hệ Thống Ammo / Magazine & Bullet Pickup

## 📋 Tổng Quan

Hệ thống Ammo/Magazine và Bullet Pickup được thiết kế để quản lý đạn dược trong game Top-Down 2D sử dụng Raycast shooting. Hệ thống này:

- ✅ Hỗ trợ nhiều loại đạn khác nhau (Pistol, Rifle, Shotgun, SMG, Sniper)
- ✅ Quản lý Magazine (băng đạn) và Reserve Ammo (đạn dự trữ)
- ✅ Hỗ trợ reload với cancel khi bắn/đổi vũ khí
- ✅ Tích hợp với Raycast shooting (không spawn bullet prefab)
- ✅ Hỗ trợ nhiều chế độ bắn: Single, Auto, Burst
- ✅ Hệ thống nhặt đạn từ pickup items
- ✅ Dễ mở rộng cho nhiều loại vũ khí

---

## 🏗️ Kiến Trúc Hệ Thống

### 1. Các Component Chính

```
AmmoType (Enum)
    ↓
AmmoData (ScriptableObject) ──┐
    ↓                          │
WeaponAmmoData (ScriptableObject) ──┐
    ↓                                 │
AmmoController (MonoBehaviour) ──────┘
    ↓
GunWeapon (IWeapon, IShootableWeapon)
    ↓
AmmoPickup (MonoBehaviour)
    ↓
AmmoUI (MonoBehaviour)
```

### 2. Luồng Xử Lý

#### **Bắn Đạn:**
```
Player nhấn chuột trái
    ↓
GunWeapon.Use()
    ↓
Kiểm tra CanUse() → Kiểm tra AmmoController.CanShoot()
    ↓
AmmoController.ConsumeAmmo()
    ↓
Raycast → Gây damage
```

#### **Reload:**
```
Player nhấn phím R (hoặc tự động khi hết đạn)
    ↓
AmmoController.StartReload()
    ↓
Kiểm tra CanReload() → Có đạn dự trữ?
    ↓
Chờ reloadTime
    ↓
Chuyển đạn từ Reserve → Magazine
```

#### **Nhặt Đạn:**
```
Player va chạm với AmmoPickup
    ↓
AmmoPickup.PickupAmmo()
    ↓
AmmoController.AddAmmo()
    ↓
Cộng vào Reserve (không vượt quá maxReserveAmmo)
```

---

## 📁 Cấu Trúc File

### **Core Files:**

1. **`AmmoType.cs`** - Enum định nghĩa các loại đạn
2. **`AmmoData.cs`** - ScriptableObject chứa thông tin về một loại đạn
3. **`WeaponAmmoData.cs`** - ScriptableObject chứa thông tin ammo của một vũ khí
4. **`AmmoController.cs`** - Controller quản lý ammo của Player
5. **`IShootableWeapon.cs`** - Interface cho các vũ khí có thể bắn
6. **`AmmoPickup.cs`** - Item nhặt đạn trong world
7. **`AmmoUI.cs`** - UI hiển thị thông tin ammo

### **Updated Files:**

- **`GunWeapon.cs`** - Đã tích hợp ammo system
- **`PlayerMovement.cs`** - Đã thêm xử lý reload và Single mode

---

## 🚀 Hướng Dẫn Sử Dụng

### **Bước 1: Tạo AmmoData**

1. Trong Unity Editor, click chuột phải → `Create → Weapon → Ammo Data`
2. Đặt tên: `PistolAmmoData`
3. Cấu hình:
   - **Ammo Type**: Pistol
   - **Max Magazine Size**: 30
   - **Max Reserve Ammo**: 120
   - **Reload Time**: 2.0 giây
   - **Allow Partial Reload**: true

### **Bước 2: Tạo WeaponAmmoData**

1. Click chuột phải → `Create → Weapon → Weapon Ammo Data`
2. Đặt tên: `PistolWeaponAmmoData`
3. Cấu hình:
   - **Ammo Type**: Pistol
   - **Initial Magazine Ammo**: 30
   - **Initial Reserve Ammo**: 90
   - **Fire Mode**: Auto (hoặc Single, Burst)
   - **Burst Count**: 3 (nếu Fire Mode = Burst)
   - **Burst Interval**: 0.1 giây

### **Bước 3: Setup AmmoController**

1. Thêm component `AmmoController` vào Player GameObject
2. Trong Inspector, gán tất cả `AmmoData` vào mảng `Ammo Data List`
3. Đảm bảo chỉ có một `AmmoController` trong scene (Singleton)

### **Bước 4: Setup GunWeapon**

1. Trong `GunWeapon`, gán `WeaponAmmoData` vào field `weaponAmmoData`
2. Đảm bảo `GunWeapon` được khởi tạo với đầy đủ references:
   ```csharp
   new GunWeapon(playerMovement, lineRenderer, firePoint, camera)
   ```

### **Bước 5: Tạo AmmoPickup Prefab**

1. Tạo GameObject mới
2. Thêm component `AmmoPickup`
3. Thêm `SpriteRenderer` và `Collider2D` (CircleCollider2D)
4. Cấu hình:
   - **Ammo Type**: Pistol
   - **Ammo Amount**: 30
   - **Auto Pickup**: true
   - **Destroy On Pickup**: true
5. Set Collider2D thành Trigger
6. Save thành Prefab

### **Bước 6: Setup AmmoUI**

1. Tạo UI Canvas
2. Tạo TextMeshProUGUI cho Magazine Ammo
3. Tạo TextMeshProUGUI cho Reserve Ammo
4. Tạo TextMeshProUGUI cho Reload Status (optional)
5. Thêm component `AmmoUI` vào một GameObject trong Canvas
6. Gán các UI references vào `AmmoUI`

---

## 🎮 Điều Khiển

- **Bắn**: Giữ chuột trái
- **Reload**: Nhấn phím **R**
- **Đổi vũ khí**: Phím số **1, 2, 3, 4** hoặc Scroll Wheel

---

## 🔧 API Reference

### **AmmoController**

#### **Public Methods:**

```csharp
// Kiểm tra có thể bắn không
bool CanShoot(AmmoType ammoType)

// Sử dụng đạn (gọi khi bắn)
bool ConsumeAmmo(AmmoType ammoType, int amount = 1)

// Bắt đầu reload
bool StartReload(AmmoType ammoType)

// Hủy reload
void CancelReload()

// Thêm đạn vào reserve (khi nhặt pickup)
int AddAmmo(AmmoType ammoType, int amount)

// Lấy số đạn trong băng
int GetCurrentMagazine(AmmoType ammoType)

// Lấy số đạn dự trữ
int GetCurrentReserve(AmmoType ammoType)

// Kiểm tra đang reload không
bool IsReloading(AmmoType ammoType)

// Kiểm tra có thể reload không
bool CanReload(AmmoType ammoType)
```

#### **Events:**

```csharp
// Khi ammo thay đổi
event Action<AmmoType, int, int> OnAmmoChanged

// Khi bắt đầu reload
event Action<AmmoType> OnReloadStarted

// Khi hoàn thành reload
event Action<AmmoType> OnReloadCompleted

// Khi hủy reload
event Action<AmmoType> OnReloadCancelled

// Khi hết đạn hoàn toàn
event Action<AmmoType> OnOutOfAmmo
```

### **AmmoPickup**

#### **Static Methods:**

```csharp
// Spawn một AmmoPickup tại vị trí cụ thể
GameObject SpawnAmmoPickup(Vector3 position, AmmoType ammoType, int amount, GameObject prefab = null)
```

#### **Public Methods:**

```csharp
// Setup loại đạn và số lượng
void Setup(AmmoType type, int amount)

// Nhặt đạn (có thể gọi từ code)
void PickupAmmo()
```

---

## 📊 Fire Modes

### **Single Mode:**
- Bắn một viên mỗi lần nhấn chuột
- Phải nhấn lại để bắn viên tiếp theo
- Phù hợp với súng lục, sniper

### **Auto Mode:**
- Bắn liên tục khi giữ chuột
- Tốc độ bắn bị giới hạn bởi `fireRate`
- Phù hợp với súng trường, SMG

### **Burst Mode:**
- Bắn theo chùm (ví dụ: 3 viên mỗi lần nhấn)
- Có thể cấu hình `burstCount` và `burstInterval`
- Phù hợp với súng bắn tỉa, súng trường

---

## 🎯 Tích Hợp với Enemy

Để Enemy cũng sử dụng ammo system:

1. Thêm `AmmoController` vào Enemy GameObject
2. Tạo `WeaponAmmoData` cho Enemy weapon
3. Sử dụng `AmmoController.Instance.CanShoot()` và `ConsumeAmmo()` trong Enemy attack logic

**Lưu ý:** Nếu muốn Enemy có ammo riêng (không dùng chung với Player), tạo một `AmmoController` riêng cho Enemy thay vì dùng Singleton.

---

## 🔄 Mở Rộng

### **Thêm Loại Đạn Mới:**

1. Thêm vào enum `AmmoType`:
   ```csharp
   public enum AmmoType
   {
       // ... existing types
       Rocket = 6  // Ví dụ
   }
   ```

2. Tạo `AmmoData` mới cho loại đạn này
3. Tạo `WeaponAmmoData` cho vũ khí sử dụng loại đạn này

### **Thêm Vũ Khí Mới:**

1. Implement interface `IShootableWeapon`:
   ```csharp
   public class NewWeapon : IWeapon, IShootableWeapon
   {
       public AmmoType AmmoType => weaponAmmoData.ammoType;
       public WeaponAmmoData WeaponAmmoData => weaponAmmoData;
       public FireMode FireMode => weaponAmmoData.fireMode;
       
       // ... implement các method khác
   }
   ```

2. Gán `WeaponAmmoData` vào weapon
3. Sử dụng `AmmoController` trong logic bắn

### **Tích Hợp với Inventory:**

Nếu muốn tích hợp với Inventory System:

1. Tạo `AmmoItem` kế thừa từ `Item` (nếu có)
2. Khi sử dụng `AmmoItem`, gọi `AmmoController.AddAmmo()`
3. Có thể spawn `AmmoPickup` từ Inventory

---

## ⚠️ Lưu Ý Quan Trọng

1. **Singleton Pattern**: `AmmoController` sử dụng Singleton, chỉ nên có một instance trong scene
2. **Reload Cancel**: Reload sẽ tự động hủy khi:
   - Player bắn
   - Player đổi vũ khí
   - Gọi `CancelReload()` thủ công
3. **Auto Reload**: Khi hết đạn trong băng và có đạn dự trữ, hệ thống sẽ tự động reload (nếu có thể)
4. **Partial Reload**: Có thể reload khi băng đạn chưa hết (nếu `allowPartialReload = true`)
5. **Fire Rate**: Vẫn được kiểm tra song song với ammo check

---

## 🐛 Troubleshooting

### **Lỗi: "AmmoController.Instance is null"**
- Đảm bảo có `AmmoController` trong scene
- Đảm bảo `AmmoController` được khởi tạo trước khi sử dụng

### **Lỗi: "Cannot shoot even though has ammo"**
- Kiểm tra `CanShoot()` có đang reload không
- Kiểm tra `fireRate` có quá chậm không
- Kiểm tra `isEquipped` của weapon

### **Lỗi: "Reload không hoạt động"**
- Kiểm tra `CanReload()` có đủ đạn dự trữ không
- Kiểm tra `reloadTime` có hợp lý không
- Kiểm tra có đang reload loại đạn khác không

### **Lỗi: "UI không cập nhật"**
- Đảm bảo `AmmoUI` đã subscribe events
- Kiểm tra `EquipmentSystem.Instance` có tồn tại không
- Kiểm tra weapon có implement `IShootableWeapon` không

---

## 📝 Ví Dụ Code

### **Spawn AmmoPickup từ Enemy:**

```csharp
// Trong Enemy death logic
void OnDeath()
{
    // Spawn ammo pickup
    AmmoPickup.SpawnAmmoPickup(
        transform.position,
        AmmoType.Pistol,
        30,
        ammoPickupPrefab
    );
}
```

### **Kiểm tra Ammo trong Custom Script:**

```csharp
if (AmmoController.Instance != null)
{
    int magazine = AmmoController.Instance.GetCurrentMagazine(AmmoType.Pistol);
    int reserve = AmmoController.Instance.GetCurrentReserve(AmmoType.Pistol);
    
    Debug.Log($"Magazine: {magazine}, Reserve: {reserve}");
}
```

### **Lắng nghe Ammo Events:**

```csharp
void OnEnable()
{
    if (AmmoController.Instance != null)
    {
        AmmoController.Instance.OnOutOfAmmo += HandleOutOfAmmo;
    }
}

void OnDisable()
{
    if (AmmoController.Instance != null)
    {
        AmmoController.Instance.OnOutOfAmmo -= HandleOutOfAmmo;
    }
}

void HandleOutOfAmmo(AmmoType ammoType)
{
    Debug.Log($"Hết đạn loại: {ammoType}");
    // Hiển thị warning UI, tự động đổi vũ khí, etc.
}
```

---

## 🎉 Kết Luận

Hệ thống Ammo/Magazine và Bullet Pickup đã được thiết kế để:
- ✅ Dễ sử dụng và cấu hình
- ✅ Dễ mở rộng cho nhiều loại vũ khí
- ✅ Tích hợp tốt với Raycast shooting
- ✅ Hỗ trợ nhiều chế độ bắn
- ✅ Có thể tái sử dụng cho Enemy

Nếu có thắc mắc hoặc cần hỗ trợ, vui lòng tham khảo code comments hoặc liên hệ team phát triển.

---

**Version:** 1.0  
**Last Updated:** 2024  
**Author:** AI Assistant
