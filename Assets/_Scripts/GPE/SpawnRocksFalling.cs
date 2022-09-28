using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRocksFalling : MonoBehaviour
{
    [SerializeField] private GameObject _rockFalling;

    private const float _timeSpawningRockFalling = 2f;
    private void Start()
    {
        StartCoroutine(SpawnRockFalling());
    }

    IEnumerator SpawnRockFalling()
    {
        yield return new WaitForSeconds(_timeSpawningRockFalling);
        var _tranferPos = gameObject.transform.position;
        Instantiate(_rockFalling, _tranferPos, gameObject.transform.rotation);
        StartCoroutine(SpawnRockFalling());
    }
}
