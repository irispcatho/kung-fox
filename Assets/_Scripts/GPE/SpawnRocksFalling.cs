using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRocksFalling : MonoBehaviour
{
    [SerializeField] private GameObject _rockFalling;
    [SerializeField] private Sprite[]  _whichSprtiteRock;
    [Range(1,3)] [SerializeField] private int _whichRock;

    private const float _timeSpawningRockFalling = 2f;
    private void Start()
    {
        StartCoroutine(SpawnRockFalling());
    }

    IEnumerator SpawnRockFalling()
    {
        yield return new WaitForSeconds(_timeSpawningRockFalling);
        var _tranferPos = gameObject.transform.position;
        GameObject go = Instantiate(_rockFalling, _tranferPos, gameObject.transform.rotation);
        go.GetComponent<SpriteRenderer>().sprite = _whichSprtiteRock[_whichRock-1];
        StartCoroutine(SpawnRockFalling());
    }
}
