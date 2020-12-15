using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{

    private Camera cam;

    private float targetZoom;
    private float zoomFactor = 3f;
    [SerializeField] private float zoomSpeed = 10;

    private Vector3 mouseStart;

    [SerializeField] BoxCollider2D bound;

    private Vector3 minBound;
    private Vector3 maxBound;

    private float halfWidth;
    private float halfHeight;

    private bool zoomFlag;


    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        zoomFlag = false;
        minBound = bound.bounds.min;
        maxBound = bound.bounds.max;
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * (float)Screen.width / (float)Screen.height;
    }

    // Update is called once per frame
    void Update()
    {
        MoveCam();
        ZoomCam();
    }

    private void MoveCam()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIObject(Input.mousePosition) && GameManager.Instance.gameState == GameManager.State.Play)
            {
                mouseStart = new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.transform.position.z);
                mouseStart = cam.ScreenToWorldPoint(mouseStart);

                Vector2 mousePos = Input.mousePosition;
                mousePos = Camera.main.ScreenToWorldPoint(mousePos);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 10f);

                if (hit.collider != null && hit.collider.CompareTag("Untagged"))
                {
                    zoomFlag = true;
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (!IsPointerOverUIObject(Input.mousePosition) && GameManager.Instance.gameState == GameManager.State.Play)
            {
                if (zoomFlag)
                {
                    halfHeight = cam.orthographicSize;
                    halfWidth = halfHeight * (float)Screen.width / (float)Screen.height;

                    Vector3 mouseMove = new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.transform.position.z);
                    mouseMove = cam.ScreenToWorldPoint(mouseMove);
                    Vector3 diff = this.transform.position - (mouseMove - mouseStart);

                    float clampedX = Mathf.Clamp(diff.x, minBound.x + halfWidth, maxBound.x - halfWidth);
                    float clampedY = Mathf.Clamp(diff.y, minBound.y + halfHeight, maxBound.y - halfHeight);

                    this.transform.position = new Vector3(clampedX, clampedY, this.transform.position.z);
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!IsPointerOverUIObject(Input.mousePosition) && GameManager.Instance.gameState == GameManager.State.Play)
            {
                zoomFlag = false;
            }
        }
    }

    private void ZoomCam()
    {
        if(Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float scrollData;
            scrollData = Input.GetAxis("Mouse ScrollWheel");

            targetZoom -= scrollData * zoomFactor;
            targetZoom = Mathf.Clamp(targetZoom, 4.5f, 8f);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
        }

    }

    public bool IsPointerOverUIObject(Vector2 touchPos)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);

        eventDataCurrentPosition.position = touchPos;

        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;
    }
}
