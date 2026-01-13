using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object Pool cho AudioSource để tối ưu performance
/// Tránh instantiate/destroy AudioSource liên tục
/// </summary>
public class AudioSourcePool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private int maxPoolSize = 50;
    
    private Queue<AudioSource> poolQueue = new Queue<AudioSource>();
    private List<AudioSource> allAudioSources = new List<AudioSource>();
    private bool isInitialized = false;
    
    /// <summary>
    /// Set pool size settings (có thể gọi từ AudioManager)
    /// </summary>
    public void SetPoolSize(int initialSize, int maxSize)
    {
        if (isInitialized)
        {
            Debug.LogWarning("AudioSourcePool: Không thể thay đổi pool size sau khi đã khởi tạo!");
            return;
        }
        initialPoolSize = initialSize;
        maxPoolSize = maxSize;
    }
    
    void Awake()
    {
        // Không tự động initialize, chờ AudioManager gọi InitializePool()
        // Hoặc có thể initialize nếu cần
    }
    
    /// <summary>
    /// Khởi tạo pool với số lượng AudioSource ban đầu (public để AudioManager có thể gọi)
    /// </summary>
    public void InitializePool()
    {
        if (isInitialized) return;
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            AudioSource newSource = CreateNewAudioSource();
            poolQueue.Enqueue(newSource);
        }
        
        isInitialized = true;
    }
    
    /// <summary>
    /// Tạo một AudioSource mới (không enqueue vào pool)
    /// </summary>
    private AudioSource CreateNewAudioSource()
    {
        GameObject audioObj = new GameObject("AudioSource_Pooled");
        audioObj.transform.SetParent(transform);
        audioObj.SetActive(false);
        
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        allAudioSources.Add(audioSource);
        
        return audioSource;
    }
    
    /// <summary>
    /// Lấy một AudioSource từ pool (hoặc tạo mới nếu pool rỗng và chưa đạt max size)
    /// </summary>
    public AudioSource GetAudioSource()
    {
        // Đảm bảo pool đã được initialize
        if (!isInitialized)
        {
            InitializePool();
        }
        
        AudioSource audioSource;
        
        if (poolQueue.Count > 0)
        {
            audioSource = poolQueue.Dequeue();
        }
        else if (allAudioSources.Count < maxPoolSize)
        {
            // Tạo mới nếu chưa đạt max size
            audioSource = CreateNewAudioSource();
        }
        else
        {
            // Nếu đạt max size, tái sử dụng AudioSource cũ nhất (đang phát)
            audioSource = allAudioSources[0];
            allAudioSources.RemoveAt(0);
            allAudioSources.Add(audioSource);
            
            // Dừng audio đang phát
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        
        if (audioSource != null)
        {
            audioSource.gameObject.SetActive(true);
        }
        
        return audioSource;
    }
    
    /// <summary>
    /// Trả AudioSource về pool để tái sử dụng
    /// </summary>
    public void ReturnAudioSource(AudioSource audioSource)
    {
        if (audioSource == null) return;
        
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.gameObject.SetActive(false);
        audioSource.transform.SetParent(transform);
        audioSource.transform.position = Vector3.zero;
        
        if (!poolQueue.Contains(audioSource))
        {
            poolQueue.Enqueue(audioSource);
        }
    }
    
    /// <summary>
    /// Dừng tất cả AudioSource đang phát
    /// </summary>
    public void StopAll()
    {
        foreach (AudioSource source in allAudioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }
    }
}
