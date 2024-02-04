using UnityEngine;

public class GunAudioManager : MonoBehaviour
{
    [SerializeField] AudioSource _audioSource;

    [Header("Share")]
    [SerializeField] AudioClip _shot;
    [SerializeField] AudioClip _reload;
    [SerializeField] AudioClip _ads;
    [SerializeField] AudioClip _switch;

    [Header("ShotGun")]
    [SerializeField] AudioClip _cocking;
    [SerializeField] AudioClip _insertShell;

    public void PlayShotSound()
    {
        _audioSource.PlayOneShot(_shot);
    }

    public void PlayReloadSound()
    {
        if (_reload) _audioSource.PlayOneShot(_reload);
    }

    public void PlaySwitchSound()
    {
        if (_switch) _audioSource.PlayOneShot(_switch);
    }

    // shot gun
    public void PlayCocking()
    {
        _audioSource.PlayOneShot(_cocking);
    }

    public void PlayInsertShell()
    {
        _audioSource.PlayOneShot(_insertShell);
    }
}
