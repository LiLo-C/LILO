using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCam != null)
        {
            // Forces the sprite to look in the exact same direction as the camera
            transform.forward = mainCam.transform.forward;
        }
    }
}