using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{

    public string playerName;
    public string avatar;
    public Vector3 pos;
    public Vector3 headDir;
    public Vector3 moveDir;

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
    public void SetPlayerName(string name)
    {
        playerName = name;
    }
}
