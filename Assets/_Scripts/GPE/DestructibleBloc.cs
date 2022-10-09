using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleBloc : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Collider2D[] _allColliders;

    private void Start()
    {
        //PlayerManager.Instance.StartDeathBloc += StartAnimDeathBloc;
    }

    private void OnDisable()
    {
        //PlayerManager.Instance.StartDeathBloc -= StartAnimDeathBloc;
    }

    public void StartAnimDeathBloc()
    {
        _animator.SetTrigger("GoDie");
        for (int i = 0; i < _allColliders.Length; i++)
        {
            _allColliders[i].enabled = false;
        }
        StartCoroutine(DestroyBloc());
    }

    IEnumerator DestroyBloc()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
