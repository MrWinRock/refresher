using UnityEngine;
using DG.Tweening;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip themeMusic;
    [SerializeField] private AudioClip freshTimeMusic;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField, Range(0, 1)] private float maxVolume = 0.5f;

    private AudioSource _audioSource;
    private Tween _fadeTween;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.volume = 0f;
    }

    private void OnEnable()
    {
        FreshTimeManager.FreshTimeStarted += OnFreshTimeStarted;
        FreshTimeManager.FreshTimeEnded += OnFreshTimeEnded;
    }

    private void OnDisable()
    {
        FreshTimeManager.FreshTimeStarted -= OnFreshTimeStarted;
        FreshTimeManager.FreshTimeEnded -= OnFreshTimeEnded;
    }

    private void Start()
    {
        PlayMusic(themeMusic);
    }

    private void OnFreshTimeStarted()
    {
        PlayMusic(freshTimeMusic);
    }

    private void OnFreshTimeEnded()
    {
        PlayMusic(themeMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (_audioSource.clip == clip && _audioSource.isPlaying) return;

        _fadeTween?.Kill();

        if (_audioSource.isPlaying)
        {
            _fadeTween = _audioSource.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                _audioSource.clip = clip;
                _audioSource.Play();
                _fadeTween = _audioSource.DOFade(maxVolume, fadeDuration);
            });
        }
        else
        {
            _audioSource.clip = clip;
            _audioSource.Play();
            _fadeTween = _audioSource.DOFade(maxVolume, fadeDuration);
        }
    }
}
