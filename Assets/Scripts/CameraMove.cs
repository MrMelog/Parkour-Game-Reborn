using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform player;

    public float MouseSens;
    public float ForceRotSpeed;
    public float SprintFow;
    public float WalkingFow;

    private float YRot;
    private float XRot;

    public float forceXRot;
    public bool IsRunning;


    private void LateUpdate()
    {
        if (IsRunning)
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, SprintFow, Time.deltaTime * 6);
        else cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, WalkingFow, Time.deltaTime * 6);
        YRot = Mathf.Clamp(YRot - (Input.GetAxis("Mouse Y") * MouseSens), -90, 90);
        XRot = XRot + (Input.GetAxis("Mouse X") * MouseSens);
        cam.transform.localRotation = Quaternion.Euler(YRot, 0,0);
        player.localRotation = Quaternion.Euler(0, XRot, 0);
        //forceXRot += PersonMoveSc.GetSign(Mathf.DeltaAngle(forceXRot, XRot)) * ForceRotSpeed * Time.deltaTime;
        forceXRot = XRot;
    }
}
