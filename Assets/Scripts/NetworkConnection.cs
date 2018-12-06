using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkConnection : MonoBehaviour {
    enum NetworkState
    {
        None,
        Client,
        Server,
        ClientAndServer
    }
    NetworkState state = NetworkState.None;
    private static int MAX_CONNECTION = 5;
    private static int port = 5701;
    private static float lastMovementUpdate;
    private static float movementUpdateRate = 0.05f;
    private int hostId;
    //private int serverHostId;
    private int webHostId;

    public int reliable;
    public int unreliable;

    private bool isStarted = false;
    private byte error;
    private HostTopology topo;

    

    private Server server = null;
    private Client client = null;
    private List<int> cnnIds = new List<int>();

    // Use this for initialization
    public void CreateServer() {
        if (state != NetworkState.None) return;
        state = NetworkState.Server;
        server = new Server();
        server.Init(this);

        hostId = NetworkTransport.AddHost(topo, port, null);
        webHostId = NetworkTransport.AddWebsocketHost(topo, port, null);

        isStarted = true;
       // int p;
       // NetworkID net;
        //NodeID node;
        // hostId
       // NetworkTransport.GetConnectionInfo(webHostId, webHostId, out ipAddress, out p, out net, out node, out error);
    }

    public void ConnectServer(string serverIp)
    {
        if (state == NetworkState.Server) state = NetworkState.ClientAndServer;
        else if (state == NetworkState.None) state = NetworkState.Client;
        else return;
        client = gameObject.AddComponent<Client>();

        //if(isStarted) hostId = NetworkTransport.AddHost(topo, port, null);
        //"192.168.0.175""127.0.0.1"
        Debug.Log("Connect: " + serverIp);
        //connectionTime = Time.time;
        //isConnected = true;
        //Debug.Log("asdfkljahsdfklja;lkfj;adlksfj;lk:"+ NetworkTransport.Connect(hostId, serverIp, port, 0, out error));

        

        client.Init(this, (int)NetworkTransport.Connect(NetworkTransport.AddHost(topo, 0, null), serverIp, port, 0, out error));
        isStarted = true;
    }

    private void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig myConfig = new ConnectionConfig();

        reliable = myConfig.AddChannel(QosType.Reliable);
        unreliable = myConfig.AddChannel(QosType.Unreliable);
        //myConfig.ConnectTimeout = 1000;
        //myConfig.MaxConnectionAttempt = 5;

        topo = new HostTopology(myConfig, MAX_CONNECTION);
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
        int counter = 5;
        while (!noEventsLeft || counter-- > 0)
        {
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out cnnId, out channelId, recBuffer, bufferSize, out dataSize, out error);

            if ((NetworkError)error != NetworkError.Ok)
            {
                Debug.LogError("NetworkTransport error: " + error);
                return;
            }

            switch (recData)
            {
                case NetworkEventType.Nothing:
                    noEventsLeft = true;
                    break;
                case NetworkEventType.ConnectEvent:
                    //if (cnnIds.Contains(cnnId)) break;
                    cnnIds.Add(cnnId);
                    Debug.Log("Player " + recHostId + " has connected" + webHostId);
                    //if (cnnId != serverHostId)
                    // {
                    //NetworkTransport.Send(recHostId, cnnId, reliable, Encoding.Unicode.GetBytes("CONNECTED"), "CONNECTED".Length * sizeof(char), out error);

                    if (state == NetworkState.ClientAndServer || state == NetworkState.Server) server.OnInitMap(cnnId);
                    //}
                    string hostName = System.Net.Dns.GetHostName();
                    for (int i = 0; i < System.Net.Dns.GetHostEntry(hostName).AddressList.Length; i++)
                    {
                        string localIP = System.Net.Dns.GetHostEntry(hostName).AddressList[i].ToString();//NetworkManager.singleton.networkAddress
                        Send(localIP, reliable, cnnId);
                    }
                    noEventsLeft = true;
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    //Debug.Log("Player " + cnnId + " has sent: " + msg);
                    string[] splitData = msg.Split('|');
                    switch (state)
                    {
                        case NetworkState.None:
                            break;
                        case NetworkState.Client:
                            client.OnDataRecieved(splitData);
                            break;
                        case NetworkState.Server:
                            server.OnDataRecieved(cnnId, splitData);
                            break;
                        case NetworkState.ClientAndServer:
                            client.OnDataRecieved(server.OnDataRecieved(cnnId, splitData));
                            break;
                        default:
                            break;
                    }
                    

                    break;
                case NetworkEventType.DisconnectEvent:
                    if (state == NetworkState.ClientAndServer || state == NetworkState.Server) server.RemovePlayer(cnnId);
                    break;

                case NetworkEventType.BroadcastEvent:

                    break;
            }

        }
        if (Time.time - lastMovementUpdate > movementUpdateRate)
        {
            lastMovementUpdate = Time.time;
            if (state == NetworkState.ClientAndServer || state == NetworkState.Server) server.Refresh();
        }
    }

    public void Send(string message, int channelId, int cnnId)
    {
        List<int> c = new List<int>();
        c.Add(cnnId);
        Send(message, channelId, c);
    }

    public void Send(string message, int channelId, List<int> clients)
    {
        Debug.Log("HostId: " + hostId + " Sending: " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        foreach (int cnnId in clients)
        {
            NetworkTransport.Send(hostId, cnnId, channelId, msg, message.Length * sizeof(char), out error);
        }
    }
}
