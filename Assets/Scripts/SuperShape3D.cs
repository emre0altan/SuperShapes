using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class SuperShape3D : MonoBehaviour
{
    [Range(0, 1)] public float SmoothRate = 0;
    public SphereCreator SphereCreator;

    public float a = 1;
    public float b = 1;
    public float m = 1;
    public float n1 = 1;
    public float n2 = 1;
    public float n3 = 1;
    
    public float a2;
    public float b2;
    public float m2;
    public float n1_2;
    public float n2_2;
    public float n3_2;

    public bool morphToSecondSuperShape = false;
    public bool smooth = false;
    
    private int numSegments = 60;

    private Mesh mesh;
    
    private NativeArray<Vector3> vertices;
    private JobHandle meshModificationJobHandle;
    private UpdateMeshJob meshModificationJob;
    
    
    private void Update(){
        if (mesh == null){
            mesh = SphereCreator.SphereMeshFilter.mesh;
            numSegments = SphereCreator.NumSegments;
            mesh.MarkDynamic();
            vertices = new NativeArray<Vector3>(mesh.vertices, Allocator.Persistent);
        }
         
        meshModificationJob = new UpdateMeshJob()
        {
            vertices = vertices,
            numSegments =  numSegments,
            a =  a,
            b = b,
            m = m + Time.time * 2,
            n1 = n1,
            n2 = n2,
            n3 = n3,
            a2 = a2,
            b2 = b2,
            m2 = m2 + Time.time * 2,
            n1_2 = n1_2,
            n2_2 = n2_2,
            n3_2 = n3_2,
            morphIt = morphToSecondSuperShape,
            smoothRate = SmoothRate,
            smoothIt = smooth,
        };

        meshModificationJobHandle = meshModificationJob.Schedule(vertices.Length, 64);
    }
    
    private void LateUpdate()
    {
        meshModificationJobHandle.Complete();
        mesh.SetVertices(meshModificationJob.vertices);
        mesh.RecalculateNormals();
    }
    
    private void OnDestroy(){
        vertices.Dispose();
    }

    
    [BurstCompile]
    public struct UpdateMeshJob : IJobParallelFor{

        public NativeArray<Vector3> vertices;
        public int numSegments;

        public float a;
        public float b;
        public float m;
        public float n1;
        public float n2;
        public float n3;
        
        public float a2;
        public float b2;
        public float m2;
        public float n1_2;
        public float n2_2;
        public float n3_2;

        public float smoothRate;
        public bool morphIt, smoothIt;

        public void Execute(int i)
        {
            float loop2 = i % numSegments;
            float loop1 = (i - loop2) / numSegments;
            
            float lat = Map(loop1, 0, numSegments, -Mathf.PI/2f, Mathf.PI/2f);
            float lon = Map(loop2, 0, numSegments, -Mathf.PI, Mathf.PI);
            
            float r1 = SuperShape(lon, a, b, m, n1, n2, n3);
            float r2 = SuperShape(lat, a, b, m, n1, n2, n3);

            if (smoothIt)
            {
                vertices[i] = Vector3.Lerp(vertices[i], new Vector3(r1 * Mathf.Cos(lon) * r2 * Mathf.Cos(lat),
                    r1 * Mathf.Sin(lon) * r2 * Mathf.Cos(lat), 
                    r2 * Mathf.Sin(lat)),  1 - smoothRate);
            }
            else
            {
                vertices[i] = new Vector3(r1 * Mathf.Cos(lon) * r2 * Mathf.Cos(lat),
                    r1 * Mathf.Sin(lon) * r2 * Mathf.Cos(lat), 
                    r2 * Mathf.Sin(lat));
            }

            
            if (morphIt)
            {
                loop2 = i % numSegments;
                loop1 = (i - loop2) / numSegments;
            
                lat = Map(loop1, 0, numSegments, -Mathf.PI/2f, Mathf.PI/2f);
                lon = Map(loop2, 0, numSegments, -Mathf.PI, Mathf.PI);
            
                r1 = SuperShape(lon, a2, b2, m2, n1_2, n2_2, n3_2);
                r2 = SuperShape(lat, a2, b2, m2, n1_2, n2_2, n3_2);

                Vector3 vertex;
                if (smoothIt)
                {
                    vertex = Vector3.Lerp(vertices[i], new Vector3(r1 * Mathf.Cos(lon) * r2 * Mathf.Cos(lat),
                        r1 * Mathf.Sin(lon) * r2 * Mathf.Cos(lat), 
                        r2 * Mathf.Sin(lat)),  1 - smoothRate);
                }
                else
                {
                    vertex = new Vector3(r1 * Mathf.Cos(lon) * r2 * Mathf.Cos(lat),
                        r1 * Mathf.Sin(lon) * r2 * Mathf.Cos(lat), 
                        r2 * Mathf.Sin(lat));
                }
                
                
                vertices[i] += vertex;
                vertices[i] *= 0.5f;
            }
        }
        
        float SuperShape(float theta, float a, float b, float m, float n1, float n2, float n3){
            float t1 = Mathf.Abs((1 / a) * Mathf.Cos(m * theta / 4));
            t1 = Mathf.Pow(t1, n2);
            float t2 = Mathf.Abs((1 / b) * Mathf.Sin(m * theta / 4));
            t2 = Mathf.Pow(t2, n3);
        
            float t3 = t1 + t2;
            float t4 = Mathf.Pow(t3, (-1 / n1));
            return t4;
        }

        float Map(float val, float currentStart, float currentEnd, float targetStart, float targetEnd){
            return targetStart + ((targetEnd - targetStart) / (currentEnd - currentStart) * (val - currentStart));
        }
    }
}
