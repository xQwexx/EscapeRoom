using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PairItem : MonoBehaviour {


    private Renderer myRenderer;
    private bool gazedAt = false;
    private bool isInPair = false;
    public int id;

    public Material inactiveMaterial;
    public Material gazedAtMaterial;
    private LockHandler handler;

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
        handler = GetComponentInParent<LockHandler>();
    }

    public void SetGazedAt(bool gazedAt)
    {
        
        this.gazedAt = gazedAt;

    }
    public void Update()
    {
        if(handler == null) handler = GetComponentInParent<LockHandler>();
        if (gazedAt && InputControl.GetButtonDown(Controls.buttons.fire1))
        {
            //Debug.LogError("Selecting: " + this.gameObject);
            if (isInPair)
            {
                handler.OnButtonDeselected(id);
                isInPair = false;
            }
            else
            {
                handler.OnButtonSelected(id);
                isInPair = true;
            }
        }
        if (isInPair) myRenderer.material = gazedAtMaterial;
        else myRenderer.material = inactiveMaterial;
    }
    public void IsInPair(bool pair)
    {
        //Debug.LogError("Selecting: " + id);
        isInPair = pair;
    }
}
