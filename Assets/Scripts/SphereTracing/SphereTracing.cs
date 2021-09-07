using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Circle
{
    public float radius;
    public Vector2 centre;
}

public class SphereTracing : MonoBehaviour
{
    public float maxDst = 100.0f;
    public float minDst = 0.0001f;
    public Circle[] circles;
    int numCircles = 0;
    public GameObject tracingCircle;
    GameObject viewPoint;
    Vector2 direction;

    void Start()
    {
        viewPoint = GameObject.Find("ViewPoint");

        GameObject[] circleObjects;
        circleObjects = GameObject.FindGameObjectsWithTag("circle");
        circles = new Circle[circleObjects.Length];
        for (int i = 0; i < circleObjects.Length; i++)
        {
            circles[i].radius = circleObjects[i].transform.localScale.x / 2.0f;
            circles[i].centre.x = circleObjects[i].transform.position.x;
            circles[i].centre.y = circleObjects[i].transform.position.y;
            numCircles++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        DetectCursor();
        BeginTracing();
    }

    public void DetectCursor()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0.0f;
        direction = mousePos - viewPoint.transform.position;
        direction.Normalize();
    }

    public void BeginTracing()
    {
        //Clear old circles
        GameObject[] oldCircles = GameObject.FindGameObjectsWithTag("tracing");
        for (int i = 0; i < oldCircles.Length; i++)
        {
            Destroy(oldCircles[i].gameObject);
        }

        int iterations = 200;
        Vector2 p = viewPoint.transform.position;
        float dst = maxDst;
        do
        {
            dst = SignedDstToScene(p);
            TraceCircle(dst, p);
            p += (direction * dst);
            iterations--;
        }
        while (dst > minDst && dst < maxDst);
    }

    public void TraceCircle(float size, Vector2 p)
    {
        Vector2 newScale = new Vector2(size * 2, size * 2);
        GameObject go = Instantiate(tracingCircle, p, Quaternion.identity);
        go.transform.localScale = newScale;
    }

    public float Length(Vector2 v)
    {
        return Mathf.Sqrt(v.x*v.x + v.y*v.y);
    }
    public float SignedDstToCircle(Vector2 p, Vector2 centre, float radius)
    {
        return Length(centre - p) - radius;
    }

    public float SignedDstToScene(Vector2 p)
    {
        float dstToScene = maxDst;
        for (int i = 0; i < numCircles; i++)
        {
            float dstToCircle = SignedDstToCircle(p, circles[i].centre, circles[i].radius);
            dstToScene = Mathf.Min(dstToCircle, dstToScene);
        }
        return dstToScene;
    }
}