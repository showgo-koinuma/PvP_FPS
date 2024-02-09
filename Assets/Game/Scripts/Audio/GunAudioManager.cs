using UnityEngine;
using Photon.Pun;

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

    PhotonView _photonView;

    public void PlayShotSound()
    {
        if (_photonView.IsMine)
        {
            _localAudioSource.PlayOneShot(_shot);
        }
        else
        {
            _worldAudioSource.PlayOneShot(_shot);
        }
    }
    public void PlayReloadSound()
    {
        if (_photonView.IsMine)
        {
            _localAudioSource.PlayOneShot(_reload);
        }
        else
        {
            _worldAudioSource.PlayOneShot(_reload);
        }
    }
    public void PlaySwitchSound()
    {
        if (!_switch) return;

        if (_photonView.IsMine)
        {
            _localAudioSource.PlayOneShot(_switch);
        }
        else
        {
            _worldAudioSource.PlayOneShot(_switch);
        }
    }
    public void PlayHitSound()
    {
        _localAudioSource.PlayOneShot(_hit);
    }
    public void PlayHeadSound()
    {
        _localAudioSource.PlayOneShot(_head);
    }

    // shot gun
    public void PlayCocking()
    {
        if (_photonView.IsMine)
        {
            _localAudioSource.PlayOneShot(_cocking);
        }
        else
        {
            _worldAudioSource.PlayOneShot(_cocking);
        }
    }
    public void PlayInsertShell()
    {
        if (_photonView.IsMine)
        {
            _localAudioSource.PlayOneShot(_insertShell);
        }
        else
        {
            _worldAudioSource.PlayOneShot(_insertShell);
        }
    }

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }
}
