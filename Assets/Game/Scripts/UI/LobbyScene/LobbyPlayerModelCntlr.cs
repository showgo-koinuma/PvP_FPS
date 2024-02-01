using UnityEngine;

public class LobbyPlayerModelCntlr : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject _playerUI;
    [SerializeField] Transform _uiPosition;
    [Header("Animation")]
    [SerializeField] Animator _animator;
    [SerializeField] float _animationRate;
    [SerializeField] int _animationCount;

    float _timer = 0;

    private void FixedUpdate()
    {
        _playerUI.transform.position = Camera.main.WorldToScreenPoint(_uiPosition.position);

        if (_timer > _animationRate)
        {
            _timer = 0;
            int n = Random.Range(1, _animationCount + 1);
            Debug.Log(n);
            _animator.SetInteger("RandomAnimation", n);
        }
        else
        {
            _animator.SetInteger("RandomAnimation", 0);
            _timer += Time.fixedDeltaTime;
        }
    }
}
