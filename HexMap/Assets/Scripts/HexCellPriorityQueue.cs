using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCellPriorityQueue 
{
    List<HexCell> list = new List<HexCell>();

    int count = 0;

    public int Count
    {
        get
        {
            return count;
        }
    }

    int minimum = int.MaxValue;
    
    public void Enqueue(HexCell cell)
    {
        count += 1;
        int priority = cell.SearchPriority;
        if(priority<minimum)
        {
            minimum = priority;
        }
        while(priority>=list.Count)
        {
            list.Add(null);
        }
        cell.NextWithSamePriority = list[priority];
        list[priority] = cell;
    }

    public HexCell Dequeue()
    {
        count -= 1;
        for(;minimum<list.Count;minimum++)
        {
            HexCell cell = list[minimum];
            if(cell!=null)
            {
                list[minimum] = cell.NextWithSamePriority;
                return cell;
            }
        }
        return null;
    }

    public void Change(HexCell cell,int oldPriority)
    {
        HexCell current = list[oldPriority];
        HexCell next = current.NextWithSamePriority;
        if(current==cell)
        {
            list[oldPriority] = next;
        }
        else
        {
            while(next!=cell)
            {
                current = next;
                next = current.NextWithSamePriority;
            }
            current.NextWithSamePriority = cell.NextWithSamePriority;
        }
        Enqueue(cell);
        count -= 1;
    }

    public void Clear()
    {
        list.Clear();
        count = 0;
        minimum = int.MaxValue;
    }
   
}
