﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public PlayerData data = new PlayerData();
    public GameObject head;
    public GameObject body;

    public float moveSpeed = 2f;
    public float rotationSpeed = 3f;

    void Update()
    {
        gameObject.transform.position = data.pos;
        /*
        if (!isHostPlayer)
        {
            transform.position = pos;
            head.transform.rotation = headDir;
            body.transform.rotation = moveDir;
        }*/

        //transform.position = pos;
            Quaternion newRotation = Quaternion.LookRotation(data.headDir);
            head.transform.rotation = Quaternion.Slerp(head.transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
        if (data.moveDir != Vector3.zero)
        {
            //Debug.LogError(moveDir);
            newRotation = Quaternion.LookRotation(data.moveDir);
            //Debug.LogError(newRotation);
            body.transform.localRotation = Quaternion.Slerp(body.transform.localRotation, newRotation, rotationSpeed * Time.deltaTime);
            //Debug.LogError(body.transform.localRotation);
            //body.transform.Rotate(Vector3.up, newRotation.y);
            //moveDir = body.transform.localRotation;
            //sssDebug.LogError(moveDir);
            
            
            //headDir = Quaternion.LookRotation(camera.transform.forward);
            //head.transform.localRotation = headDir;
        }


        //if (InputControl.GetButton(Controls.buttons.fire1)) ExecuteEvents.Execute<IPointerClickHandler>(raytrace.GetCurrentGameObject(), new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
    }
    public void SetPlayerName(string name)
    {
        data.playerName = name;
        GetComponentInChildren<Text>().text = name;
    }

}
