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

public class Server {

    NetworkConnection connection;

    public GameObject thing;

    private Dictionary<int, int> playersImageRoomResolved = new Dictionary<int, int>();

    private List<int> clientIds;
    private Dictionary<int, PlayerData> players;
    private ObjectsHandler objects;
    private RoomHandler rooms;
    private GenerateLVL lvlgenerator;

    public void Init(NetworkConnection connection)
    {
        this.connection = connection;

        players = new Dictionary<int, PlayerData>();
        //objects = FindObjectOfType<ObjectsHandler>();
        //rooms = FindObjectOfType<RoomHandler>();

        //objects = connection.gameObject.GetComponent<ObjectsHandler>();
        rooms = connection.gameObject.GetComponent<RoomHandler>();
        clientIds = new List<int>();
        lvlgenerator = new GenerateLVL();
        lvlgenerator.Generate(rooms);
        //Debug.LogError("lvlgenerator run");
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
        connection.Send(posmsg, connection.unreliable, clientIds);

    }



    public string[] OnDataRecieved(int cnnId, string[] data)
    {
        if (data == null) return null;
        switch (data[0])
        {
            case "INIT":
                //OnInit(cnnId, 1);
                OnConnection(cnnId);
                break;
            case "NAMEIS":
                OnNameIs(cnnId, data[1]);
                break;
            case "GRABOBJECT":
                objects.SetObjectPlace(int.Parse(data[2]), stringToVec(data[3]), stringToQuat(data[4]));
                connection.Send( "GRABBED|" + data[1] + "|" + data[2] + "|" + data[3] + "|" + data[4], connection.unreliable, clientIds);
                break;
            case "MYPOS":
                string[] d = data[1].Split('$');
                players[cnnId].setPlayerData(stringToVec(d[0]), stringToVec(d[1]), stringToVec(d[2]));
                break;
            case "RESOLVING":
                if (lvlgenerator.roomsData[int.Parse(data[1])].neededResult == 1)
                {
                    //rooms.OnDoorOpen(int.Parse(data[1]), 1);
                    connection.Send("RESOLVED|" + data[1] + "|" + "1", connection.reliable, clientIds);
                }
                else
                {
                    if (!playersImageRoomResolved.ContainsKey(cnnId))
                    {
                        playersImageRoomResolved.Add(cnnId, 1);
                        if(playersImageRoomResolved.Count == lvlgenerator.roomsData[int.Parse(data[1])].neededResult) connection.Send("RESOLVED|" + data[1] + "|" + "1", connection.reliable, clientIds);
                    }
                }
                break;
            case "NOTRESOLVING":
                if (playersImageRoomResolved.ContainsKey(cnnId)) playersImageRoomResolved.Remove(cnnId);
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
        if (clientIds.Contains(cnnId)) return;
        players.Add(cnnId, new PlayerData());
        clientIds.Add(cnnId);
        players[cnnId].playerName = playerName;
        //players[cnnId].avatar.GetComponentInChildren<TextMesh>().text = playerName;
        connection.Send("CNN|" + playerName + '|' + cnnId, connection.reliable, clientIds);
       
        
        //Debug.LogError(players[cnnId]);
        
        
    }
    
    public void OnConnection(int cnnId)
    {
        
        if (clientIds.Contains(cnnId)) return;
        //Debug.LogError(cnnId);
        string msg = "ASKNAME|" + cnnId + "|";
        foreach (int cl in clientIds)
        {
            msg += players[cl].playerName + "%" + cl + "|";
        }
        msg = msg.Trim('|');
        connection.Send(msg, connection.reliable, cnnId);

        //if (!players.ContainsKey(cnnId)) 
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
        connection.Send("DC|" + playerCnnId, connection.reliable, clientIds);
    }

    public void OnInitMap(int cnnId)
    {
    string msg = "";
        if (cnnId % 2 == 0)
        {
            foreach (var room in lvlgenerator.roomsData)
            {
                switch (room.locker.type)
                {
                    case GenerateLVL.ItemType.PairLocker:
                        msg = "LOCK|PAIR%" + room.room.id.ToString() + "|";
                        connection.Send(msg, connection.reliable, cnnId);
                        foreach (var item in room.player1)
                        {
                            //Debug.LogError(item.gObject.transform.position);
                            switch (item.type)
                            {
                                case GenerateLVL.ItemType.LockerButton:
                                    msg = "LOCK|" + room.room.id.ToString() + "|ADD|" + item.prefabName + "|" + item.id.ToString() + "%" + item.position.ToString();// + "%" + item.gObject.transform.localScale.ToString() + "%" + item.gObject.transform.rotation.ToString();
                                    break;
                                case GenerateLVL.ItemType.None:
                                    msg = "OBJECTS|" + room.room.id.ToString() + "|" + item.prefabName + "%" + item.position.ToString() + "%" + item.localScale.ToString() + "%" + item.rotation.ToString();
                                    break;
                            }
                            connection.Send(msg, connection.reliable, cnnId);
                        }

                        msg = "LOCK|" + room.room.id.ToString() + "|PASSWORD|";
                        foreach (var item in room.password) msg += item.ToString() + "%";
                        msg = msg.Trim('%');
                        msg += "|" + room.pswSegment.ToString();
                        connection.Send(msg, connection.reliable, cnnId);
                        break;
                    case GenerateLVL.ItemType.ColorLocker:
                        msg = "LOCK|COLOR%" + room.room.id.ToString() + "%" + room.locker.position.ToString() + "|"; //+ "%" + room.locker.gObject.transform.rotation.ToString() + "|";
                        connection.Send(msg, connection.reliable, cnnId);

                        msg = "LOCK|" + room.room.id.ToString() + "|PASSWORD|";
                        foreach (var item in room.password) msg += item.ToString() + "%";
                        msg = msg.Trim('%');
                        msg += "|" + room.pswSegment.ToString();
                        connection.Send(msg, connection.reliable, cnnId);
                        break;
                    case GenerateLVL.ItemType.ImageLocker:
                        msg = "LOCK|IMAGE%" + room.room.id.ToString() + "%" + room.locker.position.ToString() + "|" + room.password[0] + "|"; //+ "%" + room.locker.gObject.transform.rotation.ToString() + "|";
                        connection.Send(msg, connection.reliable, cnnId);

                        //msg = "LOCK|" + room.room.id.ToString() + "|PASSWORD|" + room.password[0] + "|1";
                        //connection.Send(msg, connection.reliable, cnnId);
                        break;


                }


            }
        }
        else
        {
            foreach (var room in lvlgenerator.roomsData)
            {
                switch (room.locker.type)
                {
                    case GenerateLVL.ItemType.PairLocker:
                        msg = "LOCK|PAIR%" + room.room.id.ToString() + "|";
                        connection.Send(msg, connection.reliable, cnnId);
                        foreach (var item in room.player2)
                        {
                            //Debug.LogError(item.gObject.transform.position);
                            switch (item.type)
                            {
                                case GenerateLVL.ItemType.None:
                                    msg = "OBJECTS|" + room.room.id.ToString() + "|" + item.prefabName + "%" + item.position.ToString() + "%" + item.localScale.ToString() + "%" + item.rotation.ToString();
                                    break;
                            }
                            connection.Send(msg, connection.reliable, cnnId);
                        }
                        break;
                    case GenerateLVL.ItemType.ColorLocker:
                        msg = "TEXT|" + room.room.id.ToString() + "|" + room.player2[0].prefabName + "%" + room.player2[0].position.ToString() + "|";// + "%" + room.player2[0].gObject.transform.rotation.ToString() + "|";
                        connection.Send(msg, connection.reliable, cnnId);

                        break;
                    case GenerateLVL.ItemType.ImageLocker:
                        msg = "LOCK|IMAGE%" + room.room.id.ToString() + "%" + room.locker.position.ToString() + "|" + room.password[1] + "|";// + "%" + room.locker.gObject.transform.rotation.ToString() + "|";
                        connection.Send(msg, connection.reliable, cnnId);
                        break;


                }


            }
        }
        //Send(msg, reliableChannel, cnnId);
        connection.Send("CONNECTED", connection.reliable, cnnId);

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
