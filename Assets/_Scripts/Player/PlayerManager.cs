using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Unity.VisualScripting;
using Unity.Collections.LowLevel.Unsafe;

public class PlayerManager : MonoBehaviour
{
    public event Action PlayerDeath;
    public event Action PlayerWin;

    [SerializeField] private GameObject fx_Death;
    [SerializeField] private GameObject fx_Win;
    [SerializeField] private Transform _spawnPoint;

    private Rigidbody2D rb;

    private const float _timeDeathScale = .2f;

    public static PlayerManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        MainGameManager.Instance.ResetPosPlayer += ResetPosPlayer;
        MainGameManager.Instance.ResetScalePlayer += ResetScalePlayer;

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerKilled>() || collision.gameObject.GetComponent<RockFalling>())
        {
            PlayerDeath?.Invoke();
            PlayerDeathAnim();
            ShakeCam.Instance.StartShakingCam(0.5f);
        
            if (collision.gameObject.GetComponent<RockFalling>())
                Destroy(collision.gameObject);
        }


        if (collision.gameObject.GetComponent<DestructibleBloc>())
        {
            ShakeMap.Instance.StartShakingCam(-.3f);
            Destroy(collision.gameObject);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerWin>())
        {
            PlayerWin?.Invoke();
            PlayerWinAnim();
        }
        if (collision.gameObject.GetComponent<DarkZone>())
        {
            print("salut c'est la darkzone");
        }
    }

    private void PlayerDeathAnim()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        gameObject.transform.DOScale(Vector2.zero, _timeDeathScale).SetEase(Ease.InBounce);
        var trasnferPos = gameObject.transform;
        Instantiate(fx_Death, trasnferPos);
    }

    private void PlayerWinAnim()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        Instantiate(fx_Win, gameObject.transform);
    }

    private void ResetPosPlayer()
    {
        gameObject.transform.DOMove(_spawnPoint.position, 0);
    }

    private void ResetScalePlayer()
    {
        gameObject.transform.DOScale(Vector2.one, _timeDeathScale).SetEase(Ease.InBounce).OnComplete(ResetRBPlayer);
        RipplePostProcessor.Instance.RippleEffect(gameObject.transform.position);
    }

    private void ResetRBPlayer()
    {
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }


    private void OnDisable()
    {
        MainGameManager.Instance.ResetPosPlayer -= ResetPosPlayer;
        MainGameManager.Instance.ResetScalePlayer -= ResetScalePlayer;
    }
}
