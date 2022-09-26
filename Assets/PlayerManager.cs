using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class PlayerManager : MonoBehaviour
{
    public event Action PlayerDeath;

    [SerializeField] private GameObject fx_Death;
    [SerializeField] private Transform _spawnPoint;

    private Rigidbody2D rb;
    
    public static PlayerManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        MainGameManager.Instance.ResetPlayer += ResetPlayer;

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<KillPlayer>())
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            PlayerDeath?.Invoke();
            PlayerAnimParticules();
        }
    }

    private void PlayerAnimParticules()
    {
        gameObject.transform.DOScale(Vector2.zero, .2f).SetEase(Ease.InBounce);
        var trasnferPos = gameObject.transform;
        Instantiate(fx_Death, trasnferPos);
    }

    private void ResetPlayer()
    {
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        gameObject.transform.DOMove(_spawnPoint.position, 0);
        gameObject.transform.DOScale(Vector2.one, .2f).SetEase(Ease.InBounce);

    }
}
