# Giải Thích Các Lực Boid

## Tổng Quan

Boid Movement System sử dụng 4 lực chính để tính toán hướng di chuyển của enemy. Mỗi lực có vai trò riêng và được kết hợp với nhau để tạo ra hành vi di chuyển tự nhiên như bầy đàn.

---

## 1. Separation (Tách Biệt) 🔴

### Mục Đích
Tránh enemy đứng chồng lên nhau, đặc biệt quan trọng khi nhiều enemy cùng tấn công player.

### Cách Hoạt Động
- Enemy phát hiện các neighbor trong phạm vi `neighborRadius`
- Với mỗi neighbor quá gần, enemy tính toán vector đẩy ra xa
- Vector này được normalize và chia cho khoảng cách (càng gần càng mạnh)
- Tổng hợp tất cả các vector đẩy để tạo lực Separation cuối cùng

### Công Thức
```
separationForce = Σ((currentPos - neighborPos).normalized / distance)
```

### Khi Nào Quan Trọng
- **Khi enemy tấn công**: Giúp enemy không đứng chồng lên nhau khi ở trong tầm tấn công
- **Khi spawn nhiều enemy**: Tránh tình trạng enemy spawn và đứng cùng một chỗ

### Điều Chỉnh
- **Tăng `separationWeight`**: Enemy sẽ tránh nhau mạnh hơn, khoảng cách lớn hơn
- **Giảm `separationWeight`**: Enemy có thể đứng gần nhau hơn

### Ví Dụ
```
Enemy A ở vị trí (0, 0)
Enemy B ở vị trí (1, 0) - quá gần!

Separation force = (0,0) - (1,0) = (-1, 0) - đẩy sang trái
```

---

## 2. Cohesion (Gắn Kết) 🔵

### Mục Đích
Giữ enemy di chuyển thành nhóm, không tách rời hoàn toàn.

### Cách Hoạt Động
- Enemy tính toán trung tâm khối lượng (center of mass) của tất cả neighbor
- Tạo vector từ vị trí hiện tại đến trung tâm khối lượng
- Vector này là lực Cohesion - kéo enemy về phía nhóm

### Công Thức
```
centerOfMass = Σ(neighborPositions) / neighborCount
cohesionForce = (centerOfMass - currentPos).normalized
```

### Khi Nào Quan Trọng
- **Khi đuổi player**: Giúp enemy di chuyển thành nhóm thay vì tách rời
- **Khi có nhiều enemy**: Tạo cảm giác enemy hoạt động có tổ chức

### Điều Chỉnh
- **Tăng `cohesionWeight`**: Enemy sẽ di chuyển thành nhóm chặt chẽ hơn
- **Giảm `cohesionWeight`**: Enemy có thể tách rời hơn

### Ví Dụ
```
Enemy A ở (0, 0)
Enemy B ở (3, 0)
Enemy C ở (0, 3)

Center of Mass = ((0+3+0)/3, (0+0+3)/3) = (1, 1)
Cohesion force = (1, 1) - (0, 0) = (1, 1) - kéo về trung tâm nhóm
```

---

## 3. Alignment (Căn Chỉnh) 🟡

### Mục Đích
Làm cho enemy di chuyển cùng hướng với neighbor, tạo cảm giác có tổ chức.

### Cách Hoạt Động
- Enemy lấy velocity (hướng di chuyển) của tất cả neighbor
- Tính trung bình các velocity này
- Normalize để tạo lực Alignment - hướng di chuyển chung

### Công Thức
```
averageVelocity = Σ(neighborVelocities.normalized) / neighborCount
alignmentForce = averageVelocity.normalized
```

### Khi Nào Quan Trọng
- **Khi đuổi player**: Giúp enemy di chuyển cùng hướng, trông có tổ chức hơn
- **Khi có nhiều enemy**: Tạo cảm giác như một đơn vị thống nhất

### Điều Chỉnh
- **Tăng `alignmentWeight`**: Enemy sẽ di chuyển cùng hướng mạnh hơn
- **Giảm `alignmentWeight`**: Enemy có thể di chuyển hướng khác nhau hơn

### Ví Dụ
```
Enemy A di chuyển về phía (1, 0) - phải
Enemy B di chuyển về phía (1, 0) - phải
Enemy C di chuyển về phía (0, 1) - lên

Average velocity = ((1,0) + (1,0) + (0,1)) / 3 = (0.67, 0.33)
Alignment force = (0.67, 0.33).normalized - hướng chung là phải-lên
```

---

## 4. Target Attraction (Hút Về Mục Tiêu) 🟢

### Mục Đích
Đảm bảo enemy luôn có xu hướng đuổi theo player, không bị phân tán bởi các lực khác.

### Cách Hoạt Động
- Enemy tính vector từ vị trí hiện tại đến target (Player)
- Normalize vector này để tạo lực Target Attraction

### Công Thức
```
targetForce = (targetPos - currentPos).normalized
```

### Khi Nào Quan Trọng
- **Luôn luôn**: Đây là lực quan trọng nhất để enemy đuổi player
- **Khi có nhiều lực khác**: Đảm bảo enemy vẫn đuổi player dù có các lực Boid khác

### Điều Chỉnh
- **Tăng `targetWeight`**: Enemy đuổi player mạnh hơn, ít bị phân tán
- **Giảm `targetWeight`**: Enemy có thể bị phân tán bởi các lực khác

### Ví Dụ
```
Enemy ở (0, 0)
Player ở (5, 5)

Target force = (5, 5) - (0, 0) = (5, 5).normalized = (0.707, 0.707)
- Hướng về player (góc 45 độ)
```

---

## Kết Hợp Các Lực

Tất cả các lực được kết hợp với trọng số tương ứng:

```
totalForce = separationForce * separationWeight
           + cohesionForce * cohesionWeight
           + alignmentForce * alignmentWeight
           + targetForce * targetWeight

finalDirection = totalForce.normalized
```

### Ví Dụ Kết Hợp

Giả sử:
- Separation: (0.5, 0) với weight = 2 → contribution = (1, 0)
- Cohesion: (0, 0.5) với weight = 1 → contribution = (0, 0.5)
- Alignment: (0.3, 0.3) với weight = 1 → contribution = (0.3, 0.3)
- Target: (0.707, 0.707) với weight = 3 → contribution = (2.12, 2.12)

```
totalForce = (1, 0) + (0, 0.5) + (0.3, 0.3) + (2.12, 2.12)
           = (3.42, 2.92)

finalDirection = (3.42, 2.92).normalized
               ≈ (0.76, 0.65)
```

Hướng cuối cùng sẽ nghiêng về phía target (vì targetWeight cao nhất) nhưng vẫn chịu ảnh hưởng của các lực khác.

---

## Điều Chỉnh Trọng Số

### Cân Bằng Cơ Bản (Khuyến Nghị)
```
Separation Weight: 2.0  (quan trọng để tránh chồng lên nhau)
Cohesion Weight:  1.0  (giữ nhóm vừa phải)
Alignment Weight: 1.0  (cùng hướng vừa phải)
Target Weight:    3.0  (ưu tiên đuổi player)
```

### Enemy Tấn Công Tập Trung
```
Separation Weight: 3.0  (tăng để tránh chồng lên nhau khi tấn công)
Cohesion Weight:  0.5  (giảm để không quá tập trung)
Alignment Weight: 1.5  (tăng để di chuyển cùng hướng)
Target Weight:    4.0  (tăng để đuổi player mạnh hơn)
```

### Enemy Di Chuyển Phân Tán
```
Separation Weight: 1.0  (giảm để có thể gần nhau hơn)
Cohesion Weight:  0.3  (giảm để tách rời hơn)
Alignment Weight: 0.5  (giảm để di chuyển tự do hơn)
Target Weight:    2.0  (giảm để ít tập trung vào player)
```

---

## Lưu Ý

1. **Target Weight nên cao nhất**: Đảm bảo enemy luôn đuổi player
2. **Separation Weight nên cao**: Quan trọng để tránh chồng lên nhau
3. **Cohesion và Alignment**: Điều chỉnh tùy theo style game bạn muốn
4. **Test và điều chỉnh**: Mỗi game có style khác nhau, cần test để tìm cân bằng phù hợp
