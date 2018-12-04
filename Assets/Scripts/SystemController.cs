using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;

public class SystemController : MonoBehaviour {

    AsyncOperation asyncLoadLevel;
    private Text statusOutput;
    bool created = false;
    bool isServer = false;
    string ipAddress;
    string pName;
    bool isVrEnabled;
    // Use this for initialization
    void Start () {
        statusOutput = GameObject.Find("StatusOutput").GetComponent<Text>();
    }
	

    void Awake()
    {
        if (!created)
        {
            DontDestroyOnLoad(this.gameObject);
            //created = true;
            Debug.Log("Awake: " + this.gameObject);
        }
    }

    public void CreateServer()
    {
        statusOutput.text = "Loading...";
        pName = GameObject.Find("NameInput").GetComponent<InputField>().text;
        ipAddress = "127.0.0.1";
        isVrEnabled = false;
        isServer = true;
        //StartCoroutine(LoadDevice("cardboard"));
        StartCoroutine(LoadLevel("Server", "Cardboard"));
        
    }
    public void ConnectToServer()
    {
        statusOutput.text = "Loading...";
        pName = GameObject.Find("NameInput").GetComponent<InputField>().text;
        if (pName == "")
        {
            statusOutput.text = "You must enter a name!";
            return;
        }
        ipAddress = "127.0.0.1";
        isVrEnabled = true;
        ///StartCoroutine(LoadDevice("Cardboard"));

        StartCoroutine(LoadLevel("Server", "Cardboard"));
        //Debug.LogWarning("Load: Main " + GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkConnection>());
        //GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkConnection>().ConnectServer(ipAddress);
    }
    public void Exit()
    {
        isVrEnabled = false;
        StartCoroutine(LoadLevel("GUI", "None"));
    }

    IEnumerator LoadLevel(string sceneName, string device)
    {
        if (String.Compare(XRSettings.loadedDeviceName, device, true) != 0)
        {
            XRSettings.LoadDeviceByName(device);
            yield return null;
            XRSettings.enabled = isVrEnabled;
        }
        asyncLoadLevel = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!asyncLoadLevel.isDone && String.Compare(XRSettings.loadedDeviceName, device, true) != 0)
        {
            print("Loading the Scene: " + sceneName);
            print("Loading the Device: " + device);
            yield return null;   
        }
        //gameObject.AddComponent<ViewerServer>();
        GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<Player>().SetPlayerName(pName);
        //ViewerServer gameSystem = 
        // GameObject.FindGameObjectWithTag("GameController").
        // GetComponent<ViewerServer>().Init();
        Debug.LogError(GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<Player>());
        //Debug.LogWarning("Load: Main " + GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkConnection>());
        var network = GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkConnection>();
        if(isServer) network.CreateServer();
        network.ConnectServer(ipAddress);
    }
    

}
