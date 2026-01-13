using UnityEngine;

/// <summary>
/// Script test để debug Audio System
/// Gắn vào GameObject và test phát audio
/// </summary>
public class AudioSystemTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private AudioID testAudioID = AudioID.Gun_Fire;
    [SerializeField] private KeyCode testKey = KeyCode.T;
    
    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            TestPlayAudio();
        }
    }
    
    public void TestPlayAudio()
    {
        Debug.Log($"=== Audio System Test ===");
        Debug.Log($"Test AudioID: {testAudioID}");
        
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager.Instance is NULL! Kiểm tra xem AudioManager có trong scene không.");
            return;
        }
        
        Debug.Log("AudioManager.Instance tồn tại!");
        
        // Test phát audio
        AudioManager.Instance.PlayAudio(testAudioID);
        Debug.Log($"Đã gọi PlayAudio({testAudioID})");
    }
    
    [ContextMenu("Test Play Audio")]
    private void TestPlayAudioContextMenu()
    {
        TestPlayAudio();
    }
}
