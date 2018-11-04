﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ServerClient
{
    public int connectionId;
    public string playerName;
    public Vector3 position;

    public ServerClient(int connectionId)
    {
        this.connectionId = connectionId;
        this.playerName = "Temp";
    }
}

public class Server : MonoBehaviour {

    private const int MAX_CONNECTION = 5;
    private int port = 5701;

    private int hostId;
    private int webHostId;

    private int reliableChannel;
    private int unreliableChannel;

    private bool isStarted = false;
    private byte error;

    private List<ServerClient> clients = new List<ServerClient>();

    private float lastMovementUpdate;
    private float movementUpdateRate = 0.05f;

    private void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, port, null);
        webHostId = NetworkTransport.AddWebsocketHost(topo, port, null);

        isStarted = true;
    }


    private void Update()
    {
        if (!isStarted) return;

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.Nothing: break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Player " + connectionId + " has connected");
                OnConnection(connectionId);
                break;
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Player " + connectionId + " has sent: " + msg);
                string[] splitData = msg.Split('|');

                switch (splitData[0])
                {
                    case "NAMEIS":
                        OnNameIs(connectionId, splitData[1]);
                        break;
                    case "CNN":
                        break;
                    case "DC":
                        break;
                    case "MYPOS":
                        OnMyPosition(connectionId, new Vector3(float.Parse(splitData[1]), float.Parse(splitData[2]), float.Parse(splitData[3])));
                        break;
                    default:
                        Debug.Log("Invalid message: " + msg);
                        break;
                }
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Player " + connectionId + " has disconnected");
                OnDisconnection(connectionId);
                break;

            case NetworkEventType.BroadcastEvent:

                break;
        }

        if(Time.time - lastMovementUpdate > movementUpdateRate)
        {
            lastMovementUpdate = Time.time;
            string posmsg = "ASKPOS";
            foreach (ServerClient sc in clients)
            {
                posmsg += '|' + sc.connectionId.ToString() + '%' + sc.position.x.ToString() + '%' + sc.position.y.ToString() + '%' + sc.position.z.ToString();
            }
            posmsg.Trim('|');
            Send(posmsg, unreliableChannel, clients);
        }
    }

    private void OnMyPosition(int cnnId, Vector3 pos)
    {
        clients.Find(x => x.connectionId == cnnId).position = pos;
    }

    private void OnNameIs(int cnnId, string playerName)
    {
        clients.Find(cn => cn.connectionId == cnnId).playerName = playerName;
        Send("CNN|" + playerName + '|' + cnnId, reliableChannel, clients);
    }

    private void OnConnection(int cnnId)
    {
        

        string msg = "ASKNAME|" + cnnId + "|";
        foreach (ServerClient sc in clients)
        {
            msg += sc.playerName + "%" + sc.connectionId + "|";
        }
        msg = msg.Trim('|');
        ServerClient c = new ServerClient(cnnId);
        clients.Add(c);
        Send(msg, reliableChannel, cnnId);
    }

    private void OnDisconnection(int cnnId)
    {
        clients.Remove(clients.Find(c => c.connectionId == cnnId));

        Send("DC|" + cnnId, reliableChannel, clients);
    }

    private void Send(string message, int channelId, int cnnId)
    {
        List<ServerClient> c = new List<ServerClient>();
        c.Add(clients.Find(cn => cn.connectionId == cnnId));
        Send(message, channelId, c);
    }

    private void Send(string message, int channelId, List<ServerClient> c)
    {
        Debug.Log("Sending: " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        foreach  (ServerClient sc in c)
        {
            NetworkTransport.Send(hostId, sc.connectionId, channelId, msg, message.Length * sizeof(char), out error);

        }
    }
}
