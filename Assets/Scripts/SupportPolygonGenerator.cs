using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SupportPolygonGenerator : MonoBehaviour
{
    public Transform[] verticePositions;
    Vector3[] vertices;
    int[] triangles;
    Mesh mesh;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        CreateShape();
        UpdateMesh();

        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void FixedUpdate()
    {
        
    }

    void CreateShape()
    {
        vertices = new Vector3[verticePositions.Length];
        for (int i = 0; i < verticePositions.Length; i++)
        {
            vertices[i] = verticePositions[i].position;
        }

        triangles = new int[]
        {
            0,3,2,
            2,1,0
        };
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }
}
