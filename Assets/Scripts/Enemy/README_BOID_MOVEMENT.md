# Boid Movement System - Hướng Dẫn Sử Dụng

## Tổng Quan

Boid Movement System là hệ thống di chuyển theo kiểu bầy đàn cho Enemy trong game 2D top-down. Hệ thống này tính toán hướng di chuyển dựa trên các lực Boid, giúp enemy di chuyển một cách tự nhiên và tránh đứng chồng lên nhau.

## Các Lực Boid

### 1. Separation (Tách Biệt)
- **Mục đích**: Tránh đứng chồng lên enemy khác
- **Cách hoạt động**: Khi enemy phát hiện neighbor quá gần, nó sẽ tạo lực đẩy ra xa. Lực này càng mạnh khi khoảng cách càng nhỏ.
- **Khi nào quan trọng**: Khi nhiều enemy cùng đuổi player, separation giúp chúng không đứng chồng lên nhau khi tấn công.

### 2. Cohesion (Gắn Kết)
- **Mục đích**: Giữ khoảng cách vừa phải với đồng đội
- **Cách hoạt động**: Enemy tính toán trung tâm khối lượng (center of mass) của các neighbor và di chuyển về phía đó.
- **Khi nào quan trọng**: Giúp enemy di chuyển thành nhóm thay vì tách rời hoàn toàn.

### 3. Alignment (Căn Chỉnh)
- **Mục đích**: Di chuyển cùng hướng với neighbor
- **Cách hoạt động**: Enemy tính toán hướng di chuyển trung bình của các neighbor và điều chỉnh để di chuyển cùng hướng.
- **Khi nào quan trọng**: Tạo cảm giác enemy di chuyển có tổ chức, như một đơn vị thống nhất.

### 4. Target Attraction (Hút Về Mục Tiêu)
- **Mục đích**: Hướng về Player
- **Cách hoạt động**: Enemy luôn có xu hướng di chuyển về phía target (Player).
- **Khi nào quan trọng**: Đảm bảo enemy vẫn đuổi theo player dù có các lực Boid khác.

## Cách Tích Hợp Với Enemy AI

### Tự Động Tích Hợp

BoidMovement đã được tích hợp tự động vào `EnemyController` và `MeleeEnemyController`. Khi bạn thêm component `BoidMovement` vào enemy prefab, hệ thống sẽ tự động:

1. **Trong Chase State**: Nếu `enableInChase = true`, enemy sẽ sử dụng Boid direction thay vì direction trực tiếp đến player.
2. **Trong Attack State**: Nếu `enableInAttack = true`, enemy sẽ sử dụng Boid với hệ số giảm lực (`attackStateForceMultiplier`).

### Ví Dụ Sử Dụng

#### Cách 1: Sử Dụng Tự Động (Khuyến Nghị)

```csharp
// Trong ChaseState, chỉ cần gọi Move() như bình thường
// EnemyController sẽ tự động sử dụng Boid nếu có component BoidMovement

public override void FixedUpdate()
{
    if (!controller.HasPlayerTarget())
    {
        return;
    }
    
    Vector2 playerPosition = controller.GetPlayerPosition();
    Vector2 enemyPosition = controller.transform.position;
    Vector2 directionToPlayer = (playerPosition - enemyPosition).normalized;
    
    // Move() sẽ tự động sử dụng Boid nếu có và đang ở Chase state
    controller.Move(directionToPlayer);
}
```

#### Cách 2: Sử Dụng Trực Tiếp Boid Direction

```csharp
// Nếu muốn kiểm soát nhiều hơn, có thể lấy Boid direction trực tiếp

public override void FixedUpdate()
{
    if (!controller.HasPlayerTarget())
    {
        return;
    }
    
    // Lấy Boid direction
    Vector2 boidDirection = controller.GetBoidDirection();
    
    if (boidDirection != Vector2.zero)
    {
        // Sử dụng Boid direction
        controller.Move(boidDirection);
    }
    else
    {
        // Fallback về direction gốc
        Vector2 playerPosition = controller.GetPlayerPosition();
        Vector2 directionToPlayer = (playerPosition - (Vector2)controller.transform.position).normalized;
        controller.Move(directionToPlayer);
    }
}
```

## Hướng Dẫn Setup Trong Unity Inspector

### Bước 1: Thêm Component BoidMovement

1. Chọn Enemy Prefab (Melee hoặc Shooter)
2. Trong Inspector, click **Add Component**
3. Tìm và thêm **Boid Movement**

### Bước 2: Cấu Hình Boid Settings

#### Neighbor Detection
- **Neighbor Radius**: Bán kính phát hiện neighbor (khuyến nghị: 3-5)
- **Enemy Layer**: Layer của Enemy để phát hiện neighbor
  - Tạo Layer mới: `Enemy` hoặc sử dụng layer có sẵn
  - Gán layer này cho tất cả enemy prefab

#### Boid Forces Weights
Điều chỉnh các trọng số để có hành vi mong muốn:

- **Separation Weight** (2.0): 
  - Cao hơn = enemy tránh nhau mạnh hơn
  - Thấp hơn = enemy có thể đứng gần nhau hơn
  
- **Cohesion Weight** (1.0):
  - Cao hơn = enemy di chuyển thành nhóm chặt chẽ hơn
  - Thấp hơn = enemy có thể tách rời hơn
  
- **Alignment Weight** (1.0):
  - Cao hơn = enemy di chuyển cùng hướng mạnh hơn
  - Thấp hơn = enemy có thể di chuyển hướng khác nhau
  
- **Target Weight** (3.0):
  - Cao hơn = enemy đuổi player mạnh hơn
  - Thấp hơn = enemy có thể bị phân tán bởi các lực khác

#### State Control

- **Enable In Chase**: Bật Boid khi ở Chase state (khuyến nghị: `true`)
- **Enable In Attack**: Bật Boid khi ở Attack state (khuyến nghị: `false` hoặc `true` với `attackStateForceMultiplier` thấp)
- **Attack State Force Multiplier**: Hệ số giảm lực khi ở Attack state (0-1)
  - `0` = tắt hoàn toàn Boid trong Attack
  - `0.3` = giảm lực Boid xuống 30% (khuyến nghị)
  - `1` = giữ nguyên lực Boid

### Bước 3: Setup Layer

1. Vào **Edit > Project Settings > Tags and Layers**
2. Tạo Layer mới: `Enemy` (hoặc sử dụng layer có sẵn)
3. Gán layer này cho tất cả Enemy prefab:
   - Chọn Enemy prefab
   - Trong Inspector, ở phần Layer, chọn `Enemy`
4. Trong BoidMovement component, set **Enemy Layer** = `Enemy`

### Bước 4: Kiểm Tra

1. Chạy game và spawn nhiều enemy cùng lúc
2. Quan sát enemy di chuyển:
   - Chúng không nên đứng chồng lên nhau
   - Chúng nên di chuyển thành nhóm khi đuổi player
   - Chúng nên có xu hướng di chuyển cùng hướng

### Bước 5: Điều Chỉnh (Tùy Chọn)

Nếu hành vi chưa như mong muốn:

1. **Enemy đứng chồng lên nhau**: Tăng `Separation Weight`
2. **Enemy tách rời quá xa**: Tăng `Cohesion Weight`
3. **Enemy di chuyển hỗn loạn**: Tăng `Alignment Weight`
4. **Enemy không đuổi player tốt**: Tăng `Target Weight`
5. **Enemy quá gần nhau khi tấn công**: Giảm `Attack State Force Multiplier` hoặc tắt `Enable In Attack`

## Debug Gizmos

Bật **Show Debug Gizmos** trong Inspector để xem:

- **Vòng tròn xanh lá**: Neighbor radius
- **Mũi tên đỏ**: Lực Separation (đẩy ra)
- **Mũi tên xanh dương**: Lực Cohesion (kéo về trung tâm)
- **Mũi tên vàng**: Lực Alignment (cùng hướng)
- **Mũi tên xanh lá**: Lực Target (hướng về player)
- **Mũi tên trắng**: Tổng hợp lực (direction cuối cùng)

## Mở Rộng Trong Tương Lai

### Obstacle Avoidance

Để thêm obstacle avoidance:

1. Thêm method `CalculateObstacleAvoidance()` trong `BoidMovement.cs`
2. Sử dụng `Physics2D.Raycast` hoặc `Physics2D.CircleCast` để phát hiện obstacle
3. Thêm lực tránh obstacle vào `CalculateBoidDirection()`

Ví dụ:

```csharp
private Vector2 CalculateObstacleAvoidance()
{
    Vector2 avoidanceForce = Vector2.zero;
    float avoidanceRadius = 2f;
    
    // Raycast về phía trước để phát hiện obstacle
    RaycastHit2D hit = Physics2D.CircleCast(
        transform.position,
        0.5f,
        rb.velocity.normalized,
        avoidanceRadius,
        obstacleLayer
    );
    
    if (hit.collider != null)
    {
        // Tính lực tránh
        Vector2 avoidanceDirection = -hit.normal;
        avoidanceForce = avoidanceDirection.normalized;
    }
    
    return avoidanceForce;
}
```

### Formation System

Để thêm formation system:

1. Tạo class `FormationManager` để quản lý formation
2. Thêm method `CalculateFormationForce()` trong `BoidMovement`
3. Enemy sẽ cố gắng giữ vị trí trong formation khi di chuyển

## Lưu Ý Performance

- BoidMovement sử dụng `Physics2D.OverlapCircleNonAlloc` để tối ưu performance
- Cache size mặc định là 20 neighbor, có thể tăng nếu cần
- Nếu có quá nhiều enemy (>50), cân nhắc giảm `neighborRadius` hoặc tối ưu thêm

## Troubleshooting

### Enemy không di chuyển theo Boid
- Kiểm tra `Enable In Chase` đã được bật chưa
- Kiểm tra Enemy Layer đã được set đúng chưa
- Kiểm tra enemy có Rigidbody2D không

### Enemy vẫn đứng chồng lên nhau
- Tăng `Separation Weight`
- Tăng `Neighbor Radius` để phát hiện neighbor tốt hơn
- Kiểm tra Enemy Layer có đúng không

### Enemy không đuổi player
- Tăng `Target Weight`
- Kiểm tra target đã được set chưa (tự động tìm Player tag)

### Performance chậm
- Giảm `Neighbor Radius`
- Giảm số lượng enemy trong scene
- Tối ưu Physics2D settings trong Project Settings
