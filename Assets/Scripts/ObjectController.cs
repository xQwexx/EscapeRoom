using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class ObjectController : MonoBehaviour
{
    //private Vector3 startingPosition;
    private Renderer myRenderer;
    private bool gazedAt;
    public bool fix = false;
    public int id;

    public Material inactiveMaterial;
    public Material gazedAtMaterial;
    private ObjectsHandler handler;

    void Start()
    {

        //startingPosition = transform.localPosition;
        myRenderer = GetComponent<Renderer>();
        handler = FindObjectOfType<ObjectsHandler>();
        SetGazedAt(false);
    }

    public void SetGazedAt(bool gazedAt)
    {
        this.gazedAt = gazedAt;
        if (inactiveMaterial != null && gazedAtMaterial != null)
        {
            myRenderer.material = gazedAt ? gazedAtMaterial : inactiveMaterial;
            return;
        }
    }
    public void Update()
    {

        if (!fix && gazedAt && InputControl.GetButtonDown(Controls.buttons.fire1))
        {
            //Debug.LogError("Selecting: " + this.gameObject);
            handler.OnObjectSelected(this.gameObject);
        }
    }

    public void Recenter()
    {
#if !UNITY_EDITOR
      GvrCardboardHelpers.Recenter();
#else
        if (GvrEditorEmulator.Instance != null)
        {
            GvrEditorEmulator.Instance.Recenter();
        }
#endif  // !UNITY_EDITOR
    }


    internal void setHandler(ObjectsHandler objectsHandler)
    {
        handler = objectsHandler;
    }
}
