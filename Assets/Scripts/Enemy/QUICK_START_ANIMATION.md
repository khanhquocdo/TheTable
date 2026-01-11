# Quick Start - Enemy Animation System

## 🚀 Setup Nhanh (5 phút)

### Bước 1: Attach Component (1 phút)
1. Chọn Enemy GameObject
2. Add Component → `EnemyAnimator`
3. Component sẽ tự động gán `Animator`, `Rigidbody2D`, `EnemyController`

### Bước 2: Tạo Animator Controller (2 phút)
1. Tạo Animator Controller: `Assets/Animations/Enemy/Enemy.controller`
2. Thêm 5 Parameters:
   - `DirectionX` (Float)
   - `DirectionY` (Float)
   - `Speed` (Float)
   - `IsMoving` (Bool)
   - `IsAttacking` (Bool)

### Bước 3: Tạo Blend Trees (2 phút)
1. Tạo 4 Blend Trees (Idle, AttackIdle, Run, AttackRun)
2. Mỗi Blend Tree: Type = **2D Freeform Directional**
3. Add 8 motions cho 8 hướng
4. Set Positions theo 8 hướng (Up, Up-Right, Right, Down-Right, Down, Down-Left, Left, Up-Left)

### Bước 4: Setup State Machine
1. Tạo 4 States, gán Blend Trees
2. Tạo Transitions với conditions:
   - `IsMoving = true/false`
   - `IsAttacking = true/false`
3. Set Default State = Idle

### Bước 5: Gán Animator Controller
1. Chọn Enemy GameObject
2. Drag `Enemy.controller` vào Animator component
3. Test trong Play Mode!

---

## 📋 Animator Parameters Checklist

- [ ] `DirectionX` (Float)
- [ ] `DirectionY` (Float)
- [ ] `Speed` (Float)
- [ ] `IsMoving` (Bool)
- [ ] `IsAttacking` (Bool)

---

## 🎯 Animation States Checklist

- [ ] `Idle` → `Idle_BlendTree`
- [ ] `AttackIdle` → `AttackIdle_BlendTree`
- [ ] `Run` → `Run_BlendTree`
- [ ] `AttackRun` → `AttackRun_BlendTree`

---

## 🔄 Transition Conditions

| From | To | Conditions |
|------|-----|------------|
| Idle | AttackIdle | `IsAttacking = true` AND `IsMoving = false` |
| Idle | Run | `IsMoving = true` AND `IsAttacking = false` |
| Idle | AttackRun | `IsMoving = true` AND `IsAttacking = true` |
| AttackIdle | Idle | `IsAttacking = false` AND `IsMoving = false` |
| AttackIdle | Run | `IsMoving = true` AND `IsAttacking = false` |
| AttackIdle | AttackRun | `IsMoving = true` AND `IsAttacking = true` |
| Run | Idle | `IsMoving = false` AND `IsAttacking = false` |
| Run | AttackIdle | `IsMoving = false` AND `IsAttacking = true` |
| Run | AttackRun | `IsMoving = true` AND `IsAttacking = true` |
| AttackRun | Idle | `IsMoving = false` AND `IsAttacking = false` |
| AttackRun | AttackIdle | `IsMoving = false` AND `IsAttacking = true` |
| AttackRun | Run | `IsMoving = true` AND `IsAttacking = false` |

**Lưu ý:**
- Transition Duration: 0.1-0.2s
- Has Exit Time: **false**
- Can Transition To Self: **false**

---

## 🐛 Troubleshooting

### Animation không chạy?
- ✅ Kiểm tra Animator Controller đã gán vào Animator component
- ✅ Kiểm tra Animator component enabled
- ✅ Kiểm tra EnemyAnimator component attached

### Direction sai?
- ✅ Kiểm tra Blend Tree Position (8 hướng)
- ✅ Verify Parameters DirectionX, DirectionY
- ✅ Check Gizmos trong Scene View (green ray = final direction)

### Không chuyển animation?
- ✅ Kiểm tra Transition Conditions
- ✅ Verify Parameters IsMoving, IsAttacking
- ✅ Check Has Exit Time = false

---

## 📚 Full Documentation

Xem file `README_ENEMY_ANIMATION.md` để biết chi tiết đầy đủ.

---

**Done! Animation system đã sẵn sàng! 🎉**

