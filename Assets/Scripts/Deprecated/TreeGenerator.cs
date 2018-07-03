using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TreeGenerator : MonoBehaviour {

    public class Disk{
        public List<Vector3> points;
        GameObject disk;

        public Disk(Vector3 pos, Vector3 dir, float rad, int pointsNumber = 8) {
            points = new List<Vector3>();
            disk = new GameObject();
            List<GameObject> gList = new List<GameObject>();
            for (int i = 0; i < pointsNumber; i++) {
                float x = Mathf.Cos(i * ((Mathf.PI * 2) / pointsNumber)) * rad;
                float z = Mathf.Sin(i * ((Mathf.PI * 2) / pointsNumber)) * rad;
                GameObject s = new GameObject();
                s.transform.parent = disk.transform;
                s.transform.position = new Vector3(x, 0.0f, z);
                s.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                gList.Add(s);
            }
            disk.transform.position = pos;
            disk.transform.up = dir.normalized;
            for (int i = 0; i < gList.Count; i++) {
                points.Add(gList[i].transform.position);
                Destroy(gList[i]);
            }
            Destroy(disk);
        }
    }

    public List<Branch> B = new List<Branch>();
    public Branch root;

    public float gDist = 1.0f;
    public float iDistMult = 18;
    public float RDistMult = 2;

    private float iDist;
    private float rDist;

    public int diskPoints = 8;

    public Material treeMat;

    public PointCloud pointCloud;// = new PointCloud();

    private int grow = 0;
    private bool draw = false;
    private int t = 1000;

    // Use this for initialization
    void Start() {

        // Generate the atraction points
        //cloud = new PointsCloud(pNumber);
        print("Generatin point cloud...");
        pointCloud = new PointCloud();
        pointCloud.init(transform);
        print(pointCloud.pointList.Count.ToString() + " points generated.");

        //Init tree parameters
        iDist = gDist * iDistMult;
        rDist = gDist * RDistMult;

        //Add the first element of the tree
        root = new Branch(new Vector3(0.0f, 0.0f, 0.0f), null);
        B.Add(new Branch(new Vector3(0.0f, 0.0f, 0.0f), null));

        print("Growing the trunk...");
        growTrunk();

        print("Growing the tree...");
    }

    // Update is called once per frame
    void Update() {

        if (grow == 0 && t > 0) {
            int sBranch = B.Count;
            growTree();
            int eBranch = B.Count;
            if (sBranch == eBranch) {
                print("Generating the mesh...");
                grow = 1;
            }
            t--;
        }else if (grow == 1) {
            pointCloud.pointList.Clear();
            B[0].calculateRad();
            nodeRelocation();
            shortSons();
            generateMesh();
            grow = 2;
            print("Tree generation finished.");
        }
    }

    private void OnDrawGizmos() {

        /*for (int i = 0; i < pointCloud.pointList.Count; i++) {
            Gizmos.DrawSphere(pointCloud.pointList[i], 1);
        }*/

        if (grow < 2) {
            for (int i = 0; i < B.Count; i++) {
                if (B[i].parent != null) {
                    Gizmos.color = Color.black;
                    Gizmos.DrawLine(B[i].position, B[i].parent.position);
                }
            }
        }
    }

    public void growTrunk() {

        int branchIndex = 0;
        bool found = false;
        while (!found) {
            for (int i = 0; i < pointCloud.pointList.Count && !found; i++) {
                if (Vector3.Distance(B[branchIndex].position, pointCloud.pointList[i]) < iDist) {
                    found = true;
                }
            }
            if (!found) {
                B[branchIndex].atractionPoints = new List<Vector3>(pointCloud.pointList);
                B.Add(B[branchIndex].grow(gDist));
                B[branchIndex].canGrow = false;
                branchIndex++;
            }
        }
    }

    public void growTree() {

        for (int i = 0; i < pointCloud.pointList.Count; i++) {
            float distToNextB = Mathf.Infinity;
            int nextBranchI = -1;
            for (int j = 0; j < B.Count; j++) {
                if (B[j].canGrow) {
                    float dist = Vector3.Distance(pointCloud.pointList[i], B[j].position);
                    if (dist < distToNextB) {
                        distToNextB = dist;
                        nextBranchI = j;
                    }
                }
            }
            if (distToNextB <= iDist) {
                B[nextBranchI].atractionPoints.Add(pointCloud.pointList[i]);
            }
        }

        int bLength = B.Count;
        for (int i = 0; i < bLength; i++) {
            if (B[i].canGrow) {
                Branch newBranch = B[i].grow(gDist);
                if (newBranch != null) {
                    B.Add(newBranch);
                } else {
                    B[i].canGrow = false;
                }
            }
        }

        for (int i = 0; i < pointCloud.pointList.Count; i++) {
            bool rm = false;
            for (int j = 0; j < B.Count && !rm; j++) {
                if(Vector3.Distance(pointCloud.pointList[i], B[j].position) <= rDist) {
                    pointCloud.pointList.RemoveAt(i);
                    rm = true;
                    i--;
                }
            }
        }
    }

    public void shortSons() {
        for (int i = 0; i < B.Count; i++) {
            B[i].sortSons();
        }
    }

    public void nodeRelocation() {
        for (int i = 1; i < B.Count; i++) {
            Vector3 dir = (B[i].parent.position - B[i].position).normalized;
            float dist = Vector3.Distance(B[i].parent.position, B[i].position);
            B[i].position += dir * (dist / 2);
        }
    }

    public void generateMesh() {

        GameObject tree = new GameObject();
        tree.name = "Tree";
        //int diskPoints = 8;
        for (int j = 0; j < B.Count; j++) {
            for (int k = 0; k < B[j].sons.Count; k++) {

                GameObject go = new GameObject();
                go.name = "Branch";
                go.transform.parent = tree.transform;
                go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();

                Mesh mesh = new Mesh();
                go.GetComponent<MeshFilter>().mesh = mesh;

                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                mr.material = treeMat;

                if (B[j].sons[k].sons.Count > 0) {

                    Disk d0 = new Disk(B[j].position, B[j].sons[k].position - B[j].position, B[j].rad, diskPoints);
                    Disk d1 = new Disk(B[j].sons[k].position, B[j].sons[k].sons[0].position - B[j].sons[k].position, B[j].sons[k].rad, diskPoints);

                    Vector3[] vertices = new Vector3[(diskPoints+1) * 2];
                    Vector3[] normals = new Vector3[(diskPoints+1) * 2];
                    Vector2[] uv = new Vector2[(diskPoints + 1) * 2];
                    for (int i = 0; i < (diskPoints+1); i++) {
                        vertices[i] = d0.points[i % diskPoints];
                        vertices[i + (diskPoints+1)] = d1.points[i % diskPoints];

                        normals[i] = d0.points[i % diskPoints] - B[j].position;
                        normals[i + (diskPoints + 1)] = d1.points[i % diskPoints] - B[j].sons[k].position;

                        uv[i] = new Vector2((float)i/(float)diskPoints, 0.0f);
                        uv[i + (diskPoints + 1)] = new Vector2((float)i / (float)diskPoints, 1.0f);
                    }
                    mesh.vertices = vertices;
                    mesh.normals = normals;
                    mesh.uv = uv;

                    int[] tri = new int[6 * diskPoints];
                    int face = 0;
                    for (int i = 0; i < diskPoints; i++) {
                        tri[face] = (i + 1);
                        tri[face + 1] = i;
                        tri[face + 2] = (diskPoints+1) + i;

                        tri[face + 3] = ((i + 1)) + (diskPoints + 1);
                        tri[face + 4] = (i + 1);
                        tri[face + 5] = (diskPoints+1) + i;

                        face += 6;
                    }
                    mesh.triangles = tri;                    

                } else {
                    Disk d0 = new Disk(B[j].position, B[j].sons[k].position - B[j].position, B[j].rad, diskPoints);
                    Disk d1 = new Disk(B[j].position, B[j].sons[k].position - B[j].position, B[j].sons[k].rad, diskPoints);

                    Vector3[] vertices = new Vector3[diskPoints * 2];
                    for (int i = 0; i < diskPoints; i++) {
                        vertices[i] = d0.points[i];//  diskList[j].points[i];
                    }
                    for (int i = 0; i < diskPoints; i++) {
                        vertices[i + diskPoints] = d1.points[i];//   diskList[j + 1].points[i];
                    }
                    mesh.vertices = vertices;

                    int[] tri = new int[6 * diskPoints + 3 * (diskPoints - 2)];
                    int cosa = 0;
                    for (int i = 0; i < diskPoints; i++) {
                        tri[cosa] = (i + 1) % diskPoints;
                        tri[cosa + 1] = i;
                        tri[cosa + 2] = diskPoints + i;

                        tri[cosa + 3] = ((i + 1) % diskPoints) + diskPoints;
                        tri[cosa + 4] = (i + 1) % diskPoints;
                        tri[cosa + 5] = diskPoints + i;

                        cosa += 6;
                    }

                    for (int i = 1; i < diskPoints - 1; i++) {
                        tri[cosa] = i+1;
                        tri[cosa + 1] = i;
                        tri[cosa + 2] = 0;
                        cosa += 3;
                    }

                    mesh.triangles = tri;

                    mesh.RecalculateNormals();                  
                }
            }            
        }

        /*MeshFilter[] meshFilters = tree.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
        foreach (Transform child in tree.transform) {
            GameObject.Destroy(child.gameObject);
        }
        tree.AddComponent<MeshFilter>();
        tree.AddComponent<MeshRenderer>();
        tree.GetComponent<MeshFilter>().mesh = new Mesh();
        tree.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        MeshUtility.Optimize(tree.GetComponent<MeshFilter>().mesh);

        GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Material diffuse = primitive.GetComponent<MeshRenderer>().sharedMaterial;
        DestroyImmediate(primitive);

        MeshRenderer mr1 = tree.GetComponent<MeshRenderer>();
        mr1.material = treeMat;*/
    }
}
