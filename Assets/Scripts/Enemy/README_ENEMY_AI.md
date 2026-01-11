# Hệ Thống AI Enemy - Hướng Dẫn Sử Dụng

## Tổng Quan

Hệ thống AI Enemy sử dụng **State Pattern** để quản lý các hành vi, dễ dàng mở rộng cho các loại enemy khác nhau (shotgun, sniper, etc.).

## Cấu Trúc

### 1. EnemyData (ScriptableObject)
- Chứa tất cả thông số cấu hình của enemy
- Tạo mới: `Right Click > Create > Enemy > Enemy Data`
- Có thể tạo nhiều EnemyData khác nhau cho từng loại enemy

### 2. EnemyController
- Controller chính quản lý State Machine
- Xử lý Detection, Movement, Shooting
- **Required Components**: Rigidbody2D

### 3. States
- **EnemyIdleState**: Đứng yên hoặc patrol
- **EnemyChaseState**: Đuổi theo Player
- **EnemyAttackState**: Tấn công Player (bắn đạn)

### 4. Bullet System
- **EnemyBullet**: Đạn di chuyển thẳng, gây damage
- **EnemyBulletPool**: Object Pooling để tối ưu performance

## Setup

### Bước 1: Tạo EnemyData
1. `Right Click > Create > Enemy > Enemy Data`
2. Cấu hình các thông số:
   - Movement: moveSpeed, minDistance, maxAttackDistance
   - Detection: detectRadius, playerLayer, obstacleLayer
   - Attack: fireRate, bulletDamage, bulletSpeed, bulletLifetime

### Bước 2: Setup Layers
1. Tạo Layer "Player" và gán cho Player GameObject
2. Tạo Layer "Obstacle" cho các vật cản
3. Trong EnemyData, set:
   - `playerLayer`: Chọn layer "Player"
   - `obstacleLayer`: Chọn layer "Obstacle"

### Bước 3: Tạo Bullet Prefab
1. Tạo GameObject với:
   - SpriteRenderer (hoặc hình ảnh đạn)
   - Rigidbody2D (Gravity Scale = 0)
   - Collider2D (Is Trigger = true)
   - Component `EnemyBullet`
2. Lưu thành Prefab

### Bước 4: Setup EnemyBulletPool
1. Tạo GameObject trong scene: "EnemyBulletPool"
2. Add component `EnemyBulletPool`
3. Gán Bullet Prefab vào `bulletPrefab`
4. Set `playerLayer` và `obstacleLayer`

### Bước 5: Setup Enemy
1. Tạo Enemy GameObject với:
   - SpriteRenderer
   - Rigidbody2D
   - Collider2D (nếu cần)
   - Component `Enemy`
   - Component `EnemyController`
   - Component `Health`
2. Trong EnemyController:
   - Gán EnemyData
   - Tạo FirePoint (tự động tạo nếu chưa có)
   - Gán SpriteRenderer (tự động tìm nếu chưa có)

### Bước 6: Đảm Bảo Player có Tag
- Player GameObject phải có tag "Player"

## Mở Rộng Cho Các Loại Enemy Khác

### Ví dụ: Shotgun Enemy
1. Tạo EnemyData mới: `ShotgunEnemyData`
2. Tạo State mới: `EnemyShotgunAttackState` (kế thừa từ `EnemyAttackState`)
3. Override method `Shoot()` để bắn nhiều viên đạn cùng lúc

### Ví dụ: Sniper Enemy
1. Tạo EnemyData mới: `SniperEnemyData`
2. Tăng `detectRadius` và `maxAttackDistance`
3. Tăng `bulletDamage` và giảm `fireRate`
4. Có thể thêm state `AimState` trước khi bắn

## API Reference

### EnemyController
- `ChangeState(EnemyStateType)`: Chuyển đổi state
- `CanDetectPlayer()`: Kiểm tra có phát hiện Player không
- `HasLineOfSightToPlayer()`: Kiểm tra Line of Sight
- `GetDistanceToPlayer()`: Lấy khoảng cách đến Player
- `Move(Vector2)`: Di chuyển enemy
- `LookAt(Vector2)`: Quay mặt về vị trí
- `Shoot(Vector2)`: Bắn đạn

### EnemyBulletPool
- `SpawnBullet(position, direction, speed, damage, lifetime)`: Spawn đạn

## Debug

- Bật `showDebugGizmos` trong EnemyController để xem:
  - Detect radius (vàng)
  - Min distance (đỏ)
  - Max attack distance (xanh)
  - Line to player (xanh = có LOS, đỏ = không có LOS)



