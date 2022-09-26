using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

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

    public void LaunchGame()
    {
        StartCoroutine(LaunchGameTransi());
    }

    IEnumerator LaunchGameTransi()
    {
        FadeOn?.Invoke();
        yield return new WaitForSeconds(TimeFadeOn);
        SceneManager.LoadScene(0);
    }
}
