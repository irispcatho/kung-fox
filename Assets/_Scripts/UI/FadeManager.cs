using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class FadeManager : MonoBehaviour
{
    [SerializeField] private GameObject _fade = null;
    [SerializeField] private GameObject _tempFadeSystem = null;
    [SerializeField] private GameObject[] _tpPoints;
    private float _timeFadeOn = 0f;
    private float _timeFadeOff = 0f;

    void Start()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1))
        {
            _timeFadeOn = MainMenuManager.Instance.TimeFadeOn;
            _timeFadeOff = MainMenuManager.Instance.TimeFadeOff;
            MainMenuManager.Instance.FadeOn += FadeOn;
            MainMenuManager.Instance.FadeOff += FadeOff;
        }
        else
        {
            _timeFadeOn = MainGameManager.Instance.TimeFadeOn;
            _timeFadeOff = MainGameManager.Instance.TimeFadeOff;
            MainGameManager.Instance.FadeOn += FadeOn;
            MainGameManager.Instance.FadeOff += FadeOff;
        }

        _tempFadeSystem.transform.DOScale(Vector2.one, 0);
    }

    private void FadeOn()
    {
        _fade.transform.DOMoveY(_tpPoints[0].transform.position.y, _timeFadeOn);//.SetEase(Ease.Linear);
    }

    private void FadeOff()
    {
        _fade.transform.DOMoveY(_tpPoints[1].transform.position.y, _timeFadeOff).SetEase(Ease.Linear).OnComplete(ResetFade);
    }

    private void ResetFade()
    {
        _fade.transform.DOMoveY(_tpPoints[2].transform.position.y, 0);
    }

    private void OnDisable()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1))
        {
            MainMenuManager.Instance.FadeOn -= FadeOn;
            MainMenuManager.Instance.FadeOff -= FadeOff;
        }
        else
        {
            MainGameManager.Instance.FadeOn -= FadeOn;
            MainGameManager.Instance.FadeOff -= FadeOff;
        }
    }
}
