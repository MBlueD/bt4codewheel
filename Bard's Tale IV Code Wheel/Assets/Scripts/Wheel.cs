using System.Collections;
using UnityEngine;

public class Wheel : MonoBehaviour {

    public Collider2D bottomWheel;
    public Collider2D middleWheel;
    public Collider2D topWheel;

    public bool allowMovingBottomWheel = false;

    private BoxCollider2D wheelBounds;

    private bool dragActive = false;
    private Collider2D dragTarget = null;
    private Vector3 initialDragPoint = Vector3.zero;
    private Quaternion initialWheelRotation;
    private Quaternion rotator;

    private Rect screenBounds;

    private void Start()
    {
        wheelBounds = GetComponent<BoxCollider2D>();
        screenBounds = new Rect();
        StartCoroutine(CheckScreenSize());
    }

    void Update () {
        Vector3 v = Input.mousePosition;
        v = Camera.main.ScreenToWorldPoint(v);
        v.z = 0;

        if (!dragActive && Input.GetMouseButtonDown(0))
        {
            dragTarget = GetOverlappingWheel(v);
            if (dragTarget != null && (dragTarget != bottomWheel || allowMovingBottomWheel))
            {
                dragActive = true;
                initialDragPoint = v.normalized;
                initialWheelRotation = dragTarget.transform.rotation;
            }
        }

        if (dragActive && Input.GetMouseButtonUp(0))
        {
            dragActive = false;
            dragTarget = null;
        }

        if (dragActive)
        {
            rotator.SetFromToRotation(initialDragPoint.normalized, v.normalized);
            dragTarget.gameObject.transform.rotation = rotator * initialWheelRotation;
        } 

    }

    // Returns the topmost wheel that overlaps the passed point
    private Collider2D GetOverlappingWheel(Vector2 v)
    {
        if (topWheel.OverlapPoint(v))
        {
            return topWheel;
        }
        else if (middleWheel.OverlapPoint(v))
        {
            return middleWheel;
        }
        else if (bottomWheel.OverlapPoint(v))
        {
            return bottomWheel;
        }

        return null;
    }

    private IEnumerator CheckScreenSize()
    {
        while (true)
        {
            Rect r = GetViewBounds();
            if (r.width != screenBounds.width || r.height != screenBounds.height)
            {
                UpdateScaling();
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    // Scale the wheel so that it will fit within the current view
    private void UpdateScaling()
    {
        Rect r = GetViewBounds();
        Vector2 wheelRect = wheelBounds.size;

        float xScale = r.width / wheelRect.x;
        float yScale = r.height / wheelRect.y;

        float newScale = Mathf.Min(xScale, yScale);

        gameObject.transform.localScale = new Vector3(newScale, newScale, 1);

        screenBounds = r;
    }

    // Get window size in world units
    public static Rect GetViewBounds()
    {
        Vector2 screenDimensions = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        Vector2 cameraPosition = Camera.main.transform.position;
        Vector2 offsets = screenDimensions - cameraPosition;

        Rect viewBounds = new Rect();
        viewBounds.center = new Vector2(0, 0);

        viewBounds.xMin = cameraPosition.x - offsets.x;
        viewBounds.yMin = cameraPosition.y - offsets.y;
        viewBounds.xMax = cameraPosition.x + offsets.x;
        viewBounds.yMax = cameraPosition.y + offsets.y;

        return viewBounds;
    }
}
