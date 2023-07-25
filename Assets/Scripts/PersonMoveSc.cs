using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PersonMoveSc : MonoBehaviour
{
    [SerializeField] private CharacterController cc;
    [SerializeField] private CameraMove cm;
    [SerializeField] private CapsuleCollider gt;
    [SerializeField] private RectTransform st;

    public float Speed;
    public float SprintMod;
    public float JmpForce;
    public float gravity = 9;
    public float acsSpeed = 1;
    public float airAcsSpeed = 0.1f;
    public float MaxSprintPoints;

    private float jmpDelay;

    private float YSpeed;

    private float sprintMod;
    private float HorMove;
    private float VerMove;

    public float _sprintPoints;

    public float SprintPoints {
        get { return _sprintPoints; }
        private set { 
            _sprintPoints = Mathf.Clamp(value, 0, MaxSprintPoints);
            st.localScale = new Vector3(_sprintPoints / MaxSprintPoints, 1, 1);
        }
    }


    private Vector3 direct;
    private float VerAcs = 0;
    private float HorAcs = 0;

    private Vector3 right;
    private Vector3 forward;

    private Vector3 rotForward;
    private Vector3 rotRight;

    private Vector3 jmpVector;

    private Ray ray;

    private RaycastHit Hit;


    private void Start()
    {
        cc = GetComponent<CharacterController>();
        cm = GetComponent<CameraMove>();
        gt = GetComponentInChildren<CapsuleCollider>();
        Cursor.lockState = CursorLockMode.Locked;
        right = Vector3.right; forward = Vector3.forward;
        jmpDelay = 0;
    }

    void Update()
    {
        if (IsGrounded(ref Hit))
        {
            if (Time.time - jmpDelay > 0.2f && Hit.distance <= 1.1f)
            {
                jmpVector.y = -0.5f;
                jmpVector.x = Mathf.Lerp(jmpVector.x, 0, Time.deltaTime * acsSpeed);
                jmpVector.z = Mathf.Lerp(jmpVector.z, 0, Time.deltaTime * acsSpeed);
            }
            HorMove = Input.GetAxis("Horizontal");
            VerMove = Input.GetAxis("Vertical");
            HorAcs = Mathf.Lerp(HorAcs, GetSign(HorMove), Time.deltaTime * acsSpeed);
            VerAcs = Mathf.Lerp(VerAcs, GetSign(VerMove), Time.deltaTime * acsSpeed);

            if (Hit.normal.y < 0.85f)
            {
                jmpVector += Hit.normal * Time.deltaTime * 40;
            }
                




            if (Input.GetKey(KeyCode.Space) && Time.time - jmpDelay > 0.2f && Hit.distance <= 1.1f)
            {
                Jump();
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
        }
        else
        {
            if (IsTopCollision())
            { 
                jmpVector.y *=-1;
            }
            
            jmpVector.y = Mathf.Clamp(jmpVector.y - gravity * Time.deltaTime, -12, 22);
            jmpVector.x = Mathf.Lerp(jmpVector.x, 0, Time.deltaTime * airAcsSpeed);
            jmpVector.z = Mathf.Lerp(jmpVector.z, 0, Time.deltaTime * airAcsSpeed);
        }

        rotForward.x = forward.x * Mathf.Sin(Mathf.Deg2Rad * (cm.forceXRot + 90)) - forward.z * Mathf.Cos(Mathf.Deg2Rad * (cm.forceXRot + 90));
        rotForward.z = forward.x * Mathf.Cos(Mathf.Deg2Rad * (cm.forceXRot + 90)) + forward.z * Mathf.Sin(Mathf.Deg2Rad * (cm.forceXRot + 90));

        rotRight.x = right.x * Mathf.Sin(Mathf.Deg2Rad * (cm.forceXRot + 90)) - right.z * Mathf.Cos(Mathf.Deg2Rad * (cm.forceXRot + 90));
        rotRight.z = right.x * Mathf.Cos(Mathf.Deg2Rad * (cm.forceXRot + 90)) + right.z * Mathf.Sin(Mathf.Deg2Rad * (cm.forceXRot + 90));

        direct = (rotForward * VerAcs + rotRight * HorAcs).normalized * Mathf.Max(Mathf.Abs(HorAcs), Mathf.Abs(VerAcs)) * Speed * sprintMod + jmpVector;
        
        cc.Move(direct * Time.deltaTime);
        
    }


    private void Jump()
    {
        jmpVector = Hit.normal * JmpForce;
        jmpDelay = Time.time;
    }

    public static float GetSign(float n)
    { 
        if (n == 0)
            return 0;
        return n / Mathf.Abs(n);
    }

    private bool IsTopCollision()
    {
        Ray ray = new(transform.position, transform.up);
        return Physics.Raycast(ray, 1.1f);
    }

    private bool IsGrounded(ref RaycastHit _hit)
    {
        ray = new(transform.position, transform.up * -1);
        RaycastHit[] hits;
        hits = Physics.SphereCastAll(transform.position, 0.39f, transform.up * -1, 0.65f);
        if (hits.Length > 1)
        {
            RaycastHit maxhit = hits[0];
            foreach (RaycastHit hit in hits) { 
                if (hit.normal.y > maxhit.normal.y && hit.transform.tag != "Player")
                    maxhit = hit;
            }
            _hit = maxhit;
            Debug.Log(maxhit.normal.y + "y");
            Debug.Log(hits.Length);
            return maxhit.normal.y > 0.5f;
        }      
        return false;
    }

}
