using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FadeManager : MonoBehaviour
{
    [SerializeField] private GameObject _fade = null;
    [SerializeField] private GameObject[] _tpPoints;
    private float _timeFadeOn = 0f;
    private float _timeFadeOff = 0f;

    void Start()
    {
        _timeFadeOn = MainMenuManager.Instance.TimeFadeOn;
        _timeFadeOff = MainMenuManager.Instance.TimeFadeOff;

        MainMenuManager.Instance.FadeOn += FadeOn;
        MainMenuManager.Instance.FadeOff += FadeOff;
    }

    private void FadeOn()
    {
        _fade.transform.DOMoveY(_tpPoints[1].transform.position.y, _timeFadeOn);//.SetEase(Ease.Linear);
        print("fadeon");
    }

    private void FadeOff()
    {
        _fade.transform.DOMoveY(_tpPoints[2].transform.position.y, _timeFadeOff).OnComplete(ResetFade).SetEase(Ease.Linear);
    }

    private void ResetFade()
    {
        _fade.transform.DOMoveY(_tpPoints[0].transform.position.y, 0);
    }

    private void OnDisable()
    {
        MainMenuManager.Instance.FadeOn -= FadeOn;
        MainMenuManager.Instance.FadeOff -= FadeOff;
    }
}
