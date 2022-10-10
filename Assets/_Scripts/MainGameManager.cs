using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;


public class MainGameManager : MonoBehaviour
{
    public event Action FadeOn;
    public event Action FadeOff;
    public event Action ResetPosPlayer;
    public event Action ResetScalePlayer;

    [SerializeField] private GameObject win_Text = null;

    [Header("FadeSystem")]
    public float TimeFadeOn = 0f;
    public float TimeFadeOff = 0f;

    private float chrono = 0;
    [SerializeField] private TextMeshProUGUI chronoText = null;
    [SerializeField] private GameObject chronoParent = null;
    [SerializeField] private Transform tpEndChronoText = null;
    private bool isGameEnded = false;
    private bool hasMoveChrono = false;


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

    private void Update()
    {
        if (!isGameEnded)
        {
            chrono += Time.deltaTime;
            TimeSpan time = TimeSpan.FromSeconds(chrono);
            chronoText.text = time.ToString(@"mm\:ss");
        }
        else
        {
            if (!hasMoveChrono)
                hasMoveChrono = true;
        }
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

    private IEnumerator PlayerDeathTransi() //Timing a revoir
    {
        yield return new WaitForSeconds(TimeFadeOn);
        FadeOn?.Invoke();
        yield return new WaitForSeconds(TimeFadeOn * .5f);
        ResetPosPlayer?.Invoke();
        yield return new WaitForSeconds(TimeFadeOn * .5f);
        FadeOff?.Invoke();
        yield return new WaitForSeconds(TimeFadeOn * .5f);
        ResetScalePlayer?.Invoke();
    }

    private void PlayerWin()
    {
        StartCoroutine(PlayerWinTransi());
    }

    private IEnumerator PlayerWinTransi()
    {
        isGameEnded = true;
        chronoParent.gameObject.transform.DOScale(Vector2.zero, .5f);
        yield return new WaitForSeconds(TimeFadeOn * 3);
        FadeOn?.Invoke();
        yield return new WaitForSeconds(TimeFadeOn);
        win_Text.SetActive(true);
        win_Text.transform.DOScale(Vector3.one, .5f);
        ChangeChronoTextEndGame();
        chronoParent.gameObject.transform.DOScale(Vector2.one, .5f);
        yield return new WaitForSeconds(10f);
        SceneManager.LoadScene(0);
    }

    void ChangeChronoTextEndGame()
    {
        chronoParent.gameObject.transform.position = tpEndChronoText.position;
        var getTime = chronoText.text;
        //chronoText.text = $"Time : {getTime}";
        //chronoText.alignment = TextAlignmentOptions.Center;
    }





    private void OnDisable()
    {
        PlayerManager.Instance.PlayerDeath -= PlayerDeath;
        PlayerManager.Instance.PlayerWin -= PlayerWin;
    }
}
