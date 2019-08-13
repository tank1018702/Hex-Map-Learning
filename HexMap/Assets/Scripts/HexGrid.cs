using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class HexGrid : MonoBehaviour
{
    //public int width = 6;
    //public int height = 6;

    
    public int cellCountX = 20, cellCountZ = 15;


    int chunkCountX, chunkCountZ;

    public HexCell cellPrefab;

    public Text cellLabelPrefab;

    public int seed;
   
    HexCell[] cells;

    public Texture2D noiseSource;

    public HexGridChunk chunkPrefab;

    HexGridChunk[] chunks;


    private void OnEnable()
    {
        if(!HexMetrics.noiseSource)
        {
            HexMetrics.noiseSource = noiseSource;
            HexMetrics.InitializeHashGrid(seed);
        }    
    }


    private void Awake()
    {
        HexMetrics.noiseSource = noiseSource;
        HexMetrics.InitializeHashGrid(seed);

        CreateMap(cellCountX, cellCountZ);
    }

    public bool CreateMap(int x,int z)
    {
        if(x<=0||x%HexMetrics.chunkSizeX!=0||
           z<=0||z%HexMetrics.chunkSizeZ!=0)
        {
            Debug.LogError("Unsupported map size");
            return false;
        }
        ClearPath();

        if(chunks!=null)
        {
            for(int i=0;i<chunks.Length;i++)
            {
                Destroy(chunks[i].gameObject);
            }
        }
        cellCountX = x;
        cellCountZ = z;
        chunkCountX = cellCountX / HexMetrics.chunkSizeX;
        chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;
        CreateChunks();
        CreateCells();
        return true;
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

        //label.text = cell.coordinates.ToStringOnSpearateLines();
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

    public void Save(BinaryWriter writer)
    {
        writer.Write(cellCountX);
        writer.Write(cellCountZ);

        for(int i=0;i<cells.Length;i++)
        {
            cells[i].Save(writer);
        }
    }

    public void Load(BinaryReader reader,int header)
    {
        ClearPath();
        
        int x = 20, z = 15;
        if(header>=1)
        {
            x = reader.ReadInt32();
            z = reader.ReadInt32();
        }

        if(x!=cellCountX||z!=cellCountZ)
        {
            if (!CreateMap(x, z))
            {
                return;
            }
        }
       
        
        for(int i=0;i<cells.Length;i++)
        {
            cells[i].Load(reader);
        }
        for(int i=0;i<chunks.Length;i++)
        {
            chunks[i].Refresh();
        }
    }

    HexCell currentPathFrom, currentPathTo;

    bool currentPathExists;

    public void FindPath(HexCell fromCell,HexCell toCell,int speed)
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        ClearPath();
        currentPathFrom = fromCell;
        currentPathTo = toCell;
        currentPathExists= Search(fromCell, toCell, speed);
        ShowPath(speed);
        sw.Stop();
        Debug.Log(sw.ElapsedMilliseconds);
    }


    HexCellPriorityQueue searchFrontier;

    int searchFrontierPhase;

    bool Search(HexCell fromCell,HexCell toCell,int speed)
    {
        searchFrontierPhase += 2;
        if(searchFrontier==null)
        {
            searchFrontier = new HexCellPriorityQueue();
        }
        else
        {
            searchFrontier.Clear();
        }

        fromCell.SearchPhase = searchFrontierPhase;
        fromCell.Distance = 0;
        searchFrontier.Enqueue(fromCell);

        while(searchFrontier.Count>0)
        {
            HexCell current = searchFrontier.Dequeue();
            current.SearchPhase += 1;
            if(current==toCell)
            {
                return true;
            }

            int currentTurn = current.Distance / speed;

            for(HexDirection d=HexDirection.NE;d<=HexDirection.NW;d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if(neighbor==null|| neighbor.SearchPhase>searchFrontierPhase)
                {
                    continue;
                }
                if(neighbor.IsUnderwater)
                {
                    continue;
                }
                HexEdgeType edgeType = current.GetEdgeType(neighbor);
                if(edgeType==HexEdgeType.Cliff)
                {
                    continue;
                }

                int moveCost;
                if(current.HasRoadThroughEdge(d))
                {
                    moveCost = 1;
                }
                else if(current.Walled!=neighbor.Walled)
                {
                    continue;
                }
                else
                {
                    moveCost = edgeType==HexEdgeType.Flat?5:10;
                    moveCost += neighbor.UrbanLevel + neighbor.Farmlevel + neighbor.PlantLevel;
                }

                int distance = current.Distance + moveCost;
                int turn = distance / speed;
                if(turn>currentTurn)
                {
                    distance = turn * speed + moveCost;
                }
                if(neighbor.SearchPhase<searchFrontierPhase)
                {
                    neighbor.SearchPhase = searchFrontierPhase;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates);
                    searchFrontier.Enqueue(neighbor);
                }
                else if(distance<neighbor.Distance)
                {
                    int oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    searchFrontier.Change(neighbor, oldPriority);
                }      
            }
        }
        return false;
 
    }

    void ShowPath(int speed)
    {
        if(currentPathExists)
        {
            HexCell current = currentPathTo;
            while(current!=currentPathFrom)
            {
                int turn = current.Distance / speed;
                current.SetLabel(turn.ToString());
                current.EnableHighlight(Color.white);
                current = current.PathFrom;
            }
            currentPathFrom.EnableHighlight(Color.blue);
            currentPathTo.EnableHighlight(Color.red);
        }
    }

    void ClearPath()
    {
        if (currentPathExists)
        {
            HexCell current = currentPathTo;
            while (current != currentPathFrom)
            {
                
                current.SetLabel(null);
                current.DisableHighlight();
                current = current.PathFrom;
            }
            current.DisableHighlight();
            currentPathExists = false;
        }
        else if(currentPathFrom)
        {
            currentPathTo.DisableHighlight();
            currentPathFrom.DisableHighlight();
        }
        currentPathTo = currentPathFrom = null;
    }

}
