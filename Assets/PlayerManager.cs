using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Unity.VisualScripting;

public class PlayerManager : MonoBehaviour
{
    public event Action PlayerDeath;
    public event Action PlayerWin;

    [SerializeField] private GameObject fx_Death;
    [SerializeField] private GameObject fx_Win;
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
        if (collision.gameObject.GetComponent<PlayerKilled>())
        {
            PlayerDeath?.Invoke();
            PlayerDeathAnim();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerWin>())
        {
            PlayerWin?.Invoke();
            PlayerWinAnim();
        }
    }

    private void PlayerDeathAnim()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        gameObject.transform.DOScale(Vector2.zero, .2f).SetEase(Ease.InBounce);
        var trasnferPos = gameObject.transform;
        Instantiate(fx_Death, trasnferPos);
    }

    private void PlayerWinAnim()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        Instantiate(fx_Win, gameObject.transform);

    }

    private void ResetPlayer()
    {
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        gameObject.transform.DOMove(_spawnPoint.position, 0);
        gameObject.transform.DOScale(Vector2.one, .2f).SetEase(Ease.InBounce);

    }
}
