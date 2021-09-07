using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class RayMarching : MonoBehaviour
{
    [SerializeField]
    private ComputeShader raymarchingShader;

    RenderTexture target;
    Camera cam;
    Light lightSource;
    List<ComputeBuffer> buffersToDispose;

    struct ShapeData
    {
        public Vector3 position;
        public Vector3 scale;
        public Vector3 colour;
        public int shapeType;
        public int operation;
        public float blendStrength;

        public static int GetSize()
        {
            return sizeof(float) * 10 + sizeof(int) * 2;
        }
    }

    void Init()
    {
        cam = Camera.current;
        lightSource = FindObjectOfType<Light>();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Init();
        buffersToDispose = new List<ComputeBuffer>();

        InitRenderTexture();
        CreateScene();
        SetParameters();

        raymarchingShader.SetTexture(0, "Source", source);
        raymarchingShader.SetTexture(0, "Destination", target);

        //Organise the thread groups so there is one thread per pixel on the screen
        int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 8.0f);
        raymarchingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Graphics.Blit(target, destination);

        foreach (var buffer in buffersToDispose)
        {
            buffer.Dispose();
        }
    }

    void InitRenderTexture()
    {
        // Checks if the RenderTexture exists and matches the dimensions of the camera
        if (target == null || target.width != cam.pixelWidth || target.height != cam.pixelHeight)
        {
            if (target != null)
            {
                //Discard the current render texture and make a new one
                target.Release();
            }
            target = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
        }
    }

    void CreateScene()
    {
        List<Shape> allShapes = new List<Shape>(FindObjectsOfType<Shape>());
        // order all of teh shapes by their operation
        allShapes.Sort((a, b) => a.operation.CompareTo(b.operation));

        ShapeData[] shapeData = new ShapeData[allShapes.Count];
        for (int i = 0; i < allShapes.Count; i++)
        {
            var s = allShapes[i];
            Vector3 col = new Vector3(s.colour.r, s.colour.g, s.colour.b);
            shapeData[i] = new ShapeData()
            {
                position = s.Position,
                scale = s.Scale,
                colour = col,
                shapeType = (int)s.shapeType,
                operation = (int)s.operation,
                blendStrength = s.blendStrength * 3
            };
        }

        ComputeBuffer shapeBuffer = new ComputeBuffer(shapeData.Length, ShapeData.GetSize());
        shapeBuffer.SetData(shapeData);
        raymarchingShader.SetBuffer(0, "shapes", shapeBuffer);
        raymarchingShader.SetInt("numShapes", shapeData.Length);

        buffersToDispose.Add(shapeBuffer);
    }

    void SetParameters()
    {
        raymarchingShader.SetMatrix("cameraToWorld", cam.cameraToWorldMatrix);
        raymarchingShader.SetMatrix("cameraInverseProjection", cam.projectionMatrix.inverse);
        raymarchingShader.SetVector("lightDirection", lightSource.transform.forward);
    }
}