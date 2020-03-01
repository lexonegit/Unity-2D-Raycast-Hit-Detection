using System.Collections;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(AnimLoop());
    }

    public float startDelay = 0f;

    private IEnumerator AnimLoop()
    {
        yield return new WaitForSeconds(startDelay);

        Sensors sensors = GetComponent<Sensors>();
        Animator anim = GetComponent<Animator>();

        while (true)
        {
            anim.SetTrigger("do"); //Start animation

            sensors.Play();

            yield return new WaitForSeconds(0.6f);

            //Stop raycasting
            sensors.Stop();

            yield return new WaitForSeconds(0.75f);

            //Start again (Loop)
        }
    }
}
