using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public string playerName;
    //public static string avatar;
    public Vector3 pos;
    public Vector3 headDir;
    public Vector3 moveDir;

    public void setPlayerData(Vector3 pos, Vector3 moveDir, Vector3 headDir)
    {
        this.pos = pos;
        this.moveDir = moveDir;
        this.headDir = headDir;
    }
}
