/// <summary>
/// Enum định nghĩa tất cả các Audio ID trong game
/// Mỗi ID tương ứng với một âm thanh cụ thể
/// </summary>
public enum AudioID
{
    // ========== Player ==========

    // ========== Player Weapons ==========
    // Gun
    Gun_Fire,
    Gun_Hit,
    Gun_Empty,

    // Grenade
    Grenade_Throw,
    Grenade_Explosion,
    Grenade_Fuse,

    // Molotov
    Molotov_Throw,
    Molotov_Break,
    Molotov_FireArea_Start,
    Molotov_FireArea_Burning, // Loop

    // C4
    C4_Place,
    C4_Detonate,
    C4_Beep, // Loop khi đã đặt

    // ========== Enemy ==========
    // Melee Enemy
    Enemy_Melee_Attack,
    Enemy_Melee_Hit,
    Enemy_Melee_Death,
    Enemy_Melee_Hurt,

    // Shooter Enemy
    Enemy_Shooter_Fire,
    Enemy_Shooter_Death,
    Enemy_Shooter_Hurt,

    // Tank
    Tank_Fire,
    Tank_Explosion,
    Tank_Engine, // Loop
    Tank_Turret_Rotate,
    Tank_Death,

    // ========== UI ==========
    UI_Click,
    UI_Hover,
    UI_Pickup,
    UI_Error,
    UI_Success,

    // ========== Ambient ==========
    Ambient_Wind,
    Ambient_Fire,

    // ========== Placeholder - Thêm các ID mới ở đây ==========
    Player_Footstep,
    Player_Hit,
    Player_Death,
    None // Không có audio (optional, dùng cho trường hợp không có sound)
}
