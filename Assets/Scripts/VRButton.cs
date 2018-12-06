using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class VRButton : MonoBehaviour {
    protected Renderer myRenderer;
    protected bool gazedAt = false;
    public int id;

    public Material inactiveMaterial;
    public Material gazedAtMaterial;
    protected LockHandler handler;
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
        //inactiveMaterial = new Material(myRenderer.material);
        //gazedAtMaterial = Color.yellow;
        handler = GetComponentInParent<LockHandler>();
    }

    public void SetGazedAt(bool gazedAt)
    {
        this.gazedAt = gazedAt;
        //myRenderer.material = gazedAt ? gazedAtMaterial : inactiveMaterial;
    }

    // Update is called once per frame
    void Update () {
        if (handler == null) handler = GetComponentInParent<LockHandler>();
        if (gazedAt && InputControl.GetButtonDown(Controls.buttons.fire1))
        {
            //Debug.LogError("Selecting: " + this.gameObject);
            OnPlayerEvent();
        }
        OnNoPlayerEvent();
    }

    virtual protected void OnPlayerEvent()
    {
        handler.OnButtonSelected(id);
    }

    virtual protected void OnNoPlayerEvent()
    {
    }
}
