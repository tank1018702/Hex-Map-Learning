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
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    void Triangulate(HexDirection direction, HexCell cell)
    {
        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);

        AddTriangles(center, v1, v2);
        AddTriangleColor(cell.color);

        if (direction <= HexDirection.SE)
        {
            TriangulateConnection(direction, cell, v1, v2);
        }

        //AddTriangles(center, 
        //    center + HexMetrics.GetFirstSolidCorner(direction), 
        //    center + HexMetrics.GetSecondSolidCorner(direction));

        //HexCell prevNeighbor = cell.GetNeighbor(direction.Previous()) ?? cell;
        //HexCell neighbor = cell.GetNeighbor(direction) ?? cell;
        //HexCell nextNeighbor = cell.GetNeighbor(direction.Next()) ?? cell;

        //Color bridgeColor = (cell.color + neighbor.color) * 0.5f;
        //AddQuadColor(cell.color, bridgeColor);

        //AddTriangles(v1, center + HexMetrics.GetFirstCorner(direction), v3);
        //AddTriangleColor(cell.color, (cell.color + prevNeighbor.color + neighbor.color) / 3f, bridgeColor);

        //AddTriangles(v2, v4, center + HexMetrics.GetSecondCorner(direction));
        //AddTriangleColor(cell.color, bridgeColor, (cell.color + neighbor.color + nextNeighbor.color) / 3f);
        //AddQuadColor(cell.color, 
        //             cell.color, 
        //            (cell.color + prevNeighbor.color + neighbor.color) / 3f,
        //            (cell.color + neighbor.color + nextNeighbor.color) / 3f);

        //Color edgeColor = (cell.color + neighbor.color) * 0.5f;
        //AddTriangleColor(cell.color,
        //    (cell.color + prevNeighbor.color + neighbor.color) / 3f,
        //    (cell.color + neighbor.color + nextNeighbor.color) / 3f);

    }

    void TriangulateConnection(HexDirection direction,HexCell cell,Vector3 v1,Vector3 v2)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        if (!neighbor) { return; }

        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;

        AddQuad(v1, v2, v3, v4);
        AddQuadColor(cell.color, neighbor.color);

        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if(nextNeighbor&&direction<=HexDirection.E)
        {
            AddTriangles(v2, v4, v2+HexMetrics.GetBridge(direction.Next()));
            AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
        }
    }

    void AddTriangleColor(Color color)
    {
        colors.Add(color); colors.Add(color); colors.Add(color);
    }

    void AddTriangleColor(Color c1,Color c2,Color c3)
    {
        colors.Add(c1); colors.Add(c2); colors.Add(c3);
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
    
    void AddQuad(Vector3 v1,Vector3 v2,Vector3 v3 ,Vector3 v4)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1); vertices.Add(v2); vertices.Add(v3); vertices.Add(v4);

        triangles.Add(vertexIndex); triangles.Add(vertexIndex+2); triangles.Add(vertexIndex+1);
        triangles.Add(vertexIndex+1); triangles.Add(vertexIndex+2); triangles.Add(vertexIndex+3);
    }

    void AddQuadColor(Color c1,Color c2)
    {
        colors.Add(c1); colors.Add(c1); colors.Add(c2); colors.Add(c2);
    }
   

    // Update is called once per frame
    void Update()
    {
        
    }
}
