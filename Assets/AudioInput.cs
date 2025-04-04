using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GetAudioInput : MonoBehaviour
{
    // Microphone properties
    private AudioSource audioSource;
    private string microphone;
    private float[] samples;
    private float[] spectrum;
    private float volume;
    private float pitch;

    // Character control properties
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    private Rigidbody rb;

    // For detecting pitch (frequency range)
    private const int sampleWindow = 1024;

    void Start()
    {
        // Initialize AudioSource
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        // Get the default microphone device
        microphone = Microphone.devices[0];
        audioSource.clip = Microphone.Start(microphone, true, 10, 44100);
        audioSource.loop = true;

        // Wait for microphone to start
        while (Microphone.GetPosition(microphone) <= 0) { }

        // Start playing the audio from the microphone
        audioSource.Play();

        // Initialize arrays for audio analysis
        samples = new float[sampleWindow];
        spectrum = new float[sampleWindow];
    }

    void Update()
    {
        // Get audio data from microphone
        audioSource.GetOutputData(samples, 0);  // Get data for volume calculation
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);  // Get frequency data

        // Calculate volume (RMS - Root Mean Square)
        volume = GetVolume();

        // Calculate pitch (dominant frequency)
        pitch = GetPitch();

        // Perform actions based on pitch and volume
        HandleCharacterActions(pitch, volume);
    }

    // Calculate the loudness (volume) based on RMS
    float GetVolume()
    {
        float sum = 0;
        foreach (float sample in samples)
        {
            sum += sample * sample;
        }
        return Mathf.Sqrt(sum / sampleWindow);
    }

    // Get the dominant pitch (frequency) from the spectrum data
    float GetPitch()
    {
        int highestIndex = 0;
        float highestMagnitude = 0;

        for (int i = 0; i < sampleWindow; i++)
        {
            if (spectrum[i] > highestMagnitude)
            {
                highestMagnitude = spectrum[i];
                highestIndex = i;
            }
        }

        // Convert the index to frequency
        float frequency = highestIndex * AudioSettings.outputSampleRate / sampleWindow;
        return frequency;
    }

    // Handle character actions based on pitch and volume
    void HandleCharacterActions(float pitch, float volume)
    {
        if (pitch >= 200 && pitch < 400)
        {
            MoveCharacter(Vector3.right, volume); // Medium frequency -> Move right
        }
        else if (pitch >= 400 && pitch < 600)
        {
            JumpCharacter(volume); // Higher frequency -> Jump
        }
        else if (pitch >= 600)
        {
            AttackCharacter(volume); // Very high frequency -> Attack
        }
    }

    // Move the character in a direction based on volume
    void MoveCharacter(Vector3 direction, float volume)
    {
        // Adjust the speed based on the volume (louder = faster)
        float moveSpeed = this.moveSpeed * (volume > 0.1f ? 2f : 1f);
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    // Make the character jump based on volume (higher volume = higher jump)
    void JumpCharacter(float volume)
    {
        if (volume > 0.5f && Mathf.Abs(rb.velocity.y) < 0.01f) // Check if character is on the ground
        {
            rb.AddForce(Vector3.up * jumpForce * (volume > 1.0f ? 1.5f : 1f), ForceMode.Impulse);
        }
    }

    // Trigger an attack action (volume can affect attack power)
    void AttackCharacter(float volume)
    {
        // For demonstration, just print the attack power
        float attackPower = volume * 10f;
        Debug.Log("Attack Power: " + attackPower);
    }
}
