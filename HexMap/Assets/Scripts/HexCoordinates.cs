
using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
    [SerializeField]
    private int x, z;

    public int X
    {
        get { return x; }

    }
    public int Z
    {
        get { return z; }
    }
   

    public int Y
    {
        get
        {
            return -X - Z;
        }
    }

  

    public HexCoordinates(int x, int z)
    {
        this.x = x;
        this.z= z;
    }

    public static HexCoordinates FromOffsetCoordinates(int x, int z)
    {
        return new HexCoordinates(x - z / 2, z);
    }

    public override string ToString()
    {
        return "(" + X.ToString() + "," + Y.ToString()+","+ Z.ToString() + ")";
    }

    public string ToStringOnSpearateLines()
    {
        return X.ToString() + "\n"+ Y.ToString() +"\n" + Z.ToString();
    }

    public static HexCoordinates FromPosition(Vector3  position)
    {
        float x = position.x / (HexMetrics.innerRadius * 2);
        float y = -x;
        float offset = position.z / (HexMetrics.outerRadius * 3f);
        x -= offset;
        y -= offset;

        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x - y);

        if(iX+iY+iZ!=0)
        {
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x - y - iZ);

            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        return new HexCoordinates(iX, iZ);
    }
}

public struct EdgeVertices
{
    public Vector3 v1, v2, v3, v4;

    public EdgeVertices(Vector3 corner1,Vector3 corner2)
    {
        v1 = corner1;
        v2 = Vector3.Lerp(corner1, corner2, 1f / 3f);
        v3 = Vector3.Lerp(corner1, corner2, 2f / 3f);
        v4 = corner2;
    }

    public static EdgeVertices TerraceLerp(EdgeVertices a,EdgeVertices b,int t)
    {
        EdgeVertices result;
        result.v1 = HexMetrics.TerraceLerp(a.v1, b.v1, t);
        result.v2 = HexMetrics.TerraceLerp(a.v2, b.v2, t);
        result.v3 = HexMetrics.TerraceLerp(a.v3, b.v3, t);
        result.v4 = HexMetrics.TerraceLerp(a.v4, b.v4, t);

        return result;
    }
}

