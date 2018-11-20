using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Client : MonoBehaviour {

    private const int MAX_CONNECTION = 5;
    private int port = 5701;

    private int hostId;
    //private int webHostId;
    private int connectionId;

    private float connectionTime;

    private int reliableChannel;
    private int unreliableChannel;

    private bool isStarted = false;
    private bool isConnected = false;
    private byte error;
    bool created = false;

    private PlayersHandler players;
    private ObjectsHandler objects;
    private RoomHandler rooms;


    public void Connect(string pIp)
    {
        
        

        NetworkTransport.Init();
        ConnectionConfig myConfig = new ConnectionConfig();

        reliableChannel = myConfig.AddChannel(QosType.Reliable);
        unreliableChannel = myConfig.AddChannel(QosType.Unreliable);
        //myConfig.ConnectTimeout = 1000;
        //myConfig.MaxConnectionAttempt = 5;

        HostTopology topo = new HostTopology(myConfig, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, 0);
        connectionId = NetworkTransport.Connect(hostId, pIp, port, 0, out error);//"192.168.0.175""127.0.0.1"
        Debug.Log("Connect: " + pIp );
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
        
        //Debug.Log("INIT Sended");
        players = GetComponent<PlayersHandler>();
        objects = GetComponent<ObjectsHandler>();
        rooms = GetComponent<RoomHandler>();

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
        Debug.Log("Client: " + connectionId + " recieved: " + string.Join("|", data));
        switch (data[0])
        {
            case "CONNECTED":
                //StartCoroutine(Loadlvl());
                Send("INIT", reliableChannel);
                break;
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
            case "MOVABLEOBJECTS":
                for (int i = 1; i < data.Length; i++)
                {
                    string[] d = data[i].Split('%');
                    Debug.Log(data[i]);
                    objects.SpawnObjects(int.Parse(d[0]), Instantiate(Resources.Load<GameObject>(d[1]), stringToVec(d[2]), stringToQuat(d[3])));
                }
                break;
            case "OBJECTS":
                for (int i = 2; i < data.Length; i++)
                {
                    string[] d = data[i].Split('%');
                    Debug.Log(data[i]);
                    GameObject o = Instantiate(Resources.Load<GameObject>(d[0]), stringToVec(d[1]), stringToQuat(d[3]));
                    o.transform.localScale = stringToVec(d[2]);
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
            case "LOCK":
                OnLockDataRecieved(data);
                break;
            case "ROOMRESOLVED":
                Debug.LogWarning("Recieved message: " + string.Join("|", data));
                rooms.OnDoorOpen(int.Parse(data[1]), int.Parse(data[2]));
                break;
            default:
                Debug.LogWarning("Invalid message: " + string.Join("|", data));
                break;
        }
    }

    /*IEnumerator Loadlvl()
    {
        //SceneManager.LoadScene("Main", LoadSceneMode.Single);
        //Debug.Log("Scene loaded");
        //yield return null;
        //players = GetComponent<PlayersHandler>();
        //objects = GetComponent<ObjectsHandler>();
        //rooms = GetComponent<RoomHandler>();
        
    }*/

    private void OnLockDataRecieved(string[] data)
    {
        LockHandler locker;
        Debug.LogError("Locker: " + string.Join("|", data));
        int lockId;
        if (int.TryParse(data[1], out lockId))
        {
            if ((locker = rooms.GetRoom(lockId).GetComponentInChildren<LockHandler>()) == null)
            {
                Debug.LogError("Locker not here: " + string.Join("|", data));
                return;
            }
        }
        else
        {
            string[] d = data[1].Split('%');
            switch (d[0])
            {
                case "COLOR":
                    locker = Resources.Load<GameObject>("Prefabs/ColorLock").GetComponent<KeyLockHandler>();
                    //locker.name = "ColorLock";
                    locker.gameObject.transform.parent = rooms.GetRoom(int.Parse(d[1])).transform;
                    locker.gameObject.transform.position = stringToVec(d[2]);
                    break;
                case "PAIR":
                    locker = new GameObject().AddComponent<PairLockHandler>();
                    locker.name = "PairLock";
                    locker.gameObject.transform.parent = rooms.GetRoom(int.Parse(d[1])).transform;
                    break;
                default:
                    Debug.LogError("Invalid message: " + string.Join("|", data));
                    return;
            }
        }
        switch (data[2])
        {
            case "ADD":
                //Debug.LogError(data[1]);
                Debug.LogError("Locker: " + string.Join("|", data));
                Debug.LogError(locker);
                //Debug.LogError(string.Join("|", data));
                int buttonCount = locker.transform.childCount - 4;
                for (int i = 4; i < data.Length; i++)
                {
                    string[] d = data[i].Split('%');
                    LockerButton button = Instantiate(Resources.Load<GameObject>(data[3])).AddComponent<LockerButton>();
                    button.gameObject.transform.parent = locker.gameObject.transform;
                    button.gameObject.transform.position = stringToVec(d[1]);
                    button.gameObject.transform.localScale = stringToVec(d[2]);
                    button.gameObject.transform.rotation = stringToQuat(d[3]);
                    button.id = buttonCount + i;// int.Parse(d[0]);
                    button.SetLockerHandler(locker);
                    Material mat = (Material)Resources.Load("Material/Blue", typeof(Material)); ;
                    //mat.SetColor("_SpecColor", Color.red);
                    button.GetComponent<LockerButton>().SetSelected(mat);

                }
                //Instantiate(Resources.Load<GameObject>(data[1]));
                break;
            case "PASSWORD":
                //Debug.LogError(data[1]);
                //PairLockHandler locker;
                Debug.LogError(string.Join("|", data));
                int[] password = Array.ConvertAll(data[3].Split('%'), delegate (string s) { return int.Parse(s); });
                Debug.LogError(password.Length);
                Debug.LogWarning("Invalid message: " + string.Join("|", data[3].Split('%')));
                locker.SetPassword(password, int.Parse(data[4]));
                //Instantiate(Resources.Load<GameObject>(data[1]));
                break;
            default:
                Debug.LogWarning("Invalid message: " + string.Join("|", data));
                break;
        }
    }

    private void OnAskName(string[] data)
    {
        players.setOurPlayerId(int.Parse(data[1]));

        Send("NAMEIS|" + players.getPlayerName(connectionId), reliableChannel);

        for (int i = 2; i < data.Length; i++)
        {
            string[] d = data[i].Split('%');
            players.SpawnPlayer(d[0], int.Parse(d[1]));

        }
    }

    public void OnRoomResolving(int roomId)
    {
        Debug.LogWarning("Resolved Room: " + roomId);
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
        Debug.Log("Client Sending: " + message);
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
