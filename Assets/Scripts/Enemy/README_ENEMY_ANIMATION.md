# Hướng Dẫn Setup Animation System cho Enemy

## 📋 Tổng Quan

Hệ thống animation cho Enemy sử dụng **Animator Controller + Blend Tree (2D Freeform Directional)** để quản lý 4 animation chính với 8 hướng.

---

## 🎯 Sơ Đồ Logic Chọn Animation

```
                    ┌─────────────────┐
                    │  Enemy State    │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
        ┌─────▼─────┐  ┌────▼────┐  ┌─────▼─────┐
        │   Idle    │  │  Chase  │  │  Attack   │
        │  State    │  │  State  │  │  State    │
        └─────┬─────┘  └────┬────┘  └─────┬─────┘
              │             │              │
              │     ┌───────┴───────┐      │
              │     │  Is Moving?   │      │
              │     └───────┬───────┘      │
              │             │              │
        ┌─────▼─────────────▼──────────────▼─────┐
        │       Animation Selection Logic        │
        └───────────────────┬────────────────────┘
                            │
            ┌───────────────┴───────────────┐
            │                               │
     ┌──────▼──────┐              ┌────────▼──────┐
     │  IsMoving?  │              │ IsAttacking?  │
     └──────┬──────┘              └────────┬──────┘
            │                               │
    ┌───────┴───────┐            ┌─────────┴─────────┐
    │               │            │                    │
┌───▼───┐    ┌──────▼─────┐  ┌──▼───┐       ┌───────▼─────┐
│ False │    │   True     │  │False │       │    True     │
│ Idle  │    │    Run     │  │      │       │ AttackRun   │
└───────┘    └────────────┘  │      │       └─────────────┘
                             │      │
                      ┌──────▼──────▼──────┐
                      │  IsMoving && Attack│
                      │     AttackIdle     │
                      └────────────────────┘

Direction Priority:
1. Nếu IsAttacking → Dùng Aim Direction (hướng nhắm vào Player)
2. Nếu IsMoving → Dùng Move Direction (hướng di chuyển)
3. Nếu đứng yên → Giữ Last Valid Direction (hướng cuối cùng)
```

**Kết quả 4 Animation:**
- **Idle**: `IsMoving = false` AND `IsAttacking = false`
- **AttackIdle**: `IsMoving = false` AND `IsAttacking = true`
- **Run**: `IsMoving = true` AND `IsAttacking = false`
- **AttackRun**: `IsMoving = true` AND `IsAttacking = true`

### Decision Table

| IsMoving | IsAttacking | Animation Selected | Blend Tree Used |
|----------|-------------|-------------------|-----------------|
| `false`  | `false`     | **Idle**          | `Idle_BlendTree` |
| `false`  | `true`      | **AttackIdle**    | `AttackIdle_BlendTree` |
| `true`   | `false`     | **Run**           | `Run_BlendTree` |
| `true`   | `true`      | **AttackRun**     | `AttackRun_BlendTree` |

**Direction Selection Priority:**
1. **Priority 1**: Nếu `IsAttacking = true` → Dùng **Aim Direction** (hướng từ Enemy đến Player)
2. **Priority 2**: Nếu `IsMoving = true` → Dùng **Move Direction** (hướng từ `Rigidbody2D.velocity`)
3. **Priority 3**: Nếu đứng yên → Giữ **Last Valid Direction** (hướng cuối cùng trước khi dừng)

**8 Hướng Mapping:**
- **Up (0°)**: (0, 1)
- **Up-Right (45°)**: (0.707, 0.707)
- **Right (90°)**: (1, 0)
- **Down-Right (135°)**: (0.707, -0.707)
- **Down (180°)**: (0, -1)
- **Down-Left (225°)**: (-0.707, -0.707)
- **Left (270°)**: (-1, 0)
- **Up-Left (315°)**: (-0.707, 0.707)

---

## 📝 Danh Sách Animator Parameters

### Float Parameters (cho Blend Tree)
| Parameter Name | Type | Default | Mô tả |
|---------------|------|---------|-------|
| `DirectionX` | Float | 0 | Hướng X (-1 đến 1) cho Blend Tree |
| `DirectionY` | Float | 0 | Hướng Y (-1 đến 1) cho Blend Tree |
| `Speed` | Float | 0 | Magnitude của velocity (để transition mượt) |

### Bool Parameters
| Parameter Name | Type | Default | Mô tả |
|---------------|------|---------|-------|
| `IsMoving` | Bool | false | Có đang di chuyển không |
| `IsAttacking` | Bool | false | Có đang tấn công không |

---

## 🛠️ Cách Setup Blend Tree 8 Hướng

### Bước 1: Tạo Animator Controller

1. Tạo Animator Controller mới: `Assets/Animations/Enemy/Enemy.controller`
2. Mở Animator Window (Window → Animation → Animator)
3. Chọn Animator Controller vừa tạo

### Bước 2: Tạo Parameters

Trong Animator Window, tab **Parameters**, thêm các parameters sau:
- `DirectionX` (Float)
- `DirectionY` (Float)
- `Speed` (Float)
- `IsMoving` (Bool)
- `IsAttacking` (Bool)

### Bước 3: Tạo Blend Tree cho Idle

1. Right-click trong Animator Window → **Create State** → **From New Blend Tree**
2. Đặt tên: `Idle_BlendTree`
3. Double-click vào Blend Tree để mở
4. Trong Inspector, chọn:
   - **Blend Type**: `2D Freeform Directional`
   - **Parameters**: `DirectionX`, `DirectionY`
5. Add Motion: Right-click → **Add Motion Field**
6. Thêm 8 animation clip cho 8 hướng (ví dụ: `Enemy_Idle_Up`, `Enemy_Idle_UpRight`, ...)
7. Thiết lập Position cho mỗi motion:
   - **Up (0°)**: (0, 1)
   - **Up-Right (45°)**: (0.707, 0.707)
   - **Right (90°)**: (1, 0)
   - **Down-Right (135°)**: (0.707, -0.707)
   - **Down (180°)**: (0, -1)
   - **Down-Left (225°)**: (-0.707, -0.707)
   - **Left (270°)**: (-1, 0)
   - **Up-Left (315°)**: (-0.707, 0.707)

### Bước 4: Tạo Blend Tree cho AttackIdle, Run, AttackRun

Lặp lại Bước 3 cho:
- `AttackIdle_BlendTree` (8 hướng)
- `Run_BlendTree` (8 hướng)
- `AttackRun_BlendTree` (8 hướng)

### Bước 5: Setup State Machine

1. Tạo 4 states:
   - `Idle` → Motion: `Idle_BlendTree`
   - `AttackIdle` → Motion: `AttackIdle_BlendTree`
   - `Run` → Motion: `Run_BlendTree`
   - `AttackRun` → Motion: `AttackRun_BlendTree`

2. Tạo Transitions giữa các states:

```
Idle → AttackIdle:
  Condition: IsAttacking = true AND IsMoving = false

Idle → Run:
  Condition: IsMoving = true AND IsAttacking = false

Idle → AttackRun:
  Condition: IsMoving = true AND IsAttacking = true

AttackIdle → Idle:
  Condition: IsAttacking = false AND IsMoving = false

AttackIdle → Run:
  Condition: IsMoving = true AND IsAttacking = false

AttackIdle → AttackRun:
  Condition: IsMoving = true AND IsAttacking = true

Run → Idle:
  Condition: IsMoving = false AND IsAttacking = false

Run → AttackIdle:
  Condition: IsMoving = false AND IsAttacking = true

Run → AttackRun:
  Condition: IsMoving = true AND IsAttacking = true

AttackRun → Idle:
  Condition: IsMoving = false AND IsAttacking = false

AttackRun → AttackIdle:
  Condition: IsMoving = false AND IsAttacking = true

AttackRun → Run:
  Condition: IsMoving = true AND IsAttacking = false
```

**Lưu ý quan trọng:**
- Set **Transition Duration** = 0.1-0.2s để chuyển mượt
- **Has Exit Time** = false (không chờ animation kết thúc)
- **Can Transition To Self** = false (tránh loop)

### Bước 6: Set Default State

1. Right-click vào `Idle` state → **Set as Layer Default State**

---

## 💻 Script C# - EnemyAnimator

Script `EnemyAnimator.cs` tự động cập nhật animation dựa trên:
- **Movement Direction**: Từ `Rigidbody2D.velocity`
- **Aim Direction**: Từ hướng nhắm vào Player
- **Movement State**: Tính từ velocity magnitude
- **Attack State**: Từ `EnemyController.IsAttacking`

### Cách Sử Dụng

1. **Attach Component:**
   - Add `EnemyAnimator` component vào Enemy GameObject
   - Đảm bảo có `Animator`, `Rigidbody2D`, `EnemyController` components

2. **Setup Inspector:**
   - Gán Animator component (auto-assign nếu chưa có)
   - Gán Rigidbody2D (auto-assign nếu chưa có)
   - Gán EnemyController (auto-assign nếu chưa có)
   - Tùy chỉnh `Movement Threshold` (mặc định: 0.1)
   - Tùy chỉnh `Direction Dead Zone` (mặc định: 0.01)

3. **Animation sẽ tự động chạy!** Script cập nhật trong `Update()`

### Public Methods (Optional)

```csharp
// Set trạng thái tấn công thủ công (nếu cần override)
enemyAnimator.SetIsAttacking(true);

// Set hướng nhắm thủ công (nếu cần override)
enemyAnimator.SetAimDirection(Vector2.up);

// Reset về trạng thái ban đầu
enemyAnimator.ResetToDefault();
```

---

## ✅ Best Practices

### 1. Animation Clips Naming Convention

Đặt tên nhất quán:
```
Enemy_[State]_[Direction]
```
Ví dụ:
- `Enemy_Idle_Up`
- `Enemy_Idle_UpRight`
- `Enemy_AttackIdle_Right`
- `Enemy_Run_DownLeft`
- `Enemy_AttackRun_Left`

### 2. Blend Tree Threshold Settings

- Sử dụng **automatic thresholds** để Unity tự tính toán
- Hoặc manual threshold với khoảng cách đều nhau giữa các motion

### 3. Animation Loop Settings

- **Idle, AttackIdle**: Loop = true
- **Run, AttackRun**: Loop = true
- Tất cả animation nên có cùng frame rate (khuyến nghị: 12-24 FPS)

### 4. Transition Settings

- **Duration**: 0.1-0.2s (đủ mượt, không quá chậm)
- **Exit Time**: false (responsive)
- **Interruption Source**: None (tránh bị gián đoạn bất ngờ)

### 5. Performance Optimization

- Sử dụng **Animator Culling** = Always Animate (đảm bảo animation luôn chạy)
- Hoặc Cull Update Transforms nếu enemy off-screen
- Disable Animator khi enemy death (tiết kiệm performance)

### 6. Debug Tools

Script `EnemyAnimator` có built-in Gizmos:
- **Green Ray**: Final direction (hướng animation hiện tại)
- **Red Ray**: Aim direction (khi đang bắn)
- **Blue Ray**: Move direction (khi đang di chuyển)

Enable Gizmos trong Scene View để xem.

---

## ⚠️ Lỗi Thường Gặp & Cách Fix

### 1. Animation không chuyển đổi mượt

**Nguyên nhân:**
- Transition duration quá ngắn/dài
- Has Exit Time = true

**Fix:**
- Set Transition Duration = 0.1-0.2s
- Uncheck Has Exit Time
- Kiểm tra điều kiện transition đúng

### 2. Direction nhấp nháy / không ổn định

**Nguyên nhân:**
- Direction threshold quá nhỏ
- Velocity thay đổi liên tục

**Fix:**
- Tăng `directionDeadZone` trong EnemyAnimator (0.01 → 0.05)
- Tăng `movementThreshold` để filter noise
- Sử dụng `lastValidDirection` khi đứng yên

### 3. Animation sai hướng

**Nguyên nhân:**
- Blend Tree position không đúng
- Direction mapping sai

**Fix:**
- Kiểm tra 8 hướng trong Blend Tree có đúng coordinates không
- Verify `EightDirections` array trong EnemyAnimator.cs
- Test với gizmos để xem direction ray

### 4. Enemy không chuyển sang AttackIdle/AttackRun

**Nguyên nhân:**
- `IsAttacking` parameter không được set
- Transition condition sai

**Fix:**
- Kiểm tra `EnemyController.IsAttacking` có được set khi vào AttackState không
- Verify transition condition: `IsAttacking = true`
- Check Animator Window Parameters panel

### 5. Animation bị lag / giật

**Nguyên nhân:**
- Update quá thường xuyên
- Blend Tree quá phức tạp

**Fix:**
- EnemyAnimator update mỗi frame là OK (Unity tối ưu rồi)
- Giảm số motion trong Blend Tree (nếu không cần 8 hướng, dùng 4 hướng)
- Check performance profiler

### 6. Animation không reset khi enemy chết

**Nguyên nhân:**
- Không handle death state

**Fix:**
- Disable Animator khi enemy death:
```csharp
// Trong EnemyController.OnEnemyDeath()
Animator animator = GetComponent<Animator>();
if (animator != null)
{
    animator.enabled = false;
}
```

### 7. Blend Tree không nhận direction

**Nguyên nhân:**
- Parameter name sai
- Parameter type sai (phải là Float)

**Fix:**
- Verify parameter name: `DirectionX`, `DirectionY` (case-sensitive)
- Check parameter type = Float
- Verify Animator Controller được gán vào Animator component

---

## 🔧 Tùy Chỉnh Nâng Cao

### Thay đổi số hướng (từ 8 → 4)

Nếu chỉ cần 4 hướng (Up, Down, Left, Right):

1. Trong `EnemyAnimator.cs`, thay đổi `EightDirections`:
```csharp
private static readonly Vector2[] EightDirections = new Vector2[]
{
    Vector2.up,
    Vector2.right,
    Vector2.down,
    Vector2.left
};
```

2. Trong Blend Tree, chỉ add 4 motion thay vì 8

### Thêm Animation khác (Jump, Dodge, etc.)

1. Thêm Bool parameter: `IsJumping`, `IsDodging`
2. Thêm Blend Tree hoặc Animation Clip
3. Thêm logic trong `EnemyAnimator.cs` hoặc tạo state riêng trong Animator Controller

---

## 📚 Tài Liệu Tham Khảo

- [Unity Animator Controller Documentation](https://docs.unity3d.com/Manual/class-AnimatorController.html)
- [Blend Tree Documentation](https://docs.unity3d.com/Manual/class-BlendTree.html)
- [2D Freeform Directional Blend Tree](https://docs.unity3d.com/Manual/2DFreeformDirectionalBlendTree.html)

---

## 🎮 Checklist Setup

- [ ] Tạo Animator Controller
- [ ] Tạo 5 Parameters (DirectionX, DirectionY, Speed, IsMoving, IsAttacking)
- [ ] Import 32 animation clips (4 states × 8 hướng)
- [ ] Tạo 4 Blend Trees (Idle, AttackIdle, Run, AttackRun)
- [ ] Setup 8 hướng trong mỗi Blend Tree
- [ ] Tạo 4 States và gán Blend Trees
- [ ] Tạo Transitions với điều kiện đúng
- [ ] Set Default State = Idle
- [ ] Attach EnemyAnimator component vào Enemy
- [ ] Gán Animator Controller vào Animator component
- [ ] Test trong Play Mode
- [ ] Verify Gizmos hiển thị đúng direction
- [ ] Tối ưu Transition Duration và Settings

---

**Chúc bạn setup thành công! 🚀**
