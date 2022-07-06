using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperPacGomme : MonoBehaviour
{
    private float pulseTime;
    private float scaleMultiply;
    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
        scaleMultiply = 2f;
        pulseTime = 1f;

        StartCoroutine(DoPulse());
    }

    private void Update()
    {
        if (!GameManager.INSTANCE.GameRunning)
            return;

        Color newColor = new Color(Mathf.Lerp(0.5f, 1,
            Mathf.PingPong(GameManager.INSTANCE.GameTimer, 1)),
            Mathf.PingPong(GameManager.INSTANCE.GameTimer * 3, 1),
            Mathf.PingPong(GameManager.INSTANCE.GameTimer * 5, 1));
        GetComponent<SpriteRenderer>().color = newColor;
    }

    private IEnumerator DoPulse()
    {
        float counter = 0f;

        while (true)
        {
            counter += GameManager.INSTANCE.DeltaTime;

            float calculatedScale = Mathf.Cos((counter / pulseTime) * (Mathf.PI * 2) + Mathf.PI) * (scaleMultiply - 1);
            calculatedScale = (calculatedScale + (scaleMultiply - 1)) / 2;
            Vector3 newScale = initialScale + initialScale * calculatedScale;
            transform.localScale = newScale;

            yield return null;
        }
    }
}
