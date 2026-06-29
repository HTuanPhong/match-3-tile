using Cysharp.Threading.Tasks;
using UnityEngine;

public class AudioService
{
  private readonly AudioSource _musicSource;
  private readonly AudioSource _effectSource;

  public AudioService(AudioSource music, AudioSource effect)
  {
    _musicSource = music;
    _effectSource = effect;
  }

  /// <summary>
  /// Loads a background track via Addressables, then transitions into it smoothly using a volume fade.
  /// </summary>
  public async UniTask PlayMusic(AudioClip newClip, bool fade = true, float fadeDuration = 0.4f)
  {
    if (newClip == null) return;
    // If music is already playing, fade it out first
    if (_musicSource.isPlaying && fade && _musicSource.clip != newClip)
    {
      float startVolume = _musicSource.volume;
      for (float t = 0; t < fadeDuration; t += Time.deltaTime)
      {
        _musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
        await UniTask.Yield();
      }
    }

    _musicSource.clip = newClip;
    _musicSource.loop = true;
    _musicSource.Play();

    // Fade the new track up to max volume
    if (fade)
    {
      for (float t = 0; t < fadeDuration; t += Time.deltaTime)
      {
        _musicSource.volume = Mathf.Lerp(0f, 1f, t / fadeDuration);
        await UniTask.Yield();
      }
      _musicSource.volume = 1f;
    }
  }

  /// <summary>
  /// Plays a sound effect
  /// </summary>
  public void PlayEffect(AudioClip clip)
  {
    if (clip == null) return;
    _effectSource.PlayOneShot(clip);
  }
}