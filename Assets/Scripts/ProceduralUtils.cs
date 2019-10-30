using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Mapbox.Unity.Utilities;
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class ProceduralUtils {
    private GameObject _map = GameObject.Find("Map");

    //translate wsString to world position vertices
    public List<List<Vector3>> GetCoordinates(string wsString)
    {
        //get coordinate data from websocket string
        List<List<JsonData>> Coordinates = new List<List<JsonData>>();
        JsonData json = JsonMapper.ToObject(wsString);
        List<JsonData> drawObjList = new List<JsonData>();
        for (int i = 0; i < json["features"].Count; i++)
        {
            Coordinates.Add(new List<JsonData>());
            drawObjList.Add(JsonMapper.ToObject("{\"coordinates\":[],\"height\":3}"));
            //since the JSON received from ws has a superfluous item, we iterate until count-1
            int pCount = json["features"][i]["geometry"]["coordinates"][0].Count - 1;
            JsonData[] arr = new JsonData[pCount];
            for (int j = 0; j < pCount; j++)
            {
                arr[j] = JsonMapper.ToObject("{\"lat\":" + json["features"][i]["geometry"]["coordinates"][0][j][1] + ",\"lon\":" + json["features"][i]["geometry"]["coordinates"][0][j][0] + "}");
                Coordinates[i].Add(json["features"][i]["geometry"]["coordinates"][0][j]);
                //string item = JsonMapper.ToJson(json["features"][i]["geometry"]["coordinates"][0][j]);//translate JSONObject to string
            }
            foreach (JsonData item in arr)
            {
                drawObjList[i]["coordinates"].Add(item);
            }
            if (json["features"][i]["properties"].Keys.Contains("height"))
            {
                drawObjList[i]["height"] = json["features"][i]["properties"]["height"];
            }
        }

        //translate geo-coordinates to Unity world position
        List<List<Vector3>> v3List = new List<List<Vector3>>();
        for (int i = 0; i < drawObjList.Count; i++)
        {
            List<Vector3> baseVertices = new List<Vector3>();
            v3List.Add(new List<Vector3>());
            List<Vector3> capVertices = new List<Vector3>();
            for (int j = 0; j < Coordinates[i].Count; j++)
            {
                Vector2d v2d = GeoToWorld(Coordinates[i][j], _map);
                Vector3 v3 = Vector2dToVector3(v2d);
                baseVertices.Add(v3);
                capVertices = GetCap(baseVertices, float.Parse(JsonMapper.ToJson(drawObjList[i]["height"])));
            }
            v3List[i] = capVertices;

            List<List<Vector3>> sideVertices = new List<List<Vector3>>();
            sideVertices = GetSideVertices(baseVertices, float.Parse(JsonMapper.ToJson(drawObjList[i]["height"])));
            RenderSides(sideVertices);
            // Debug.Log(float.Parse(JsonMapper.ToJson(drawObjList[i]["height"])));
            // v3List.AddRange(sideVertices);
        }
        return v3List;
    }

    //extrude a volume (multiple faces) from one base
    public List<List<Vector3>> GetSideVertices(List<Vector3> basePoints, float height)
    {
        // add first vertice to the end of the list
        List<Vector3> newBasePoints = new List<Vector3>();
        newBasePoints.AddRange(basePoints);
        newBasePoints.Add(new Vector3());
        newBasePoints[basePoints.Count] = basePoints[0];
        List<List<Vector3>> extrudedVertices = new List<List<Vector3>>();

        for (int i = 0; i < newBasePoints.Count - 1; i++)
        {
            List<Vector3> faceVertices = new List<Vector3>();
            Vector3 h1 = new Vector3(newBasePoints[i].x, height, newBasePoints[i].z);
            Vector3 h2 = new Vector3(newBasePoints[i + 1].x, height, newBasePoints[i + 1].z);
            faceVertices.Add(newBasePoints[i]);
            faceVertices.Add(h1);
            faceVertices.Add(h2);
            faceVertices.Add(newBasePoints[i + 1]);
            extrudedVertices.Add(faceVertices);
        }
        return extrudedVertices;
    }
    //render sides
    public void RenderSides(List<List<Vector3>> verticesList)
    {
        //create parent go for the draw objects
        if (!GameObject.Find("drawGroup"))
        {
            GameObject drawGroup = new GameObject("drawGroup");
            drawGroup.transform.SetParent(GameObject.Find("ImageTarget").transform);
        }

        for (int i = 0; i < verticesList.Count; i++)
        {
            // Debug.Log(verticesList[i].ToString());
            //create game object
            GameObject thisDraw = new GameObject("draw sides" + i);
            thisDraw.transform.SetParent(GameObject.Find("drawGroup").transform);

            //set transform,avoid overlap with ground
            thisDraw.transform.localScale = _map.transform.localScale;


            // Convert vertices list to array 
            Vector3[] vertices = new Vector3[4]
            {
                verticesList[i][0],
                verticesList[i][3],
                verticesList[i][1],
                verticesList[i][2]
            };

            int[] tris = new int[6]
            {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
            };

            // Create the mesh
            Mesh msh = new Mesh();
            msh.vertices = vertices;
            msh.triangles = tris;
            msh.RecalculateNormals();

            //msh.RecalculateNormals();
            //msh.RecalculateBounds();

            // Set up game object with mesh;
            thisDraw.AddComponent(typeof(MeshRenderer));
            MeshFilter filter = thisDraw.AddComponent(typeof(MeshFilter)) as MeshFilter;
            Renderer rend = thisDraw.AddComponent<MeshRenderer>();
            filter.mesh = msh;
            thisDraw.transform.Translate(Vector3.up * 0.001f, Space.World);
        }
    }
    //get cap from base
    public List<Vector3> GetCap(List<Vector3> basePoints, float height)
    {
        List<Vector3> capVertices = new List<Vector3>();
        for (int i = 0; i < basePoints.Count; i++)
        {
            capVertices.Add(new Vector3(basePoints[i].x, height, basePoints[i].z));
        }
        return capVertices;
    }
    //!!! the problem is that the algo is not working for 3D objects now
    //create the building mesh with vertices in the world coordinates
    public void CreateMesh(List<Vector3> vertices, int i)
    {
        //create parent go for the draw objects
        if (!GameObject.Find("drawGroup"))
        {
            GameObject drawGroup = new GameObject("drawGroup");
            drawGroup.transform.SetParent(GameObject.Find("ImageTarget").transform);
        }


        //create game object
        GameObject thisDraw = new GameObject("draw " + i);
        thisDraw.transform.SetParent(GameObject.Find("drawGroup").transform);

        //set transform,avoid overlap with ground
        thisDraw.transform.localScale = _map.transform.localScale;

        //get Vector2D array for triangulation
        Vector2[] v2 = new Vector2[vertices.Count];
        for (int j = 0; j < v2.Length; j++)
        {
            v2[j] = new Vector2(vertices[j].x, vertices[j].z);
        }

        //triangulation
        Triangulator tr = new Triangulator(v2);
        int[] indices = tr.Triangulate();

        // Convert vertices list to array 
        Vector3[] vers = new Vector3[v2.Length];
        for (int j = 0; j < vers.Length; j++)
        {
            vers[j] = new Vector3(v2[j].x, vertices[j].y, v2[j].y);
        }

        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = vers;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        // Set up game object with mesh;
        thisDraw.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = thisDraw.AddComponent(typeof(MeshFilter)) as MeshFilter;
        Renderer rend = thisDraw.AddComponent<MeshRenderer>();
        filter.mesh = msh;
        thisDraw.transform.Translate(Vector3.up * 0.001f, Space.World);
    }
    Vector3 Vector2dToVector3(Vector2d v2d)
    {
        Vector3 v3 = new Vector3();
        if (v2d != null && v2d.GetType() == typeof(Vector2d))
        {
            v3 = new Vector3(float.Parse(v2d.x.ToString()), 0f, float.Parse(v2d.y.ToString()));
        }
        return v3;
    }

    Vector2d GeoToWorld(JsonData geoJson, GameObject map)
    {
        //have to reverse latitude and longitude to comply to mapbox Conversion functionality
        Vector2d v2d = Conversions.GeoToWorldPosition(float.Parse(geoJson[1].ToString()), float.Parse(geoJson[0].ToString()), map.GetComponent<AbstractMap>().CenterMercator, map.GetComponent<AbstractMap>().WorldRelativeScale);
        return v2d;
    }

}
