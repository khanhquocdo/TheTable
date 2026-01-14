using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Audio Manager trung tâm - quản lý tất cả audio trong game
/// Singleton pattern, sử dụng AudioSource Pooling
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Database")]
    [SerializeField] private AudioDatabase audioDatabase;
    
    [Header("Audio Source Pool")]
    [SerializeField] private AudioSourcePool audioSourcePool;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;
    
    [Range(0f, 1f)]
    [SerializeField] private float uiVolume = 1f;
    
    [Range(0f, 1f)]
    [SerializeField] private float ambientVolume = 1f;
    
    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private int maxPoolSize = 50;
    
    // Dictionary để track các AudioSource đang phát theo AudioID (cho loop sounds)
    private Dictionary<AudioID, AudioSource> activeLoopSources = new Dictionary<AudioID, AudioSource>();
    
    // Dictionary để track tất cả AudioSource đang phát theo AudioID (bao gồm cả non-loop)
    private Dictionary<AudioID, List<AudioSource>> activeAudioSourcesByID = new Dictionary<AudioID, List<AudioSource>>();
    
    // List để track tất cả AudioSource đang phát (cho cleanup)
    private List<AudioSource> activeSources = new List<AudioSource>();
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSourcePool();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    /// <summary>
    /// Khởi tạo AudioSourcePool nếu chưa có
    /// </summary>
    private void InitializeAudioSourcePool()
    {
        if (audioSourcePool == null)
        {
            GameObject poolObj = new GameObject("AudioSourcePool");
            poolObj.transform.SetParent(transform);
            audioSourcePool = poolObj.AddComponent<AudioSourcePool>();
            audioSourcePool.SetPoolSize(initialPoolSize, maxPoolSize);
            audioSourcePool.InitializePool();
        }
    }
    
    void Start()
    {
        // Validate audioDatabase
        if (audioDatabase == null)
        {
            Debug.LogError("AudioManager: AudioDatabase chưa được gán! Hãy tạo AudioDatabase và gán vào AudioManager.");
        }
    }
    
    /// <summary>
    /// Phát audio theo AudioID (Global - không theo vị trí)
    /// </summary>
    public void PlayAudio(AudioID audioID)
    {
        PlayAudio(audioID, Vector3.zero, false);
    }
    
    /// <summary>
    /// Phát audio theo AudioID tại vị trí cụ thể (Positional)
    /// </summary>
    public void PlayAudio(AudioID audioID, Vector3 position)
    {
        PlayAudio(audioID, position, true);
    }
    
    /// <summary>
    /// Phát audio theo AudioID (Core method)
    /// </summary>
    private void PlayAudio(AudioID audioID, Vector3 position, bool usePosition)
    {
        if (audioID == AudioID.None) return;
        
        if (audioDatabase == null)
        {
            Debug.LogError("AudioManager: AudioDatabase chưa được gán!");
            return;
        }
        
        if (audioSourcePool == null)
        {
            Debug.LogError("AudioManager: AudioSourcePool chưa được khởi tạo!");
            InitializeAudioSourcePool();
            if (audioSourcePool == null)
            {
                return;
            }
        }
        
        AudioData audioData = audioDatabase.GetAudioData(audioID);
        if (audioData == null)
        {
            Debug.LogWarning($"AudioManager: Không tìm thấy AudioData cho AudioID '{audioID}'");
            return;
        }
        
        if (audioData.clip == null)
        {
            Debug.LogWarning($"AudioManager: AudioData cho AudioID '{audioID}' không có AudioClip!");
            return;
        }
        
        // Nếu là loop sound, kiểm tra xem đã đang phát chưa
        if (audioData.loop)
        {
            if (activeLoopSources.ContainsKey(audioID))
            {
                AudioSource existingSource = activeLoopSources[audioID];
                if (existingSource != null && existingSource.isPlaying)
                {
                    // Đã đang phát, không cần phát lại
                    return;
                }
                else
                {
                    // Đã dừng nhưng chưa cleanup, cleanup trước
                    activeLoopSources.Remove(audioID);
                    if (activeSources.Contains(existingSource))
                    {
                        activeSources.Remove(existingSource);
                    }
                    if (audioSourcePool != null)
                    {
                        audioSourcePool.ReturnAudioSource(existingSource);
                    }
                }
            }
        }
        
        // Lấy AudioSource từ pool
        AudioSource audioSource = audioSourcePool.GetAudioSource();
        if (audioSource == null)
        {
            Debug.LogError("AudioManager: Không thể lấy AudioSource từ pool!");
            return;
        }
        
        // Cấu hình AudioSource
        ConfigureAudioSource(audioSource, audioData, position, usePosition);
        
        // Phát audio
        audioSource.Play();
        
        // Track AudioSource
        if (!activeSources.Contains(audioSource))
        {
            activeSources.Add(audioSource);
        }
        
        // Track AudioSource theo AudioID
        if (!activeAudioSourcesByID.ContainsKey(audioID))
        {
            activeAudioSourcesByID[audioID] = new List<AudioSource>();
        }
        if (!activeAudioSourcesByID[audioID].Contains(audioSource))
        {
            activeAudioSourcesByID[audioID].Add(audioSource);
        }
        
        // Nếu là loop, thêm vào activeLoopSources
        if (audioData.loop)
        {
            activeLoopSources[audioID] = audioSource;
        }
        
        // Nếu không loop, tự động trả về pool sau khi phát xong
        if (!audioData.loop)
        {
            StartCoroutine(ReturnAudioSourceWhenFinished(audioSource, audioData.clip.length));
        }
    }
    
    /// <summary>
    /// Cấu hình AudioSource theo AudioData
    /// </summary>
    private void ConfigureAudioSource(AudioSource audioSource, AudioData audioData, Vector3 position, bool usePosition)
    {
        audioSource.clip = audioData.clip;
        audioSource.volume = GetFinalVolume(audioData);
        audioSource.pitch = audioData.pitch;
        audioSource.loop = audioData.loop;
        
        // Cấu hình 3D/2D
        if (usePosition && audioData.is3D)
        {
            audioSource.transform.position = position;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = audioData.maxDistance;
            audioSource.minDistance = audioData.minDistance;
        }
        else
        {
            audioSource.spatialBlend = 0f; // 2D sound (global)
            audioSource.transform.position = Vector3.zero;
        }
    }
    
    /// <summary>
    /// Tính volume cuối cùng (master * type volume * audioData volume)
    /// </summary>
    private float GetFinalVolume(AudioData audioData)
    {
        float typeVolume = 1f;
        
        switch (audioData.audioType)
        {
            case AudioType.SFX:
                typeVolume = sfxVolume;
                break;
            case AudioType.UI:
                typeVolume = uiVolume;
                break;
            case AudioType.Ambient:
                typeVolume = ambientVolume;
                break;
        }
        
        return masterVolume * typeVolume * audioData.volume;
    }
    
    /// <summary>
    /// Coroutine trả AudioSource về pool sau khi phát xong
    /// </summary>
    private IEnumerator ReturnAudioSourceWhenFinished(AudioSource audioSource, float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        
        if (audioSource != null && !audioSource.loop)
        {
            ReturnAudioSourceToPool(audioSource);
        }
    }
    
    /// <summary>
    /// Dừng audio theo AudioID (chỉ dừng loop sounds)
    /// </summary>
    public void StopAudio(AudioID audioID)
    {
        if (activeLoopSources.TryGetValue(audioID, out AudioSource audioSource))
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            ReturnAudioSourceToPool(audioSource);
            activeLoopSources.Remove(audioID);
        }
    }
    
    /// <summary>
    /// Dừng tất cả audio đang phát của một AudioID (bao gồm cả non-loop)
    /// </summary>
    public void StopAllAudioByID(AudioID audioID)
    {
        if (activeAudioSourcesByID.TryGetValue(audioID, out List<AudioSource> sources))
        {
            // Tạo copy của list để tránh modify trong khi iterate
            List<AudioSource> sourcesCopy = new List<AudioSource>(sources);
            
            // Dừng tất cả AudioSource của AudioID này
            foreach (AudioSource source in sourcesCopy)
            {
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                }
                ReturnAudioSourceToPool(source);
            }
            
            // Cleanup
            activeAudioSourcesByID.Remove(audioID);
            
            // Cleanup từ activeLoopSources nếu có
            if (activeLoopSources.ContainsKey(audioID))
            {
                activeLoopSources.Remove(audioID);
            }
        }
    }
    
    /// <summary>
    /// Trả AudioSource về pool và cleanup
    /// </summary>
    private void ReturnAudioSourceToPool(AudioSource audioSource)
    {
        if (audioSource == null) return;
        
        if (activeSources.Contains(audioSource))
        {
            activeSources.Remove(audioSource);
        }
        
        // Cleanup từ activeAudioSourcesByID
        foreach (var kvp in activeAudioSourcesByID)
        {
            if (kvp.Value.Contains(audioSource))
            {
                kvp.Value.Remove(audioSource);
                // Nếu list rỗng, xóa entry
                if (kvp.Value.Count == 0)
                {
                    activeAudioSourcesByID.Remove(kvp.Key);
                }
                break;
            }
        }
        
        if (audioSourcePool != null)
        {
            audioSourcePool.ReturnAudioSource(audioSource);
        }
    }
    
    /// <summary>
    /// Dừng tất cả audio
    /// </summary>
    public void StopAllAudio()
    {
        foreach (var kvp in activeLoopSources)
        {
            if (kvp.Value != null)
            {
                kvp.Value.Stop();
            }
        }
        
        activeLoopSources.Clear();
        
        foreach (AudioSource source in activeSources)
        {
            if (source != null)
            {
                source.Stop();
            }
        }
        
        activeSources.Clear();
        
        if (audioSourcePool != null)
        {
            audioSourcePool.StopAll();
        }
    }
    
    /// <summary>
    /// Điều chỉnh Master Volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllActiveVolumes();
    }
    
    /// <summary>
    /// Điều chỉnh SFX Volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAllActiveVolumes();
    }
    
    /// <summary>
    /// Điều chỉnh UI Volume
    /// </summary>
    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        UpdateAllActiveVolumes();
    }
    
    /// <summary>
    /// Điều chỉnh Ambient Volume
    /// </summary>
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        UpdateAllActiveVolumes();
    }
    
    /// <summary>
    /// Cập nhật volume cho tất cả AudioSource đang phát
    /// Lưu ý: Method này sẽ cập nhật volume cho các AudioSource đang phát,
    /// nhưng vì không track AudioData cho mỗi source, nên chỉ update cho loop sources (có track AudioID)
    /// </summary>
    private void UpdateAllActiveVolumes()
    {
        foreach (var kvp in activeLoopSources)
        {
            if (kvp.Value != null && kvp.Value.isPlaying)
            {
                AudioData audioData = audioDatabase.GetAudioData(kvp.Key);
                if (audioData != null)
                {
                    kvp.Value.volume = GetFinalVolume(audioData);
                }
            }
        }
    }
    
    /// <summary>
    /// Get Master Volume
    /// </summary>
    public float GetMasterVolume() => masterVolume;
    
    /// <summary>
    /// Get SFX Volume
    /// </summary>
    public float GetSFXVolume() => sfxVolume;
    
    /// <summary>
    /// Get UI Volume
    /// </summary>
    public float GetUIVolume() => uiVolume;
    
    /// <summary>
    /// Get Ambient Volume
    /// </summary>
    public float GetAmbientVolume() => ambientVolume;
    
    void OnDestroy()
    {
        StopAllAudio();
    }
}
