/*
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Use plugin namespace
using HybridWebSocket;

public class WebSocketDemo : MonoBehaviour
{
    public string wsData;

    // Use this for initialization
    void Start()
    {

        // Create WebSocket instance
        //WebSocket ws = WebSocketFactory.CreateInstance("ws://echo.websocket.org");
        WebSocket ws = WebSocketFactory.CreateInstance("ws://localhost:8080");
        //WebSocket ws = WebSocketFactory.CreateInstance("ws://csl.local.hcuhh.de:8081/ws"); 

        // Add OnOpen event listener
        ws.OnOpen += () =>
        {
            Debug.Log("WS connected!");
            Debug.Log("WS state: " + ws.GetState().ToString());

            //ws.Send(Encoding.UTF8.GetBytes("Hello from Unity 3D!"));
        };

        // Add OnMessage event listener
        ws.OnMessage += (byte[] msg) =>
        {
            Debug.Log("WS received message: " + Encoding.UTF8.GetString(msg));
            wsData = Encoding.UTF8.GetString(msg);
            //ws.Close();
        };

        // Add OnError event listener
        ws.OnError += (string errMsg) =>
        {
            Debug.Log("WS error: " + errMsg);
        };

        // Add OnClose event listener
        ws.OnClose += (WebSocketCloseCode code) =>
        {
            Debug.Log("WS closed with code: " + code.ToString());
        };

        // Connect to the server
        ws.Connect();

    }

    // Update is called once per frame
    void Update()
    {

    }
}

*/