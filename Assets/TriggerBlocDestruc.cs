using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBlocDestruc : MonoBehaviour
{
    public event Action StartDeathBloc;

    public static TriggerBlocDestruc Instance;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        StartCoroutine(DestroyObject());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<DestructibleBloc>())
        {
            StartDeathBloc?.Invoke();
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
