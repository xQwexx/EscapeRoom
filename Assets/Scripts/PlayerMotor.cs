using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour {

    private CharacterController controller;
    private Camera camera;
    public float PlayerSpeed = 3;


    // Use this for initialization
    void Start()
    {
        controller = GetComponent<CharacterController>();
        camera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3();

        movement = camera.transform.right * InputControl.GetAxis(Controls.axes.horizontal) * (PlayerSpeed/2);

        if (InputControl.GetButton(Controls.buttons.jump))
        {
            movement.y += 5;
        }
        else
        {
            movement.y += -5;
        }

        movement += camera.transform.forward * InputControl.GetAxis(Controls.axes.vertical) * PlayerSpeed;

        controller.Move(movement * Time.deltaTime);
    }
}
