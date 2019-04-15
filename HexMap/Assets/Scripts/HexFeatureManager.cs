
using UnityEngine;

public class HexFeatureManager : MonoBehaviour
{
    public HexFeatureCollection[] urbanCollections,farmCollections,plantCollections;

    Transform container;

    public void Clear()
    {
        if(container)
        {
            Destroy(container.gameObject);
        }
        container = new GameObject("Features Container").transform;
        container.SetParent(transform, false);
    }

    public void Apply()
    {

    }
   
    public void AddFeature(HexCell cell, Vector3 position)
    {
        HexHash hash = HexMetrics.SampleHashGrid(position);
      
       
        Transform prefab = PickPrefab(urbanCollections, cell.UrbanLevel, hash.a,hash.d);
        Transform otherPrefab = PickPrefab(farmCollections, cell.Farmlevel, hash.b, hash.d);

        float usedHash = hash.a;
        if(prefab)
        {
            if(otherPrefab&&hash.b<hash.a)
            {
                prefab = otherPrefab;
                usedHash = hash.b;
            }
        }
        else if(otherPrefab)
        {
            prefab = otherPrefab;
        }
        otherPrefab = PickPrefab(plantCollections, cell.PlantLevel, hash.c, hash.d);

        if(prefab)
        {
            if(otherPrefab&&hash.c<usedHash)
            {
                prefab = otherPrefab;
            }
        }
        else if(otherPrefab)
        {
            prefab = otherPrefab;
        }
        else
        {
            return;
        }




        Transform instance = Instantiate(prefab);
        position.y += instance.localScale.y * 0.5f;
        instance.localPosition = position;
        instance.localRotation = Quaternion.Euler(0f, 360f * hash.e, 0f);
        instance.SetParent(container, false);
    }

    Transform PickPrefab(HexFeatureCollection[] collection ,int level,float hash, float choice)
    {
        
        if(level>0)
        {
            float[] thresholds = HexMetrics.GetFeatureThresholds(level - 1);
            for(int i=0;i<thresholds.Length;i++)
            {
                if(hash<thresholds[i])
                {

                    return collection[i].Pick(choice);
                }
            }
        }
        return null;
    }
}
