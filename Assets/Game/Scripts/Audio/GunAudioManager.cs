using UnityEngine;

public class GunAudioManager : MonoBehaviour
{
    [SerializeField] AudioClip _shot;
    [SerializeField] AudioClip _reload;
    [SerializeField] AudioClip _ads;
    [SerializeField] AudioClip _switch;

    AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponentInParent<AudioSource>();
    }

    public void PlayShotSound()
    {
        _audioSource.PlayOneShot(_shot);
    }

    public void PlayReloadSound()
    {
        //_audioSource.PlayOneShot(_reload);
    }

    public void PlaySwitchSound()
    {
        //_audioSource.PlayOneShot(_switch);
    }
}
