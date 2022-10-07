using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DashBallsController : MonoBehaviour
{
    public SpriteRenderer BallSpriteRenderer;
    public bool IsCharged;

    private void Start()
    {
        IsCharged = true;
        BallSpriteRenderer.color = Color.blue;
    }

    public void InitiateTimer(float timer)
    {
        StartCoroutine(WaitForDash(timer));
    }

    private IEnumerator WaitForDash(float timer)
    {
        yield return new WaitForSeconds(timer);
        NewPlayerController.Instance._remainingDashes++;
        IsCharged = true;
        BallSpriteRenderer.color = Color.blue;
        StopCoroutine(WaitForDash(timer));
    }
}
