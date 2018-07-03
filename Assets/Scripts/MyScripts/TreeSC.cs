using System.Collections.Generic;
using UnityEngine;

// TreeSC stores a 3D tree model dividen in branches, the trei is
// automatically generated using the Space Colonization algorithm. 

public class TreeSC{

    // The root branch of the tree
    public BranchSC root;
    
    // List of branches that are still growing
    public List<BranchSC> growBranchList;

    // A variable to look if this tree can still grow
    public bool canGrow;
    
    // Game object to store the tree
    public GameObject tree;

    /// <summary>
    /// Constructs a new Tree
    /// </summary>
    /// <param name="pos">The position wher the tree root is going to be placed</param>
    public TreeSC(Vector3 pos, Material mat) {
        tree = new GameObject("Tree");
        tree.transform.position = pos;
        root = new BranchSC(pos, null, true, mat, ref tree);
        growBranchList = new List<BranchSC>();
        canGrow = false;
    }
    
    /// <summary>
    /// Grow the truck of the tree until it reaches the point cloud.
    /// </summary>
    /// <param name="growDist">The distance that grows each new branch</param>
    /// <param name="attractionDist">The maximum distanche at witch a branch can be attracted</param>
    /// <param name="pointList">List of points in the point cloud</param>
    public void growTrunk(float growDist, float attractionDist, ref List<Vector3> pointList) {
        float minDist = Mathf.Infinity;
        bool found = false;

        int timeout = 1000;

        BranchSC currentBranch = this.root;
        
        while (!found && timeout > 0) {
            // Look in the point list if there is any point that can affect 
            // the trunk branches
            for (int i = 0; i < pointList.Count && !found; i++) {
                
                // For each poitn calculate the ditance to the last branch of the trunk
                float dist = Vector3.Distance(currentBranch.pos, pointList[i]);

                // Udate the grow direction of the last branch
                currentBranch.growDir += (pointList[i] - currentBranch.pos).normalized;
                
                if (dist < minDist) {
                    minDist = dist;
                    if (minDist < attractionDist) {
                        found = true;
                    }
                }
            }

            if (!found) {
                BranchSC newBranch = currentBranch.grow(growDist, true);
                currentBranch = newBranch;
                //endedBranchList.Add(growBranchList[growBranchList.Count - 1]);
                //growBranchList.RemoveAt(growBranchList.Count - 1);
                //growBranchList.Add(newBranch);
            } else {
                currentBranch.growDir = Vector3.zero;
                this.growBranchList.Add(currentBranch);
            }
            timeout--;
        }
        if(timeout <= 0) {
            Debug.LogError("Error generating the trunk");
        }
        this.canGrow = true;
    }

    /// <summary>
    /// Grow the tree until all the points in the point cloud are removed, or all the
    /// remaining points are unreachable
    /// </summary>
    /// <param name="growDist">The distance that grows each new branch</param>
    /// <param name="attractionDist">The maximum distanche at witch a branch can be attracted</param>
    /// <param name="removeDist">The maximum distance </param>
    /// <param name="pointList">List of points in the point cloud</param>
    public void growTree(float growDist, float attractionDist, float removeDist ,ref List<Vector3> pointList)
    {
        while (canGrow)
        {
            growTreeIteration(growDist, attractionDist, removeDist, ref pointList);
        }
        // Remove all the remaining points in the point cloud
        pointList.Clear();
        
        // remove all the remaining branches in the growBranchList
        growBranchList.Clear();
    }

    /// <summary>
    /// Grows all the branches in the growBranchLIst in the direction defined by the
    /// points in the point cloud. If a branch can't grow anymore it is removed from
    /// the growBranchList.
    /// If no branch grows in the iteration, canGrow is set to false
    /// </summary>
    /// <param name="growDist">The distance that grows each new branch</param>
    /// <param name="attractionDist">The maximum distanche at witch a branch can be attracted</param>
    /// <param name="removeDist">The maximum distance </param>
    /// <param name="pointList">List of points in the point cloud</param>
    public void growTreeIteration(float growDist, float attractionDist, float removeDist ,ref List<Vector3> pointList) {
        
        if (this.canGrow) {
            
            // Calculate the grow direction of all the branches with the points in 
            // the point cloud
            foreach (Vector3 point in pointList) {
                float minDist = Mathf.Infinity;
                int minDistIndex = -1;
                for (int i = 0; i < this.growBranchList.Count; i++) {
                    float dist = Vector3.Distance(point, this.growBranchList[i].pos);
                    if (dist < minDist) {
                        minDist = dist;
                        minDistIndex = i;
                    }
                }
                if (minDist <= attractionDist && minDistIndex >= 0) {
                    this.growBranchList[minDistIndex].growDir += (point - this.growBranchList[minDistIndex].pos);
                }
            }
            
            // Try to add a new branch to all the branches in the growBranchList
            bool addedNewBranch = false;
            int branchNum = this.growBranchList.Count;
            for (int i = 0; i < branchNum; i++) {
                BranchSC newBranch = this.growBranchList[i].grow(growDist, false);
                if (newBranch == null) {
                    growBranchList.RemoveAt(i);
                    i--;
                    branchNum--;
                } else {
                    growBranchList.Add(newBranch);
                    addedNewBranch = true;
                }
            }
            // If no branch has grown, set canGrow to false
            if(!addedNewBranch) {
                canGrow = false;
            }

            // remove all the points that are in removeDistance range from any of the branches
            for (int i = 0; i < pointList.Count; i++) {
                bool rm = false;
                for (int j = 0; j < growBranchList.Count && !rm; j++) {
                    if (Vector3.Distance(pointList[i], growBranchList[j].pos) <= removeDist) {
                        pointList.RemoveAt(i);
                        rm = true;
                        i--;
                    }
                }
            }
            
            // Recalcualte the radious of all the branches in the tree
            root.calculateRad();
        }
    }

    public void nodeRelocation()
    {
        root.relocateChilds();
        root.recalcualteMesh();
    }

}
