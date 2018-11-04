using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyLockHandler : LockHandler {
    void Start()
    {
        room = GetComponentInParent<Room>();
        password.Add(1);
        password.Add(1);
        password.Add(2);
    }

    public override void OnButtonSelected(int selected)
    {
        indvBuffer.Add(selected);

        int countertemp = counter;
        for (int i = counter; i < indvBuffer.Count; i++)
        {
            if (indvBuffer[i] == password[i]) countertemp++;
            else
            {
                indvBuffer.RemoveRange(counter, countertemp - counter +1);
                countertemp = counter;
            }
        }
        if (counter + indvNumber == countertemp) counter = countertemp;
        if (counter == password.Count)
        {
            counter = 0;
            indvBuffer.RemoveRange(0, indvBuffer.Count);
            room.OnRoomResolved();
            
        }

    }

    public override void OnButtonDeselected(int selected)
    {
        return;
    }


}
