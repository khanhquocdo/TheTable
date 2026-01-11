# Hướng Dẫn Setup Hệ Thống Inventory cho Grenade và Molotov

## Tổng Quan

Hệ thống này cho phép Player nhặt Grenade và Molotov từ map, lưu trữ trong inventory với số lượng giới hạn, và chỉ sử dụng được khi có số lượng > 0.

## Các Component Đã Tạo

1. **InventorySystem.cs** - Quản lý số lượng items
2. **GrenadePickup.cs** - Script cho Grenade pickup item
3. **MolotovPickup.cs** - Script cho Molotov pickup item
4. **IConsumableWeapon.cs** - Interface cho weapons có số lượng
5. **GrenadeWeapon.cs** (đã cập nhật) - Kiểm tra số lượng từ inventory
6. **MolotovWeapon.cs** (đã cập nhật) - Kiểm tra số lượng từ inventory
7. **EquipmentSlotUI.cs** (đã cập nhật) - Hiển thị số lượng trên UI

---

## Bước 1: Setup InventorySystem trên Player

### 1.1. Thêm Component vào Player

1. Chọn GameObject **Player** trong Hierarchy
2. Trong Inspector, click **Add Component**
3. Tìm và thêm **InventorySystem**

### 1.2. Cấu Hình InventorySystem

Trong Inspector của **InventorySystem**:

- **Grenade Max Stack**: `10` (số lượng tối đa Grenade có thể mang)
- **Molotov Max Stack**: `10` (số lượng tối đa Molotov có thể mang)

> **Lưu ý**: InventorySystem sẽ tự động khởi tạo với số lượng = 0 cho cả Grenade và Molotov.

---

## Bước 2: Tạo Pickup Items trong Scene

### 2.1. Tạo Grenade Pickup Prefab

1. Tạo GameObject mới trong Scene:
   - Right-click trong Hierarchy → **Create Empty**
   - Đặt tên: `GrenadePickup`

2. Thêm SpriteRenderer:
   - Click **Add Component** → **Sprite Renderer**
   - Gán sprite cho Grenade (có thể dùng sprite từ Resources hoặc tạo mới)

3. Thêm Collider2D:
   - Click **Add Component** → **Box Collider 2D** (hoặc **Circle Collider 2D**)
   - Đánh dấu **Is Trigger** = `true`

4. Thêm Script:
   - Click **Add Component** → **GrenadePickup**

5. Cấu hình GrenadePickup:
   - **Amount**: `1` (số lượng Grenade khi nhặt)
   - **Pickup Radius**: `0.5` (bán kính nhặt)
   - **Player Layer**: Chọn layer của Player (thường là "Player")
   - **Sprite Renderer**: Kéo SpriteRenderer vào đây (hoặc để trống, script sẽ tự tìm)
   - **Pickup Effect Prefab**: (Optional) Prefab effect khi nhặt
   - **Pickup SFX**: (Optional) AudioClip khi nhặt
   - **Enable Float Animation**: `true` (animation float lên xuống)
   - **Float Speed**: `2`
   - **Float Amount**: `0.2`

6. Tạo Prefab:
   - Kéo GameObject `GrenadePickup` từ Hierarchy vào thư mục `Assets/Resources/Prefab`
   - Xóa GameObject trong Scene (hoặc giữ lại để test)

### 2.2. Tạo Molotov Pickup Prefab

Làm tương tự như GrenadePickup:

1. Tạo GameObject mới: `MolotovPickup`
2. Thêm **Sprite Renderer** với sprite Molotov
3. Thêm **Collider2D** (Is Trigger = true)
4. Thêm Component **MolotovPickup**
5. Cấu hình tương tự GrenadePickup
6. Tạo Prefab trong `Assets/Resources/Prefab`

---

## Bước 3: Setup UI để Hiển thị Số Lượng

### 3.1. Cập nhật EquipmentSlotUI

1. Tìm GameObject có component **EquipmentSlotUI** trong Scene (thường là Canvas)

2. Trong Inspector của **EquipmentSlotUI**, bạn sẽ thấy:
   - **Slot Icons** (Array size: 3) - Icon của từng slot
   - **Slot Borders** (Array size: 3) - Viền highlight
   - **Slot Numbers** (Array size: 3) - Số slot (1, 2, 3)
   - **Slot Amount Texts** (Array size: 3) - **MỚI**: Text hiển thị số lượng

### 3.2. Tạo TextMeshProUGUI cho Số Lượng

Cho mỗi slot (Slot 1, Slot 2, Slot 3):

1. Tìm GameObject của slot UI (ví dụ: `Slot1`, `Slot2`, `Slot3`)

2. Tạo TextMeshProUGUI con:
   - Right-click trên slot GameObject → **UI** → **Text - TextMeshPro**
   - Đặt tên: `AmountText`

3. Cấu hình TextMeshProUGUI:
   - **Rect Transform**: 
     - Anchor: Bottom-Right
     - Position: Điều chỉnh để text ở góc dưới bên phải của icon
   - **Text**: `0` (mặc định)
   - **Font Size**: `16-20` (tùy kích thước slot)
   - **Color**: `White`
   - **Alignment**: Center
   - **Bold**: Có thể bật để dễ nhìn

4. Gán vào EquipmentSlotUI:
   - Chọn GameObject có **EquipmentSlotUI**
   - Trong **Slot Amount Texts** array:
     - Element 0: Kéo `Slot1/AmountText` vào
     - Element 1: Kéo `Slot2/AmountText` vào
     - Element 2: Kéo `Slot3/AmountText` vào

### 3.3. Layout UI Mẫu

```
Canvas
└── EquipmentSlots
    ├── Slot1
    │   ├── Icon (Image)
    │   ├── Border (Image)
    │   ├── NumberText (TextMeshProUGUI) - "1"
    │   └── AmountText (TextMeshProUGUI) - "5" (số lượng)
    ├── Slot2
    │   └── ... (tương tự)
    └── Slot3
        └── ... (tương tự)
```

---

## Bước 4: Đặt Pickup Items trong Scene

### 4.1. Đặt Grenade Pickup

1. Kéo prefab `GrenadePickup` từ `Assets/Resources/Prefab` vào Scene
2. Đặt ở vị trí muốn player nhặt
3. Lặp lại để đặt nhiều Grenade pickup trong map

### 4.2. Đặt Molotov Pickup

1. Kéo prefab `MolotovPickup` từ `Assets/Resources/Prefab` vào Scene
2. Đặt ở vị trí muốn player nhặt
3. Lặp lại để đặt nhiều Molotov pickup trong map

---

## Bước 5: Kiểm Tra Layer Settings

### 5.1. Đảm Bảo Player Layer Được Set Đúng

1. Chọn GameObject **Player**
2. Trong Inspector, kiểm tra **Layer**:
   - Nên đặt là layer riêng (ví dụ: "Player")
   - Hoặc sử dụng layer mặc định nhưng phải khớp với cấu hình trong Pickup scripts

### 5.2. Cấu Hình Layer trong Pickup Scripts

1. Chọn một **GrenadePickup** trong Scene
2. Trong Inspector, phần **Player Layer**:
   - Chọn layer của Player (ví dụ: chỉ chọn "Player" layer)

---

## Bước 6: Test Hệ Thống

### 6.1. Test Nhặt Item

1. Chạy game (Play mode)
2. Di chuyển Player đến gần GrenadePickup hoặc MolotovPickup
3. Kiểm tra:
   - Item biến mất khi Player va chạm
   - Số lượng trên UI slot tăng lên
   - Console log hiển thị: "Added X Grenade/Molotov"

### 6.2. Test Sử Dụng Item

1. Chuyển sang slot có Grenade hoặc Molotov (phím 1, 2, 3 hoặc scroll wheel)
2. Click chuột để ném
3. Kiểm tra:
   - Item được ném ra
   - Số lượng trên UI giảm đi 1
   - Khi số lượng = 0, không thể ném (CanUse() trả về false)
   - Icon slot bị làm mờ khi số lượng = 0

### 6.3. Test Max Stack

1. Nhặt nhiều Grenade/Molotov cho đến khi đạt max stack
2. Kiểm tra:
   - Console log hiển thị: "Grenade/Molotov đã đạt max stack!"
   - Không thể nhặt thêm khi đã đầy

---

## Tùy Chỉnh Nâng Cao

### Thay Đổi Max Stack

Trong **InventorySystem**:
- **Grenade Max Stack**: Thay đổi giá trị (mặc định: 10)
- **Molotov Max Stack**: Thay đổi giá trị (mặc định: 10)

### Thay Đổi Số Lượng Khi Nhặt

Trong **GrenadePickup** hoặc **MolotovPickup**:
- **Amount**: Thay đổi số lượng nhặt được (mặc định: 1)

### Thêm Items Mới (Smoke Bomb, Flash Bomb)

1. Thêm vào enum `WeaponType` trong `IWeapon.cs`:
   ```csharp
   SmokeBomb,
   FlashBomb
   ```

2. Tạo Pickup script tương tự (copy từ GrenadePickup và sửa)

3. Thêm vào `InventorySystem.InitializeItems()`:
   ```csharp
   items[WeaponType.SmokeBomb] = new ItemData(WeaponType.SmokeBomb, smokeBombMaxStack);
   ```

4. Tạo Weapon class implement `IConsumableWeapon`

---

## Troubleshooting

### Item Không Biến Mất Khi Nhặt

- Kiểm tra **Collider2D** có **Is Trigger** = `true`
- Kiểm tra **Player Layer** trong Pickup script có đúng không
- Kiểm tra Player có Collider2D không

### Số Lượng Không Hiển Thị trên UI

- Kiểm tra **Slot Amount Texts** trong EquipmentSlotUI đã được gán chưa
- Kiểm tra TextMeshProUGUI có được kích hoạt không
- Kiểm tra InventorySystem có được gắn vào Player không

### Không Thể Ném Khi Có Số Lượng

- Kiểm tra InventorySystem.Instance không null
- Kiểm tra Console log để xem lỗi cụ thể
- Đảm bảo GrenadeWeapon/MolotovWeapon đã được cập nhật với code mới

### Số Lượng Không Giảm Khi Ném

- Kiểm tra `Use()` method có gọi `InventorySystem.Instance.UseItem()` không
- Kiểm tra `CanUse()` có kiểm tra `HasAmmo()` không

---

## Kết Luận

Hệ thống đã được setup hoàn chỉnh! Player giờ có thể:
- ✅ Nhặt Grenade và Molotov từ map
- ✅ Lưu trữ với số lượng giới hạn
- ✅ Chỉ sử dụng khi có số lượng > 0
- ✅ Xem số lượng trên UI slot
- ✅ Slot bị disable/gray khi hết item

Hệ thống dễ mở rộng để thêm Smoke Bomb, Flash Bomb sau này!
