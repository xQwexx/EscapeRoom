using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
        isServer = true;
        //StartCoroutine(LoadDevice("cardboard"));
        StartCoroutine(LoadLevel("Server", "Cardboard", true));
        
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
        ///StartCoroutine(LoadDevice("Cardboard"));

        StartCoroutine(LoadLevel("Server", "Cardboard", true));
        //Debug.LogWarning("Load: Main " + GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkConnection>());
        //GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkConnection>().ConnectServer(ipAddress);
    }
    public void Exit()
    {
        StartCoroutine(LoadLevel("GUI", "", false));
    }

    IEnumerator LoadLevel(string sceneName, string device, bool isVrEnabled)
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

        GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<Player>().SetPlayerName(pName);


        EventTrigger trigger = GameObject.FindGameObjectWithTag("Exit").AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((d) => { Exit(); });
        trigger.triggers.Add(entry);
        //Debug.LogWarning("Load: Main " + GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkConnection>());
        var network = GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkConnection>();
       
        if (isServer) network.CreateServer();
        network.ConnectServer(ipAddress);
    }
    

}
