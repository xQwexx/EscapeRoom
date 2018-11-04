using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class ObjectController : MonoBehaviour
{
    //private Vector3 startingPosition;
    private Renderer myRenderer;
    private bool gazedAt;
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
        if (gazedAt && InputControl.GetButtonDown(Controls.buttons.fire1))
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

    public void TeleportRandomly(BaseEventData eventData)
    {
        // Pick a random sibling, move them somewhere random, activate them,
        // deactivate ourself.
        //int sibIdx = transform.GetSiblingIndex();
        //int numSibs = 4;//transform.parent.childCount;
        //sibIdx = (sibIdx + Random.Range(1, numSibs)) % numSibs;
       // GameObject randomSib = transform.parent.GetChild(sibIdx).gameObject;

        // Move to random new location ±100º horzontal.
        Vector3 direction = Quaternion.Euler(
            0,
            UnityEngine.Random.Range(-90, 90),
            0) * Vector3.forward;
        // New location between 1.5m and 3.5m.
        float distance = 2 * UnityEngine.Random.value + 1.5f;
        Vector3 newPos = direction * distance;
        // Limit vertical position to be fully in the room.
        newPos.y = Mathf.Clamp(newPos.y, -1.2f, 4f);
        //randomSib.transform.localPosition = newPos;
        transform.position = newPos;
        //randomSib.SetActive(true);
        //gameObject.SetActive(false);
        SetGazedAt(false);
    }

    internal void setHandler(ObjectsHandler objectsHandler)
    {
        handler = objectsHandler;
    }
}
