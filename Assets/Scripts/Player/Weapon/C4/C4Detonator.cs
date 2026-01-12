using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý và kích nổ các C4 đang được đặt trong scene
/// </summary>
public class C4Detonator : MonoBehaviour
{
    private static readonly List<C4Explosive> activeC4s = new List<C4Explosive>();

    [Header("Input Settings")]
    [SerializeField] private KeyCode detonateKey = KeyCode.X;

    void Update()
    {
        if (Input.GetKeyDown(detonateKey))
        {
            DetonateAll();
        }
    }

    /// <summary>
    /// Đăng ký một C4 mới
    /// </summary>
    public static void Register(C4Explosive c4)
    {
        if (c4 == null) return;
        if (!activeC4s.Contains(c4))
        {
            activeC4s.Add(c4);
        }
    }

    /// <summary>
    /// Hủy đăng ký C4
    /// </summary>
    public static void Unregister(C4Explosive c4)
    {
        if (c4 == null) return;
        if (activeC4s.Contains(c4))
        {
            activeC4s.Remove(c4);
        }
    }

    /// <summary>
    /// Kích nổ toàn bộ C4 đang hoạt động
    /// </summary>
    public void DetonateAll()
    {
        // Copy ra list tạm để tránh modify collection khi iterate
        var c4List = new List<C4Explosive>(activeC4s);
        foreach (var c4 in c4List)
        {
            if (c4 != null)
            {
                c4.Detonate();
            }
        }
    }
}

