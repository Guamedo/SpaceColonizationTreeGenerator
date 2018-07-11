using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class BranchSC{
    
    
    public Vector3 pos;
    public float rad;
    public Vector3 growDir;
    public int growIteration;
    
    public BranchSC parent;
    public List<BranchSC> childs;

    public bool hasLeaf;
    public List<GameObject> leaves;
    public List<float> leavesTimeOut;
    public bool isTrunk;
    
    public Material branchMaterial;
    public List<BranchMesh> meshList;
    public GameObject go;
    public GameObject tree;

    public BranchSC(Vector3 pos, BranchSC parent, bool isTrunk, Material mat, ref GameObject tree, int growIteration, float initialRad = 0.1f) {
        this.pos = pos;
        this.tree = tree;
        this.parent = parent;
        this.growIteration = growIteration;
        childs = new List<BranchSC>();
        rad = initialRad;
        growDir = new Vector3(0.0f, 0.0f, 0.0f);
        hasLeaf = false;
        meshList = new List<BranchMesh>();
        go = new GameObject("Branch");
        go.transform.position = pos;
        go.transform.parent = tree.transform;
        branchMaterial = mat;
        leaves = new List<GameObject>();
        leavesTimeOut = new List<float>();
    }

    public BranchSC grow(float growDist, bool growTrunk, Vector3 tropism) {
        if(this.growDir.magnitude > 0.5) {

            Vector3 newPos = this.pos + (this.growDir.normalized + tropism).normalized * growDist;
            foreach(BranchSC child in this.childs) {
                if(Vector3.Distance(child.pos, newPos) <= 0.5) {
                    this.growDir = new Vector3(0.0f, 0.0f, 0.0f);
                    return null;
                }
            }

            if (parent != null)
            {
                if(Vector3.Distance(parent.pos, newPos) <= 0.5) {
                    this.growDir = new Vector3(0.0f, 0.0f, 0.0f);
                    return null;
                }
            }
            
            BranchSC newBranch = new BranchSC(newPos, this, growTrunk, branchMaterial, ref tree, growIteration + 1);
            this.growDir = new Vector3(0.0f, 0.0f, 0.0f);
            this.childs.Add(newBranch);

            BranchMesh newMesh;
            if (this.parent != null){
                newMesh = new BranchMesh(this.pos, 
                                        this.pos - this.parent.pos,
                                        newBranch.pos - this.pos,
                                        this.rad,
                                        newBranch.rad);
            }else{
                newMesh = new BranchMesh(this.pos,
                                        this.pos - tree.transform.position,
                                        newBranch.pos - this.pos,
                                        this.rad,
                                        newBranch.rad);    
            }
            
            GameObject newBranchObject = new GameObject("BranchChild");
            newBranchObject.AddComponent<MeshRenderer>();
            newBranchObject.AddComponent<MeshFilter>();
            newBranchObject.AddComponent<MeshCollider>();
            newBranchObject.GetComponent<MeshFilter>().mesh = newMesh.mesh;
            newBranchObject.GetComponent<MeshRenderer>().material = branchMaterial;
            newBranchObject.GetComponent<MeshCollider>().sharedMesh = newMesh.mesh;
            newBranchObject.transform.parent = go.transform;
            meshList.Add(newMesh);
            
            return newBranch;
        } else {
            this.growDir = new Vector3(0.0f, 0.0f, 0.0f);
            return null;
        }
    }

    public float calculateRad(int maxGrowIteration, float n = 2.0f) {
        if(this.childs.Count == 0) {
            return this.rad + (1 - growIteration/maxGrowIteration)*0.0f;
        } else {
            float radVal = 0.0f;
            foreach(BranchSC child in this.childs) {
                radVal += Mathf.Pow(child.calculateRad(maxGrowIteration, n), n);
            }
            this.rad = Mathf.Pow(radVal, 1.0f / n);
            for (int i = 0; i < meshList.Count; i++)
            {
                meshList[i].recalculateMesh(rad, childs[i].rad);
            }
            return this.rad + (1 - growIteration/maxGrowIteration)*0.0f;
        }
    }

    static int sortByRad(BranchSC b1, BranchSC b2) {
        return -b1.rad.CompareTo(b2.rad);
    }

    public void sortSons() {
        this.childs.Sort(sortByRad);
    }

    public void relocateChilds()
    {

        for (int i = 0; i < childs.Count; i++)
        {
            childs[i].relocateChilds();
            Vector3 dir = pos - childs[i].pos;
            childs[i].pos += dir / 2;
        }
    }

    public void recalcualteMesh()
    {
        for (int i = 0; i < childs.Count; i++)
        {
            if (parent != null)
            {
                meshList[i].startRing = new Ring(pos, pos - parent.pos, rad);
                meshList[i].endRing = new Ring(childs[i].pos, childs[i].pos - pos, childs[i].rad);
                meshList[i].recalculateMesh();
            } else
            {
                meshList[i].startRing = new Ring(pos, pos - tree.transform.position, rad);
                meshList[i].endRing = new Ring(childs[i].pos, childs[i].pos - pos, childs[i].rad);
                meshList[i].recalculateMesh();
            }
            childs[i].recalcualteMesh();
        }
    }

    public void generateLeaves(Material mat)
    {
        if (childs.Count == 0)
        {
            for (int i = 0; i < (int)Random.RandomRange(2, 10); i++)
            {
                TwoSideQuad quad = new TwoSideQuad(pos, 4.0f);
                leaves.Add(quad.generateQuad(mat));
                leavesTimeOut.Add(Random.Range(5.0f, 60.0f));
            }
            hasLeaf = true;
        } else
        {
            if (!isTrunk && rad <= 0.1f)
            {
                for (int i = 0; i < (int)Random.RandomRange(0, 4); i++)
                {
                    TwoSideQuad quad = new TwoSideQuad(pos, 4.0f);
                    leaves.Add(quad.generateQuad(mat));
                    leavesTimeOut.Add(Random.Range(5.0f, 60.0f));
                }
                hasLeaf = true;
            }
            for (int i = 0; i < childs.Count; i++)
            {
                childs[i].generateLeaves(mat);
            }
        }
    }

    public void updateLeaves(float t)
    {
        for (int i = 0; i < leaves.Count; i++)
        {
            if (leavesTimeOut[i] <= t)
            {
                leaves[i].AddComponent<Rigidbody>();
                leavesTimeOut[i] = Mathf.Infinity;
            }
        }

        for (int i = 0; i < childs.Count; i++)
        {
            childs[i].updateLeaves(t);
        }
    }
}
