using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageHandler : LockHandler {
    public override void OnButtonDeselected(int selected)
    {
       
    }

    public override void OnButtonSelected(int selected)
    {
        if (selected == password[0]) room.OnRoomResolved();
        else room.OnRoomNotResolved();
    }



}
