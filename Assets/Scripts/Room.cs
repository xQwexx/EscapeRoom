using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {
    public DoorHandler[] doors;

    RoomHandler handler;
    public int id { get; set; }
    private Vector3 dim;

	// Use this for initialization
	void Start () {
        doors = GetComponentsInChildren<DoorHandler>();
        handler = FindObjectOfType<RoomHandler>();
        handler.AddRoom(this);

        foreach (var item in GetComponentsInChildren<Collider>())
        {
            dim.x = ((item.bounds.size.x).CompareTo(dim.x) == 1) ? item.bounds.size.x : dim.x;
            dim.y = ((item.bounds.size.y).CompareTo(dim.y) == 1) ? item.bounds.size.y : dim.y;
            dim.z = ((item.bounds.size.z).CompareTo(dim.z) == 1) ? item.bounds.size.z : dim.z;
        }
    }
	
    public void OnDoorOpen(int doorId)
    {
        if (doorId > doors.Length -1) doorId = 0;
        doors[doorId].OnDoorAction();
    }

    public void OnRoomResolved()
    {
        handler.OnRoomResolved(id);
    }

    public void AddObject(GameObject child)
    {
    }
    public Vector3 GetRoomDimension()
    {
        return dim;
    }
}
