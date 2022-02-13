using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class SphereCreator : MonoBehaviour{
    public int NumSegments = 12;
    public float Radius = 2f;
    public MeshFilter SphereMeshFilter;
    
    private Mesh sphereMesh;

    private void Start(){
        sphereMesh = SphereMeshFilter.mesh;
        CreateSphere();
        
    }

    private void CreateSphere(){
        sphereMesh.vertices = CreateVertices();
        sphereMesh.triangles =  CreateTriangles();

        sphereMesh.RecalculateBounds();
        sphereMesh.RecalculateNormals();
    }


    private int[] CreateTriangles(){
        List<int> tris = new List<int>();

        int bound = NumSegments * (NumSegments - 1) - 1;
        for (int i = 0; i < bound; i++){
            tris.Add(i);
            tris.Add(i + NumSegments + 1);
            tris.Add(i + NumSegments);
                
            tris.Add(i);
            tris.Add(i + 1);
            tris.Add(i + NumSegments + 1);
        }
        
        tris.Add(bound);
        tris.Add(bound + 1);
        tris.Add(bound + NumSegments);
        
        tris.Add(bound + 1);
        tris.Add(bound + NumSegments + 1);
        tris.Add(bound + NumSegments);

        for (int i = 0; i < NumSegments-1; i++)
        {
            tris.Add(bound + 1 + i);
            tris.Add(bound + 2 + i);
            tris.Add(NumSegments * NumSegments);
        }
        
        return tris.ToArray();
    }


    private Vector3[] CreateVertices(){
        Vector3[] vertices = new Vector3[NumSegments * NumSegments + 1];

        for (int i = 0; i < NumSegments; i++){
            float lat = Map(i, 0, NumSegments, -Mathf.PI, Mathf.PI);
            
            for (int j = 0; j < NumSegments; j++){
                float lon = Map(j, 0, NumSegments, -Mathf.PI/2, Mathf.PI/2);

                float xPos = Radius * Mathf.Sin(lat) * Mathf.Cos(lon);
                float yPos = Radius * Mathf.Sin(lat) * Mathf.Sin(lon);
                float zPos = Radius * Mathf.Cos(lat);
                
                vertices[i * NumSegments + j] = new Vector3(xPos, yPos, zPos);
            }
        }
        
        vertices[vertices.Length-1] = new Vector3(0, 0, 0);
        
        return vertices;
    }



    float Map(float val, float currentStart, float currentEnd, float targetStart, float targetEnd){
        float targetRange = targetEnd - targetStart;
        float currentRange = currentEnd - currentStart;
        float rangeIncrements = targetRange / currentRange;
        return targetStart + (rangeIncrements * (val - currentStart));
    }
}
