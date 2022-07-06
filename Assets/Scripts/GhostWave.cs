using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostWave : MonoBehaviour
{
    private float startX = -0.575f;
    private float width = 1.15f;
    private float waveSpeed = 4.5f;

    void Start()
    {
        transform.localPosition = new Vector3(startX, transform.localPosition.y, transform.localPosition.z);
    }

    void Update()
    {
        float move = Time.deltaTime * waveSpeed;
        float newX = transform.localPosition.x + move;

        if (newX >= startX + width)
        {
            newX -= width;
        }

        transform.localPosition = new Vector3(newX, transform.localPosition.y, transform.localPosition.z);
    }
}
