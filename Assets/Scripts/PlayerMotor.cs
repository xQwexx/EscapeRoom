using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMotor : MonoBehaviour
{

    private CharacterController controller;
    private Camera playerCamera;
    private Player avatar;

    

    // Use this for initialization
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        avatar = GetComponentInChildren<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection = playerCamera.transform.right * InputControl.GetAxis(Controls.axes.horizontal) * avatar.moveSpeed;

        avatar.headDir = playerCamera.transform.forward;

        moveDirection += playerCamera.transform.forward * InputControl.GetAxis(Controls.axes.vertical) * avatar.moveSpeed;

        moveDirection.y = 0;
        avatar.moveDir = moveDirection;


        if (InputControl.GetButton(Controls.buttons.jump))
        {
            moveDirection.y += 5;
        }
        else
        {
            moveDirection.y += -5;
        }
        controller.Move(moveDirection* Time.deltaTime);
        avatar.pos = transform.position;

        

        //if (InputControl.GetButton(Controls.buttons.fire1)) ExecuteEvents.Execute<IPointerClickHandler>(raytrace.GetCurrentGameObject(), new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
    }
    /*public Transform getCameraTranform()
    {
        //Debug.Log(" alkdasssssssssssssssssssssssssssssssssssssssssssssdfjh" + camera.transform);
        return camera.transform;
    }*/

    public void onGrabObject(GameObject selectedObject)
    {
        selectedObject.transform.position = transform.position + playerCamera.transform.forward * 2f;
        //Debug.Log(" utania" + selectedObject.transform.position);
        Vector3 rotation = new Vector3();
        rotation = playerCamera.transform.up * InputControl.GetAxis(Controls.axes.mouseX);
        rotation -= playerCamera.transform.forward * InputControl.GetAxis(Controls.axes.mouseY);
        selectedObject.transform.Rotate(rotation * 2000f * Time.deltaTime, Space.Self);
    }
}
