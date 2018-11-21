using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkConnection : MonoBehaviour {

    private const int MAX_CONNECTION = 5;
    private int port = 5701;

    //private int hostId;
    //private int serverHostId;
    private int webHostId;

    public int reliable;
    public int unreliable;

    private bool isStarted = false;
    private byte error;
    private string ipAddress;

    private float lastMovementUpdate;
    private float movementUpdateRate = 0.05f;

    private Server server = null;
    private Client client = null;

    // Use this for initialization
    public void CreateServer() {
        server = new Server();
        

        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        reliable = cc.AddChannel(QosType.Reliable);
        unreliable= cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        server.Init(this, NetworkTransport.AddHost(topo, port, null));
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
        client = new Client();
        

        NetworkTransport.Init();
        ConnectionConfig myConfig = new ConnectionConfig();

        reliable = myConfig.AddChannel(QosType.Reliable);
        unreliable = myConfig.AddChannel(QosType.Unreliable);
        //myConfig.ConnectTimeout = 1000;
        //myConfig.MaxConnectionAttempt = 5;

        HostTopology topo = new HostTopology(myConfig, MAX_CONNECTION);

        int hostId = NetworkTransport.AddHost(topo, 0, null);
        //"192.168.0.175""127.0.0.1"
        Debug.Log("Connect: " + serverIp);
        //connectionTime = Time.time;
        //isConnected = true;

        isStarted = true;

        client.Init(this, hostId, NetworkTransport.Connect(hostId, serverIp, port, 0, out error));
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
                case NetworkEventType.ConnectEvent:
                    Debug.Log("Player " + cnnId + " has connected");
                    //if (cnnId != serverHostId)
                    //{
                        //Send("CONNECTED", reliable, cnnId);
                        server.OnConnection(cnnId);
                    //}
                    string hostName = System.Net.Dns.GetHostName();
                    for (int i = 0; i < System.Net.Dns.GetHostEntry(hostName).AddressList.Length; i++)
                    {
                        string localIP = System.Net.Dns.GetHostEntry(hostName).AddressList[i].ToString();//NetworkManager.singleton.networkAddress
                        //Send(localIP, reliable, cnnId);
                    }
                    noEventsLeft = true;
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    //Debug.Log("Player " + cnnId + " has sent: " + msg);
                    string[] splitData = msg.Split('|');
                    client.OnDataRecieved(server.OnDataRecieved(cnnId, splitData));

                    break;
                case NetworkEventType.DisconnectEvent:
                    server.RemovePlayer(cnnId);
                    break;

                case NetworkEventType.BroadcastEvent:

                    break;
            }

        }
        if (Time.time - lastMovementUpdate > movementUpdateRate)
        {
            lastMovementUpdate = Time.time;
            server.Refresh();
        }
    }

    public void Send(int hostId, string message, int channelId, int cnnId)
    {
        Debug.Log("HostId: " + hostId + " Sending: " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostId, cnnId, channelId, msg, message.Length * sizeof(char), out error);
    }

    public void Send(int hostId, string message, int channelId, List<int> clients)
    {
        Debug.Log("HostId: " + hostId + " Sending: " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        foreach (int cnnId in clients)
        {
            NetworkTransport.Send(hostId, cnnId, channelId, msg, message.Length * sizeof(char), out error);
        }
    }
}
