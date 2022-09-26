using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] GameObject[] _menus;

    public event Action FadeOn;
    public event Action FadeOff;

    public float TimeFadeOn = 0f;
    public float TimeFadeOff = 0f;

    public static MainMenuManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(test());
    }

    IEnumerator test()
    {
        yield return new WaitForSeconds(.01f);
        FadeOn?.Invoke();
    }

    public void LaunchCredits()
    {
        StartCoroutine(TransiToCredit());
    }

    IEnumerator TransiToCredit()
    {
        FadeOff?.Invoke();
        yield return new WaitForSeconds(TimeFadeOn);
        _menus[0].SetActive(false);
        _menus[1].SetActive(true);
        FadeOn?.Invoke();
    }
}
