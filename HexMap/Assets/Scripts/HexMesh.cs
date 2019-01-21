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
        v3.y = v4.y = neighbor.Elevation * HexMetrics.elevationStep;

        if(cell.GetEdgeType(direction)==HexEdgeType.Slope)
        {
            TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
        }
        else
        {
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(cell.color, neighbor.color);
        }

        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());

        if(nextNeighbor&&direction<=HexDirection.E)
        {
            Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Elevation * HexMetrics.elevationStep;

            if(cell.Elevation<=neighbor.Elevation)
            {
                if(cell.Elevation<=nextNeighbor.Elevation)
                {
                    TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
                }
            }
            else if(neighbor.Elevation<=nextNeighbor.Elevation)
            {
                TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
            }
            else
            {
                TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
            }

            //AddTriangles(v2, v4, v5);
            //AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
        }
    }

    void TriangulateEdgeTerraces(Vector3 beginLeft,Vector3 BeginRight,HexCell beginCell,
                                 Vector3 endLeft,  Vector3 endRight,  HexCell endCell)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(BeginRight, endRight, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, 1);

        AddQuad(beginLeft, BeginRight, v3, v4);
        AddQuadColor(beginCell.color, c2);

        for(int i=2;i<HexMetrics.terraceSteps;i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c2;
            v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
            v4 = HexMetrics.TerraceLerp(BeginRight, endRight, i);
            c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);

            AddQuad(v1,v2,v3,v4);
            AddQuadColor(c1, c2);
        }

        AddQuad(v3, v4, endLeft, endRight);
        AddQuadColor(c2, endCell.color);
    }

    void TriangulateCorner(Vector3 bottom,HexCell bottomCell,
                           Vector3 left,  HexCell leftCell,
                           Vector3 right, HexCell rightCell)
    {
        HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

        if(leftEdgeType==HexEdgeType.Slope)
        {
            if(rightEdgeType==HexEdgeType.Slope)
            {
                TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }
            else if (rightEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);     
            }
            else
            {
                TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
            }     
        }
        else if (rightEdgeType == HexEdgeType.Slope)
        {
            if (leftEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);            
            }
            else
            {
                TriangulateCornerCliffTerrace(bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }
        else if(leftCell.GetEdgeType(rightCell)==HexEdgeType.Slope)
        {
            if(leftCell.Elevation<rightCell.Elevation)
            {
                TriangulateCornerCliffTerrace(right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else
            {
                TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);  
            }
        }
        else
        {
            AddTriangles(bottom, left, right);
            AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color);
        }  
    }

    void TriangulateCornerTerraces(Vector3 begin,HexCell beginCell,
                                   Vector3 left, HexCell leftCell,
                                   Vector3 right,HexCell rightCell)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);

        Color c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);
        Color c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, 1);

        AddTriangles(begin, v3, v4);
        AddTriangleColor(beginCell.color, c3, c4);

        for(int i=2;i<HexMetrics.terraceSteps;i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;
            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4= HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
            c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, i);

            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2, c3, c4);
        }

        AddQuad(v3, v4, left, right);
        AddQuadColor(c3, c4, leftCell.color, rightCell.color);

    }

        

    void TriangulateCornerTerracesCliff(Vector3 begin,HexCell beginCell,
                                        Vector3 left, HexCell leftCell,
                                        Vector3 right,HexCell rightCell)
    {
        float t =Mathf.Abs( 1f / (rightCell.Elevation - beginCell.Elevation));
        Vector3 boundary = Vector3.Lerp(begin, right, t);
        Color boundaryColor = Color.Lerp(beginCell.color, rightCell.color, t);

        TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

        if(leftCell.GetEdgeType(rightCell)==HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        else
        {
            AddTriangles(left, right, boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
        }
    }
    void TriangulateCornerCliffTerrace(Vector3 begin, HexCell beginCell,
                                       Vector3 left, HexCell leftCell,
                                       Vector3 right, HexCell rightCell)
    {
        float t =Mathf.Abs( 1f / (leftCell.Elevation - beginCell.Elevation));
        Vector3 boundary = Vector3.Lerp(begin, left, t);
        Color boundaryColor = Color.Lerp(beginCell.color, leftCell.color, t);

        TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        else
        {
            AddTriangles(left, right, boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
        }
    }

    void TriangulateBoundaryTriangle(Vector3 begin, HexCell beginCell,
                                 Vector3 left, HexCell leftCell,
                                 Vector3 boundary, Color boundaryColor)
    {
        Vector3 v2 = HexMetrics.TerraceLerp(begin, left, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);

        AddTriangles(begin, v2, boundary);
        AddTriangleColor(beginCell.color, c2, boundaryColor);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;

            v2 = HexMetrics.TerraceLerp(begin, left, i);
            c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);

            AddTriangles(v1, v2, boundary);
            AddTriangleColor(c1, c2, boundaryColor);
        }

        AddTriangles(v2, left, boundary);
        AddTriangleColor(c2, leftCell.color, boundaryColor);


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
   
    void AddQuadColor(Color c1,Color c2,Color c3,Color c4)
    {
        colors.Add(c1); colors.Add(c2); colors.Add(c3); colors.Add(c4);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
