using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PairLockHandler : LockHandler {

    public override void OnButtonSelected(int selected)
    {
        indvBuffer.Add(selected);
        gameObject.transform.GetChild(selected).GetComponent<LockerButton>().OnSelected(true);
        counter = 0;
        for (int i = 0; i < password.Count / indvNumber; i++)
        {
            int countertemp = counter;
            for (int j = 0; j < indvBuffer.Count / indvNumber; j++)
            {
                for (int k = 0; k < indvNumber; k++)
                {
                    for (int kl = 0; kl < indvNumber; kl++)
                    {
                        if (indvBuffer[j * indvNumber + kl] == password[i * indvNumber + k]) countertemp++;
                    }
                }
            }
            if (counter + indvNumber == countertemp) counter = countertemp;
            else return;

        }
        if (counter == password.Count && password.Count == indvBuffer.Count)
        {
            Debug.Log("Room Resolved" + room);
            room.OnRoomResolved();
            counter = 0;
            //indvBuffer.RemoveRange(0, indvNumber);
        }
    }

    public override void OnButtonDeselected(int id)
    {
        for (int i = 0; i < indvBuffer.Count; i++)
        {
            if (indvBuffer[i] == id)
            {
                int blockCount = i / indvNumber * indvNumber;
                int blockRange = indvBuffer.Count - blockCount < indvNumber ? indvBuffer.Count - blockCount : indvNumber;
                for (int j = blockCount; j < blockCount + blockRange; j++)
                {
                    Debug.Log(j + " index delete" + indvBuffer[j]);
                    gameObject.transform.GetChild(indvBuffer[j]).GetComponent<LockerButton>().OnSelected(false);
                }
                indvBuffer.RemoveRange(blockCount, blockRange);
            }
        }
    }
}
