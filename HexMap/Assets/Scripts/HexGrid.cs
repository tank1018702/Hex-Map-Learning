using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    //public int width = 6;
    //public int height = 6;

    public int chunkCountX = 4, chunkCountZ = 3;

    int cellCountX, cellCountZ;

    public HexCell cellPrefab;

    public Text cellLabelPrefab;

    //Canvas gridCanvas;

    //HexMesh hexMesh;


    HexCell[] cells;

    public Texture2D noiseSource;

    public HexGridChunk chunkPrefab;

    HexGridChunk[] chunks;


    private void OnEnable()
    {
        HexMetrics.noiseSource = noiseSource;
    }


    private void Awake()
    {
        HexMetrics.noiseSource = noiseSource;

        //gridCanvas = GetComponentInChildren<Canvas>();
        //hexMesh = GetComponentInChildren<HexMesh>();

        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

        CreateChunks();
        CreateCells();
      
    }

    void CreateCells()
    {
        cells = new HexCell[cellCountX * cellCountZ];

        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    void CreateChunks()
    {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        for(int z=0,i=0;z<chunkCountZ;z++)
        {
            for(int x=0;x<chunkCountX;x++)
            {
                HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
            }
        }
    }

    //private void Start()
    //{
        
    //    hexMesh.TriangulateAll(cells);
        
    //}

    void CreateCell(int x,int z,int i)
    {
        Vector3 position;
        position.x = (x+z*0.5f-z/2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);



        HexCell cell = cells[i] = Instantiate(cellPrefab);

        //cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;

        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.Color = Color.white;

        if(x>0)
        {
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }
        if(z>0)
        {
            if((z&1)==0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
                if(x>0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                if(x<cellCountX-1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                }
            }
        }

        Text label = Instantiate(cellLabelPrefab);
        //label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);

        label.text = cell.coordinates.ToStringOnSpearateLines();
        cell.uiRect = label.rectTransform;
        cell.Elevation = 0;

        AddCellToChunk(x, z, cell);

    }

    void AddCellToChunk(int x,int z,HexCell cell)
    {
        int chunkX = x / HexMetrics.chunkSizeX;
        int chunkZ = z / HexMetrics.chunkSizeZ;
        HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

        int localX = x - chunkX * HexMetrics.chunkSizeX;
        int localZ = z - chunkZ * HexMetrics.chunkSizeZ;

        chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
    }

    public HexCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
        //HexCell cell = cells[index];
        //cell.Color = Color;
        
        return cells[index];     
    }

    public HexCell GetCell(HexCoordinates coordinates)
    {
        int z = coordinates.Z;
        if (z < 0 || z >= cellCountZ) { return null; }

        int x = coordinates.X + z / 2;
        if (x < 0 || x >= cellCountX) { return null; }

        return cells[x + z * cellCountX];
    }

    public void ShowLabelUI(bool visible)
    {
        for(int i=0;i<chunks.Length;i++)
        {
            chunks[i].ShowLabelUI(visible);
        }
    }
    //public  void Refresh()
    //{
    //    hexMesh.TriangulateAll(cells);
    //}
}
