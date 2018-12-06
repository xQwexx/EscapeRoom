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

    private int hostId;
    private NetworkConnection connection;


    private bool isStarted;

    private PlayersHandler players;
    private ObjectsHandler objects;
    private RoomHandler rooms;

    public void Init(NetworkConnection connection, int cnnId)
    {
        this.connection = connection;
        this.hostId = cnnId;

        isStarted = false;
        players = connection.gameObject.GetComponent<PlayersHandler>();
        objects = connection.gameObject.GetComponent<ObjectsHandler>();
        objects.SetClient(this);
        rooms = connection.gameObject.GetComponent<RoomHandler>();
        rooms.SetClient(this);
    }


    internal void OnGrabObject(int objId, GameObject selectedObject)
    {
        string msg = "GRABOBJECT|" + players.getOurPlayerId().ToString() + '|' + objId.ToString() + '|' + selectedObject.transform.position.ToString() + '|' + selectedObject.transform.rotation.ToString();
        connection.Send(msg, connection.unreliable, hostId);
    }


    

    public string[] OnDataRecieved(string[] data)
    {
        if (data == null) return null;
        if(!data[0].Equals("ASKPOS")) Debug.Log("Client: " + hostId + " recieved: " + string.Join("|", data));
        switch (data[0])
        {
            case "ASKPOS":
                if (!isStarted) return null;
                string msg = "MYPOS|" + players.setPlayersPosition(data);
                connection.Send(msg, connection.unreliable, hostId);
                break;
            case "CONNECTED":
                connection.Send("INIT", connection.reliable, hostId);
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
            /*case "MOVABLEOBJ":
                for (int i = 1; i < data.Length; i++)
                {
                    string[] d = data[i].Split('%');
                    Debug.Log(data[i]);
                    objects.SpawnObjects(int.Parse(d[0]), Instantiate(Resources.Load<GameObject>(d[1]), stringToVec(d[2]), stringToQuat(d[3])));
                }
                break;*/
            case "OBJECTS":
                for (int i = 2; i < data.Length; i++)
                {
                    string[] d = data[i].Split('%');
                    Debug.Log(data[i]);
                    GameObject o = Instantiate(Resources.Load<GameObject>(d[0]));
                    o.transform.position = stringToVec(d[1]);
                    o.transform.localScale = stringToVec(d[2]);
                    o.transform.rotation = stringToQuat(d[3]);
                }
                break;
            /*case "MOVEOBJECT":
                for (int i = 1; i < data.Length; i++)
                {
                    string[] d = data[i].Split('%');
                    Debug.Log(data[i]);
                    objects.SetObjectPlace(int.Parse(d[0]),  stringToVec(d[1]), stringToQuat(d[2]));
                }
                break;*/
            case "TEXT":
                {
                    string[] d = data[2].Split('%');
                    Debug.Log(data[2]);
                    GameObject t = Instantiate(Resources.Load<GameObject>("Prefabs/TextOutput"));
                    t.transform.position = stringToVec(d[1]);
                    t.transform.SetParent(rooms.GetRoom(int.Parse(data[1])).transform);
                    t.GetComponentInChildren<Text>().text = d[0];
                }
                break;
            case "GRABBED":
                objects.SetObjectPlace(int.Parse(data[2]), stringToVec(data[3]), stringToQuat(data[4]));
                break;
            case "LOCK":
                OnLockDataRecieved(data);
                break;
            case "RESOLVED":
                Debug.LogWarning("Recieved message: " + string.Join("|", data));
                rooms.OnDoorOpen(int.Parse(data[1]), int.Parse(data[2]));
                break;
            default:
                Debug.LogWarning("Invalid message: " + string.Join("|", data));
                break;
        }
        //Debug.Log("Cliens: " + hostId + " recieved from Server message: " + string.Join("|", data));
        return null;
    }

    private void OnLockDataRecieved(string[] data)
    {
        LockHandler locker;
        //Debug.LogError("Locker: " + string.Join("|", data));
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
                    locker = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ColorLock")).GetComponent<KeyLockHandler>();
                    locker.name = "ColorLock";
                    locker.gameObject.transform.parent = rooms.GetRoom(int.Parse(d[1])).transform;
                    locker.gameObject.transform.position = stringToVec(d[2]);
                    //locker.gameObject.transform.rotation = stringToQuat(d[3]);
                    break;
                case "PAIR":
                    locker = new GameObject().AddComponent<PairLockHandler>();
                    locker.name = "PairLock";
                    locker.gameObject.transform.parent = rooms.GetRoom(int.Parse(d[1])).transform;
                    break;
                case "IMAGE":
                    locker = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Images")).GetComponent<ImageHandler>();
                    locker.name = "ImageLock";
                    locker.gameObject.transform.parent = rooms.GetRoom(int.Parse(d[1])).transform;
                    locker.gameObject.transform.position = stringToVec(d[2]);
                    locker.SetPassword(new int[]{ int.Parse(data[2])}, 1);
                    //locker.gameObject.transform.rotation = stringToQuat(d[3]);
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
                //Debug.LogError("Locker: " + string.Join("|", data));
                //Debug.LogError(locker);
                //Debug.LogError(string.Join("|", data));
                int buttonCount = locker.transform.childCount - 4;
                for (int i = 4; i < data.Length; i++)
                {
                    string[] d = data[i].Split('%');
                    LockerButton button = Instantiate(Resources.Load<GameObject>(data[3])).AddComponent<LockerButton>();
                    button.gameObject.transform.parent = locker.gameObject.transform;
                    button.gameObject.transform.position = stringToVec(d[1]);
                    //button.gameObject.transform.localScale = stringToVec(d[2]);
                    //button.gameObject.transform.LookAt(stringToVec(d[3]));
                    button.id = buttonCount + i;// int.Parse(d[0]);
                    //button.SetLockerHandler(locker);
                    Material mat = (Material)Resources.Load("Material/Blue", typeof(Material)); ;
                    //mat.SetColor("_SpecColor", Color.red);
                    button.GetComponent<LockerButton>().selectedMaterial = mat;
                    button.gameObject.SetActive(true);

                }
                //Instantiate(Resources.Load<GameObject>(data[1]));
                break;
            case "PASSWORD":
                //Debug.LogError(data[1]);
                //PairLockHandler locker;
                Debug.LogError(string.Join("|", data));
                int[] password = Array.ConvertAll(data[3].Split('%'), delegate (string s) { return int.Parse(s); });
                //Debug.LogError(password.Length);
                //Debug.LogWarning("Invalid message: " + string.Join("|", data[3].Split('%')));
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
        //Debug.LogError("hostID: "+ data[1]);
       // if (int.Parse(data[1]) != hostId) return;
        players.setOurPlayerId(int.Parse(data[1]));

        connection.Send("NAMEIS|" + players.getPlayerName(int.Parse(data[1])), connection.reliable, hostId);

        for (int i = 2; i < data.Length; i++)
        {
            string[] d = data[i].Split('%');
            players.SpawnPlayer(d[0], int.Parse(d[1]));

        }
    }

    public void OnRoomResolving(int roomId)
    {
        Debug.LogWarning("Resolved Room: " + roomId);
        connection.Send("RESOLVING|" + roomId.ToString(), connection.reliable, hostId);
    }

    internal void OnRoomNotResolving(int roomId)
    {
        connection.Send("NOTRESOLVING|" + roomId.ToString(), connection.reliable, hostId);
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


    
    /*
    //Game Interface
    public void OnGrabObject(GameObject o)
    {
        Send("GRAB|" + ourClientId.ToString() + "|" + o.ToString() , unreliableChannel);
    }*/
}
