using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string playerName;
    public string avatar;
    public Vector3 pos { get; set; }
    public Vector3 headDir { get; set; }
    //public Quaternion moveDir { get; set; }
    public Vector3 moveDir { get; set; }

    public GameObject head;
    public GameObject body;

    public float moveSpeed = 2f;
    public float rotationSpeed = 3f;

    void Update()
    {
        /*
        if (!isHostPlayer)
        {
            transform.position = pos;
            head.transform.rotation = headDir;
            body.transform.rotation = moveDir;
        }*/

        //transform.position = pos;
            Quaternion newRotation = Quaternion.LookRotation(headDir);
            head.transform.rotation = Quaternion.Slerp(head.transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
        if (moveDir != Vector3.zero)
        {
            newRotation = Quaternion.LookRotation(moveDir);
            body.transform.localRotation = Quaternion.Slerp(body.transform.localRotation, newRotation, rotationSpeed * Time.deltaTime);
            body.transform.Rotate(Vector3.up, newRotation.y);
            //moveDir = body.transform.localRotation;
            //sssDebug.LogError(moveDir);
            
            
            //headDir = Quaternion.LookRotation(camera.transform.forward);
            //head.transform.localRotation = headDir;
        }


        //if (InputControl.GetButton(Controls.buttons.fire1)) ExecuteEvents.Execute<IPointerClickHandler>(raytrace.GetCurrentGameObject(), new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
    }
    public override string ToString()
    {
        return pos.ToString() + '$' + moveDir.ToString() + '$' + headDir.ToString();
    }
    public void StringToPlayer(string player)
    {

        string[] data = player.Split('$');
        pos = stringToVec(data[0]);
        moveDir = stringToVec(data[1]);
        headDir = stringToVec(data[2]);
    }


    private Vector3 stringToVec(string s)
    {
        string[] temp = s.Substring(1, s.Length - 2).Split(',');
        return new Vector3(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
    }
}
