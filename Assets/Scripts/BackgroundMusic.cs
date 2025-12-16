using UnityEngine;

/// <summary>
/// Plays background music continuously. Attach to any GameObject in the scene.
/// Music stops automatically when the scene changes.
/// </summary>
public class BackgroundMusic : MonoBehaviour
{
    [Header("Music Settings")]
    [SerializeField] private AudioClip musicClip; // The music to play
    [SerializeField] [Range(0f, 1f)] private float volume = 0.5f; // Volume level (0 to 1)
    [SerializeField] private bool playOnStart = true; // Start playing automatically
    
    private AudioSource audioSource;
    
    void Start()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("BackgroundMusic: Added AudioSource component");
        }
        
        // Configure audio source
        audioSource.clip = musicClip;
        audioSource.volume = volume;
        audioSource.loop = true; // Loop continuously
        audioSource.playOnAwake = false; // We control when to play
        
        // Play music if enabled and clip is assigned
        if (playOnStart && musicClip != null)
        {
            audioSource.Play();
            Debug.Log($"BackgroundMusic: Playing '{musicClip.name}' at volume {volume}");
        }
        else if (musicClip == null)
        {
            Debug.LogWarning("BackgroundMusic: No music clip assigned! Please assign an AudioClip in the Inspector.");
        }
    }
    
    /// <summary>
    /// Manually play the music. Useful if playOnStart is false.
    /// </summary>
    public void PlayMusic()
    {
        if (audioSource != null && musicClip != null)
        {
            audioSource.Play();
            Debug.Log("BackgroundMusic: Music started");
        }
    }
    
    /// <summary>
    /// Stop the music.
    /// </summary>
    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("BackgroundMusic: Music stopped");
        }
    }
    
    /// <summary>
    /// Pause the music.
    /// </summary>
    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("BackgroundMusic: Music paused");
        }
    }
    
    /// <summary>
    /// Resume the music if paused.
    /// </summary>
    public void ResumeMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
            Debug.Log("BackgroundMusic: Music resumed");
        }
    }
    
    /// <summary>
    /// Change the volume.
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
}
