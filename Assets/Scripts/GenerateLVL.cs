using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class GenerateLVL : MonoBehaviour {

    public List<GameObject> player1 = new List<GameObject>();
    public List<GameObject> player2 = new List<GameObject>();
    Collection<Vector2Int> key = new Collection<Vector2Int>();
    public int[] password;
    public int individualNumber = 2;
    Vector3 dim;
    private GameObject obstacle;
    LockHandler locker;
    public Material selectedMaterial;
    // Use this for initialization
    void Start () {

        //dim = GetComponentsInChildren<Collider>().bounds.size;// Vector3.Scale(transform.localScale, ); //gameObject.transform.TransformPoint(1, 1, 1) - gameObject.transform.TransformPoint(0, 0, 0);
        //GetComponent<Room>().id = 1;
        dim = GetComponent<Room>().GetRoomDimension();
        obstacle = Resources.Load<GameObject>("Prefabs/Obstacle");
        Create(30, 1);

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Create(int piece, int pswLength)
    {
        password = new int[2 * pswLength];
        Vector3 pos;
        float border = 0.3f;
        locker = Instantiate(new GameObject()).AddComponent<PairLockHandler>();
        locker.transform.parent = gameObject.transform;
        for (int i = 0; i < piece; i++)
        {
            pos.x = Random.Range(transform.position.x - dim.x/2 + border, transform.position.x + dim.x/2 - border);
            pos.y = 0;
            pos.z = Random.Range(transform.position.z - dim.z / 2 + border, transform.position.z + dim.z / 2 - border);
            //Debug.LogError(obstacle);
            GameObject temp = Instantiate(obstacle);
            temp.transform.parent = locker.gameObject.transform;
            temp.transform.position = pos;
            temp.AddComponent<LockerButton>().id = i;
            temp.GetComponent<LockerButton>().SetSelected(selectedMaterial);
            player1.Add(temp);
        }
        
        int counter = 0;
        //int[] password = new int[2 * pswLength];
        while (counter < pswLength)
        {
            bool newPair = true;
            int pair1, pair2;
            pair1 = Random.Range(0, piece);
            pair2 = Random.Range(0, piece);
            if (pair1 == pair2) newPair = false;
            //Debug.LogError(pair1 +"asdf"+ pair2);
            Vector2Int pair = new Vector2Int(pair1, pair2);

            //Debug.LogError(pair);
            foreach (var item in key)
            {
                if (item.x == pair.x || item.x == pair.y || item.y == pair.x || item.y == pair.y) newPair = false;
            }
            if (newPair == true)
            {
                key.Add(pair);
                password[counter * 2] = pair.x;
                password[counter * 2 + 1] = pair.y;
                counter++;
                //Debug.LogError(pair);
                GameObject link = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Vector3 between = player1[pair1].transform.position - player1[pair2].transform.position;
                link.transform.localScale = new Vector3(0.1f, 0.1f, between.magnitude);
                link.transform.position = player1[pair2].transform.position + (between / 2.0f);
                link.GetComponent<Renderer>().material.color = Color.black;
                //transform.rotation = Quaternion.LookRotation(between);
                link.transform.LookAt(player1[pair1].transform.position);
                link.transform.position = new Vector3(link.transform.position.x, 4.9f, link.transform.position.z);
                player2.Add(link);
            }
        }
        locker.SetPassword(password, individualNumber);
    }
}
