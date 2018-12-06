using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class ColorButton : VRButton
{
    /*
    public LockHandler locker;

    private Renderer myRenderer;
    private bool gazedAt;
    public int id;

    public Material inactiveMaterial;
    public Material gazedAtMaterial;
    // Use this for initialization
    void Start () {

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

        SetGazedAt(false);
    }

    public void SetGazedAt(bool gazedAt)
    {
        this.gazedAt = gazedAt;
        
    }

    // Update is called once per frame
    void Update () {
        if (gazedAt && InputControl.GetButtonDown(Controls.buttons.fire1))
        {
            locker.OnButtonSelected(id);
            myRenderer.material.color = inactiveMaterial.color + gazedAtMaterial.color;
        }
        else if (!gazedAt)
        {
            myRenderer.material = inactiveMaterial;
        }
    }*/
    protected override void OnPlayerEvent()
    {
        handler.OnButtonSelected(id);
        myRenderer.material.color = inactiveMaterial.color + gazedAtMaterial.color;
    }
    protected override void OnNoPlayerEvent()
    {
        if (!gazedAt) myRenderer.material = inactiveMaterial;
    }

}