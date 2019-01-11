using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    Mesh hexMesh;
    List<Vector3> vertices;
    List<int> triangles;

    MeshCollider meshCollider;

    List<Color> colors;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        hexMesh.name = "Hex Mesh";
        vertices = new List<Vector3>();
        triangles = new List<int>();
        colors = new List<Color>();
    }
    void Start()
    {
       
    }


    public void TriangulateAll(HexCell[] cells)
    {
        hexMesh.Clear();
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
        for(int i=0;i<cells.Length;i++)
        {
            Triangulate(cells[i]);
        }
        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.colors = colors.ToArray();

        hexMesh.RecalculateNormals();
        meshCollider.sharedMesh = hexMesh;

    }
    void Triangulate(HexCell cell)
    {
        Vector3 center = cell.transform.localPosition;
        for(int i=0;i<6;i++)
        {
            AddTriangles(center, center + HexMetrics.corners[i], center + HexMetrics.corners[i+1]);
            AddTriangleColor(cell.color);
        }
        

    }

    void AddTriangleColor(Color color)
    {
        colors.Add(color); colors.Add(color); colors.Add(color);
    }

    void AddTriangles(Vector3 v1,Vector3 v2,Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);

    }
    
   

    // Update is called once per frame
    void Update()
    {
        
    }
}
