using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestObject : MonoBehaviour
{
    public bool rotate = false;
    public float rotateSpeed = 100f;

    private void FixedUpdate()
    {
        if (rotate)
        {
            transform.Rotate(0, 0, -rotateSpeed * Time.deltaTime);
        }
    }

    public void ReceiveHit()
    {
        GetComponent<SpriteRenderer>().color = Color.red;

        StopAllCoroutines();
        StartCoroutine(Reset());
    }

    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(0.15f);

        GetComponent<SpriteRenderer>().color = Color.white;
    }
}
