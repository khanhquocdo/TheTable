# Enemy Spawn System

Hệ thống spawn và respawn enemy với Object Pooling, hỗ trợ player proximity check để tạm dừng respawn khi player quá gần.

## Kiến trúc

### 1. EnemyPool.cs
**Singleton pattern** - Quản lý object pool cho tất cả enemy types.

**Chức năng:**
- Dictionary-based pool system (mỗi enemy prefab có pool riêng)
- Tự động khởi tạo pool khi cần
- Reset enemy về trạng thái ban đầu khi return về pool
- Hỗ trợ nhiều loại enemy prefab khác nhau

**Thành phần:**
- `poolDictionary`: Dictionary lưu Queue cho mỗi prefab
- `allEnemiesDictionary`: Dictionary lưu List tất cả enemy đã tạo
- `InitializePool(GameObject prefab)`: Khởi tạo pool cho một prefab
- `GetEnemy(GameObject prefab)`: Lấy enemy từ pool
- `ReturnEnemyToPool(GameObject enemy, GameObject prefab)`: Trả enemy về pool

### 2. EnemySpawnPoint.cs
**Component** - Quản lý logic spawn/respawn cho mỗi spawn point.

**Chức năng:**
- Spawn enemy từ pool tại vị trí spawn point
- Đếm thời gian respawn sau khi enemy chết
- Tạm dừng timer khi player trong block radius
- Chỉ spawn khi player không quá gần
- Gắn EnemySpawnedHandler vào enemy khi spawn

**Thành phần:**
- `enemyPrefab`: Prefab của enemy sẽ spawn
- `respawnDelay`: Thời gian chờ trước khi respawn (giây)
- `blockRespawnRadius`: Bán kính block respawn (nếu player trong vùng này, timer tạm dừng)
- `playerLayer`: Layer của Player để detect

### 3. EnemySpawnedHandler.cs
**Component** - Được gắn tự động vào enemy khi spawn, track spawn point và return về pool khi chết.

**Chức năng:**
- Subscribe Health.OnDeath event
- Thông báo cho EnemySpawnPoint khi enemy chết
- Tự động được add bởi EnemySpawnPoint (không cần add vào prefab)

### 4. Health.cs (đã cập nhật)
**Component** - Đã được cập nhật để hỗ trợ pooling.

**Thay đổi:**
- Kiểm tra EnemySpawnedHandler component
- Không destroy enemy nếu có spawn handler (handler sẽ return về pool)
- Thêm method `ResetHealth()` để reset health khi reuse từ pool

## Luồng hoạt động

1. **Khởi tạo:**
   - EnemyPool.Instance được tạo (Singleton, DontDestroyOnLoad)
   - EnemySpawnPoint.Start() gọi EnemyPool.InitializePool() cho prefab của nó
   - Spawn enemy đầu tiên

2. **Enemy chết:**
   - Health.Die() được gọi
   - Health kiểm tra EnemySpawnedHandler, nếu có thì không destroy
   - Health.OnDeath event được invoke
   - EnemySpawnedHandler.OnEnemyDeath() được gọi
   - EnemySpawnPoint.OnEnemyDeath() được gọi
   - Enemy được return về pool
   - EnemySpawnPoint bắt đầu respawn timer

3. **Respawn:**
   - Timer đếm thời gian (respawnDelay)
   - Nếu player trong blockRespawnRadius, timer tạm dừng
   - Khi player rời khỏi vùng, timer tiếp tục
   - Khi timer hết, kiểm tra player một lần nữa
   - Nếu player không quá gần, spawn enemy mới từ pool

## Setup trong Unity

### Bước 1: Tạo EnemyPool
1. Tạo GameObject mới trong scene: `EnemyPool`
2. Add component `EnemyPool`
3. Cấu hình:
   - `Initial Pool Size Per Prefab`: Số lượng enemy ban đầu cho mỗi prefab (mặc định: 5)
   - `Max Pool Size Per Prefab`: Số lượng enemy tối đa cho mỗi prefab (mặc định: 30)
4. GameObject này sẽ tự động được DontDestroyOnLoad

### Bước 2: Setup Enemy Prefab
**Quan trọng:** Enemy prefab cần có:
- Component `Health`
- Component `Rigidbody2D` (nếu enemy di chuyển)
- Collider2D (nếu cần)

**Lưu ý:** KHÔNG cần add `EnemySpawnedHandler` vào prefab - component này sẽ được add tự động khi spawn.

### Bước 3: Tạo Spawn Point
1. Tạo GameObject mới tại vị trí muốn spawn enemy (có thể là Empty GameObject)
2. Add component `EnemySpawnPoint`
3. Cấu hình:
   - `Enemy Prefab`: Kéo enemy prefab vào đây
   - `Respawn Delay`: Thời gian chờ trước khi respawn (giây, mặc định: 5)
   - `Block Respawn Radius`: Bán kính block respawn (mặc định: 3)
   - `Player Layer`: Chọn layer của Player (thường là "Player" layer)
   - `Show Gizmos`: Bật/tắt hiển thị gizmos trong Scene view
   - `Gizmo Color`: Màu hiển thị spawn point
   - `Block Radius Color`: Màu hiển thị block radius

### Bước 4: Setup Player Layer
1. Đảm bảo Player GameObject có:
   - Tag: "Player" (hoặc có component PlayerMovement)
   - Layer: Đặt layer phù hợp (ví dụ: "Player")
   - Collider2D: Player cần có Collider2D để detect proximity

2. Trong EnemySpawnPoint, chọn đúng `Player Layer` trong Inspector

### Bước 5: (Tùy chọn) Cấu hình Health Component trên Enemy Prefab
- Nếu enemy dùng với spawn system, có thể set `Destroy On Death = false` (không bắt buộc vì Health đã tự động check spawn handler)

## Gizmos

EnemySpawnPoint có gizmos để visualize trong Scene view:
- **Đỏ (Gizmo Color)**: Vị trí spawn point
- **Vàng (Block Radius Color)**: Bán kính block respawn

Bật `Show Gizmos` trong Inspector để xem.

## Mở rộng

Hệ thống được thiết kế để dễ mở rộng:

### Thêm Boss Spawn
- Tạo BossSpawnPoint kế thừa từ EnemySpawnPoint
- Override logic spawn/respawn nếu cần
- Có thể thêm trigger conditions (ví dụ: spawn sau khi clear hết enemy thường)

### Thêm Wave Spawn
- Tạo EnemyWaveManager
- Quản lý nhiều EnemySpawnPoint
- Enable/disable spawn points theo wave

### Thêm Spawn Giới Hạn Số Lần
- Thêm field `maxSpawnCount` vào EnemySpawnPoint
- Đếm số lần spawn
- Dừng spawn khi đạt max

## Lưu ý

1. **EnemyPool là Singleton**: Chỉ cần một instance trong scene (hoặc trong toàn bộ game nếu DontDestroyOnLoad)

2. **Player cần Collider2D**: EnemySpawnPoint sử dụng Physics2D.OverlapCircle để detect player, nên Player cần có Collider2D

3. **Enemy Prefab không cần EnemySpawnedHandler**: Component này sẽ được add tự động khi spawn

4. **Health component tự động detect spawn handler**: Không cần cấu hình gì thêm, Health sẽ tự động không destroy enemy nếu có spawn handler

5. **Pool được khởi tạo tự động**: Khi EnemySpawnPoint spawn enemy lần đầu, pool sẽ được tự động khởi tạo nếu chưa có
