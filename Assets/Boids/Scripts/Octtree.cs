﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Octtree
{
    private int maxObjects = 10;
    private int maxSubdivision = 5;

    private int level;
    public Box box;
    private List<Boid> objects;
    public Octtree[] nodes;

    public Octtree(int _level, Box _newBox)
    {
        this.level = _level;
        this.box = _newBox;
        this.objects = new List<Boid>();
        this.nodes = new Octtree[8];
    }

    public void Clear()
    {
        objects.Clear();

        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                nodes[i].Clear();
                nodes[i] = null;
            }
        }
    }

    private void Split()
    {
        Vector3 subBoxSize = box.Size / 2f;

        nodes[0] = new Octtree(level + 1, new Box(subBoxSize.x, subBoxSize.y, subBoxSize.z, new Vector3(box.Center.x - box.Size.x / 4, box.Center.y - box.Size.y / 4, box.Center.z - box.Size.z / 4)));
        nodes[1] = new Octtree(level + 1, new Box(subBoxSize.x, subBoxSize.y, subBoxSize.z, new Vector3(box.Center.x + box.Size.x / 4, box.Center.y - box.Size.y / 4, box.Center.z - box.Size.z / 4)));
        nodes[2] = new Octtree(level + 1, new Box(subBoxSize.x, subBoxSize.y, subBoxSize.z, new Vector3(box.Center.x + box.Size.x / 4, box.Center.y - box.Size.y / 4, box.Center.z + box.Size.z / 4)));
        nodes[3] = new Octtree(level + 1, new Box(subBoxSize.x, subBoxSize.y, subBoxSize.z, new Vector3(box.Center.x - box.Size.x / 4, box.Center.y - box.Size.y / 4, box.Center.z + box.Size.z / 4)));
        nodes[4] = new Octtree(level + 1, new Box(subBoxSize.x, subBoxSize.y, subBoxSize.z, new Vector3(box.Center.x - box.Size.x / 4, box.Center.y + box.Size.y / 4, box.Center.z - box.Size.z / 4)));
        nodes[5] = new Octtree(level + 1, new Box(subBoxSize.x, subBoxSize.y, subBoxSize.z, new Vector3(box.Center.x + box.Size.x / 4, box.Center.y + box.Size.y / 4, box.Center.z - box.Size.z / 4)));
        nodes[6] = new Octtree(level + 1, new Box(subBoxSize.x, subBoxSize.y, subBoxSize.z, new Vector3(box.Center.x + box.Size.x / 4, box.Center.y + box.Size.y / 4, box.Center.z + box.Size.z / 4)));
        nodes[7] = new Octtree(level + 1, new Box(subBoxSize.x, subBoxSize.y, subBoxSize.z, new Vector3(box.Center.x - box.Size.x / 4, box.Center.y + box.Size.y / 4, box.Center.z + box.Size.z / 4)));

    }

    private int GetIndex(Vector3 _position)
    {
        bool isInTopQuadrant = _position.y > box.Center.y && _position.y < (box.Center.y + box.Size.y / 2);
        bool isInBottomQuadrant = _position.y > (box.Center.y - box.Size.y / 2) && _position.y < box.Center.y;

        bool isInLeftQuadrant = _position.x > (box.Center.x - box.Size.x / 2f) && _position.x < box.Center.x;
        bool isInRightQuadrant = _position.x > box.Center.x && _position.x < (box.Center.x + box.Size.x / 2f);

        bool isInFrontQuadrant = _position.z > (box.Center.z - box.Size.z / 2f) && _position.z < box.Center.z;
        bool isInBackQuadrant = _position.z > box.Center.z && _position.z < (box.Center.z + box.Size.z / 2f);

        if (isInBottomQuadrant)
        {
            if (isInLeftQuadrant)
            {
                if (isInFrontQuadrant)
                {
                    return 0;
                }
                else if (isInBackQuadrant)
                {
                    return 3;
                }
            }
            else if (isInRightQuadrant)
            {
                if (isInFrontQuadrant)
                {
                    return 1;
                }
                else if (isInBackQuadrant)
                {
                    return 2;
                }
            }
        }
        else if (isInTopQuadrant)
        {
            if (isInLeftQuadrant)
            {
                if (isInFrontQuadrant)
                {
                    return 4;
                }
                else if (isInBackQuadrant)
                {
                    return 7;
                }
            }
            else if (isInRightQuadrant)
            {
                if (isInFrontQuadrant)
                {
                    return 5;
                }
                else if (isInBackQuadrant)
                {
                    return 6;
                }
            }
        }

        return -1; //<- no node / return to parent node
    }

    public void Insert(Boid _boid)
    {
        //--if node is not a leaf node
        if (nodes[0] != null)
        {
            int index = GetIndex(_boid.transform.position); //<- Convert boid position into index of quadtree node

            //-- if boid can be put into one specific node
            if (index != -1)
                nodes[index].Insert(_boid);

            return;
        }

        objects.Add(_boid); //<- add boid to the List of Objects

        //-- if the maximum object count has been reached and the node can be split
        if (objects.Count > maxObjects && level < maxSubdivision)
        {
            //-- if there are no child nodes
            if (nodes[0] == null)
                Split();

            int i = 0;
            while (i < objects.Count)
            {
                int index = GetIndex(objects[i].transform.position); //<- Convert boid position into index of quadtree node
                //-- if object can be put into one specific node
                if (index != -1)
                {
                    nodes[index].Insert(objects[i]); //<- insert into child node
                    objects.RemoveAt(i); //<- remove from current node
                }
                else
                    i++;
            }
        }
    }

    public List<Boid> Retrieve(List<Boid> retrievedObjects, Boid _boid)
    {
        int index = GetIndex(_boid.transform.position);
        if(index != -1 && nodes[0] != null)
            nodes[index].Retrieve(retrievedObjects, _boid);

        retrievedObjects = objects;

        return retrievedObjects;
    }

    public void Show()
    {
        Debug.DrawLine(box.Bounds[0], box.Bounds[1], Color.red);
        Debug.DrawLine(box.Bounds[0], box.Bounds[3], Color.red);
        Debug.DrawLine(box.Bounds[0], box.Bounds[4], Color.red);
        Debug.DrawLine(box.Bounds[1], box.Bounds[2], Color.red);
        Debug.DrawLine(box.Bounds[1], box.Bounds[5], Color.red);
        Debug.DrawLine(box.Bounds[2], box.Bounds[3], Color.red);
        Debug.DrawLine(box.Bounds[2], box.Bounds[6], Color.red);
        Debug.DrawLine(box.Bounds[3], box.Bounds[7], Color.red);
        Debug.DrawLine(box.Bounds[4], box.Bounds[7], Color.red);
        Debug.DrawLine(box.Bounds[4], box.Bounds[5], Color.red);
        Debug.DrawLine(box.Bounds[5], box.Bounds[6], Color.red);
        Debug.DrawLine(box.Bounds[6], box.Bounds[7], Color.red);
    }
}