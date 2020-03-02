using System.Collections.Generic;
using UnityEngine;

// Hit handling on line 178
// Implement your own hit handling logic there (interfaces, etc)

public class Sensors : MonoBehaviour
{
    [Header("Options")]
    [Header("Experiment with these settings to find out what works the best")]

    [Tooltip("Raycast every N FixedUpdate iteration frame (1 = default) (Higher = more performant but raycasting is less frequent and thus less accurate) Increasing this can cause slightly inconsistent results but it shouldn't matter too much")]
    [Range(1, 8)]
    public int raycastRate = 1;

    [Tooltip("How many sensor points should there be along the start and end point (Higher = less performant but more accurate)")]
    public int sensorCount = 3;
    public bool playOnStart = true;

    [Space(8f)]

    [Header("Raycast methods")]
    public bool horizontalRaycasts = true;
    public bool verticalRaycasts = true;
    public bool intersectionRaycastsTop = true;
    public bool intersectionRaycastsBottom = true;

    [Header("Hit handling options")]
    [Tooltip("Saves resources by stopping raycasting instantly after the first hit")]
    public bool stopAfterFirstHit = false;

    [Header("References")]
    public Transform startPoint;
    public Transform endPoint;
    public Transform target;

    [Header("Debug")]
    [Tooltip("Display raycasts in editor (runtime)")]
    public bool showDebugRays = true;

    private float debugRayLifetime = 0.4f;
    private Vector2[] sensors;
    private Vector2[] lastPositions;

    [HideInInspector] public bool playing = false;

    private void Awake()
    {
        InitializeSensors();

        if (playOnStart)
            Play();
    }

    /// <summary>
    /// Resets hit position and starts raycasting
    /// </summary>
    public void Play()
    {
        //Reset lastPositions
        for (int i = 0; i < lastPositions.Length; ++i)
            lastPositions[i] = GetTransformPoint(sensors[i]);

        //Reset hitObjects
        hitObjects.Clear();

        playing = true;
    }

    /// <summary>
    /// Stops the raycasting
    /// </summary>
    public void Stop()
    {
        playing = false;
    }

    public int frames = 0;
    private void FixedUpdate()
    {
        if (!playing)
            return;

        frames++;
        if (frames % raycastRate != 0)
            return;

        frames = 0;

        RaycastProcedure();
    }

    /// <summary>
    /// Initializes sensor positions
    /// </summary>
    public void InitializeSensors()
    {
        sensors = new Vector2[sensorCount];
        lastPositions = new Vector2[sensorCount];

        float d = 1f / (sensorCount - 1);
        float lerpValue = 0f;

        for (int i = 0; i < sensorCount; ++i)
        {
            sensors[i] = Vector2.Lerp(startPoint.localPosition, endPoint.localPosition, lerpValue); //Set sensors between startPoint and endPoint evenly

            lerpValue += d;
        }
    }

    /// <summary>
    /// Returns position relative to target transform 
    /// </summary>
    private Vector2 GetTransformPoint(Vector2 v)
    {
        return target.TransformPoint(v);
    }

    private void RaycastProcedure()
    {
        //Raycast order (assuming all are enabled)

        //1. Horizontal raycasts
        //2. Intersection top raycasts
        //3. Intersection bottom raycasts
        //4. Vertical raycasts

        for (int i = 0; i < sensors.Length; ++i)
        {
            Vector2 currentPosition = GetTransformPoint(sensors[i]);

            //Horizontal
            if (horizontalRaycasts)
                Raycast(lastPositions[i], currentPosition, RaycastType.Horizontal);

            //Raycast in intersection shape ( \ shape ) Top-to-Bottom
            if (intersectionRaycastsTop && i > 0)
                Raycast(lastPositions[i], GetTransformPoint(sensors[i - 1]), RaycastType.IntersectionTop);

            //Raycast in intersection shape ( / shape ) Bottom-to-Top
            if (intersectionRaycastsBottom && i < sensorCount - 1)
                Raycast(lastPositions[i], GetTransformPoint(sensors[i + 1]), RaycastType.IntersectionBottom);

            //Set last position
            lastPositions[i] = currentPosition;
        }

        //Raycast from startPoint to endPoint (Vertical)
        if (verticalRaycasts)
            Raycast(lastPositions[0], lastPositions[sensorCount - 1], RaycastType.Vertical);
    }

    private RaycastHit2D[] hits;
    private enum RaycastType { Horizontal, Vertical, IntersectionTop, IntersectionBottom };
    private void Raycast(Vector2 from, Vector2 to, RaycastType rayType)
    {
        bool hitDetected = false;

        hits = Physics2D.LinecastAll(from, to);

        //Iterate results
        foreach (RaycastHit2D hit in hits)
        {
            //Check if collider exists and is not the collider attached to target transform (self)
            if (hit.collider != null && hit.collider.transform != target)
            {
                hitDetected = true;
                HandleHit(hit);
            }
        }

        #region Debug rays
        if (showDebugRays)
        {
            Color lineColor = Color.white;

            switch (rayType)
            {
                case RaycastType.Horizontal:
                    lineColor = Color.white;
                    break;

                case RaycastType.Vertical:
                    lineColor = Color.cyan;
                    break;

                case RaycastType.IntersectionTop:
                    lineColor = Color.magenta;
                    break;

                case RaycastType.IntersectionBottom:
                    lineColor = Color.yellow;
                    break;
            }

            //Red whenever it hits something
            Debug.DrawLine(from, to, hitDetected ? Color.red : lineColor, debugRayLifetime);
        }
        #endregion

    }

    private HashSet<Collider2D> hitObjects = new HashSet<Collider2D>();
    private void HandleHit(RaycastHit2D hit)
    {
        //Ignore objects that have already been hit
        if (hitObjects.Contains(hit.collider))
            return;
        else
            hitObjects.Add(hit.collider);

        Debug.Log("Hit detected! gameObject's name: " + hit.collider.gameObject.name);

        if (stopAfterFirstHit)
            Stop();

        if (showDebugRays)
        {
            //Draw a + symbol on hit point
            Debug.DrawRay(hit.point + new Vector2(0, 0.2f), Vector2.down * 0.4f, Color.red, debugRayLifetime * 1.5f);
            Debug.DrawRay(hit.point + new Vector2(-0.2f, 0), Vector2.right * 0.4f, Color.red, debugRayLifetime * 1.5f);
        }

        //////////////////////////////////////////////////////////////////
        // You probably want to implement a interface or something here //
        //////////////////////////////////////////////////////////////////


    }

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            foreach (Vector2 v in sensors)
                Gizmos.DrawWireSphere(GetTransformPoint(v), 0.075f);
        }

        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startPoint.position, 0.1f);
            Gizmos.DrawWireSphere(endPoint.position, 0.1f);
        }
    }
    #endregion
}
