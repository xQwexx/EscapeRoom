using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomHandler : MonoBehaviour {


    private List<Room> rooms = new List<Room>();
    private Client client;

	// Use this for initialization
	void Start () {
        client = GetComponent<Client>();
        /*
        Room[] localrooms = GetComponentsInChildren<Room>();
        foreach(var room in localrooms)
        {
            rooms.Add(room.id, room);
        }*/
    }


    public void OnDoorOpen(int roomId, int doorId)
    {
        rooms[roomId].OnDoorOpen(doorId);
    }

    public void AddRoom(Room room)
    {
        room.id = rooms.Count;
        rooms.Add(room);
    }

    public Room GetRoom(int roomId)
    {
        return rooms[roomId];
    }

    public void OnRoomResolved(int roomId)
    {
        client.OnRoomResolving(roomId);
    }

}
