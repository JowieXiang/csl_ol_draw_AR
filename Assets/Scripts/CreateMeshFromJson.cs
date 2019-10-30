using System;
using UnityEngine;
//using HybridWebSocket;
using System.Collections.Generic;
using System.Collections;
using SocketIO;



public class CreateMeshFromJson : MonoBehaviour
{
    private string wsData;
    private SocketIOComponent socket;

    // Use this for initialization
    void Start()
    {
        socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
        socket.On("message", TestBoop);
        StartCoroutine("BeepBoop");
    }

    public void TestBoop(SocketIOEvent e)
    {
        string[] arr = e.data["text"].ToString().Split(char.Parse(@"\"));
        string message = String.Join(null, arr);
        message = message.Substring(1, message.Length - 2);
        // Debug.Log(message);
        wsData = message;
        List<List<Vector3>> verticesList = new List<List<Vector3>>();
        ProceduralUtils procedural = new ProceduralUtils();
        verticesList = procedural.GetCoordinates(wsData);

        for(int i =0; i < verticesList.Count; i++)
        {
            procedural.CreateMesh(verticesList[i],i);
        }
        verticesList.Clear();
        if (e.data == null) { return; }
    }

    private IEnumerator BeepBoop()
    {
        // wait 1 seconds and continue
        yield return new WaitForSeconds(1);

        socket.Emit("beep");

        // wait 3 seconds and continue
        yield return new WaitForSeconds(3);

        socket.Emit("beep");

        // wait 2 seconds and continue
        yield return new WaitForSeconds(2);

        socket.Emit("beep");

        // wait ONE FRAME and continue
        yield return null;

        socket.Emit("beep");
        socket.Emit("beep");
    }

}

