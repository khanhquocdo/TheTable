using UnityEngine;

/// <summary>
/// ScriptableObject chứa cấu hình cho một Audio
/// Mỗi AudioData tương ứng với một AudioID
/// </summary>
[CreateAssetMenu(fileName = "NewAudioData", menuName = "Audio/Audio Data")]
public class AudioData : ScriptableObject
{
    [Header("Audio Clip")]
    [Tooltip("AudioClip cần phát")]
    public AudioClip clip;
    
    [Header("Audio Settings")]
    [Tooltip("Volume (0-1)")]
    [Range(0f, 1f)]
    public float volume = 1f;
    
    [Tooltip("Pitch (0.5-2.0)")]
    [Range(0.5f, 2f)]
    public float pitch = 1f;
    
    [Tooltip("Có lặp lại không")]
    public bool loop = false;
    
    [Header("Audio Type")]
    [Tooltip("Loại audio (SFX, UI, Ambient)")]
    public AudioType audioType = AudioType.SFX;
    
    [Header("Positional Settings")]
    [Tooltip("Có phát theo vị trí 3D không (nếu false sẽ phát global)")]
    public bool is3D = true;
    
    [Tooltip("Khoảng cách tối đa có thể nghe (chỉ áp dụng khi is3D = true)")]
    public float maxDistance = 50f;
    
    [Tooltip("Min distance (chỉ áp dụng khi is3D = true)")]
    public float minDistance = 1f;
    
    /// <summary>
    /// Validate dữ liệu
    /// </summary>
    private void OnValidate()
    {
        if (clip == null)
        {
            Debug.LogWarning($"AudioData '{name}': AudioClip chưa được gán!");
        }
        
        volume = Mathf.Clamp01(volume);
        pitch = Mathf.Clamp(pitch, 0.5f, 2f);
    }
}
