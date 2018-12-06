using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class LockerButton : MonoBehaviour
{

    protected LockHandler locker;

    protected Renderer myRenderer;
    private bool gazedAt = false;
    private bool isSelected = false;
    public int id;
    private Material inactiveMaterial;
    public Material selectedMaterial = null;
    private Color gazedAtMaterial;
    // Use this for initialization
    void Start()
    {

        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((d) => { SetGazedAt(true); });
        trigger.triggers.Add(entry);
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerExit;
        entry.callback.AddListener((data) => { SetGazedAt(false); });
        trigger.triggers.Add(entry);

        myRenderer = GetComponent<Renderer>();
        inactiveMaterial = new Material(myRenderer.material);
        gazedAtMaterial = Color.yellow;
        locker = GetComponentInParent<LockHandler>();

    }

    public void SetGazedAt(bool gazedAt)
    {
        Debug.Log(id + " Button gazedStatus: " + isSelected);
        this.gazedAt = gazedAt;
        myRenderer.material.color = gazedAt ? myRenderer.material.color + gazedAtMaterial : myRenderer.material.color - gazedAtMaterial;
    }

    public void OnSelected(bool isSelected)
    {
        if (selectedMaterial == null) return;
        Debug.Log( id +" Button SelectedStatus: " + isSelected );
        this.isSelected = isSelected;
        myRenderer.material.color = isSelected ? myRenderer.material.color - inactiveMaterial.color + selectedMaterial.color : myRenderer.material.color + inactiveMaterial.color - selectedMaterial.color;
    }

    // Update is called once per frame
    void Update()
    {

        if (gazedAt && InputControl.GetButtonDown(Controls.buttons.fire1))
        {
            //Debug.LogError("Selecting: " + this.gameObject);
            if (!isSelected)
            {
                locker.OnButtonSelected(id);
            }
            else
            {
                locker.OnButtonDeselected(id);
            }
        }
    }
    /*public void SetSelected(Material selected)
    {
        selectedMaterial = selected;
    }/*
    public void SetLockerHandler(LockHandler locker)
    {
        //this.locker = locker;
    }*/
}
