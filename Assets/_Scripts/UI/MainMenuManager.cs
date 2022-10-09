using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] GameObject[] _menus;

    public event Action FadeOn;
    public event Action FadeOff;

    public float TimeFadeOn = 0f;
    public float TimeFadeOff = 0f;

    private bool _canLaunchTransi = true;

    public static MainMenuManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(.01f);
        FadeOff?.Invoke();
    }
    
    public void LaunchCredits()
    {
        StartCoroutine(TransiToCredit(true));
    }
    public void LeaveCredits()
    {
        StartCoroutine(TransiToCredit(false));
    }

    IEnumerator TransiToCredit(bool which)
    {
        if (_canLaunchTransi)
        {
            _canLaunchTransi = false;
            FadeOn?.Invoke();
            yield return new WaitForSeconds(TimeFadeOn);
            _menus[0].SetActive(!which);
            _menus[1].SetActive(which);
            _menus[2].SetActive(false);
            FadeOff?.Invoke();
            yield return new WaitForSeconds(TimeFadeOff);
            _canLaunchTransi = true;
        }
    }

    public void LeaveGame()
    {
        StartCoroutine(LeaveGameTransi());
    }

    IEnumerator LeaveGameTransi()
    {
        FadeOn?.Invoke();
        yield return new WaitForSeconds(TimeFadeOn);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }

    public void LaunchMapping()
    {
        StartCoroutine(LaunchMappingTransi());
    }

    IEnumerator LaunchMappingTransi()
    {
        FadeOn?.Invoke();
        yield return new WaitForSeconds(TimeFadeOn);
        _menus[3].transform.DOScale(Vector3.one, .5f).SetEase(Ease.InCirc);
    }

    public void LaunchGame()
    {
        StartCoroutine(LaunchGameTransi());
    }

    IEnumerator LaunchGameTransi()
    {
        _menus[3].transform.DOScale(Vector3.zero, .5f).SetEase(Ease.InCirc);
        yield return new WaitForSeconds(TimeFadeOn);
        SceneManager.LoadScene(1);
    }

    public void LaunchSelectSoundButton()
    {
        AudioManager.Instance.PlaySound("MenuSelect");
    }
}
