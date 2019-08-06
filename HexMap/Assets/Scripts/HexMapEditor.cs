using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
using System;

public class HexMapEditor : MonoBehaviour
{
   

    public HexGrid hexGrid;

    public Material terrainMaterial;

    int activeElevation;
    int activeWaterLevel;
    int activeTerrainTypeIndex; 

    bool applyColor;
    bool applyElevation;
    bool applyWaterLevel;

    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell,searchFromCell,searchToCell;

    int activeUrbanLevel,activeFarmLevel,activePlantLevel,activeSpecialIndex;

    bool applyUrbanLevel, applyFarmLevel,applyPlantLevel,applySpecialIndex;

    bool editMode;


    private void Awake()
    {
        terrainMaterial.DisableKeyword("GRID_ON");
    }


    enum OptionalToggle
    {
        Ignore,Yes,No
    }

    OptionalToggle riverMode,roadMode,walledMode;


    void Update()
    {
        if (Input.GetMouseButton(0)&& !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
        else
        {
            previousCell = null;
        }
    }

    public void SetWalledMode(int mode)
    {
        walledMode = (OptionalToggle)mode;
    }

    public void SetApplyUrbanLevel(bool toggle)
    {
        applyUrbanLevel = toggle;
    }

    public void SetUrbanLevel(float level)
    {
        activeUrbanLevel = (int)level;
    }

    public void SetApplayFarmLevel(bool toggle)
    {
        applyFarmLevel = toggle;
    }
    public void SetFarmLevel(float  level)
    {
        activeFarmLevel = (int)level;
    }
    public void SetApplayPlantLevel(bool toggle)
    {
        applyPlantLevel = toggle;
    }
    public void SetPlantLevel(float level)
    {
        activePlantLevel = (int)level;
    }

    public void SetApplySpecialIndex(bool toggle)
    {
        applySpecialIndex = toggle;
    }

    public void SetSpecialIndex(float index)
    {
        activeSpecialIndex = (int)index;
    }

    public void SetTerrainTypeIndex(int index)
    {
        activeTerrainTypeIndex = index;
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            
            HexCell currentCell = hexGrid.GetCell(hit.point);
            if(previousCell&&previousCell!=currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }

            if(editMode)
            {
                EditCells(currentCell);
            }
            else if(Input.GetKey(KeyCode.LeftShift)&&searchToCell!=currentCell)
            {
                if(searchFromCell)
                {
                    searchFromCell.DisableHighlight();
                }
                searchFromCell = currentCell;
                searchFromCell.EnableHighlight(Color.blue);
                if(searchToCell)
                {
                    hexGrid.FindPath(searchFromCell, searchToCell);
                }
            }
            else if(searchFromCell&&searchFromCell!=currentCell)
            {
                searchToCell = currentCell;
                hexGrid.FindPath(searchFromCell, searchToCell);
            }
            previousCell = currentCell;

            //hexGrid.ColorCell(hit.point, activeColor);
            //hexGrid.TouchCell(hit.point);
        }
        else
        {
            previousCell = null;
        }
    }

    void ValidateDrag(HexCell currentCell)
    {
        for(dragDirection=HexDirection.NE;
            dragDirection<=HexDirection.NW;
            dragDirection++)
        {
            if(previousCell.GetNeighbor(dragDirection)==currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
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
            if(activeTerrainTypeIndex>=0)
            {
                cell.TerrainTypeIndex = activeTerrainTypeIndex;
            }    
            if (applyElevation)
            {
                cell.Elevation = activeElevation;
            }
            if(applyWaterLevel)
            {
                cell.WaterLevel = activeWaterLevel;
            }
            if(applySpecialIndex)
            {
                cell.SpecialIndex = activeSpecialIndex;
            }
            if(applyUrbanLevel)
            {
                cell.UrbanLevel = activeUrbanLevel;
            }
            if(walledMode!=OptionalToggle.Ignore)
            {
                cell.Walled = walledMode == OptionalToggle.Yes;
            }
            if(applyFarmLevel)
            {
                cell.Farmlevel = activeFarmLevel;
            }
            if(applyPlantLevel)
            {
                cell.PlantLevel = activePlantLevel;
            }
            if(riverMode==OptionalToggle.No)
            {
                cell.RemoveRiver();
            }
            if(roadMode==OptionalToggle.No)
            {
                cell.RemoveRoads();
            }
           if(isDrag)
            {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if(otherCell)
                {
                    if(riverMode==OptionalToggle.Yes)
                    {
                        otherCell.SetOutgoingRiver(dragDirection);
                    }
                    if(roadMode==OptionalToggle.Yes)
                    {
                        otherCell.AddRoad(dragDirection);
                    }
                }
            }
        }
      
        //hexGrid.Refresh();
    }

    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }



    int brushSize;

    public  void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

    public void SetEvelation(float evelation)
    {
        activeElevation = (int)evelation;
        
    }

    public void ShowLabelUI(bool visible)
    {
        hexGrid.ShowLabelUI(visible);
    }

    public void  SetRiverMode(int mode)
    {
        riverMode = (OptionalToggle)mode;
    }
    public  void SetRoadMode(int mode)
    {
        roadMode = (OptionalToggle)mode;
    }

    public void SetApplyWaterLevel(bool toggle)
    {
        applyWaterLevel = toggle;
    }

    public void SetWaterLevel(float  level)
    {
        activeWaterLevel = (int)level;
    }

    public void ShowGrid (bool visible)
    {

        if(visible)
        {
            terrainMaterial.EnableKeyword("GRID_ON");
        }
        else
        {
            terrainMaterial.DisableKeyword("GRID_ON");
        }
    }

    public void SetEditMode(bool toggle)
    {
        editMode = toggle;
        hexGrid.ShowLabelUI(!toggle);
    }

}
