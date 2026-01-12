# Tank Enemy AI System

Hệ thống AI Enemy Tank Vehicle hoàn chỉnh cho game 2D top-down Unity, với thân xe và tháp pháo tách riêng, bắn đạn pháo có splash damage.

## Tổng quan

Hệ thống bao gồm các component chính:
- **TankEnemyController**: Controller chính quản lý AI và state machine
- **TankMovement**: Xử lý di chuyển và xoay thân xe
- **TankTurret**: Xử lý xoay tháp pháo độc lập
- **TankWeapon**: Xử lý bắn đạn pháo
- **TankProjectile**: Đạn pháo với splash damage
- **TankProjectilePool**: Object Pooling cho projectile
- **TankEnemyData**: ScriptableObject chứa dữ liệu cấu hình
- **States**: Các state classes (Idle, MoveToPosition, Attack)

## Cấu trúc File

```
Assets/Scripts/Enemy/Tank/
├── TankEnemyController.cs      # Controller chính
├── TankMovement.cs            # Di chuyển thân xe
├── TankTurret.cs               # Xoay tháp pháo
├── TankWeapon.cs               # Bắn đạn pháo
├── TankProjectile.cs           # Đạn pháo với splash damage
├── TankProjectilePool.cs       # Object Pooling
├── TankEnemyData.cs            # Data ScriptableObject
├── README_TANK_ENEMY.md        # File này
└── States/
    ├── TankEnemyState.cs       # Base state class
    ├── TankIdleState.cs         # Idle state
    ├── TankMoveToPositionState.cs # MoveToPosition state
    └── TankAttackState.cs      # Attack state
```

## Cài đặt

### Bước 1: Tạo TankEnemyData ScriptableObject

1. Trong Unity Editor, click chuột phải trong Project window
2. Chọn `Create > Enemy > Tank Enemy Data`
3. Đặt tên file (ví dụ: `BasicTankEnemyData`)
4. Cấu hình các thông số:
   - **Movement Settings**: `moveSpeed` (tốc độ di chuyển), `bodyRotationSpeed` (tốc độ xoay thân xe)
   - **Detection Settings**: `detectRadius` (bán kính phát hiện), `playerLayer`, `obstacleLayer`
   - **Combat Settings**: `optimalAttackDistance`, `minAttackDistance`, `maxAttackDistance`
   - **Turret Settings**: `turretRotationSpeed` (tốc độ xoay tháp pháo)
   - **Weapon Settings**: `fireRate`, `projectileDamage`, `projectileSpeed`, `projectileLifetime`
   - **Projectile Explosion Settings**: `explosionRadius`, `minDamage`, `damageThroughObstacles`

### Bước 2: Tạo Tank Projectile Prefab

1. Tạo GameObject mới trong scene
2. Thêm các component sau:
   - **SpriteRenderer** (hoặc hình ảnh đạn pháo)
   - **Rigidbody2D** (Gravity Scale = 0, Collision Detection = Continuous)
   - **CircleCollider2D** (Is Trigger = true)
   - **TankProjectile** component
3. Lưu thành Prefab (ví dụ: `TankProjectilePrefab`)

### Bước 3: Setup TankProjectilePool

1. Tạo GameObject trong scene: "TankProjectilePool"
2. Add component `TankProjectilePool`
3. Gán Tank Projectile Prefab vào `projectilePrefab`
4. Cấu hình `initialPoolSize` và `maxPoolSize`

### Bước 4: Setup GameObject Tank Enemy

1. Tạo GameObject mới trong scene (ví dụ: "TankEnemy")
2. Thêm các component sau:
   - **Rigidbody2D** (tự động thêm bởi RequireComponent)
     - Gravity Scale = 0
     - Drag = 5
     - Angular Drag = 10
   - **Health** (tự động thêm bởi RequireComponent)
   - **TankEnemyController**
   - **TankMovement** (tự động thêm nếu chưa có)
   - **SpriteRenderer** (cho thân xe)

3. Tạo child GameObject "Turret":
   - Thêm **TankTurret** component
   - Thêm **TankWeapon** component
   - Thêm **SpriteRenderer** (cho tháp pháo)
   - Tạo child GameObject "FirePoint" (điểm bắn đạn)

4. Cấu trúc GameObject:
   ```
   TankEnemy (TankEnemyController, TankMovement, Rigidbody2D, Health)
   └── Turret (TankTurret, TankWeapon)
       └── FirePoint (Transform - điểm bắn đạn)
   ```

5. Gán **TankEnemyData** vào `TankEnemyController`

### Bước 5: Setup Layers

Đảm bảo có các Layer sau:
- **Player**: Layer cho Player
- **Obstacle**: Layer cho các vật cản (tường, cột, etc.)

Gán các layer này vào `TankEnemyData`:
- `playerLayer`: Chọn layer Player
- `obstacleLayer`: Chọn layer Obstacle

### Bước 6: Setup Player Tag

Đảm bảo Player GameObject có tag `"Player"`.

## Cách hoạt động

### State Machine

Tank có 3 trạng thái chính:

1. **Idle State**:
   - Đứng yên
   - Chuyển sang **MoveToPosition** khi phát hiện Player trong `detectRadius`

2. **MoveToPosition State**:
   - Di chuyển đến khoảng cách tối ưu để bắn (`optimalAttackDistance`)
   - Turret tự động aim về Player
   - Chuyển sang **Attack** khi vào khoảng cách bắn (`minAttackDistance` đến `maxAttackDistance`)
   - Chuyển về **Idle** nếu mất Player

3. **Attack State**:
   - Dừng di chuyển
   - Turret aim về Player
   - Bắn đạn pháo khi turret đã aim trúng và hết cooldown
   - Chuyển về **MoveToPosition** nếu Player ra khỏi khoảng cách bắn hoặc mất Line of Sight

### Movement System

- **Tank Body**: Di chuyển chậm, xoay theo hướng di chuyển
- **Tank Turret**: Xoay độc lập với thân xe, luôn aim về Player

### Weapon System

- **Projectile**: Bay thẳng trong không gian 2D
- **Explosion**: Khi chạm mục tiêu hoặc obstacle, đạn nổ
- **Splash Damage**: Gây damage trong bán kính `explosionRadius`
  - Damage tại tâm: `projectileDamage`
  - Damage ở rìa: `minDamage`
  - Damage giảm dần theo khoảng cách (linear interpolation)
  - Chỉ ảnh hưởng Player (không ảnh hưởng enemy khác)

### Detection System

- **Distance Detection**: Phát hiện Player trong `detectRadius`
- **Line of Sight**: Sử dụng `Physics2D.Raycast` để kiểm tra obstacle
- Tank chỉ tấn công khi có Line of Sight đến Player

## Tùy chỉnh

### Tạo Light Tank (nhanh, sát thương thấp)

1. Tạo TankEnemyData mới: `LightTankEnemyData`
2. Cấu hình:
   - `moveSpeed = 3f` (nhanh hơn)
   - `fireRate = 1.5f` (bắn nhanh hơn)
   - `projectileDamage = 20f` (sát thương thấp hơn)
   - `explosionRadius = 2f` (bán kính nổ nhỏ hơn)

### Tạo Heavy Tank (chậm, sát thương cao)

1. Tạo TankEnemyData mới: `HeavyTankEnemyData`
2. Cấu hình:
   - `moveSpeed = 1.5f` (chậm hơn)
   - `fireRate = 0.5f` (bắn chậm hơn)
   - `projectileDamage = 50f` (sát thương cao hơn)
   - `explosionRadius = 4f` (bán kính nổ lớn hơn)

## Debug

### Gizmos

Khi chọn GameObject có `TankEnemyController`:
- **Vàng**: Bán kính phát hiện (`detectRadius`)
- **Đỏ**: Khoảng cách tối đa để bắn (`maxAttackDistance`)
- **Xanh lá**: Khoảng cách tối thiểu để bắn (`minAttackDistance`)
- **Xanh dương**: Khoảng cách tối ưu để bắn (`optimalAttackDistance`)
- **Xanh/Đỏ**: Đường đến Player (xanh = có Line of Sight, đỏ = không có)

Khi chọn GameObject có `TankProjectile`:
- **Đỏ**: Bán kính nổ (`explosionRadius`)

## Lưu ý

1. **Performance**: Sử dụng Object Pooling cho projectile để tối ưu performance
2. **Line of Sight**: Tank chỉ tấn công khi có Line of Sight đến Player (không có obstacle chặn)
3. **Splash Damage**: Damage giảm dần theo khoảng cách từ tâm nổ
4. **Turret Rotation**: Tháp pháo xoay độc lập với thân xe, luôn aim về Player
5. **Movement**: Tank di chuyển chậm và xoay thân xe theo hướng di chuyển

## Troubleshooting

### Tank không di chuyển
- Kiểm tra `Rigidbody2D` có được gán đúng không
- Kiểm tra `moveSpeed` trong `TankEnemyData` > 0
- Kiểm tra `TankEnemyController` có được enable không

### Tank không bắn
- Kiểm tra `TankProjectilePool` có tồn tại trong scene không
- Kiểm tra `fireRate` trong `TankEnemyData` > 0
- Kiểm tra Turret có aim trúng Player không (tolerance = 5 độ)
- Kiểm tra `TankWeapon` component có được gán vào Turret không

### Turret không xoay
- Kiểm tra `TankTurret` component có được gán vào Turret GameObject không
- Kiểm tra `turretRotationSpeed` trong `TankEnemyData` > 0
- Kiểm tra Turret GameObject có phải là child của Tank không

### Projectile không nổ
- Kiểm tra `TankProjectile` component có được gán vào Projectile Prefab không
- Kiểm tra Collider2D có `Is Trigger = true` không
- Kiểm tra LayerMask có đúng không

### Splash damage không hoạt động
- Kiểm tra `explosionRadius` trong `TankEnemyData` > 0
- Kiểm tra Player có component `Health` không
- Kiểm tra `playerLayer` trong `TankEnemyData` có đúng không

### Tank không phát hiện Player
- Kiểm tra `detectRadius` trong `TankEnemyData`
- Kiểm tra Player có tag `"Player"` không
- Kiểm tra có obstacle chặn Line of Sight không
- Kiểm tra `obstacleLayer` trong `TankEnemyData` có đúng không

## Mở rộng

Để thêm tính năng mới:

1. **Thêm State mới**: Tạo class kế thừa `TankEnemyState`
2. **Thêm Behavior mới**: Tạo component mới và gọi từ Controller hoặc States
3. **Tùy chỉnh Projectile**: Chỉnh sửa `TankProjectile` để thêm hiệu ứng mới

## Ví dụ sử dụng

```csharp
// Lấy reference đến TankEnemyController
TankEnemyController tank = GetComponent<TankEnemyController>();

// Kiểm tra trạng thái
if (tank.CanDetectPlayer())
{
    Debug.Log("Tank đã phát hiện Player!");
}

// Lấy khoảng cách đến Player
float distance = tank.GetDistanceToPlayer();
```

## Tác giả

Hệ thống được thiết kế để dễ mở rộng và tùy chỉnh cho các loại tank enemy khác nhau.
