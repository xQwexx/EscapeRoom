using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class GenerateLVL {
    const int PAIR_ITEM_COUNT = 1;
    const int PAIR_ITEM_PIECE_COUNT = 30;
    const string PAIR_ITEM_PREFAB = "Prefabs/Obstacle";

    public enum ItemType
    {
        Locker,
        LockerButton,
        None
    }

    public struct Item
    {
        public ItemType type;
        public string prefabName;
        public GameObject gObject;
    }
    public struct RoomData{
        public Room room;
        public List<Item> player1;
        public List<Item> player2;
        public int[] password;
        public int pswSegment;
    }
    public List<RoomData> roomsData = new List<RoomData>();

    LockHandler locker;
    public Material selectedMaterial;
    // Use this for initialization

    public void Generate(RoomHandler rooms)
    {
        List<int> lvlNumber = new List<int>();
        foreach (Room room in rooms.GetRooms())
        {
            roomsData.Add(new RoomData { room = room, player1 = new List<Item>(), player2 = new List<Item>(), password = new int[2 * PAIR_ITEM_COUNT] });
        }
        
        for (int i = 0; i < 3; i++)
        {
            lvlNumber.Add(i);
        }
        for (int i = 0; i < roomsData.Count; i++)
        {
            int index = Random.Range(0, lvlNumber.Count);

            switch (lvlNumber[index])
            {
                case 0:
                    Create(i);
                    break;
                case 1:
                    ColorText(i);
                    break;
                case 2:
                    ColorText(i);
                    break;
                default:
                    break;
            }
            lvlNumber.RemoveAt(index);
        }

    }

    public void Create(int roomIndex)
    {
        Vector3 pos;
        float border = 0.3f;
        Vector3 dim = roomsData[roomIndex].room.GetRoomDimension();
        locker = new GameObject().AddComponent<PairLockHandler>();
        locker.gameObject.SetActive(false);
        locker.transform.parent = roomsData[roomIndex].room.gameObject.transform;
        roomsData[roomIndex].player1.Add(new Item { type = ItemType.Locker, prefabName = "PairLockHandler", gObject = locker.gameObject });

        Vector3 roomPos = roomsData[roomIndex].room.transform.position;
        for (int i = 0; i < PAIR_ITEM_PIECE_COUNT; i++)
        {
            pos.x = Random.Range(roomPos.x - dim.x/2 + border, roomPos.x + dim.x/2 - border);
            pos.y = 0;
            pos.z = Random.Range(roomPos.z - dim.z / 2 + border, roomPos.z + dim.z / 2 - border);
            //Debug.LogError(obstacle);
            Item temp = new Item();
            temp.type = ItemType.LockerButton;
            temp.prefabName = PAIR_ITEM_PREFAB;
            temp.gObject = Resources.Load<GameObject>(PAIR_ITEM_PREFAB);
            temp.gObject.SetActive(false);
            //temp.gObject.transform.SetParent(locker.gameObject.transform);
            temp.gObject.transform.position = pos;
            temp.gObject.AddComponent<LockerButton>().id = i;
            temp.gObject.GetComponent<LockerButton>().SetSelected(selectedMaterial);
            
            roomsData[roomIndex].player1.Add(temp);
        }
        
        int counter = 0;
        //int[] password = new int[2 * pswLength];
        while (counter < PAIR_ITEM_COUNT)
        {
            bool newPair = true;
            int pair1, pair2;
            pair1 = Random.Range(0, PAIR_ITEM_PIECE_COUNT);
            pair2 = Random.Range(0, PAIR_ITEM_PIECE_COUNT);
            if (pair1 == pair2) newPair = false;
            //Debug.LogError(pair1 +"asdf"+ pair2);

            //Debug.LogError(pair);
            foreach (int item in roomsData[roomIndex].password)
            {
                if (item == pair1 || item == pair2) newPair = false;
            }
            if (newPair == true)
            {
                roomsData[roomIndex].password[counter * 2] = pair1;
                roomsData[roomIndex].password[counter * 2 + 1] = pair2;
                counter++;
                //Debug.LogError(pair);
                Item link = new Item();
                link.type = ItemType.None;
                link.gObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Vector3 between = roomsData[roomIndex].player1[pair1].gObject.transform.position - roomsData[roomIndex].player1[pair2].gObject.transform.position;
                link.gObject.transform.localScale = new Vector3(0.1f, 0.1f, between.magnitude);
                link.gObject.transform.position = roomsData[roomIndex].player1[pair2].gObject.transform.position + (between / 2.0f);
                link.gObject.GetComponent<Renderer>().material.color = Color.black;
                //transform.rotation = Quaternion.LookRotation(between);
                link.gObject.transform.LookAt(roomsData[roomIndex].player1[pair1].gObject.transform.position);
                link.gObject.transform.position = new Vector3(link.gObject.transform.position.x, 4.9f, link.gObject.transform.position.z);
                roomsData[roomIndex].player2.Add(link);
            }
        }
        locker.SetPassword(roomsData[roomIndex].password, roomsData[roomIndex].pswSegment);
    }
    string ColorText(int roomIndex)
    {
        int[] password;
        string[] key = sentence[Random.Range(0, sentence.Length)].ToUpper().Split(' ');
        int[] keyOrder = new int[key.Length];
        int[] keyCode = new int[key.Length];
        password = new int[key.Length];
        for (int i = 0; i < key.Length; i++)
        {
            keyOrder[i] = i;
            keyCode[i] = Random.Range(0, 4);
        }


        for (int n = key.Length - 1; n > 0; --n)
        {
            int k = Random.Range(0, n + 1);
            int temp = keyOrder[n];
            keyOrder[n] = keyOrder[k];
            keyOrder[k] = temp;
        }
        string formatedResult = "";
        for (int i = 0; i < keyOrder.Length; i++)
        {
            switch (keyCode[keyOrder[i]])
            {
                case 0:
                    formatedResult += "<color=yellow>";
                    break;
                case 1:
                    formatedResult += "<color=red>";
                    break;
                case 2:
                    formatedResult += "<color=blue>";
                    break;
                case 3:
                    formatedResult += "<color=green>";
                    break;
            }
            formatedResult += key[keyOrder[i]];
            formatedResult += "</color> ";
            password[i] = keyCode[keyOrder[i]];
        }
        return formatedResult;
    }
    static string[] sentence = {"Two wrongs don't make a right.",
        "The pen is mightier than the sword.",
        "When in Rome, do as the Romans.",
        "The squeaky wheel gets the grease.",
        "When the going gets tough, the tough get going.",
        "No man is an island.",
        "Fortune favors the bold.",
        "People who live in glass houses should not throw stones.",
        "Hope for the best, but prepare for the worst.",
        "Better late than never.",
        "Birds of a feather flock together.",
        "Keep your friends close and your enemies closer.",
        "A picture is worth a thousand words.",
        "There's no such thing as a free lunch.",
        "There's no place like home.",
        "Discretion is the greater part of valor.",
        "The early bird catches the worm.",
        "Never look a gift horse in the mouth.",
        "You can't make an omelet without breaking a few eggs.",
        "God helps those who help themselves.",
        "You can't always get what you want.",
        "Cleanliness is next to godliness.",
        "A watched pot never boils.",
        "Beggars can't be choosers.",
        "Actions speak louder than words.",
        "If it ain't broke, don't fix it.",
        "Practice makes perfect.",
        "Too many cooks spoil the broth.",
        "Easy come, easy go.",
        "Don't bite the hand that feeds you.",
        "All good things must come to an end.",
        "If you can't beat 'em, join 'em.",
        "One man's trash is another man's treasure.",
        "There's no time like the present.",
        "Beauty is in the eye of the beholder.",
        "Necessity is the mother of invention.",
        "A penny saved is a penny earned.",
        "Familiarity breeds contempt.",
        "You can't judge a book by its cover.",
        "Good things come to those who wait.",
        "Don't put all your eggs in one basket.",
        "Two heads are better than one.",
        "The grass is always greener on the other side of the hill.",
        "Do unto others as you would have them do unto you.",
        "A chain is only as strong as its weakest link.",
        "Honesty is the best policy.",
        "Absence makes the heart grow fonder.",
        "You can lead a horse to water, but you can't make him drink.",
        "Don't count your chickens before they hatch.",
        "If you want something done right, you have to do it yourself."};
}
