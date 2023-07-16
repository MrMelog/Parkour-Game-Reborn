using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonMoveSc : MonoBehaviour
{
    [SerializeField] private CharacterController cc;
    [SerializeField] private CameraMove cm;

    public float Speed;
    public float SprintMod;
    public float JmpForce;
    public float gravity = 9;
    public float acsSpeed = 1;
    public float airAcsSpeed = 0.1f;

    private float jmpDelay;

    private float YSpeed;

    private float sprintMod;
    private float HorMove;
    private float VerMove;
    private float SprintPoints;

    private Vector3 direct;
    [SerializeField]
    private float VerAcs = 0;
    [SerializeField]
    private float HorAcs = 0;

    private Vector3 right;
    private Vector3 forward;

    private Vector3 rotForward;
    private Vector3 rotRight;




    private void Start()
    {
        cc = GetComponent<CharacterController>();
        cm = GetComponent<CameraMove>();    
        Cursor.lockState = CursorLockMode.Locked;
        right = Vector3.right; forward = Vector3.forward;
    }

    void Update()
    {
        if (cc.isGrounded)
        {
            HorMove = Input.GetAxis("Horizontal");
            VerMove = Input.GetAxis("Vertical");
            HorAcs = Mathf.Lerp(HorAcs, GetSign(HorMove), Time.deltaTime * acsSpeed);
            VerAcs = Mathf.Lerp(VerAcs, GetSign(VerMove), Time.deltaTime * acsSpeed);

            if (Input.GetKey(KeyCode.Space) && Time.time - jmpDelay > 0.15f)
            {
                Jump();
            }
        }
        else
        {
            
            YSpeed -= gravity * Time.deltaTime;
            YSpeed = Mathf.Clamp(YSpeed, -30, 30);
        }
        if (Input.GetKey(KeyCode.LeftShift) && SprintPoints > 0)
        {
            sprintMod = SprintMod;
            SprintPoints -= Time.deltaTime;
        }
        else 
        {
            sprintMod = 1;
            SprintPoints += Time.deltaTime;
        }
        rotForward.x = forward.x * Mathf.Sin(Mathf.Deg2Rad * (cm.forceXRot + 90)) - forward.z * Mathf.Cos(Mathf.Deg2Rad * (cm.forceXRot + 90));
        rotForward.z = forward.x * Mathf.Cos(Mathf.Deg2Rad * (cm.forceXRot + 90)) + forward.z * Mathf.Sin(Mathf.Deg2Rad * (cm.forceXRot + 90));

        rotRight.x = right.x * Mathf.Sin(Mathf.Deg2Rad * (cm.forceXRot + 90)) - right.z * Mathf.Cos(Mathf.Deg2Rad * (cm.forceXRot + 90));
        rotRight.z = right.x * Mathf.Cos(Mathf.Deg2Rad * (cm.forceXRot + 90)) + right.z * Mathf.Sin(Mathf.Deg2Rad * (cm.forceXRot + 90));

        direct = (rotForward * VerAcs + rotRight * HorAcs).normalized * Mathf.Max(Mathf.Abs(HorAcs), Mathf.Abs(VerAcs)) * Speed * sprintMod;
        direct.y = YSpeed;
        cc.Move(direct * Time.deltaTime);
        
    }


    private void Jump()
    {
        YSpeed = JmpForce;
        jmpDelay = Time.time;
    }

    public static float GetSign(float n)
    { 
        if (n == 0)
            return 0;
        return n / Mathf.Abs(n);
    }
 
}
