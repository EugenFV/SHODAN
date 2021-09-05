using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraLookController : MonoBehaviour
{
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;

    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    public float frameCounter = 20;

    public float smoothingTime = 0.5f;

    protected float rotationX = 0F;
    protected float rotationY = 0F;

    protected List<float> rotArrayX = new List<float>();
    protected float rotAverageX = 0F;

    protected List<float> rotArrayY = new List<float>();
    protected float rotAverageY = 0F;

    protected Quaternion originalRotation;

    //protected SmoothFollower follower;

    private void Start()
    {
        

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
            rb.freezeRotation = true;
        originalRotation = transform.localRotation;
        //follower = new SmoothFollower(smoothingTime);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Cursor.visible = !Cursor.visible;
        }
        switch (axes)
        {
            case RotationAxes.MouseXAndY:
                XYModeUpdate();
                break;
            case RotationAxes.MouseX:
                XModeUpdate();
                break;
            case RotationAxes.MouseY:
                YModeUpdate();
                break;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F))
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }
        }
        return Mathf.Clamp(angle, min, max);
    }

    protected abstract void XYModeUpdate();
    protected abstract void XModeUpdate();
    protected abstract void YModeUpdate();
}
