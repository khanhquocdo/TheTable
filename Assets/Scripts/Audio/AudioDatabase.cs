using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject chứa database của tất cả AudioData
/// Mỗi AudioID sẽ map với một AudioData
/// </summary>
[CreateAssetMenu(fileName = "AudioDatabase", menuName = "Audio/Audio Database")]
public class AudioDatabase : ScriptableObject
{
    [System.Serializable]
    public class AudioEntry
    {
        public AudioID audioID;
        public AudioData audioData;
    }
    
    [Header("Audio Entries")]
    [SerializeField] private List<AudioEntry> audioEntries = new List<AudioEntry>();
    
    private Dictionary<AudioID, AudioData> audioDictionary;
    private bool isInitialized = false;
    
    /// <summary>
    /// Khởi tạo dictionary từ list (gọi tự động khi cần)
    /// </summary>
    private void InitializeDictionary()
    {
        if (isInitialized) return;
        
        audioDictionary = new Dictionary<AudioID, AudioData>();
        
        foreach (var entry in audioEntries)
        {
            if (entry.audioData != null && !audioDictionary.ContainsKey(entry.audioID))
            {
                audioDictionary[entry.audioID] = entry.audioData;
            }
            else if (audioDictionary.ContainsKey(entry.audioID))
            {
                Debug.LogWarning($"AudioDatabase: AudioID '{entry.audioID}' đã tồn tại, bỏ qua entry trùng lặp.");
            }
        }
        
        isInitialized = true;
    }
    
    /// <summary>
    /// Lấy AudioData theo AudioID
    /// </summary>
    public AudioData GetAudioData(AudioID audioID)
    {
        if (!isInitialized)
        {
            InitializeDictionary();
        }
        
        if (audioDictionary != null && audioDictionary.TryGetValue(audioID, out AudioData data))
        {
            return data;
        }
        
        // Debug warning nếu không tìm thấy (trừ None)
        if (audioID != AudioID.None)
        {
            Debug.LogWarning($"AudioDatabase: Không tìm thấy AudioData cho AudioID '{audioID}'");
        }
        
        return null;
    }
    
    /// <summary>
    /// Kiểm tra xem AudioID có tồn tại trong database không
    /// </summary>
    public bool HasAudioID(AudioID audioID)
    {
        if (!isInitialized)
        {
            InitializeDictionary();
        }
        
        return audioDictionary != null && audioDictionary.ContainsKey(audioID);
    }
    
    /// <summary>
    /// Validate dữ liệu trong Editor
    /// </summary>
    private void OnValidate()
    {
        // Reset initialized flag khi editor thay đổi
        isInitialized = false;
    }
}
