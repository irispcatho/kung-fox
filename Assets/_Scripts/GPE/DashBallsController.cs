using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DashBallsController : MonoBehaviour
{
    public SpriteRenderer BallSpriteRenderer;
    public bool IsCharged;
    private bool _isCooldown = false;
    private float nextReset;
    [SerializeField] private float resetRate;
    private bool canReset = false;


    private void Start()
    {
        IsCharged = true;
    }

    //public void InitiateTimer(float timer)
    //{
    //    StartCoroutine(WaitForDash(timer));
    //}

    public void LaunchTimer()
    {
        canReset = true;
    }

    //private IEnumerator WaitForDash(float timer)
    //{
    //    yield return new WaitForSeconds(timer);
    //    NewPlayerController.Instance._remainingDashes++;
    //    IsCharged = true;
    //    BallSpriteRenderer.enabled = true;
    //    StopCoroutine(WaitForDash(timer));
    //}

    private void Update()
    {
        if (!_isCooldown && canReset)
        {
            _isCooldown = true;
            nextReset = resetRate;
        }

        if (_isCooldown && NewPlayerController.Instance.canResetDash)
        {
            nextReset -= Time.deltaTime;

            if (nextReset <= 0)
            {
                _isCooldown = false;
                NewPlayerController.Instance._remainingDashes++;
                IsCharged = true;
                BallSpriteRenderer.enabled = true;
                AudioManager.Instance.PlaySound("ResetDash");
                canReset = false;
            }
        }
    }
}
