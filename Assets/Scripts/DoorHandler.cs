using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DoorHandler : MonoBehaviour {

    private bool isClose = true;
    float smooth = 2.0f;
    float DoorOpenAngle = 0.0f;
    float DoorCloseAngle = 90.0f;
    public GameObject door;
    // Use this for initialization
    void Start () {
        DoorCloseAngle = door.transform.rotation.y;
        DoorOpenAngle = DoorCloseAngle + 90.0f;
    }
	
	// Update is called once per frame
	void Update () {
        if (isClose)
        {

            var target = Quaternion.Euler(0, DoorCloseAngle, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * smooth);
        }
        else
        {
            //transform.Rotate(Vector3.right, Space.World);

            var target1 = Quaternion.Euler(0, DoorOpenAngle, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, target1, Time.deltaTime * smooth);
        }

    }
    public void OnDoorAction()
    {
        Debug.Log("Door Opening");
        isClose = !isClose;
    }
}
