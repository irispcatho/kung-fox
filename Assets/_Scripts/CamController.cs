using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private float lerp;

    private void Update()
    {
        Vector3 pos = transform.position;
        Vector3 targetPos = target.transform.position;
        transform.position = Vector3.Lerp(new Vector3(pos.x, pos.y, pos.z), new Vector3(targetPos.x, targetPos.y, pos.z), lerp);
    }
}