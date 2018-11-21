using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

public class Server : MonoBehaviour {

    NetworkConnection connection;
    private int hostId;

    private bool isStarted = false;
    private byte error;
    private string ipAddress;
    public GameObject thing;


    
    private List<int> clientIds;
    private Dictionary<int, PlayerData> players;
    private ObjectsHandler objects;
    private RoomHandler rooms;
    private GenerateLVL lvlgenerator;

    public void Init(NetworkConnection connection, int cnnId)
    {
        this.connection = connection;
        this.hostId = cnnId;
        players = new Dictionary<int, PlayerData>();
        objects = FindObjectOfType<ObjectsHandler>();
        rooms = FindObjectOfType<RoomHandler>();
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        clientIds = new List<int>();
        isStarted = true;
        lvlgenerator = new GenerateLVL();
        lvlgenerator.Generate(rooms);
        System.Random rnd = new System.Random();
    }

    private void Start()
    {
        
        /*
            for (int i = 0; i < 20; i++)
        {

            objects.SpawnObjects(i, Instantiate(Resources.Load<GameObject>("Prefabs/CubePrefab"), new Vector3(rnd.Next(-10, 10), rnd.Next(0, 2), rnd.Next(-10, 10)), new Quaternion(rnd.Next(-10, 10), rnd.Next(0, 2), rnd.Next(-10, 10), rnd.Next(-10, 10))));

        }*/

        //GameObject room = Instantiate(Resources.Load<GameObject>("Prefabs/Room"));

        //rooms.AddRoom(room.GetComponent<Room>());
        //FindObjectOfType<Client>().Connect("127.0.0.1");
    }
    public void Refresh()
    {

        string posmsg = "ASKPOS";
        foreach (int cl in clientIds)
        {
            posmsg += '|' + cl.ToString() + '%' + players[cl].ToString();
        }
        posmsg.Trim('|');
        connection.Send(hostId, posmsg, connection.unreliable, clientIds);

    }



    public string[] OnDataRecieved(int cnnId, string[] data)
    {
        if (data == null) return null;
        switch (data[0])
        {
            case "INIT":
                OnInit(cnnId);
                break;
            case "NAMEIS":
                OnNameIs(cnnId, data[1]);
                break;
            case "GRABOBJECT":
                objects.SetObjectPlace(int.Parse(data[2]), stringToVec(data[3]), stringToQuat(data[4]));
                connection.Send(hostId, data[0] + "|" + data[1] + "|" + data[2] + "|" + data[3] + "|" + data[4], connection.unreliable, clientIds);
                break;
            case "DC":
                break;
            case "MYPOS":
                players[cnnId].StringToPlayer(data[1]);
                break;
            case "ROOMRESOLVED":
                rooms.OnDoorOpen(int.Parse(data[1]), 1);
                connection.Send(hostId, "ROOMRESOLVED|" + data[1] + "|" + "1", connection.reliable, clientIds);
                break;
            default:
                // Not server case Handling
                return data;
        }
        Debug.Log("Server recieved from:  " + cnnId + " message: " + string.Join("|", data));
        return null;
    }


    private void OnNameIs(int cnnId, string playerName)
    {
        players[cnnId].playerName = playerName;
        //players[cnnId].avatar.GetComponentInChildren<TextMesh>().text = playerName;
        connection.Send(hostId, "CNN|" + playerName + '|' + cnnId, connection.reliable, clientIds);
        
    }
    
    public void OnConnection(int cnnId)
    {

        string msg = "ASKNAME|" + cnnId + "|";
        foreach (int cl in clientIds)
        {
            msg += players[cl].playerName + "%" + cl + "|";
        }
        msg = msg.Trim('|');
        
        if(!players.ContainsKey(cnnId))players.Add(cnnId, new PlayerData());
        Debug.LogError(players[cnnId]);
        connection.Send(hostId, msg, connection.reliable, cnnId);
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

        
    }

    public void RemovePlayer(int playerCnnId)
    {
        Debug.Log("Player " + playerCnnId + " has disconnected");
        players.Remove(playerCnnId);
        clientIds.Remove(playerCnnId);
        connection.Send(hostId, "DC|" + playerCnnId, connection.reliable, clientIds);
    }

    private void OnInit(int cnnId)
    {
        
        
        string msg = "";// + 0 + "|ADD|Prefabs/Obstacle";
        foreach (var room in lvlgenerator.roomsData)
        {
            foreach (var item in room.player1)
            {
                switch (item.type)
                {
                    case GenerateLVL.ItemType.Locker:
                        if (item.prefabName.Equals("PairLockHandler")) msg = "LOCK|PAIR%" + room.room.id.ToString() + "|ADD";
                        else if (item.prefabName.Equals("ColorLockHandler")) msg = "LOCK|PAIR%" + room.room.id.ToString() + "|ADD";
                        break;
                    case GenerateLVL.ItemType.LockerButton:
                        msg = "LOCK|" + room.room.id.ToString() + "|ADD|" + item.prefabName + "|" + item.gObject.GetComponent<LockerButton>().id.ToString() + "%" + item.gObject.transform.position.ToString() + "%" + item.gObject.transform.localScale.ToString() + "%" + item.gObject.transform.rotation.ToString();
                        break;
                    case GenerateLVL.ItemType.None:
                        msg = "OBJECTS|" + room.room.id.ToString() + "|" + item.prefabName + "%" + item.gObject.transform.position.ToString() + "%" + item.gObject.transform.localScale.ToString() + "%" + item.gObject.transform.rotation.ToString();
                        break;
                }
                connection.Send(hostId, msg, connection.reliable, cnnId);
            }

            msg = "LOCK|" + room.room.id.ToString() + "|PASSWORD|";
            foreach (var item in room.password) msg += item.ToString() + "%";
            msg = msg.Trim('%');
            msg += "|" + room.pswSegment.ToString();
            connection.Send(hostId, msg, connection.reliable, cnnId);

        }

        //Send(msg, reliableChannel, cnnId);


        /*msg = "OBJECTS";
        foreach (var item in rooms.GetRoom(0).GetComponent<GenerateLVL>().player2)
        {
            msg += "|" + "Prefabs/Obstacle" + "%" + item.transform.position.ToString() + "%" + item.transform.localScale.ToString() + "%" + item.transform.rotation.ToString();
            if (msg.Length > 500)
            {
                Send(msg, reliableChannel, cnnId);
                msg = "OBJECTS";
            }
        }
        //NetworkTransport.Se
        Send(msg, reliableChannel, cnnId);*/

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
