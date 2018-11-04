using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Client : MonoBehaviour {

    private const int MAX_CONNECTION = 5;
    private int port = 5701;

    private int hostId;
    private int webHostId;
    private int connectionId;

    private float connectionTime;

    private int reliableChannel;
    private int unreliableChannel;

    private bool isStarted = false;
    private bool isConnected = false;
    private string playerName;
    private byte error;

    private PlayersHandler players;
    private ObjectsHandler objects;
    private RoomHandler rooms;


    public void Connect()
    {
        string pName = "valaki"; //GameObject.Find("NameInput").GetComponent<InputField>().text;
        if(pName == "")
        {
            Debug.Log("You must enter a name!");
            return;
        }

        playerName = pName;

        NetworkTransport.Init();
        ConnectionConfig myConfig = new ConnectionConfig();

        reliableChannel = myConfig.AddChannel(QosType.Reliable);
        unreliableChannel = myConfig.AddChannel(QosType.Unreliable);
        //myConfig.ConnectTimeout = 1000;
        //myConfig.MaxConnectionAttempt = 5;

        HostTopology topo = new HostTopology(myConfig, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, 0);
        connectionId = NetworkTransport.Connect(hostId, "192.168.0.175", port, 0, out error);

        connectionTime = Time.time;
        isConnected = true;
    }

    internal void OnGrabObject(int objId, GameObject selectedObject)
    {
        string msg = "GRABOBJECT|" + players.getOurPlayerId().ToString() + '|' + objId.ToString() + '|' + selectedObject.transform.position.ToString() + '|' + selectedObject.transform.rotation.ToString();
        Send(msg, unreliableChannel);
    }

    private void Start()
    {
        players = GetComponent<PlayersHandler>();
        objects = GetComponent<ObjectsHandler>();
        rooms = GetComponent<RoomHandler>();
        this.Connect();
    }

    private void Update()
    {
        if (!isConnected) return;
        //if (Time.time + 0.1f < connectionTime) isConnected = false;
        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[2048];
        int bufferSize = 2048;
        int dataSize;
        byte error;
        var noEventsLeft = false;
        while (!noEventsLeft)
        {
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);

            if ((NetworkError)error != NetworkError.Ok)
            {
                Debug.Log("NetworkTransport error: " + error);
                return;
            }

            switch (recData)
            {
                case NetworkEventType.Nothing:
                    noEventsLeft = true;
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    //Debug.Log("Receiving: " + msg);
                    string[] splitData = msg.Split('|');
                    OnDataRecieved(splitData);
                    
                    connectionTime = Time.time;
                    break;

                case NetworkEventType.BroadcastEvent:

                    break;
            }
        }
    }

    private void OnDataRecieved(string[] data)
    {
        PairLockHandler locker;
        switch (data[0])
        {
            case "ASKNAME":
                OnAskName(data);
                break;
            case "CNN":
                players.SpawnPlayer(data[1], int.Parse(data[2]));
                isStarted = true;
                break;
            case "DC":
                players.PlayerDisconnected(int.Parse(data[1]));
                break;
            case "OBJECTS":
                for (int i = 1; i < data.Length; i++)
                {
                    string[] d = data[i].Split('%');
                    Debug.Log(data[i]);
                    objects.SpawnObjects(int.Parse(d[0]), Instantiate(Resources.Load<GameObject>(d[1]), stringToVec(d[2]), stringToQuat(d[3])));
                }
                break;
            case "MOVEOBJECT":
                for (int i = 1; i < data.Length; i++)
                {
                    string[] d = data[i].Split('%');
                    Debug.Log(data[i]);
                    objects.SetObjectPlace(int.Parse(d[0]),  stringToVec(d[1]), stringToQuat(d[2]));
                }
                break;
            case "GRABOBJECT":
                break;
            case "ASKPOS":
                if (!isStarted) return;
                string msg = "MYPOS|" + players.setPlayersPosition(data);
                Debug.Log(msg);
                Send(msg, unreliableChannel);
                break;
            case "ROOM":
                //Debug.LogError(data[1]);
                Instantiate(Resources.Load<GameObject>(data[1]));
                break;
            case "PAIRLOCK":
                //Debug.LogError(data[1]);

                
                if ((locker = rooms.GetRoom(int.Parse(data[1])).GetComponentInChildren<PairLockHandler>()) == null)
                {
                    //locker = new PairLockHandler();
                    locker = new GameObject().AddComponent<PairLockHandler>();
                    locker.name = "PairLock";
                    locker.gameObject.transform.parent = rooms.GetRoom(int.Parse(data[1])).transform;
                }
                //Debug.LogError(string.Join("|", data));
                int buttonCount = locker.transform.childCount -3;
                for (int i = 3; i < data.Length; i++)
                {
                    string[] d = data[i].Split('%');
                    LockerButton button = Instantiate(Resources.Load<GameObject>(data[2])).AddComponent<LockerButton>();
                    button.gameObject.transform.parent = locker.transform;
                    button.gameObject.transform.position = stringToVec(d[1]);
                    button.gameObject.transform.localScale = stringToVec(d[2]);
                    button.gameObject.transform.rotation = stringToQuat(d[3]);
                    button.id = i + buttonCount;
                    button.SetLockerHandler(locker);
                    Material mat = (Material)Resources.Load("Material/Blue", typeof(Material)); ;
                    //mat.SetColor("_SpecColor", Color.red);
                    button.GetComponent<LockerButton>().SetSelected(mat);
                    
                }
                //Instantiate(Resources.Load<GameObject>(data[1]));
                break;
            case "LOCKPASSWORD":
                //Debug.LogError(data[1]);
                //PairLockHandler locker;
                if ((locker = rooms.GetRoom(int.Parse(data[1])).GetComponentInChildren<PairLockHandler>()) == null)
                {
                    Debug.LogError("Locker Not Here " +  string.Join("|", data));
                    break;
                }
                //Debug.LogError(string.Join("|", data));
                int[] password = Array.ConvertAll(data[2].Split('%'), delegate (string s) { return int.Parse(s); });
                Debug.LogError( password);
                Debug.LogWarning("Invalid message: " + string.Join("|", data[2].Split('%')));
                locker.SetPassword(password, int.Parse(data[2]));
                //Instantiate(Resources.Load<GameObject>(data[1]));
                break;
            case "ROOMRESOLVED":
                rooms.OnDoorOpen(int.Parse(data[1]), int.Parse(data[2]));
                break;
            default:
                Debug.LogWarning("Invalid message: " + string.Join("|", data));
                break;
        }
    }

    private void OnAskName(string[] data)
    {
        players.setOurPlayerId(int.Parse(data[1]));

        Send("NAMEIS|" + playerName, reliableChannel);

        for (int i = 2; i < data.Length; i++)
        {
            string[] d = data[i].Split('%');
            players.SpawnPlayer(d[0], int.Parse(d[1]));

        }
    }

    public void OnRoomResolving(int roomId)
    {
        Send("ROOMRESOLVED|" + roomId.ToString(), reliableChannel);
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


    private void Send(string message, int channelId)
    {
        //Debug.Log("Sending: " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostId, connectionId, channelId, msg, message.Length * sizeof(char), out error);
    }

    
    /*
    //Game Interface
    public void OnGrabObject(GameObject o)
    {
        Send("GRAB|" + ourClientId.ToString() + "|" + o.ToString() , unreliableChannel);
    }*/
}
