# Melee Enemy AI System

Hệ thống AI Enemy cận chiến hoàn chỉnh cho game 2D top-down Unity, hỗ trợ animation 8 hướng và state machine.

## Tổng quan

Hệ thống bao gồm các component chính:
- **MeleeEnemyController**: Controller chính quản lý AI và state machine
- **MeleeEnemyAnimator**: Quản lý animation 8 hướng
- **MeleeEnemyAttack**: Xử lý tấn công và gây damage
- **MeleeEnemyData**: ScriptableObject chứa dữ liệu cấu hình
- **States**: Các state classes (Idle, Chase, Attack)

## Cấu trúc File

```
Assets/Scripts/Enemy/Melee/
├── MeleeEnemyController.cs      # Controller chính
├── MeleeEnemyAnimator.cs        # Animation controller
├── MeleeEnemyAttack.cs          # Attack component
├── MeleeEnemyData.cs            # Data ScriptableObject
└── States/
    ├── MeleeEnemyState.cs       # Base state class
    ├── MeleeEnemyIdleState.cs   # Idle state
    ├── MeleeEnemyChaseState.cs  # Chase state
    └── MeleeEnemyAttackState.cs # Attack state
```

## Cài đặt

### Bước 1: Tạo MeleeEnemyData ScriptableObject

1. Trong Unity Editor, click chuột phải trong Project window
2. Chọn `Create > Enemy > Melee Enemy Data`
3. Đặt tên file (ví dụ: `BasicMeleeEnemyData`)
4. Cấu hình các thông số:
   - **Movement Settings**: `moveSpeed` (tốc độ di chuyển)
   - **Detection Settings**: `detectRadius` (bán kính phát hiện), `playerLayer`, `obstacleLayer`
   - **Attack Settings**: `attackRange`, `attackDamage`, `windUpTime`, `attackCooldown`, `attackDuration`
   - **Idle/Patrol Settings**: `canPatrol`, `patrolRadius`, `idleTime`

### Bước 2: Setup GameObject Enemy

1. Tạo GameObject mới trong scene
2. Thêm các component sau:
   - **Rigidbody2D** (tự động thêm bởi RequireComponent)
   - **Health** (tự động thêm bởi RequireComponent)
   - **MeleeEnemyController**
   - **MeleeEnemyAnimator**
   - **MeleeEnemyAttack** (tự động thêm nếu chưa có)
   - **Animator** (cần Animator Controller với các parameter sau)

3. Gán **MeleeEnemyData** vào `MeleeEnemyController`

### Bước 3: Setup Animator Controller

Animator Controller cần có các **Parameters** sau:

#### Float Parameters:
- `MoveX` (-1 đến 1): Hướng di chuyển X
- `MoveY` (-1 đến 1): Hướng di chuyển Y
- `LastMoveX` (-1 đến 1): Hướng cuối cùng X (cho Idle/Attack)
- `LastMoveY` (-1 đến 1): Hướng cuối cùng Y (cho Idle/Attack)

#### Bool Parameters:
- `IsMoving`: Đang di chuyển
- `IsAttacking`: Đang tấn công

#### Trigger Parameters (tùy chọn):
- `IsDie`: Trigger khi enemy chết

#### Blend Tree Setup:

1. Tạo Blend Tree cho **Run** animation:
   - Type: `2D Simple Directional`
   - Parameters: `MoveX`, `MoveY`
   - Thêm 8 animation clips cho 8 hướng:
     - Up (0, 1)
     - Up-Right (0.707, 0.707)
     - Right (1, 0)
     - Down-Right (0.707, -0.707)
     - Down (0, -1)
     - Down-Left (-0.707, -0.707)
     - Left (-1, 0)
     - Up-Left (-0.707, 0.707)

2. Tạo Blend Tree cho **Idle** animation:
   - Type: `2D Simple Directional`
   - Parameters: `LastMoveX`, `LastMoveY`
   - Thêm 8 animation clips cho 8 hướng (tương tự Run)

3. Tạo Blend Tree cho **Attack** animation:
   - Type: `2D Simple Directional`
   - Parameters: `LastMoveX`, `LastMoveY`
   - Thêm 8 animation clips cho 8 hướng (tương tự Run)

4. Tạo State Machine:
   - **Idle State**: Blend Tree Idle, điều kiện: `!IsMoving && !IsAttacking`
   - **Run State**: Blend Tree Run, điều kiện: `IsMoving && !IsAttacking`
   - **Attack State**: Blend Tree Attack, điều kiện: `IsAttacking`
   - **Die State**: Animation Die, trigger: `IsDie`

### Bước 4: Setup Layers

Đảm bảo có các Layer sau:
- **Player**: Layer cho Player
- **Obstacle**: Layer cho các vật cản (tường, cột, etc.)

Gán các layer này vào `MeleeEnemyData`:
- `playerLayer`: Chọn layer Player
- `obstacleLayer`: Chọn layer Obstacle

### Bước 5: Setup Player Tag

Đảm bảo Player GameObject có tag `"Player"`.

## Cách hoạt động

### State Machine

Enemy có 3 trạng thái chính:

1. **Idle State**:
   - Đứng yên hoặc patrol (nếu `canPatrol = true`)
   - Chuyển sang **Chase** khi phát hiện Player trong `detectRadius`

2. **Chase State**:
   - Di chuyển về phía Player
   - Cập nhật animation Run 8 hướng
   - Chuyển sang **Attack** khi vào `attackRange`
   - Chuyển về **Idle** nếu mất Player

3. **Attack State**:
   - Dừng lại
   - Quay mặt về Player
   - Chạy animation Attack theo hướng
   - Gây damage sau `windUpTime`
   - Cooldown `attackCooldown` giữa các đòn tấn công
   - Chuyển về **Chase** nếu Player ra khỏi `attackRange`

### Animation System

- **MoveX, MoveY**: Cập nhật khi đang di chuyển (Run animation)
- **LastMoveX, LastMoveY**: Giữ hướng cuối cùng (cho Idle và Attack animation)
- **IsMoving**: `true` khi velocity > threshold
- **IsAttacking**: `true` khi đang trong Attack state và đang tấn công

### Attack System

- Sử dụng `Physics2D.OverlapCircle` để phát hiện Player
- Hitbox được tính từ vị trí enemy + `attackOffset` + hướng tấn công
- Gây damage thông qua component `Health` trên Player
- Có wind-up time và cooldown để gameplay mượt mà

## Tùy chỉnh

### Tạo Enemy nhanh (Fast Melee Enemy)

1. Tạo MeleeEnemyData mới: `FastMeleeEnemyData`
2. Cấu hình:
   - `moveSpeed = 5f` (nhanh hơn)
   - `attackCooldown = 1f` (tấn công nhanh hơn)
   - `attackDamage = 15f` (sát thương thấp hơn)

### Tạo Enemy tank (Tank Melee Enemy)

1. Tạo MeleeEnemyData mới: `TankMeleeEnemyData`
2. Cấu hình:
   - `moveSpeed = 2f` (chậm hơn)
   - `attackCooldown = 2f` (tấn công chậm hơn)
   - `attackDamage = 40f` (sát thương cao hơn)
   - `attackRange = 2f` (tầm tấn công lớn hơn)

## Debug

### Gizmos

Khi chọn GameObject có `MeleeEnemyController`:
- **Vàng**: Bán kính phát hiện (`detectRadius`)
- **Đỏ**: Tầm tấn công (`attackRange`)
- **Xanh/Đỏ**: Đường đến Player (xanh = có Line of Sight, đỏ = không có)

Khi chọn GameObject có `MeleeEnemyAnimator`:
- **Xanh**: Hướng hiện tại (`lastValidDirection`)
- **Xanh dương**: Hướng di chuyển (`currentMoveDirection`)

Khi chọn GameObject có `MeleeEnemyAttack`:
- **Đỏ**: Hitbox tấn công (`attackRange`)

## Lưu ý

1. **Performance**: Enemy sẽ tự disable Animator khi chết để tối ưu performance
2. **Line of Sight**: Enemy chỉ tấn công khi có Line of Sight đến Player (không có obstacle chặn)
3. **Animation**: Hệ thống tự động snap direction về 8 hướng gần nhất để animation mượt mà
4. **Attack Timing**: Wind-up time và attack duration có thể được điều chỉnh trong `MeleeEnemyData`

## Troubleshooting

### Enemy không di chuyển
- Kiểm tra `Rigidbody2D` có được gán đúng không
- Kiểm tra `moveSpeed` trong `MeleeEnemyData` > 0
- Kiểm tra `MeleeEnemyController` có được enable không

### Enemy không tấn công
- Kiểm tra `attackRange` trong `MeleeEnemyData`
- Kiểm tra Player có tag `"Player"` không
- Kiểm tra `playerLayer` trong `MeleeEnemyData` có đúng không
- Kiểm tra Player có component `Health` không

### Animation không đúng hướng
- Kiểm tra Animator Controller có đủ 8 animation clips cho mỗi Blend Tree không
- Kiểm tra các Parameters trong Animator Controller có đúng tên không
- Kiểm tra `MeleeEnemyAnimator` có được gán vào `MeleeEnemyController` không

### Enemy không phát hiện Player
- Kiểm tra `detectRadius` trong `MeleeEnemyData`
- Kiểm tra Player có tag `"Player"` không
- Kiểm tra có obstacle chặn Line of Sight không
- Kiểm tra `obstacleLayer` trong `MeleeEnemyData` có đúng không

## Mở rộng

Để thêm tính năng mới:

1. **Thêm State mới**: Tạo class kế thừa `MeleeEnemyState`
2. **Thêm Behavior mới**: Tạo component mới và gọi từ Controller hoặc States
3. **Tùy chỉnh Animation**: Chỉnh sửa `MeleeEnemyAnimator` để thêm parameter mới

## Ví dụ sử dụng

```csharp
// Lấy reference đến MeleeEnemyController
MeleeEnemyController enemy = GetComponent<MeleeEnemyController>();

// Kiểm tra trạng thái tấn công
if (enemy.IsAttacking)
{
    Debug.Log("Enemy đang tấn công!");
}

// Lấy khoảng cách đến Player
float distance = enemy.GetDistanceToPlayer();
```

## Tác giả

Hệ thống được thiết kế để dễ mở rộng và tùy chỉnh cho các loại melee enemy khác nhau.
