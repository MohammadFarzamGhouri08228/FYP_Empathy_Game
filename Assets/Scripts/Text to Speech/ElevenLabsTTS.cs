using System;
using System.Collections;
using System.Text;
using TMPro; // Needed for TextMeshPro
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class ElevenLabsTTS : MonoBehaviour
{
    [Header("ElevenLabs API Settings")]
    [Tooltip("Paste your ElevenLabs API Key here")]
    public string apiKey = "3e6909801cc1533c1302631ae9bdebf943800d9e08063d101a886a697a2d7a80";

    [Tooltip("Paste the Voice ID you want to use")]
    public string voiceId = "H8TbESTZqMv5yJI2td2F"; // Your Ogre Voice ID

    [Header("Auto-Read Settings")]
    [Tooltip("If assigned, the script will read this text after the intro")]
    public TextMeshProUGUI textToRead;

    private AudioSource audioSource;
    private Coroutine currentSpeechCoroutine; 

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;

        Debug.Log($"ElevenLabsTTS: Initialized. AudioSource found: {audioSource != null}, Volume: {audioSource.volume}, Mute: {audioSource.mute}");
        
        StartCoroutine(PlayIntroAndReadText());
    }

    private IEnumerator PlayIntroAndReadText()
    {
        // 1. Speak the hardcoded intro
        Speak("Hello, I am Sam. Can you hear me?");
        
        yield return new WaitForSeconds(1f);
        yield return new WaitWhile(() => audioSource.isPlaying);
        
        yield return new WaitForSeconds(0.5f); // Short pause

        // 2. Speak whatever is in the TextMeshPro box!
        if (textToRead != null && !string.IsNullOrWhiteSpace(textToRead.text))
        {
            Speak(textToRead.text);
        }
        else
        {
            Debug.LogWarning("ElevenLabsTTS: No TextMeshPro assigned, or the text was empty.");
        }
    }

    public void Speak(string textToSay)
    {
        Debug.Log($"ElevenLabsTTS: Speak() called with: \"{textToSay}\"");

        if (string.IsNullOrWhiteSpace(textToSay))
        {
            Debug.LogWarning("ElevenLabsTTS: Speak() aborted — text is null or empty.");
            return;
        }

        if (currentSpeechCoroutine != null)
        {
            StopCoroutine(currentSpeechCoroutine);
        }

        if (audioSource != null) audioSource.Stop();

        currentSpeechCoroutine = StartCoroutine(FetchAudioAndPlay(textToSay));
    }

    public void StopSpeaking()
    {
        if (currentSpeechCoroutine != null)
        {
            StopCoroutine(currentSpeechCoroutine);
        }
        if (audioSource != null) audioSource.Stop();
    }

    [Serializable]
    private class TTSPayload
    {
        public string text;
        // UPDATED: Using the fast, free-tier compatible model
        public string model_id = "eleven_turbo_v2_5"; 
    }

    private IEnumerator FetchAudioAndPlay(string text)
    {
        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}?output_format=pcm_22050";

        TTSPayload payload = new TTSPayload { text = text };
        string jsonPayload = JsonUtility.ToJson(payload);

        Debug.Log($"ElevenLabsTTS: [1/6] Building request. URL: {url}");
        Debug.Log($"ElevenLabsTTS: [2/6] JSON payload: {jsonPayload}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("xi-api-key", apiKey);

            Debug.Log("ElevenLabsTTS: [3/6] Sending request to ElevenLabs API...");

            yield return request.SendWebRequest();

            Debug.Log($"ElevenLabsTTS: [4/6] Response received. HTTP {request.responseCode}, Result: {request.result}");

            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorBody = request.downloadHandler.text;
                Debug.LogError($"ElevenLabsTTS: FAILED — {request.error} | HTTP {request.responseCode} | Body: {errorBody}");
                yield break;
            }

            byte[] pcmData = request.downloadHandler.data;

            if (pcmData == null || pcmData.Length == 0)
            {
                Debug.LogError("ElevenLabsTTS: FAILED — Server returned success but audio data is empty.");
                yield break;
            }

            Debug.Log($"ElevenLabsTTS: [5/6] Received {pcmData.Length} bytes of PCM audio. Converting to AudioClip...");

            AudioClip clip = PCMToAudioClip(pcmData, 22050);
            if (clip == null)
            {
                Debug.LogError("ElevenLabsTTS: FAILED — Could not create AudioClip from PCM data.");
                yield break;
            }

            float clipLength = clip.length;
            Debug.Log($"ElevenLabsTTS: [6/6] AudioClip created. Duration: {clipLength:F2}s, Samples: {clip.samples}");

            audioSource.clip = clip;
            audioSource.Play();
            Debug.Log($"ElevenLabsTTS: NOW PLAYING. AudioSource.isPlaying: {audioSource.isPlaying}, Volume: {audioSource.volume}, Mute: {audioSource.mute}");
        }
    }

    /// <summary>
    /// Converts raw PCM 16-bit signed little-endian bytes into a Unity AudioClip.
    /// </summary>
    private AudioClip PCMToAudioClip(byte[] pcmData, int sampleRate)
    {
        int sampleCount = pcmData.Length / 2; // 16-bit = 2 bytes per sample
        float[] samples = new float[sampleCount];
        float maxAmplitude = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            short rawSample = (short)(pcmData[i * 2] | (pcmData[i * 2 + 1] << 8));
            float sample = rawSample / 32768f;
            samples[i] = sample;
            
            if (Mathf.Abs(sample) > maxAmplitude)
            {
                maxAmplitude = Mathf.Abs(sample);
            }
        }

        Debug.Log($"ElevenLabsTTS: Audio Max Amplitude: {maxAmplitude:F4} (If 0.0000, audio is completely silent!)");

        AudioClip clip = AudioClip.Create("ElevenLabsTTS", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}