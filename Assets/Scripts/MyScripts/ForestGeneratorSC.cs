using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.XR;

public class ForestGeneratorSC : MonoBehaviour {

public float growDist;

    public float attractDitsMult;
    public float removeDistMult;

    public Vector3 tropismVector = Vector3.zero;

    public int diskPoints = 8;

    public Material treeMat;

    public PointCloud pointCloud = new PointCloud();

    public Material LeaveMaterial;

	public int Freccuency;

	public Transform planeTransform;
	
	private List<Vector3> treePosList = new List<Vector3>();

    private float attractDits;
    private float removeDist;

    private List<TreeSC> trees = new List<TreeSC>();

	private void Awake(){

		float sep = 0.5f;
		
		Vector2 planeCorner0 = new Vector2(-planeTransform.localScale.x/2.0f, -planeTransform.localScale.z/2.0f);
		float planeWidth = planeTransform.localScale.x;
		float planeHeight = planeTransform.localScale.z;
		Vector2 cellSize = new Vector2(planeTransform.localScale.x/Freccuency, planeTransform.localScale.z/Freccuency);
		
		for (int i = 0; i < Freccuency; i++)
		{
			for (int j = 0; j < Freccuency; j++)
			{
				
				float x = Random.Range(planeCorner0.x*10 + i * cellSize.x*10 + sep, 
										planeCorner0.x*10 + (i+1) * cellSize.x*10 - sep);
				float y = 0;
				
				float z = Random.Range(planeCorner0.y*10 + j * cellSize.y*10 + sep, 
										planeCorner0.y*10 + (j+1) * cellSize.y*10 - sep);
				
				treePosList.Add(new Vector3(x, y, z));
			}
		}

		foreach (var treePos in treePosList)
		{
			// Generate the point cloud
			transform.position = treePos;
			
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
			trees.Add(new TreeSC(transform.position, treeMat));

			print("Growing the trunk...");
			trees[trees.Count-1].growTrunk(growDist, attractDits, ref pointCloud.pointList, tropismVector);
        
			print("Growing the tree...");
			trees[trees.Count-1].growTree(growDist, attractDits, removeDist, ref pointCloud.pointList, tropismVector);
			trees[trees.Count-1].nodeRelocation();
			trees[trees.Count-1].generateLeaves(LeaveMaterial);
		}
	}
	
	private void OnDrawGizmos() {
		
		foreach(Vector3 point in treePosList) {
			Gizmos.color = Color.black;
			Gizmos.DrawSphere(point, 5f);
		}
	}
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
