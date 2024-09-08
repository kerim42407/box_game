using UnityEngine;
using System.Collections;

public class FactoryModelController : MonoBehaviour
{
    public Transform[] childObjects;
    public Vector3 randomOffset;
    public float moveDuration;

    private Vector3[] initialPositions;

    void Start()
    {
        initialPositions = new Vector3[childObjects.Length];
        for (int i = 0; i < childObjects.Length; i++)
        {
            initialPositions[i] = childObjects[i].localPosition;
            childObjects[i].localPosition += new Vector3(
                Random.Range(-randomOffset.x, randomOffset.x),
                Random.Range(-randomOffset.y, randomOffset.y),
                Random.Range(-randomOffset.z, randomOffset.z)
            );
        }

        StartCoroutine(MoveToInitialPositions());
    }

    IEnumerator MoveToInitialPositions()
    {
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            for (int i = 0; i < childObjects.Length; i++)
            {
                childObjects[i].localPosition = Vector3.Lerp(childObjects[i].localPosition, initialPositions[i], elapsedTime / moveDuration);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < childObjects.Length; i++)
        {
            childObjects[i].localPosition = initialPositions[i];
        }
    }
}
