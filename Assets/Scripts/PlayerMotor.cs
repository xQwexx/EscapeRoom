using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour {

    private CharacterController controller;
    public float PlayerSpeed = 3;


    // Use this for initialization
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3();

        movement.x = InputControl.GetAxis(Controls.axes.horizontal) * PlayerSpeed;

        if (InputControl.GetButton(Controls.buttons.jump))
        {
            movement.y = 5;
        }
        else
        {
            movement.y = -5;
        }

        movement.z = InputControl.GetAxis(Controls.axes.vertical) * PlayerSpeed;

        controller.Move(movement * Time.deltaTime);
    }
}
