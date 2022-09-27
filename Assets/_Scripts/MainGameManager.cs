using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class MainGameManager : MonoBehaviour
{
    public event Action FadeOn;
    public event Action FadeOff;
    public event Action ResetPlayer;

    [SerializeField] GameObject win_Text = null;

    [Header("FadeSystem")]
    public float TimeFadeOn = 0f;
    public float TimeFadeOff = 0f;


    public static MainGameManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PlayerManager.Instance.PlayerDeath += PlayerDeath;
        PlayerManager.Instance.PlayerWin += PlayerWin;
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(.01f);
        FadeOff?.Invoke();
    }

    private void PlayerDeath()
    {
        StartCoroutine(PlayerDeathTransi());
    }

    private IEnumerator PlayerDeathTransi()
    {
        yield return new WaitForSeconds(TimeFadeOn);
        FadeOn?.Invoke();
        yield return new WaitForSeconds(TimeFadeOn);
        FadeOff?.Invoke();
        yield return new WaitForSeconds(TimeFadeOn);
        ResetPlayer?.Invoke();
    }

    private void PlayerWin()
    {
        StartCoroutine(PlayerWinTransi());
    }

    private IEnumerator PlayerWinTransi()
    {
        yield return new WaitForSeconds(TimeFadeOn*3);
        FadeOn?.Invoke();
        yield return new WaitForSeconds(TimeFadeOn);
        win_Text.SetActive(true);
        win_Text.transform.DOScale(Vector3.one, .5f);
    }







    private void OnDisable()
    {
        PlayerManager.Instance.PlayerDeath -= PlayerDeath;
        PlayerManager.Instance.PlayerWin -= PlayerWin;
    }
}
