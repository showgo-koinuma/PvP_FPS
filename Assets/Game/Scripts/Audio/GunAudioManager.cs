using UnityEngine;

public class GunAudioManager : MonoBehaviour
{
    [SerializeField] AudioSource _worldAudioSource;
    [SerializeField] AudioSource _localAudioSource;

    [Header("Share")]
    [SerializeField] AudioClip _shot;
    [SerializeField] AudioClip _reload;
    [SerializeField] AudioClip _ads;
    [SerializeField] AudioClip _switch;
    [Space(10)]
    [SerializeField] AudioClip _hit;
    [SerializeField] AudioClip _head;

    [Header("ShotGun")]
    [SerializeField] AudioClip _cocking;
    [SerializeField] AudioClip _insertShell;

    public void PlayShotSound()
    {
        _worldAudioSource.PlayOneShot(_shot);
    }
    public void PlayReloadSound()
    {
        if (_reload) _worldAudioSource.PlayOneShot(_reload);
    }
    public void PlaySwitchSound()
    {
        if (_switch) _worldAudioSource.PlayOneShot(_switch);
    }
    public void PlayHitSound()
    {
        _localAudioSource.PlayOneShot(_hit);
    }
    public void PlayHeadSound()
    {
        _localAudioSource.PlayOneShot(_head);
    }
    public void PlayKillSound()
    {
        _worldAudioSource.PlayOneShot(_hit);
    }

    // shot gun
    public void PlayCocking()
    {
        _worldAudioSource.PlayOneShot(_cocking);
    }

    public void PlayInsertShell()
    {
        _worldAudioSource.PlayOneShot(_insertShell);
    }
}
