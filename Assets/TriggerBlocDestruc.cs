using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBlocDestruc : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(DestroyObject());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<DestructibleBloc>())
        {
            //StartDeathBloc?.Invoke();
            ShakeCam.Instance.StartShakingCam(-.3f);
            collision.gameObject.GetComponent<DestructibleBloc>().StartAnimDeathBloc();
            AudioManager.Instance.PlaySound("BreakBloc");
            Destroy(gameObject);
        }
    }

    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(.1f);
        Destroy(gameObject);
    }
}
