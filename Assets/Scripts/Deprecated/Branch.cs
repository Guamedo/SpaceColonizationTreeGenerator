using System.Collections.Generic;
using UnityEngine;

public class Branch {
    public Vector3 position;
    public Branch parent;
    public List<Branch> sons;
    public float rad;
    public List<Vector3> atractionPoints;
    public bool canGrow;

    public Branch(Vector3 pos, Branch p) {
        position = pos;
        parent = p;
        rad = 1.0f;
        sons = new List<Branch>();
        atractionPoints = new List<Vector3>();
        canGrow = true;
    }

    public Branch growUp(float D) {
        Vector3 dir = new Vector3(0.0f, 1.0f, 0.0f);
        Vector3 n = dir.normalized;
        Vector3 newPos = position + n * D;
        Branch newBranch = new Branch(newPos, this);
        this.sons.Add(newBranch);
        return newBranch;
    }

    public Branch growUpRandom(float D) {
        Vector3 dir = new Vector3(Random.Range(-0.2f, 0.2f), 1.0f, Random.Range(-0.2f, 0.2f));
        Vector3 n = dir.normalized;
        Vector3 newPos = position + n * D;
        Branch newBranch = new Branch(newPos, this);
        this.sons.Add(newBranch);
        return newBranch;
    }

    public Branch grow(float D) {
        if (atractionPoints.Count > 0) {
            Vector3 dir = atractionPoints[0] - position;
            for (int i = 1; i < atractionPoints.Count; i++) {
                dir += (atractionPoints[i] - position);
            }
            this.atractionPoints.Clear();
            Vector3 n = dir.normalized;
            Vector3 newPos = position + n * D;

            if (parent != null) {
                if (Vector3.Distance(newPos, parent.position) <= 0.5 || Vector3.Distance(newPos, position) <= 0.5) {
                    return null;
                }
            }

            for (int i = 1; i < sons.Count; i++) {
                if (Vector3.Distance(newPos, sons[i].position) <= 0.5) {
                    return null;
                }
            }

            Branch newBranch = new Branch(newPos, this);
            sons.Add(newBranch);
            return newBranch;
        } else {
            return null;
        }
    }

    public float calculateRad() {
        if (sons.Count > 0) {
            float sum = 0;
            for (int i = 0; i < sons.Count; i++) {
                sum += Mathf.Pow(sons[i].calculateRad(), 2.0f);
            }
            rad = Mathf.Pow(sum, 1.0f / 2.0f);
            return rad;
        } else {
            rad = 0.05f;
            return rad;
        }
    }

    static int sortByRad(Branch b1, Branch b2) {
        return -b1.rad.CompareTo(b2.rad);
    }

    public void sortSons() {
        sons.Sort(sortByRad);
    }
}
