using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    Mesh hexMesh;
    MeshCollider meshCollider;

    [NonSerialized]
    List<Vector3> vertices;
    [NonSerialized]
    List<Color> colors;
    [NonSerialized]
    List<int> triangles;
    [NonSerialized]
    List<Vector2> uvs;
    

    public bool useCollider,useColors,useUVCoordinates;

    //static List<Vector3> vertices=new List<Vector3>();
    //static List<int> triangles=new List<int>();
    //static List<Color> colors=new List<Color>();

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        if(useCollider)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        
        hexMesh.name = "Hex Mesh";
        
    }




    public void AddTriangleColor(Color Color)
    {
        colors.Add(Color); colors.Add(Color); colors.Add(Color);
    }

    public void AddTriangleColor(Color c1, Color c2, Color c3)
    {
        colors.Add(c1); colors.Add(c2); colors.Add(c3);
    }

    public void Addtriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(HexMetrics.Perturb(v1));
        vertices.Add(HexMetrics.Perturb(v2));
        vertices.Add(HexMetrics.Perturb(v3));

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }
    public void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(HexMetrics.Perturb(v1)); vertices.Add(HexMetrics.Perturb(v2)); vertices.Add(HexMetrics.Perturb(v3)); vertices.Add(HexMetrics.Perturb(v4));

        triangles.Add(vertexIndex); triangles.Add(vertexIndex + 2); triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1); triangles.Add(vertexIndex + 2); triangles.Add(vertexIndex + 3);
    }

    public void AddQuadColor(Color c1, Color c2)
    {
        colors.Add(c1); colors.Add(c1); colors.Add(c2); colors.Add(c2);
    }

    public void AddQuadColor(Color color)
    {
        colors.Add(color); colors.Add(color); colors.Add(color); colors.Add(color);
    }

    public void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        colors.Add(c1); colors.Add(c2); colors.Add(c3); colors.Add(c4);
    }

    public void AddTriangleUV(Vector2 uv1,Vector2 uv2,Vector2 uv3)
    {
        uvs.Add(uv1);
        uvs.Add(uv2);
        uvs.Add(uv3);
    }
    public void AddQuadUV(Vector2 uv1,Vector2 uv2,Vector2 uv3,Vector2 uv4)
    {
        uvs.Add(uv1);
        uvs.Add(uv2);
        uvs.Add(uv3);
        uvs.Add(uv4);
    }

    public void AddQuadUV(float uMin,float uMax,float vMin,float vMax)
    {
        uvs.Add(new Vector2(uMin, vMin));
        uvs.Add(new Vector2(uMax, vMin));
        uvs.Add(new Vector2(uMin, vMax));
        uvs.Add(new Vector2(uMax, vMax));
    }

   
    public void Clear()
    {
        hexMesh.Clear();
        vertices = ListPool<Vector3>.Get();
        if(useColors)
        {
            colors = ListPool<Color>.Get();
        }
        if(useUVCoordinates)
        {
            uvs = ListPool<Vector2>.Get();
        }
        
        triangles = ListPool<int>.Get();

    }

    public void Apply()
    {
        hexMesh.SetVertices(vertices);
        ListPool<Vector3>.Add(vertices);
        if(useColors)
        {
            hexMesh.SetColors(colors);
            ListPool<Color>.Add(colors);
        }

        if(useUVCoordinates)
        {
            hexMesh.SetUVs(0, uvs);
            ListPool<Vector2>.Add(uvs);
        }
        
        hexMesh.SetTriangles(triangles,0);
        ListPool<int>.Add(triangles);
        hexMesh.RecalculateNormals();
        if(useCollider)
        {
            meshCollider.sharedMesh = hexMesh;
        }
        
    }
}
