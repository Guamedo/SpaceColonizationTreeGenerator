using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGeneratorSC : MonoBehaviour {

    public float growDist;

    public float attractDitsMult;
    public float removeDistMult;

    public Vector3 tropismVector = Vector3.zero;

    public int diskPoints = 8;

    public Material treeMat;

    public PointCloud pointCloud = new PointCloud();

    public Material LeaveMaterial;

    private float attractDits;
    private float removeDist;

    private TreeSC tree;
    private bool treeGenerated = false;

    private void Awake() {

        // Generate the point cloud
        print("Generatin point cloud...");
        pointCloud.init(transform);
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        print(pointCloud.pointList.Count + " points generated.");

        // Set points attraction and remove dit
        attractDits = growDist * attractDitsMult;
        removeDist = growDist * removeDistMult;

        // Create the tree
        print("Generating the tree...");
        tree = new TreeSC(transform.position, treeMat);

        print("Growing the trunk...");
        tree.growTrunk(growDist, attractDits, ref pointCloud.pointList, tropismVector);
        
        print("Growing the tree...");
    }

    private void Update() {
        if (tree.canGrow){
            tree.growTreeIteration(growDist, attractDits, removeDist, ref pointCloud.pointList, tropismVector);
        } else if(!treeGenerated)
        {
            treeGenerated = true;
            tree.nodeRelocation();
            tree.generateLeaves(LeaveMaterial);
            pointCloud.pointList.Clear();
        } else
        {
            //tree.updateLeaves(Time.time);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.black;
        
        if (treeGenerated)
        {
            //drawLeaves(tree.root);
        }
        
        foreach(Vector3 point in pointCloud.pointList) {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(point, 0.5f);
        }
    }

    private void drawBranch(BranchSC branch) {
        for(int i = 0; i < branch.childs.Count; i++) {
            Gizmos.DrawLine(branch.pos, branch.childs[i].pos);
            drawBranch(branch.childs[i]);
        }
    }

    private void drawLeaves(BranchSC branch)
    {
        if (branch.hasLeaf)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(branch.pos, 2.0f);
        } else
        {
            for (int i = 0; i < branch.childs.Count; i++)
            {
                drawLeaves(branch.childs[i]);
            }
        }
    }

    /*private void generateMesh(BranchSC branch)
    {
        branch.sortSons();
        
        for(int i = 0; i < branch.childs.Count; i++) {
            GameObject newBranch = new GameObject("Branch");
            newBranch.AddComponent<MeshRenderer>();
            newBranch.AddComponent<MeshFilter>();
            if (branch.childs[i].mesh != null)
            {
                branch.mesh.recalculateMesh(branch.rad, branch.childs[i].rad);
                newBranch.GetComponent<MeshFilter>().mesh = branch.mesh.mesh;
            }
            newBranch.GetComponent<MeshRenderer>().material = treeMat;
            newBranch.transform.parent = transform;
            generateMesh(branch.childs[i]);
        }
    }*/
    
    private void generateMesh(BranchSC branch)
    {
        if (branch.meshList != null)
        {
            for (int i = 0; i < branch.meshList.Count; i++)
            {
                GameObject newBranch = new GameObject("Branch");
                newBranch.AddComponent<MeshRenderer>();
                newBranch.AddComponent<MeshFilter>();
                branch.meshList[i].recalculateMesh(branch.rad, branch.childs[i].rad);
                newBranch.GetComponent<MeshFilter>().mesh = branch.meshList[i].mesh;
                newBranch.GetComponent<MeshRenderer>().material = treeMat;
                newBranch.transform.parent = transform;
            }
            
            for (int i = 0; i < branch.childs.Count; i++)
            {
                generateMesh(branch.childs[i]);
            }
        }
    }

}
