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
    private float _nextReset;
    private float _resetRate;
    private bool _canReset = false;


    private void Start()
    {
        IsCharged = true;
        _resetRate = PlayerController.Instance.DashTimer;
    }

    public void LaunchTimer()
    {
        _canReset = true;
    }

    private void Update()
    {
        if (!_isCooldown && _canReset)
        {
            _isCooldown = true;
            _nextReset = _resetRate;
        }

        if (!_isCooldown || !PlayerController.Instance._canResetDash) return;
        _nextReset -= Time.deltaTime;

        if (!(_nextReset <= 0)) return;
        _isCooldown = false;
        PlayerController.Instance._remainingDashes++;
        IsCharged = true;
        BallSpriteRenderer.enabled = true;
        AudioManager.Instance.PlaySound("ResetDash");
        _canReset = false;
    }
}
