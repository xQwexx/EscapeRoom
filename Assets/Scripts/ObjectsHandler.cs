using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsHandler : MonoBehaviour {
    private GameObject selectedObject;
    public GameObject baseObject;
    public GameObject player;
    public Dictionary<int, GameObject> objects = new Dictionary<int, GameObject>();
    private PlayerMotor playerMotor;

    private Client client;

    private bool isSelected = false;
    // Use this for initialization
    void Start () {
        playerMotor = player.GetComponent<PlayerMotor>();
        //Debug.Log(" alksdfasssssssssjh" + camera);
    }
	
	// Update is called once per frame
	void Update () {
        if (!isSelected && InputControl.GetButtonDown(Controls.buttons.fire2))
        {
            selectedObject = Instantiate(baseObject) as GameObject;
            selectedObject.AddComponent<ObjectController>().setHandler(this);
            isSelected = true;
        }
        if (selectedObject == null || !isSelected) return;
        //camera = player.GetComponentInChildren<Camera>().gameObject;
        //objectHandlerSystem.OnGrabObject(selectedObject);
        playerMotor.onGrabObject(selectedObject);

        client.OnGrabObject(selectedObject.GetComponent<ObjectController>().id, selectedObject);


    }

    internal void SetClient(Client client)
    {
        this.client = client;
    }

    public void OnObjectSelected(GameObject selected)
    {
        Debug.Log("Selected: " + selected);

        if (!isSelected && selected != null)
        {
            selectedObject = selected;
            Debug.Log("Changed");
            isSelected = true;
            Update();
        }
        else if (isSelected)
        {
            selectedObject = null;
            Debug.Log("Drop");
            isSelected = false;
            Update();
        }
    }

    public void SetObjectPlace(int objId, Vector3 pos, Quaternion quat)
    {
        objects[objId].transform.position = pos;
        objects[objId].transform.rotation = quat;
    }
    public GameObject GetObject(int objId)
    {
        return objects[objId];
    }

    internal void SpawnObjects(int objId, GameObject obj)
    {
        obj.AddComponent<ObjectController>().setHandler(this);
        obj.GetComponent<ObjectController>().id = objId;
        objects.Add(objId, obj);
    }

}
