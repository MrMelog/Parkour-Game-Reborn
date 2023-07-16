using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform player;

    public float MouseSens;
    public float ForceRotSpeed;

    private float YRot;
    private float XRot;

    public float forceXRot;


    private void LateUpdate()
    {
        YRot = Mathf.Clamp(YRot - (Input.GetAxis("Mouse Y") * MouseSens), -90, 90);
        XRot = XRot + (Input.GetAxis("Mouse X") * MouseSens);
        cam.transform.localRotation = Quaternion.Euler(YRot, 0,0);
        player.localRotation = Quaternion.Euler(0, XRot, 0);
        forceXRot += PersonMoveSc.GetSign(Mathf.DeltaAngle(forceXRot, XRot)) * ForceRotSpeed * Time.deltaTime;
    }
}
