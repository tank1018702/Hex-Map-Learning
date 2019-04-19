using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HexCell : MonoBehaviour
{

    public HexGridChunk chunk;
    public HexCoordinates coordinates;

    Color color;
    public Color Color
    {
        get
        {
            return color;
        }
        set
        {
            if (color == value) { return; }
            color = value;
            Refresh();
        }
    }

    public RectTransform uiRect;

    [SerializeField]
    HexCell[] neighbors;



    private int elevation = int.MinValue;
    public int Elevation
    {
        get
        {
            return elevation;
        }
        set
        {
            
            if (elevation == value) { return; }
      
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.evevationPerturbStrength;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;

            //if(HasOutgoingRiver&&elevation<GetNeighbor(outgoingRiver).elevation)
            //{
            //    RemoveOutgoingRiver();
            //}
            //if(hasIncomingRiver&&elevation>GetNeighbor(incomingRiver).elevation)
            //{
            //    RemoveIncomingRiver();
            //}
            ValidateRivers();

            for(int i=0;i<roads.Length;i++)
            {
                if(roads[i]&&GetElevationDifference((HexDirection)i)>1)
                {
                    SetRoad(i, false);
                }
            }

            Refresh();
        }
    }

    public Vector3 Position
    {
        get
        {
            return transform.localPosition;
        }
    }
    #region feature
    public int UrbanLevel
    {
        get
        {
            return urbanLevel;
        }
        set
        {
            if(urbanLevel!=value)
            {
                urbanLevel = value;
                RefreshSelfOnly();
            }
        }
    }

    

    public int Farmlevel
    {
        get
        {
            return farmLevel;
        }
        set
        {
            if(farmLevel!=value)
            {
                farmLevel = value;
                RefreshSelfOnly();
            }
        }
    }

    public int PlantLevel
    {
        get
        {
            return plantLevel;
        }
        set
        {
            if(plantLevel!=value)
            {
                plantLevel = value;
                RefreshSelfOnly();
            }
        }
    }

    int urbanLevel,farmLevel,plantLevel;

    public bool Walled
    {
        get
        {
            return walled;
        }
        set
        {
            if(walled!=value)
            {
                walled = value;   
                Refresh();
            }
        }
    }
    bool walled;

    #endregion

    #region roads
    [SerializeField]
    bool[] roads;

    public bool HasRoadThroughEdge(HexDirection direction)
    {
        return roads[(int)direction];
    }

    public bool HasRoads
    {
        get
        {
            for(int i=0;i<roads.Length;i++)
            {
                if(roads[i])
                {
                    return true;
                }
            }
            return false;
        }
    }

    public void RemoveRoads()
    {
        for (int i = 0; i < neighbors.Length; i++)
        {
            if(roads[i])
            {
                SetRoad(i, false);            
            }
        }
    }

    public void AddRoad(HexDirection direction)
    {
        if(!roads[(int)direction]&&
            !HasRiverThroughEdge(direction)&&
            GetElevationDifference(direction)<=1)
        {
            SetRoad((int)direction, true);
        }
    }

    void SetRoad(int index,bool state)
    {
        roads[index] = state;
        neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
        neighbors[index].RefreshSelfOnly();
        RefreshSelfOnly();
    }

    public int GetElevationDifference(HexDirection direction)
    {
        int difference = elevation - GetNeighbor(direction).elevation;
        return Mathf.Abs(difference);
    }

    #endregion


    #region rivers
    [SerializeField]
    bool hasIncomingRiver, hasOutgoingRiver;
    [SerializeField]
    HexDirection incomingRiver, outgoingRiver;

    public float StreamBedY
    {
        get
        {
            return (elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep;
        }
    }

    public bool HasIncomingRiver
    {
        get
        {
            return hasIncomingRiver;
        }
    }

    public bool HasOutgoingRiver
    {
        get
        {
            return hasOutgoingRiver;
        }
    }

    public HexDirection IncomingRiver
    {
        get
        {
            return incomingRiver;
        }
    }

    public HexDirection OutgoingRiver
    {
        get
        {
            return outgoingRiver;
        }
    }

    public bool HasRiver
    {
        get
        {
            return hasIncomingRiver || hasOutgoingRiver;
        }
    }

    public HexDirection RiverBeginOrEndDirection
    {
        get
        {
            return hasIncomingRiver ? incomingRiver : outgoingRiver;
        }
    }

    public bool HasRiverBeginOrEnd
    {
        get
        {
            return hasIncomingRiver != hasOutgoingRiver;
        }
    }

    public float RiverSurfaceY
    {
        get
        {
            return (elevation + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep;
        }
            
    }


    public bool HasRiverThroughEdge(HexDirection direction)
    {
        return hasIncomingRiver && incomingRiver == direction ||
               hasOutgoingRiver && outgoingRiver == direction;
    }

    public void RemoveOutgoingRiver()
    {
        if (!hasOutgoingRiver) { return; }

        hasOutgoingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(outgoingRiver);
        neighbor.hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveIncomingRiver()
    {
        if (!hasIncomingRiver) { return; }

        hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(incomingRiver);
        neighbor.hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveRiver()
    {
        RemoveIncomingRiver();
        RemoveOutgoingRiver();
    }

    public void SetOutgoingRiver(HexDirection direction)
    {
        if (hasOutgoingRiver && outgoingRiver == direction) { return; }

        HexCell neighbor = GetNeighbor(direction);
        if (!IsValidRiverDestination(neighbor) ){ return; }

        RemoveOutgoingRiver();
        if(HasIncomingRiver&&incomingRiver==direction)
        {
            RemoveIncomingRiver();
        }

        hasOutgoingRiver = true;
        outgoingRiver = direction;
        //RefreshSelfOnly();

        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = direction.Opposite();
        //neighbor.RefreshSelfOnly();
        SetRoad((int)direction, false);
    }


    void RefreshSelfOnly()
    {
        chunk.Refresh();
    }

    void ValidateRivers()
    {
        if(hasOutgoingRiver&&!IsValidRiverDestination(GetNeighbor(OutgoingRiver)))
        {
            RemoveOutgoingRiver();
        }
        if(HasIncomingRiver&&!GetNeighbor(incomingRiver).IsValidRiverDestination(this))
        {
            RemoveIncomingRiver();
        }
    }

    #endregion

    #region water

    int waterLevel;

    public int WaterLevel
    {
        get
        {
            return waterLevel;
        }
        set
        {
            if(waterLevel==value)
            {
                return;
            }
            waterLevel = value;
            ValidateRivers();
            Refresh();
        }
    }

    public bool isUnderwater
    {
        get
        {
            return waterLevel > elevation;
        }
    }


    public float WaterSurfaceY
    {
        get
        {
            return (waterLevel + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep;
        }
    }

    bool IsValidRiverDestination(HexCell neighbor)
    {
        return neighbor && (elevation >= neighbor.elevation || waterLevel == neighbor.elevation);
    }


    #endregion

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction,HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);
    }
    public HexEdgeType GetEdgeType(HexCell otherCell)
    {
        return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
    }

    void Refresh()
    {
        if(chunk)
        {
            chunk.Refresh();
            for(int i=0;i<neighbors.Length;i++)
            {
                HexCell neighbor = neighbors[i];
                if(neighbor!=null&&neighbor.chunk!=chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
        
    }
}
