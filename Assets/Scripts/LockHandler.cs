using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LockHandler : MonoBehaviour {
    protected List<int> password = new List<int>();
    protected int counter = 0;
    protected Room room;

    protected int indvNumber = 1;
    protected List<int> indvBuffer = new List<int>();

    // Use this for initialization
    void Start() { room = GetComponentInParent<Room>(); }

    public abstract void OnButtonSelected(int selected);


    public abstract void OnButtonDeselected(int selected);


    public void SetPassword(int[] psw, int individualNumber = 1)
    {
        indvNumber = individualNumber;
        for (int i = 0; i < psw.Length; i++)
        {
            password.Add(psw[i]);
        }
    }
}
