using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GetAudioInput : MonoBehaviour
{
    public float volumeThreshold = 0.1f; 
    public float pitchThreshold = 0.1f; 
    [SerializeField] private Rigidbody2D rb;

    public AudioSource audioSource;
    public static float volume;
    public static float pitch;
    public float moveSpeed = 5f;     
    public float jumpForce = 50f;  

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = Microphone.Start(null, true, 1, AudioSettings.outputSampleRate);
        audioSource.loop = true;
        while (!(Microphone.GetPosition(null) > 0)) { }
        audioSource.Play();
    }

    void Update()
    {
        float[] samples = new float[audioSource.clip.samples * audioSource.clip.channels];
        audioSource.clip.GetData(samples, 0);

        volume = 0f;
        pitch = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            volume += Mathf.Abs(samples[i]);
        }
        volume /= samples.Length;

        float[] spectrum = new float[512];
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
        float maxVal = 0f;
        int maxIndex = 0;
        for (int i = 0; i < spectrum.Length; i++)
        {
            if (spectrum[i] > maxVal && spectrum[i] > pitchThreshold)
            {
                maxVal = spectrum[i];
                maxIndex = i;
            }
        }
        pitch = maxIndex * (AudioSettings.outputSampleRate / 2f) / spectrum.Length;

        if (volume > volumeThreshold)
        {
            Debug.Log("___________________");
            Debug.Log("Volume detected: " + volume);
        }

        if (pitch > pitchThreshold)
        {
            Debug.Log("Pitch detected: " + pitch);
            Debug.Log("___________________");
        }

        if (pitch < 100f) 
        {
            MoveForward();
        }
        if (pitch > 300f)
        {
            Jump();
        }

        // Both volume and pitch detection log:
        if ((volume > volumeThreshold) && (pitch > pitchThreshold))
        {
            Debug.Log("Volume detected: " + volume + " " + "Pitch detected: " + pitch);
        }
    }

    // Move the player forward
    private void MoveForward()
    {
        rb.velocity = new Vector2(transform.position.y * Time.deltaTime * moveSpeed, rb.velocity.y);
    }

    // Make the player jump
    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce);
    }

}
