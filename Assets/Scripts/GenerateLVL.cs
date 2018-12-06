using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class GenerateLVL {
    const int PAIR_ITEM_COUNT = 1;
    const int PAIR_ITEM_PIECE_COUNT = 30;
    const string PAIR_ITEM_PREFAB = "Prefabs/Obstacle";

    public List<RoomData> roomsData = new List<RoomData>();

    public enum ItemType
    {
        PairLocker,
        ColorLocker,
        ImageLocker,
        LockerButton,
        None
    }

    public struct Item
    {
        public ItemType type;
        public string prefabName;
        public int id;
        public Vector3 position;
        public Vector3 localScale;
        public Quaternion rotation;
    }
    public struct RoomData{
        public Room room;
        public int neededResult;
        public Item locker;
        public List<Item> player1;
        public List<Item> player2;
        public int[] password;
        public int pswSegment;
    }
    

    public void Generate(RoomHandler rooms)
    {
        List<int> lvlNumber = new List<int>();
        foreach (Room room in rooms.GetRooms())
        {
            roomsData.Add(new RoomData { room = room});
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
                    CreatePair(i);
                    break;
                case 1:
                    
                    ColorText(i);
                    break;
                case 2:
                    Item locker = new Item();
                    locker.type = ItemType.ImageLocker;
                    locker.prefabName = "Prefabs/Images";
                    locker.position = roomsData[i].room.transform.position;

                    RoomData data = new RoomData();
                    data.locker = locker;
                    data.password = new int[2];
                    int count = Resources.Load<GameObject>("Prefabs/Images").transform.childCount;
                    data.password[0] = Random.Range(0, count);
                    data.password[1] = Random.Range(0, count);
                    data.room = roomsData[i].room;
                    data.neededResult = 2;
                    roomsData[i] = data;

                    break;
                default:
                    break;
            }
            lvlNumber.RemoveAt(index);
        }

    }

    private void CreatePair(int roomIndex)
    {
        
        float border = 0.3f;
        Vector3 dim = roomsData[roomIndex].room.GetRoomDimension();
        Item locker = new Item();
        locker.type = ItemType.PairLocker;
        locker.position = roomsData[roomIndex].room.transform.position + new Vector3(-(dim.x / 2 - 0.2f), dim.y / 2, dim.z / 4);

        RoomData data = new RoomData();
        data.player1 = new List<Item>();
        data.player2 = new List<Item>();
        data.password = new int[2 * PAIR_ITEM_COUNT];
        data.pswSegment = 2;
        data.room = roomsData[roomIndex].room;
        data.neededResult = 1;

        Vector3 roomPos = roomsData[roomIndex].room.transform.position;
        for (int i = 0; i < PAIR_ITEM_PIECE_COUNT; i++)
        {
            Vector3 pos = new Vector3();
            pos.x = Random.Range(roomPos.x - dim.x/2 + border, roomPos.x + dim.x/2 - border);
            pos.y = 0;
            pos.z = Random.Range(roomPos.z - dim.z / 2 + border, roomPos.z + dim.z / 2 - border);

            Item temp = new Item();
            temp.type = ItemType.LockerButton;
            temp.prefabName = PAIR_ITEM_PREFAB;
            temp.position = pos;
            temp.id = i;

            data.player1.Add(temp);
        }
        
        int counter = 0;

        while (counter < PAIR_ITEM_COUNT)
        {
            bool newPair = true;
            int pair1, pair2;
            pair1 = Random.Range(0, PAIR_ITEM_PIECE_COUNT);
            pair2 = Random.Range(0, PAIR_ITEM_PIECE_COUNT);
            if (pair1 == pair2) newPair = false;


            foreach (int item in data.password)
            {
                if (item == pair1 || item == pair2) newPair = false;
            }
            if (newPair == true)
            {
                data.password[counter * 2] = pair1;
                data.password[counter * 2 + 1] = pair2;
                counter++;
                //Debug.LogError(pair);
                Item link = new Item();
                link.type = ItemType.None;

                Vector3 between = data.player1[pair1].position - data.player1[pair2].position;
                link.localScale = new Vector3(0.1f, 0.1f, between.magnitude);
                link.prefabName = PAIR_ITEM_PREFAB;

                GameObject temp = new GameObject();
                temp.transform.position = data.player1[pair2].position + (between / 2.0f);
                temp.transform.LookAt(data.player1[pair1].position);
                link.rotation = temp.transform.rotation.normalized;
                link.position = new Vector3(temp.transform.position.x, 4.9f, temp.transform.position.z);
                data.player2.Add(link);
            }
        }

        
        data.locker = locker;

        roomsData[roomIndex] = data;
    }
    void ColorText(int roomIndex)
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
                    formatedResult += "<color=blue>";
                    break;
                case 1:
                    formatedResult += "<color=red>";
                    break;
                case 2:
                    formatedResult += "<color=yellow>";
                    break;
                case 3:
                    formatedResult += "<color=green>";
                    break;
            }
            formatedResult += key[keyOrder[i]];
            formatedResult += "</color> ";
            password[i] = keyCode[i];
        }
        Item locker = new Item();
        locker.type = ItemType.ColorLocker;
        locker.prefabName = "Prefabs/ColorLock";
        Vector3 dim = roomsData[roomIndex].room.GetRoomDimension();
        locker.position = roomsData[roomIndex].room.transform.position + new Vector3(-(dim.x / 2 - 0.2f), dim.y / 2, dim.z / 4);

        Item text = new Item();
        text.type = ItemType.None;
        text.prefabName = formatedResult;
        text.position = roomsData[roomIndex].room.transform.position + new Vector3(0, dim.y / 2, 0);// + new Vector3((dim.x / 2 - 0.2f), dim.y / 2, dim.z / 4);

        RoomData data = new RoomData();
        data.locker = locker;
        data.password = password;
        data.pswSegment = password.Length;
        data.room = roomsData[roomIndex].room;
        data.neededResult = 1;
        data.player2 = new List<Item>();
        data.player2.Add(text);
        roomsData[roomIndex] = data;
         

        
    }
    static string[] sentence = {"Two wrongs don't make a right",
        "The pen is mightier than the sword",
        "When in Rome, do as the Romans",
        "The squeaky wheel gets the grease",
        "When the going gets tough, the tough get going",
        "No man is an island",
        "Fortune favors the bold",
        "People who live in glass houses should not throw stones",
        "Hope for the best, but prepare for the worst",
        "Better late than never",
        "Birds of a feather flock together",
        "Keep your friends close and your enemies closer",
        "A picture is worth a thousand words",
        "There's no such thing as a free lunch",
        "There's no place like home",
        "Discretion is the greater part of valor",
        "The early bird catches the worm",
        "Never look a gift horse in the mouth",
        "You can't make an omelet without breaking a few eggs",
        "God helps those who help themselves",
        "You can't always get what you want",
        "Cleanliness is next to godliness",
        "A watched pot never boils",
        "Beggars can't be choosers",
        "Actions speak louder than words",
        "If it ain't broke, don't fix it",
        "Practice makes perfect",
        "Too many cooks spoil the broth",
        "Easy come, easy go",
        "Don't bite the hand that feeds you",
        "All good things must come to an end",
        "If you can't beat 'em, join 'em",
        "One man's trash is another man's treasure",
        "There's no time like the present",
        "Beauty is in the eye of the beholder",
        "Necessity is the mother of invention",
        "A penny saved is a penny earned.",
        "Familiarity breeds contempt",
        "You can't judge a book by its cover",
        "Good things come to those who wait",
        "Don't put all your eggs in one basket",
        "Two heads are better than one",
        "The grass is always greener on the other side of the hill",
        "Do unto others as you would have them do unto you",
        "A chain is only as strong as its weakest link",
        "Honesty is the best policy",
        "Absence makes the heart grow fonder",
        "You can lead a horse to water, but you can't make him drink",
        "Don't count your chickens before they hatch",
        "If you want something done right, you have to do it yourself"};
}
