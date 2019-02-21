using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    public Color[] colors;

    public HexGrid hexGrid;

    private Color activeColor;

    int activeElevation;

    bool applyColor;
    bool applyElevation;

    void Awake()
    {
        SelectColor(0);
    }

    void Update()
    {
        if (Input.GetMouseButton(0)&& !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
    }



    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            EditCells(hexGrid.GetCell(hit.point));
            //hexGrid.ColorCell(hit.point, activeColor);
            //hexGrid.TouchCell(hit.point);
        }
    }

    void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        for(int r=0,z=centerZ-brushSize;z<=centerZ;z++,r++)
        {
            for(int x=centerX-r;x<=centerX+brushSize;x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }

        for (int r=0,z=centerZ+brushSize;z>centerZ;z--,r++)
        {
            for(int x=centerX-brushSize;x<=centerX+r;x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    void EditCell(HexCell cell)
    {
        if(cell)
        {
            if (applyColor)
            {
                cell.Color = activeColor;
            }
            if (applyElevation)
            {
                cell.Elevation = activeElevation;
            }
        }
      
        //hexGrid.Refresh();
    }

    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    public void SelectColor(int index)
    {
        applyColor = index >= 0;
        if (applyColor)
        {
            activeColor = colors[index];
        }
    }

    int brushSize;

    public  void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

    public void SetEvelation(float evelation)
    {
        activeElevation = (int)evelation;
        Debug.Log(activeElevation);
    }

    public void ShowLabelUI(bool visible)
    {
        hexGrid.ShowLabelUI(visible);
    }

}
