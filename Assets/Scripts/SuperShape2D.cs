using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

public class SuperShape2D : MonoBehaviour{
    public Slider Slider1, Slider2;
    public LineRenderer Line;
    
    public float Radius = 1;
    public float a = 1;
    public float b = 1;
    public float m = 5;
    public float n1 = 1;
    public float n2 = 1;
    public float n3 = 1;
    
    public int NumSegments = 50;
    
    private readonly Vector3[] _nValues = {
        new Vector3(1, 1, 1),
        new Vector3(0.3f, 0.3f, 0.3f),
        new Vector3(40f, 10f, 10f),
        new Vector3(100f, 100f, 100f),
        new Vector3(60f, 55f, 30f),
    };
    
    private NativeArray<Vector3> vertices;
    private JobHandle meshModificationJobHandle;
    private UpdateMeshJob meshModificationJob;
    
    void Start(){
        vertices = new NativeArray<Vector3>(NumSegments, Allocator.Persistent);
        Slider1.value = Radius;
        Slider2.value = 0;
        Slider1.onValueChanged.AddListener(delegate(float val){
            n1 = _nValues[(int)val].x;
            n2 = _nValues[(int)val].y;
            n3 = _nValues[(int)val].z;
            Create();
        });
        Slider2.onValueChanged.AddListener(delegate(float val){
            m = val;
            Create();
        });
        Create();
    }

    private void Create(){
        float angleIncrements = (Mathf.PI * 2) / NumSegments;
        
        meshModificationJob = new UpdateMeshJob()
        {
            vertices = vertices,
            radius = Radius,
            a =  a,
            b = b,
            m = m,
            n1 = n1,
            n2 = n2,
            n3 = n3,
            angleIncrements = angleIncrements,
        };

        meshModificationJobHandle = meshModificationJob.Schedule(NumSegments, 64);
        
        meshModificationJobHandle.Complete();
        Line.positionCount = meshModificationJob.vertices.Length+1;
        Line.SetPositions(meshModificationJob.vertices);
        Line.SetPosition(NumSegments,Line.GetPosition(0));
    }

    private void OnDestroy()
    {
        vertices.Dispose();
    }


    [BurstCompile]
    public struct UpdateMeshJob : IJobParallelFor{

        public NativeArray<Vector3> vertices;
        
        public float radius;
        public float a;
        public float b;
        public float m;
        public float n1;
        public float n2;
        public float n3;
        
        public float angleIncrements;


        public void Execute(int i)
        {
            float na = 2 / radius;
        
            // vertices[i] = GetCirclePos(i * angleIncrements, radius);
            // vertices[i] = GetSuperEllipsePos(i * angleIncrements, na);
            vertices[i] = GetSuperShapePos(i * angleIncrements, na);
        }
        
        
        Vector3 GetSuperShapePos(float angle, float radius){
            radius = SuperShape(angle);
            float xPos = radius * Mathf.Cos(angle);
            float yPos = radius * Mathf.Sin(angle);
            return new Vector3(xPos, yPos, 0);
        }

        float SuperShape(float angle){
            float part1 = (1 / a) * Mathf.Cos(angle * m / 4);
            part1 = Mathf.Abs(part1);
            part1 = Mathf.Pow(part1, n2);
        
            float part2 = (1 / b) * Mathf.Sin(angle * m / 4);
            part2 = Mathf.Abs(part2);
            part2 = Mathf.Pow(part2, n3);

            float part3 = Mathf.Pow(part1 + part2, 1 / n1);

            if (part3 == 0) return 0;
        
            return (1 / part3);
        }
        
        int sign(float val){
            if (val < 0) return -1;
            if (val > 0) return 1;
            return 0;
        }
        
        Vector3 GetCirclePos(float angle, float radius){
            float xPos = radius * Mathf.Cos(angle);
            float yPos = radius * Mathf.Sin(angle);
            return new Vector3(xPos, yPos, 0);
        }

        Vector3 GetSuperEllipsePos(float angle, float na){
            float xPos = Mathf.Pow(Mathf.Abs(Mathf.Cos(angle)), na) * a * sign(Mathf.Cos(angle));
            float yPos = Mathf.Pow(Mathf.Abs(Mathf.Sin(angle)), na) * b * sign(Mathf.Sin(angle));
            return new Vector3(xPos, yPos, 0);
        }
    
    }
}
