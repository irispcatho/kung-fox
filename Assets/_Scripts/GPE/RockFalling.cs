using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockFalling : MonoBehaviour
{
    [SerializeField] private GameObject _fx_ExplodeRock;

    const float _yOffset = .2f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<KillRocks>())
        {
            var _transferPos = new Vector2(transform.position.x, transform.position.y + _yOffset);
            Instantiate(_fx_ExplodeRock, _transferPos, transform.rotation);
            Destroy(gameObject);
        }
    }
}
