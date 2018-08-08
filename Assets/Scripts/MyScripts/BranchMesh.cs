using System.Collections.Generic;
using UnityEngine;

public class Ring
{

	public Vector3 pos;
	public List<Vector3> points;
	public float rad;
	
	public Ring(Vector3 pos, Vector3 dir, float rad, int pointsNumber = 8)
	{
		points = new List<Vector3>();
		this.pos = pos;
		this.rad = rad;
		GameObject disk = new GameObject();
		List<GameObject> gList = new List<GameObject>();
		for (int i = 0; i < pointsNumber; i++) {
			float x = Mathf.Cos(i * ((Mathf.PI * 2) / pointsNumber));
			float z = Mathf.Sin(i * ((Mathf.PI * 2) / pointsNumber));
			GameObject s = new GameObject();
			s.transform.parent = disk.transform;
			s.transform.position = new Vector3(x, 0.0f, z);
			s.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
			gList.Add(s);
		}
		disk.transform.position = pos;
		disk.transform.up = dir.normalized;
		for (int i = 0; i < gList.Count; i++) {
			points.Add(gList[i].transform.position-pos);
			MonoBehaviour.Destroy(gList[i]);
		}
		MonoBehaviour.Destroy(disk);
	}
}

public class BranchMesh
{
	public Mesh mesh;

	public Ring startRing;
	public Ring endRing;
	public int pointNumber;

	public BranchMesh(Vector3 pos, 
						Vector3 startDir, 
						Vector3 endDir, 
						float startRad, 
						float endRad, 
						int pointNumber = 8)
	{
		mesh = new Mesh();
		startRing = new Ring(pos, startDir, startRad, pointNumber);
		endRing = new Ring(pos + endDir, endDir, endRad, pointNumber);
		this.pointNumber = pointNumber;

		Vector3[] vertices = new Vector3[2*(this.pointNumber + 1)];
		Vector2[] uv = new Vector2[2*(this.pointNumber + 1)];
		Vector3[] normals = new Vector3[2*(this.pointNumber + 1)];
		for (int i = 0; i < this.pointNumber + 1; i++)
		{
			//Debug.Log(i);
			vertices[i] = startRing.pos + startRing.points[i%this.pointNumber] * startRing.rad;
			vertices[i + this.pointNumber + 1] = endRing.pos + endRing.points[i%this.pointNumber] * endRing.rad;
			
			uv[i] = new Vector2((float)i/this.pointNumber, 0);
			uv[i + this.pointNumber + 1] = new Vector2((float)i/this.pointNumber, 1);

			normals[i] = startRing.points[i % this.pointNumber];
			normals[i + this.pointNumber + 1] = endRing.points[i % this.pointNumber];
		}

		int[] tri = new int[6 * pointNumber + 3 * (pointNumber-2)];
		//int[] tri = new int[6 * pointNumber + 3];
		int face = 0;
		for (int i = 0; i < this.pointNumber; i++)
		{
			tri[face] = i + 1;
			tri[face+1] = i;
			tri[face+2] = this.pointNumber + 1 + i;
			
			tri[face+3] = i + 1 + this.pointNumber + 1;
			tri[face+4] = i + 1;
			tri[face+5] = this.pointNumber + 1 + i;
			face += 6;
		}

		
		for (int i = 2; i < this.pointNumber; i++)
		{
			tri[face] = this.pointNumber + 1;
			tri[face+1] = this.pointNumber + i + 1;
			tri[face+2] = this.pointNumber + i;
			face += 3;
		}
		

		mesh.vertices = vertices;
		mesh.triangles = tri;
		mesh.uv = uv;
		mesh.normals = normals;
	}

	public void recalculateMesh(float startRad, float endRad)
	{
		startRing.rad = startRad;
		endRing.rad = endRad;
		Vector3[] vertices = new Vector3[2*(this.pointNumber + 1)];
		for (int i = 0; i < this.pointNumber + 1; i++)
		{
			vertices[i] = startRing.pos + startRing.points[i%this.pointNumber] * startRing.rad;
			vertices[i + this.pointNumber + 1] = endRing.pos + endRing.points[i%this.pointNumber] * endRing.rad;
		}
		mesh.vertices = vertices;
	}
	
	public void recalculateMesh()
	{
		Vector3[] vertices = new Vector3[2*(this.pointNumber + 1)];
		for (int i = 0; i < this.pointNumber + 1; i++)
		{
			vertices[i] = startRing.pos + startRing.points[i%this.pointNumber] * startRing.rad;
			vertices[i + this.pointNumber + 1] = endRing.pos + endRing.points[i%this.pointNumber] * endRing.rad;
		}
		mesh.vertices = vertices;
	}

}
