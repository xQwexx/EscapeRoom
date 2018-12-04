using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomHandler : MonoBehaviour {


    private Dictionary<int,Room> rooms = new Dictionary<int, Room>();
    private Client client;
    private int roomCount = 0;

	// Use this for initialization
	void Start () {
        
        Room[] localrooms = GetComponentsInChildren<Room>();
        foreach(var room in localrooms)
        {
            roomCount++;
            rooms.Add(room.id, room);
        }
    }
    public void SetClient(Client client)
    {
        this.client = client;
    }


    public void OnDoorOpen(int roomId, int doorId)
    {
        rooms[roomId].OnDoorOpen(doorId);
    }

    public void AddRoom(Room room)
    {
        
        rooms.Add(room.id, room);
    }

    public Room GetRoom(int roomId)
    {
        return rooms[roomId];
    }

    public Room[] GetRooms()
    {
        Room[] roomsResult = new Room[roomCount];
        int i = 0;
        foreach (var room in rooms)
        {
            roomsResult[i++] = room.Value;
        }
        return roomsResult;
    }

    internal void OnRoomNotResolved(int roomId)
    {
        client.OnRoomNotResolving(roomId);
    }

    public int GetRoomCount()
    {
        return roomCount;
    }

    public void OnRoomResolved(int roomId)
    {
        client.OnRoomResolving(roomId);
    }

}
