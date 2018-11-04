using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ViewerServer : MonoBehaviour {

    private const int MAX_CONNECTION = 5;
    private int port = 5701;

    private int hostId;
    private int webHostId;

    private int reliableChannel;
    private int unreliableChannel;

    private bool isStarted = false;
    private byte error;
    public GameObject thing;


    private float lastMovementUpdate;
    private float movementUpdateRate = 0.05f;
    private List<int> clientIds;
    private PlayersHandler players;
    private ObjectsHandler objects;
    private RoomHandler rooms;

    private void Start()
    {
        players = GetComponent<PlayersHandler>();
        objects = GetComponent<ObjectsHandler>();
        rooms = GetComponent<RoomHandler>();
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, port, null);
        webHostId = NetworkTransport.AddWebsocketHost(topo, port, null);

        clientIds = new List<int>();
        isStarted = true;

        System.Random rnd = new System.Random();
        for (int i = 0; i < 20; i++)
        {

            objects.SpawnObjects(i, Instantiate(Resources.Load<GameObject>("Prefabs/CubePrefab"), new Vector3(rnd.Next(-10, 10), rnd.Next(0, 2), rnd.Next(-10, 10)), new Quaternion(rnd.Next(-10, 10), rnd.Next(0, 2), rnd.Next(-10, 10), rnd.Next(-10, 10))));

        }

        GameObject room = Instantiate(Resources.Load<GameObject>("Prefabs/Room"));

        rooms.AddRoom(room.GetComponent<Room>());
        rooms.GetRoom(0).gameObject.AddComponent<GenerateLVL>();
    }


    private void Update()
    {
        if (!isStarted) return;

        int recHostId;
        int cnnId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        var noEventsLeft = false;
        while (!noEventsLeft)
        {
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out cnnId, out channelId, recBuffer, bufferSize, out dataSize, out error);
            switch (recData)
            {
                case NetworkEventType.Nothing:
                    noEventsLeft = true;
                    break;
                case NetworkEventType.ConnectEvent:
                    Debug.Log("Player " + cnnId + " has connected");
                    OnConnection(cnnId);
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    Debug.Log("Player " + cnnId + " has sent: " + msg);
                    string[] splitData = msg.Split('|');
                    OnDataRecieved(cnnId, splitData);

                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log("Player " + cnnId + " has disconnected");
                    players.PlayerDisconnected(cnnId);
                    clientIds.Remove(cnnId);
                    Send("DC|" + cnnId, reliableChannel, clientIds);
                    break;

                case NetworkEventType.BroadcastEvent:

                    break;
            }

        }

        if(Time.time - lastMovementUpdate > movementUpdateRate)
        {
            lastMovementUpdate = Time.time;
            string posmsg = "ASKPOS";
            foreach (int cl in clientIds)
            {
                posmsg += '|' + cl.ToString() + '%' + players.getPlayerPosition(cl).ToString();
            }
            posmsg.Trim('|');
            Send(posmsg, unreliableChannel, clientIds);
        }
    }

    private void OnDataRecieved(int cnnId, string[] data)
    {
        switch (data[0])
        {
            case "NAMEIS":
                OnNameIs(cnnId, data[1]);
                break;
            case "GRABOBJECT":
                objects.SetObjectPlace(int.Parse(data[2]), stringToVec(data[3]), stringToQuat(data[4]));
                Send(data[0] + "|" + data[1] + "|" + data[2] + "|" + data[3] + "|" + data[4], unreliableChannel, clientIds);
                break;
            case "DC":
                break;
            case "MYPOS":
                players.setPlayerPosition(cnnId, data[1]);
                break;
            case "ROOMRESOLVED":
                rooms.OnDoorOpen(int.Parse(data[1]), 1);
                Send("ROOMRESOLVED|" + data[1] + "|" + "1", reliableChannel, clientIds);
                break;
            default:
                Debug.Log("Invalid message: " + data);
                break;
        }
    }


    private void OnNameIs(int cnnId, string playerName)
    {
        players.setPlayerName(cnnId, playerName);
        //players[cnnId].avatar.GetComponentInChildren<TextMesh>().text = playerName;
        Send("CNN|" + playerName + '|' + cnnId, reliableChannel, clientIds);
        
    }

    private void OnConnection(int cnnId)
    {

        string msg = "ASKNAME|" + cnnId + "|";
        foreach (int cl in clientIds)
        {
            msg += players.getPlayerName(cl) + "%" + cl + "|";
        }
        msg = msg.Trim('|');
        players.SpawnPlayer("Anonym", cnnId);
        Send(msg, reliableChannel, cnnId);
        clientIds.Add(cnnId);
        //msg =

        /*msg = "ROOM|" + "Prefabs/Room";
        Send(msg, reliableChannel, cnnId);
        GameObject room = Instantiate(Resources.Load<GameObject>("Prefabs/Room"));
        
        rooms.AddRoom(room.GetComponent<Room>());
        rooms.GetRoom(0).gameObject.AddComponent<GenerateLVL>();
        for (int i = 0; i < 20; i++)
        {
            msg = "OBJECTS|" + i.ToString() + "%" + "Prefabs/CubePrefab" + "%" + objects.GetObject(i).transform.position.ToString() + "%" + objects.GetObject(i).transform.rotation.ToString();
            Send(msg, reliableChannel, cnnId);
        }*/

        msg = "PAIRLOCK|" + "0|Prefabs/Obstacle";
        foreach (var item in rooms.GetRoom(0).GetComponent<GenerateLVL>().player1)
        {
            msg += "|" + item.GetComponent<LockerButton>().id.ToString() + "%" + item.transform.position.ToString() + "%" + item.transform.localScale.ToString() + "%" + item.transform.rotation.ToString();
            if(msg.Length> 500)
            {
                Send(msg, reliableChannel, cnnId);
                msg = "PAIRLOCK|" + "0|Prefabs/Obstacle";
            }
        }
        //NetworkTransport.Se
        Send(msg, reliableChannel, cnnId);

        msg = "LOCKPASSWORD|" + "0" + "|";
        foreach (var item in rooms.GetRoom(0).GetComponent<GenerateLVL>().password) msg += item.ToString() + "%";
        msg = msg.Trim('%');
        msg += "|" + rooms.GetRoom(0).GetComponent<GenerateLVL>().individualNumber.ToString();
        Send(msg, reliableChannel, cnnId);
    }

    private void Send(string message, int channelId, int cnnId)
    {
        List<int> c = new List<int>();
        c.Add(cnnId);
        Send(message, channelId, c);
    }

    private void Send(string message, int channelId, List<int> clients)
    {
        Debug.Log("Sending: " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        foreach  (int cnnId in clients)
        {
            NetworkTransport.Send(hostId, cnnId, channelId, msg, message.Length * sizeof(char), out error);

        }
    }
    private Vector3 stringToVec(string s)
    {
        string[] temp = s.Substring(1, s.Length - 2).Split(',');
        return new Vector3(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
    }

    private Quaternion stringToQuat(string s)
    {
        string[] temp = s.Substring(1, s.Length - 2).Split(',');
        return new Quaternion(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]), float.Parse(temp[3]));
    }
}
