using System.Collections.Generic;
using UnityEngine;

public enum PointCloudMode { fill, surround, vertex }

[System.Serializable]
public class PointCloud {

    public int numberOfPoints = 1000;

    public PointCloudMode mode = PointCloudMode.fill;
    public float surroundArea = 10.0f;

    private GameObject[] shapes;

    private Vector3 center;
    private float[] boxSize = new float[3];
    private float rad;

    public List<Vector3> pointList = new List<Vector3>();

    // Use this for initialization
    public PointCloud() {
        
    }

    public void init(Transform t) {
        int numOfChilds = t.childCount;
        int i = 0;
        foreach (Transform child in t) {
            GameObject go = child.gameObject;
            MeshRenderer mr = go.GetComponent<MeshRenderer>();

            if (mode == PointCloudMode.vertex) {
                generatePointsFromVertex(child);                
            } else {             

                Collider collider = go.GetComponent<Collider>();

                center = mr.bounds.center;
                rad = Vector3.Distance(center, mr.bounds.max) * (Mathf.Sqrt(2.0f) / 2.0f);

                Vector3 p0 = center;
                Vector3 p1 = mr.bounds.max;

                if (mode == PointCloudMode.fill) {

                    boxSize[0] = Vector3.Distance(center, new Vector3(p1.x, p0.y, p0.z));
                    boxSize[1] = Vector3.Distance(center, new Vector3(p0.x, p1.y, p0.z));
                    boxSize[2] = Vector3.Distance(center, new Vector3(p0.x, p0.y, p1.z));

                    collider.enabled = true;

                    while (pointList.Count < i * (float)numberOfPoints / (float)numOfChilds + (float)numberOfPoints / (float)numOfChilds) {
                        generatePointInShape();
                    }

                    collider.enabled = false;

                } else {
                    boxSize[0] = Vector3.Distance(center, new Vector3(p1.x, p0.y, p0.z)) + surroundArea;
                    boxSize[1] = Vector3.Distance(center, new Vector3(p0.x, p1.y, p0.z)) + surroundArea;
                    boxSize[2] = Vector3.Distance(center, new Vector3(p0.x, p0.y, p1.z)) + surroundArea;

                    collider.enabled = true;

                    while (pointList.Count < i * (float)numberOfPoints / (float)numOfChilds + (float)numberOfPoints / (float)numOfChilds) {
                        generatePointArroundShape();
                    }

                    collider.enabled = false;
                }
                
            }
            i++;
            mr.enabled = false;
        }
    }

    private void generatePointInShape() {
        Vector3 pos = new Vector3(Random.Range(-boxSize[0], boxSize[0]),
                                  Random.Range(-boxSize[1], boxSize[1]),
                                  Random.Range(-boxSize[2], boxSize[2])) + center;

        RaycastHit[] hits;
        RaycastHit[] hitsR;

        float diag = (2.0f * rad) / Mathf.Sqrt(2.0f);
        float dist = Vector3.Distance(center, pos);

        float length = diag - dist + 10.0f;

        Vector3 externPoint = pos + (pos - center).normalized * length;

        hits = Physics.RaycastAll(pos, externPoint - pos, length);
        hitsR = Physics.RaycastAll(externPoint, pos - externPoint, length);

        if ((hits.Length + hitsR.Length) % 2 != 0) {
            pointList.Add(pos);
        }
    }

    public void generatePointArroundShape() {
        Vector3 pos = new Vector3(Random.Range(-boxSize[0], boxSize[0]),
                                  Random.Range(-boxSize[1], boxSize[1]),
                                  Random.Range(-boxSize[2], boxSize[2])) + center;

        RaycastHit[] hits;
        RaycastHit[] hitsR;

        float diag = (2.0f * rad) / Mathf.Sqrt(2.0f);
        float dist = Vector3.Distance(center, pos);

        float length = diag - dist + 10.0f;

        Vector3 externPoint = pos - (pos - center).normalized * surroundArea;

        hits = Physics.RaycastAll(pos, externPoint - pos, surroundArea);
        hitsR = Physics.RaycastAll(externPoint, pos - externPoint, surroundArea);

        if ((hits.Length + hitsR.Length) % 2 != 0) {
            pointList.Add(pos);
        }
    }

    public void generatePointsFromVertex(Transform t) {

        MeshFilter mf = t.GetComponent<MeshFilter>();

        Mesh mesh = mf.mesh;
        MeshHelper.Subdivide9ByDist(mesh, 5.0f);
        mf.mesh = mesh;

        for (int i = 0; i < mf.mesh.vertexCount; i++) {
            pointList.Add(t.TransformPoint(mf.mesh.vertices[i]));
        }
    }
}
