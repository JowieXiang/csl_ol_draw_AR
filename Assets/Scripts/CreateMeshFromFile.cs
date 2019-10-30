/*
 * This model generates meshes from geojson file
 * It needs ProceduralUtilsEgypt to complete its functions
 * 
 */
using System;
using Mapbox.Unity.Utilities;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine;
using System.Collections.Generic;
using LitJson;
using System.Collections;
using System.IO;

public class CreateMeshFromFile : MonoBehaviour {
    public GameObject _map;
    public Material[] buildingMats;
    private string JSONString;
    // Use this for initialization
    void Start () {
        _map = GameObject.Find("Map");
        JSONString = File.ReadAllText(Application.dataPath + "/Resources/sample_building_outline.geojson");
        List<List<Vector3>> verticesList = new List<List<Vector3>>();
        ProceduralUtilsEgypt procedural_e = new ProceduralUtilsEgypt();
        verticesList = procedural_e.GetCoordinates(JSONString);

        for (int i = 0; i < verticesList.Count; i++)
        {
            procedural_e.CreateMesh(verticesList[i], i);
        }

        Transform drawGroup = GameObject.Find("drawGroup").GetComponent<Transform>();
        int numOfChildren = drawGroup.childCount;
        for (int i = 0; i < numOfChildren; i++)
        {
            GameObject child = drawGroup.GetChild(i).gameObject;
            child.GetComponent<MeshRenderer>().materials = buildingMats;
        }
        Debug.Log("changed material");
        verticesList.Clear();
    }

}
