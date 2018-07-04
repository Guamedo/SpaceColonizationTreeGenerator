using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoSideQuad{

    private Vector3 pos;
    private Vector3 rotation;
    private float size;
    private Mesh mesh;

    public TwoSideQuad(Vector3 pos, float size) {
        this.pos = pos;
        this.size = size;
        this.rotation = new Vector3(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180)); ;
    }

    public GameObject generateQuad(Color col1, Color col2) {

        mesh = new Mesh();

        // Add the vertices to the mesh
        Vector3[] vertices = new Vector3[8];

        vertices[0] = new Vector3(0.0f, 0.0f, 0.0f);
        vertices[1] = new Vector3(0, 1, 0) * size;
        vertices[2] = new Vector3(1, 1, 0) * size;
        vertices[3] = new Vector3(1, 0, 0) * size;

        vertices[4] = vertices[0];
        vertices[5] = vertices[1];
        vertices[6] = vertices[2];
        vertices[7] = vertices[3];

        mesh.vertices = vertices;

        // Add the triangles to the mesh

        int[] tri = new int[12];

        tri[0] = 0;
        tri[1] = 1;
        tri[2] = 2;

        tri[3] = 0;
        tri[4] = 2;
        tri[5] = 3;

        tri[6] = 0;
        tri[7] = 2;
        tri[8] = 1;

        tri[9] = 0;
        tri[10] = 3;
        tri[11] = 2;

        mesh.triangles = tri;

        // Add normals to the mesh

        Vector3[] normals = new Vector3[8];

        normals[0] = -Vector3.forward;
        normals[1] = -Vector3.forward;
        normals[2] = -Vector3.forward;
        normals[3] = -Vector3.forward;

        normals[4] = Vector3.forward;
        normals[5] = Vector3.forward;
        normals[6] = Vector3.forward;
        normals[7] = Vector3.forward;

        mesh.normals = normals;

        // Add uv to the mesh

        Vector2[] uv = new Vector2[8];

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);

        uv[4] = new Vector2(0, 0);
        uv[5] = new Vector2(1, 0);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        mesh.uv = uv;

        GameObject go = new GameObject("Quad");
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<MeshFilter>().mesh = mesh;

        go.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
        go.GetComponent<MeshRenderer>().material.color = Color.Lerp(col1, col2, Random.Range(0.0f, 1.0f));
        go.AddComponent<BoxCollider>();
        go.GetComponent<BoxCollider>().size = new Vector3(go.GetComponent<BoxCollider>().size.x,
                                                            go.GetComponent<BoxCollider>().size.y, 
                                                            0.1f);
        go.transform.position = pos;
        go.transform.eulerAngles = rotation;
        return go;
    }
}
