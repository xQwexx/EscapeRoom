using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class PlayersHandler : MonoBehaviour {
    private int ourPlayerId;

    public GameObject playerAvatarPrefab;
    public GameObject player;
    public Dictionary<int, Player> players = new Dictionary<int, Player>();

    
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void SpawnPlayer(string playerName, int cnnId)
    {
        Player go;

        if (cnnId == ourPlayerId)
        {
            go = player.GetComponentInChildren<Player>();
            //GameObject.Find("Canvas").SetActive(false);
        }
        else
        {
            go = Instantiate<GameObject>(playerAvatarPrefab).GetComponent<Player>();
            go.SetPlayerName(playerName);
            //go.AddComponent<Player>();
        }

        //Player p = new Player();
        //p.playerName = playerName;
        //p.avatar.GetComponentInChildren<TextMesh>().text = playerName;
        players.Add(cnnId, go);
    }

    public void PlayerDisconnected(int cnnId)
    {
        Destroy(players[cnnId]);
        players.Remove(cnnId);
    }

     public string setPlayersPosition(string[] data)
    {

        for (int i = 1; i < data.Length; i++)
        {
            string[] d = data[i].Split('%');
            if (int.Parse(d[0]) != ourPlayerId)
            {
                setPlayerPosition(int.Parse(d[0]), d[1]);
            }
        }
        //Debug.Log("hhhhhhhhhhhhhhhhhhhhhhhhhhh" + players[ourPlayerId].transform.position.ToString());
        return players[ourPlayerId].ToString();
    }


   public void setPlayerPosition(int cnnId, string playerData)
    {
        if (cnnId == ourPlayerId) return;
        //players[cnnId].data.pos = players[cnnId].transform.position = stringToVec(data[0]);
        players[cnnId].StringToPlayer(playerData);
    }

    public Player getPlayerPosition(int cnnId)
    {
        return players[cnnId].GetComponent<Player>();
    }

    public void setOurPlayerId(int cnnId)
    {
        ourPlayerId = cnnId;
    }

    public int getOurPlayerId()
    {
        return ourPlayerId;
    }
    public string getPlayerName(int cnnId)
    {
        if (!players.ContainsKey(cnnId)) return "Anonymus";
        return players[cnnId].playerName;
    }

    public void setPlayerName(int cnnId, string name)
    {
        players[cnnId].SetPlayerName(name);
    }

    private Vector3 stringToVec(string s)
    {
        string[] temp = s.Substring(1, s.Length - 2).Split(',');
        return new Vector3(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
    }

}
