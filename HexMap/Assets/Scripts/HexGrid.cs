using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    public int width = 6;
    public int height = 6;

    public HexCell cellPrefab;

    public Text cellLabelPrefab;

    Canvas gridCanvas;

    HexMesh hexMesh;

   

    HexCell[] cells;

    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[width*height];

        
        for(int z=0,i=0; z<height;z++)
        {
            for(int x=0;x<width;x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }
    private void Start()
    {
        hexMesh.TriangulateAll(cells);
    }

    void CreateCell(int x,int z,int i)
    {
        Vector3 position;
        position.x = (x+z*0.5f-z/2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);



        HexCell cell = cells[i] = Instantiate(cellPrefab);

        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;

        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.color = Color.white;

        Text label = Instantiate(cellLabelPrefab);
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);

        label.text = cell.coordinates.ToStringOnSpearateLines();

    }

    public void ColorCell(Vector3 position,Color color)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
        HexCell cell = cells[index];
        cell.color = color;
        hexMesh.TriangulateAll(cells);

        Debug.Log("touched at " + coordinates.ToString());
    }


}
