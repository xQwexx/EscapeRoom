using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHandler : MonoBehaviour {

    public GameObject player;
    private GameObject selectedObject;
    private Camera camera;
    private bool isSelected = false;
	// Use this for initialization
	void Start () {
        camera = GetComponentInChildren<Camera>();
    }
	
	// Update is called once per frame
	void Update () {
        if (!isSelected) return;

        selectedObject.transform.position = player.transform.position + camera.transform.forward * 2f;
        Vector3 rotation = new Vector3();
        rotation.x = -InputControl.GetAxis(Controls.axes.mouseX) ;
        rotation.y = InputControl.GetAxis(Controls.axes.mouseY);
        selectedObject.transform.Rotate(rotation* 200f * Time.deltaTime);
        //selectedObject.transform.Rotate(Vector3.right * Time.deltaTime);
    }

    public void OnObjectSelected(GameObject selected)
    {
        if (!isSelected)
        {
            selectedObject = selected;
            isSelected = true;
        }
        else
        {
            isSelected = false;
            selectedObject = null;
        }

    }
}
