using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Wheel : MonoBehaviour {

    public Collider2D bottomWheel;
    public Collider2D middleWheel;
    public Collider2D topWheel;

    public Text textForWord1, textForWord2;
    
    public bool allowMovingBottomWheel = false;
    public bool showDebug = true;

    public float snapSpeed = 0.3f;

    private BoxCollider2D wheelBounds;

    private bool dragActive = false;
    private Collider2D dragTarget = null;
    private Vector3 initialDragPoint = Vector3.zero;
    private Quaternion initialWheelRotation;
    private Quaternion rotator;

    private Rect screenBounds;

    private int midWheelSection = 0, topWheelSection = 0;

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

        UpdateWheelSections();

        if (!dragActive)
        {
            // do a bit of lerp to snap wheels to sections
            Quaternion targetRotation = Quaternion.AngleAxis(-30f * topWheelSection, Vector3.forward);
            topWheel.transform.rotation = Quaternion.Lerp(topWheel.transform.rotation, targetRotation, snapSpeed);

            targetRotation = Quaternion.AngleAxis(-15f * midWheelSection, Vector3.forward);
            middleWheel.transform.rotation = Quaternion.Lerp(middleWheel.transform.rotation, targetRotation, snapSpeed);
        }

        int[] numbers = FindVisibleCells();

        textForWord1.text = "" + numbers[0] + "- " + Codes.codes[numbers[0] - 1];
        textForWord2.text = "" + numbers[1] + "- " + Codes.codes[numbers[1] - 1];

        //Debug.Log("" + numbers[0] + " " + numbers[1] + " " + numbers[2]);

        if (showDebug)
        {
            Debug.Log("" + numbers[0] + " " + numbers[1] + " " + numbers[2]);
            DebugDrawSection(midWheelSection, 15, Color.red, 3.5f);
            DebugDrawSection(topWheelSection, 30, Color.blue, 3f);

            DebugDrawSection((topWheelSection + 2) % 12, 30, Color.yellow, 4f);
            DebugDrawSection((topWheelSection + 6) % 12, 30, Color.yellow, 4f);
            DebugDrawSection((topWheelSection + 10) % 12, 30, Color.yellow, 4f);
        }
    }

    private void DebugDrawSection(int wheelSection, float sectionArcDegrees, Color c, float length)
    {
        Vector2 start = Vector2.up;
        float angle = 360 - wheelSection * sectionArcDegrees; 
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
        Quaternion rotEnd = Quaternion.AngleAxis(-sectionArcDegrees, Vector3.forward) * rot;

        Debug.DrawLine(Vector2.zero, rot * start * length, c);
        Debug.DrawLine(Vector2.zero, rotEnd * start * length, c);
    }

    private int[] FindVisibleCells()
    {
        // get empty sections on top wheel
        int[] empties = {(topWheelSection + 2)% 12, (topWheelSection + 6) % 12, (topWheelSection + 10) % 12};
        int[] numbers = new int[3];
        int ni = 0;

        foreach (int e in empties)
        {
            // check the two midwheel sections within the topwheel empty section
            for (int i = 0; i < 2; i++)
            {
                int s = (e * 2 + i) % 24;
                int vs = s - midWheelSection;
                //s = (s + 24) % 24; // to handle negative values of s
                if (vs < 0)
                {
                    vs += 24;
                }
                if (vs % 3 == 0)
                {
                    // we have a visible number. Figure it out.
                    // The number wheel is a 2d array
                    // The section divided by 6 gives us the row
                    // the section is also the column within the row
                    // add 1 bc sections start at 0 but number wheel starts at 1
                    numbers[ni++] = (vs / 6) * 24 + s + 1;

                    if (showDebug)
                    {
                        DebugDrawSection(s, 15, Color.magenta, 6f);
                    }
                }
            }
        }

        return numbers;
    }

    private void UpdateWheelSections()
    { 
        midWheelSection = (int) (GetWheelRotation(middleWheel.transform) + 7.5) / 15; // midwheel has 24 sections, 2 per constellation
        topWheelSection = (int) (GetWheelRotation(topWheel.transform) + 15) / 30; // topwheel has 12 sections, a constellation each, and we indicate section with arrow (15 degree shift)
    }

    private float GetWheelRotation(Transform wheel)
    {
        float angle = wheel.rotation.eulerAngles.z;

        // For easier visualization and cleaner number calculations, I numbered the wheel sections starting from 0 for the bear sign and going clockwise.
        // Since the rotation angle is for the up vector (0,1,0), rotation angle starts at 0 for the up vector and increase counter-clockwise, so bear sign secton is 0 to 330.
        // I am going to map the real angle to a clockwise progression that matches with wheel section numbering (increases clockwise)
        angle = 360 - angle;

        return angle;
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
