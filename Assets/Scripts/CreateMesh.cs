using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMesh : MonoBehaviour {
    List<List<Vector3>> verticesList;

    // Use this for initialization
    void Start() {
        verticesList = new List<List<Vector3>>();
        verticesList.Add(new List<Vector3>());

        verticesList[0].Add(new Vector3(0f, 0f, 0f));
        verticesList[0].Add(new Vector3(0f, 1f, 0f));
        verticesList[0].Add(new Vector3(1f, 1f, 0f));
        Create(verticesList);
    }

	
	// Update is called once per frame
	void Update () {
		
	}

    public void Create(List<List<Vector3>> buildingVertices)
    {
        //Debug.Log(buildingVertices.Count);
        for (int i = 0; i < buildingVertices.Count; i++)
        {
            // Create a building game object
            GameObject thisBuilding = new GameObject("Building " + i);
            float height = buildingVertices[i][1].y;
            // Compute the center point of the polygon both on the ground, and at height
            // Add center vertices to end of list
            Vector3 center = FindCenter(buildingVertices[i]);
            buildingVertices[i].Add(center);
            Vector3 raisedCenter = center;
            raisedCenter.y += height;
            buildingVertices[i].Add(raisedCenter);

            List<int> tris = new List<int>();
            // Convert vertices to array for mesh
            Vector3[] vertices = buildingVertices[i].ToArray();

            // Do the triangles for the roof and the floor of the building
            // Roof points are at odd indeces
            for (int j = vertices.Length - 3; j >= 0; j--)
            {
                // Add the point
                tris.Add(j);
                // Check for wrap around
                if (j - 2 >= 0)
                {
                    tris.Add(j - 2);
                }
                else
                {
                    // If wrap around, add the first vertex
                    int diff = j - 2;
                    tris.Add(vertices.Length - 2 + diff);
                }
                // Check if its at ground or building height level, choose proper center point
                if (j % 2 == 0)
                {
                    tris.Add(vertices.Length - 2);
                }
                else
                {
                    tris.Add(vertices.Length - 1);
                }
            }

            // Do triangles which connect roof to ground
            for (int j = vertices.Length - 3; j >= 2; j--)
            {
                if (j % 2 == 1)
                {
                    tris.Add(j);
                    tris.Add(j - 1);
                    tris.Add(j - 2);
                }
                else
                {
                    tris.Add(j);
                    tris.Add(j - 2);
                    tris.Add(j - 1);
                }
            }

            int[] triangles = tris.ToArray();

            // Create and apply the mesh
            MeshFilter mf = thisBuilding.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            mf.mesh = mesh;
            Renderer rend = thisBuilding.AddComponent<MeshRenderer>();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

        }
    }

    // Find the center X-Z position of the polygon.
    public Vector3 FindCenter(List<Vector3> verts)
    {
        Vector3 center = Vector3.zero;
        // Only need to check every other spot since the odd indexed vertices are in the air, but have same XZ as previous
        for (int i = 0; i < verts.Count; i += 2)
        {
            center += verts[i];
        }
        return center / (verts.Count / 2);

    }
}
