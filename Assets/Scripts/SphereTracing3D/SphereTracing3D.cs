using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Sphere
{
    public float radius;
    public Vector3 centre;
}

public class SphereTracing3D : MonoBehaviour
{
    public float maxDst = 20.0f;
    public float minDst = 0.0001f;
    public Sphere[] spheres;
    int numSpheres = 0;
    public GameObject tracingSphere;
    public GameObject ray;
    Vector3 rayDirection;


    // Start is called before the first frame update
    void Start()
    {
        GameObject[] sphereObjects;
        sphereObjects = GameObject.FindGameObjectsWithTag("sphere");
        spheres = new Sphere[sphereObjects.Length];
        for (int i = 0; i < sphereObjects.Length; i++)
        {
            spheres[i].radius = sphereObjects[i].transform.localScale.x / 2.0f;
            spheres[i].centre = sphereObjects[i].transform.position;
            numSpheres++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        rayDirection = ray.transform.localRotation * ray.transform.forward;
        rayDirection.Normalize();

        BeginTracing();
    }

    public void BeginTracing()
    {
        //Clear old spheres
        GameObject[] oldSpheres = GameObject.FindGameObjectsWithTag("tracing");
        for (int i = 0; i < oldSpheres.Length; i++)
        {
            Destroy(oldSpheres[i].gameObject);
        }
        int iterations = 10;
        Vector3 p = ray.transform.position;
        float dst = maxDst;
        do
        {
            dst = SignedDstToScene(p);
            TraceSphere(dst, p);
            p += (rayDirection * dst);
            iterations--;
        }
        while ( dst > minDst && dst < maxDst);
    }

    public void TraceSphere(float size, Vector3 p)
    {
        Vector3 newScale = new Vector3(size * 2, size * 2, size * 2);
        GameObject go = Instantiate(tracingSphere, p, Quaternion.identity);
        go.transform.localScale = newScale;
    }

    public float Length(Vector3 v)
    {
        return Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
    }

    public float SignedDstToSphere(Vector3 p, float s)
    {
        return Length(p) - s;
    }

    public float SignedDstToScene(Vector3 p)
    {
        float dstToScene = maxDst;
        for (int i = 0; i < numSpheres; i++)
        {
            float dstToSphere = SignedDstToSphere(spheres[i].centre - p, spheres[i].radius);
            dstToScene = Mathf.Min(dstToSphere, dstToScene);
        }
        return dstToScene;
    }
}
