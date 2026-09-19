using UnityEngine;

[DisallowMultipleComponent]
public sealed class AudioDirector : MonoBehaviour
{
    private const int SampleRate = 44100;

    private static AudioDirector instance;

    private AudioSource audioSource;
    private AudioClip attackClip;
    private AudioClip dodgeClip;
    private AudioClip skillClip;
    private AudioClip ultimateClip;
    private AudioClip hitClip;
    private AudioClip clearClip;

    public static AudioDirector Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        attackClip = CreateTone("Attack", 420f, 0.08f, 0.18f, 4f);
        dodgeClip = CreateTone("Dodge", 760f, 0.07f, 0.14f, 6f);
        skillClip = CreateTone("Skill", 260f, 0.14f, 0.2f, 4f);
        ultimateClip = CreateTone("Ultimate", 110f, 0.4f, 0.24f, 2f);
        hitClip = CreateTone("Hit", 95f, 0.05f, 0.12f, 10f);
        clearClip = CreateTone("Clear", 620f, 0.5f, 0.2f, 2f);
    }

    public void PlayAttack()
    {
        Play(attackClip);
    }

    public void PlayDodge()
    {
        Play(dodgeClip);
    }

    public void PlaySkill()
    {
        Play(skillClip);
    }

    public void PlayUltimate()
    {
        Play(ultimateClip);
    }

    public void PlayHit()
    {
        Play(hitClip);
    }

    public void PlayClear()
    {
        Play(clearClip);
    }

    private void Play(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip);
    }

    private static AudioClip CreateTone(
        string clipName,
        float frequency,
        float duration,
        float volume,
        float decay)
    {
        int sampleCount = Mathf.Max(
            1,
            Mathf.RoundToInt(SampleRate * duration));

        AudioClip clip = AudioClip.Create(
            clipName,
            sampleCount,
            1,
            SampleRate,
            false);

        float[] samples = new float[sampleCount];

        for (int i = 0; i < samples.Length; i++)
        {
            float time = i / (float)SampleRate;
            float envelope = Mathf.Exp(-decay * time);
            samples[i] =
                Mathf.Sin(2f * Mathf.PI * frequency * time)
                * envelope
                * volume;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
