using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCamera : MonoBehaviour
{
    [SerializeField] private float _padding = 0.25f;

    public void ResetCenterAndWidth(Rect contentBounds)
    {
        print("Resetting camera");
        Vector3 oldPos = transform.position;
        Vector3 newPos = new Vector3(contentBounds.x + contentBounds.width / 2, contentBounds.y + contentBounds.height / 2, oldPos.z);
        transform.position = newPos;

        float w = contentBounds.width + _padding * 2;
        float h = contentBounds.height + _padding * 2;

        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = w / h;

        if (screenRatio >= targetRatio)
        {
            GetComponent<Camera>().orthographicSize = h / 2;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            GetComponent<Camera>().orthographicSize = h / 2 * differenceInSize;
        }

    }
}
