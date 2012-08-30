using System.Linq;
using UnityEngine;

public class Node<T> {
    private readonly Node<T>[] childNodes;
    //private List<T> items;

    public Node()
    {
        //items = new List<T>();
        childNodes = new Node<T>[8];
    }

    public bool HasChildren() {
        for (int i = 0; i < 8; i++) {
            if (childNodes[i] != null)
                return true;
        }
        return false;
    }

    public Node<T> GetChild(int i) {
        return childNodes[i];
    }

    public Node<T> CreateNode(int i) {
        return childNodes[i] ?? (childNodes[i] = new Node<T>());
    }
}

public class Octree<T> {
    private readonly Vector3 min;
    private readonly Vector3 max;
    public Node<T> rootNode;
    public bool updated = true;

    public Octree(Vector3 min, Vector3 max) {
        this.min = min;
        this.max = max;

        rootNode = new Node<T>();
    }

    public Vector3 GetMin() {
        return min;
    }

    public Vector3 GetSize() {
        return max - min;
    }

    public void CreateNode(params int[] p) {
        p.Aggregate(rootNode, (current1, t) => current1.CreateNode(t));
    }

    public Vector3 GetPos() {
        return (max + min) / 2;
    }
}