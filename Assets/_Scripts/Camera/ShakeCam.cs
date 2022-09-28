using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeCam : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float durationShaking = 0f;

    public static ShakeCam Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //StartShakingCam(0);
    }

    public void StartShakingCam(float _addTime) //Fonction a appeler pour lancer la coroutine
    {
        StartCoroutine(Shaking(_addTime));
    }

    private IEnumerator Shaking(float _timeAdded)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        var _addDuration = _timeAdded + durationShaking;

        while (elapsedTime < _addDuration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / _addDuration);
            transform.position = startPosition + Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.position = startPosition;
    }
}
