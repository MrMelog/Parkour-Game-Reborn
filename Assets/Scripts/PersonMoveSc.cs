using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PersonMoveSc : MonoBehaviour
{
    [SerializeField] private CharacterController cc;
    [SerializeField] private CameraMove cm;
    [SerializeField] private RectTransform st;

    public float Speed;
    public float SprintMod;
    public float JmpForce;
    public float gravity = 9;
    public float acsSpeed = 1;
    public float airAcsSpeed = 0.1f;
    public float MaxSprintPoints;

    public float acceleration;

    private float jmpDelay;

    private float YSpeed;
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

    public Vector2 velocity;
    public float jmpVelocity;


    public Vector2 keysVector;

    private enum State { Ground, Air, Slide}

    public float GrLerpSpeed;
    public float AirLerpSpeed;
    public float SlideStrenght;

    private float lerpSpeed;

    private float speed;

    private bool _isRunning;

    private bool IsRunning
    {
        get {
            if (Input.GetKeyDown(KeyCode.LeftShift) && SprintPoints >= 1)
            {
                _isRunning = true;
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift) || SprintPoints <= 0)
            {
                 _isRunning = false;
            }

            return _isRunning;
        }
    }

    private State lastState;



    private void Start()
    {
        cc = GetComponent<CharacterController>();
        cm = GetComponent<CameraMove>();
        Cursor.lockState = CursorLockMode.Locked;
        right = Vector3.right; forward = Vector3.forward;
        jmpDelay = 0;
        speed = Speed;
    }

    void Update()
    {
        keysVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        keysVector = YRot(keysVector, cm.forceXRot);
        keysVector = keysVector.normalized * Mathf.Min(keysVector.sqrMagnitude, 1);
        if (IsGrounded(ref Hit))
        {
            if (IsRunning)
                speed = Mathf.Lerp(speed, Speed * SprintMod, Time.deltaTime * lerpSpeed);
            else
                speed = Mathf.Lerp(speed, Speed, Time.deltaTime * lerpSpeed * 2);
            if (Hit.normal.y > 0.82f)
            {
                lerpSpeed = GrLerpSpeed;
                if (Time.time - jmpDelay > 0.2f)
                {
                    YSpeed = -2;
                    if (Input.GetKey(KeyCode.Space))
                    {
                        Jump2();
                    }
                }
                lastState = State.Ground;
            }
            else
            {
                lerpSpeed = AirLerpSpeed;
                Vector3 slideN = Hit.normal * SlideStrenght * Time.deltaTime;
                velocity.x += slideN.x;
                velocity.y += slideN.z;
                if (lastState != State.Slide)
                    velocity += keysVector * -YSpeed;
                //YSpeed -= slideN.y;
                lastState = State.Slide;
            }

            velocity.x = Mathf.Lerp(velocity.x, keysVector.x * speed, Time.deltaTime * lerpSpeed);
            velocity.y = Mathf.Lerp(velocity.y, keysVector.y * speed, Time.deltaTime * lerpSpeed);
        }
        else
        {
            if (IsTopCollision())                   
                YSpeed -= Mathf.Abs(YSpeed * 0.3f);
            YSpeed -= gravity * Time.deltaTime;
            velocity.x = Mathf.Lerp(velocity.x, keysVector.x * speed, Time.deltaTime * AirLerpSpeed);
            velocity.y = Mathf.Lerp(velocity.y, keysVector.y * speed, Time.deltaTime * AirLerpSpeed);
            lastState = State.Air;
        }
        if (_isRunning)
            SprintPoints -= Time.deltaTime;
        else
            SprintPoints += Time.deltaTime;
        cc.Move(new Vector3(velocity.x, YSpeed, velocity.y) * Time.deltaTime);
        cm.IsRunning = IsRunning;
        /*
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
        */
        
    }


    private void Jump()
    {
        jmpVector = Hit.normal * JmpForce;
        jmpDelay = Time.time;
    }

    private void Jump2()
    {
        Vector3 jmpVec = Hit.normal * JmpForce;
        YSpeed += jmpVec.y;
        velocity.x += jmpVec.x;
        velocity.y += jmpVec.z;
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
        RaycastHit[] hits;
        hits = Physics.SphereCastAll(transform.position, 0.40f, transform.up, 0.7f);
        foreach (var item in hits)
        {
            if (item.transform.tag != "Player")
                return true;
        }
        return false;
    }

    private bool IsGrounded(ref RaycastHit _hit)
    {
        RaycastHit[] hits;
        hits = Physics.SphereCastAll(transform.position, 0.40f, -transform.up, 0.7f);
        if (hits.Length > 1)
        {
            RaycastHit maxhit = hits[0];
            foreach (RaycastHit hit in hits) { 
                if (hit.normal.y > maxhit.normal.y && hit.transform.tag != "Player")
                    maxhit = hit;
            }
            _hit = maxhit;
            //Debug.Log(maxhit.normal.y + "y");
            //Debug.Log(hits.Length);
            return maxhit.normal.y > 0.5f;
        }
        _hit = new RaycastHit();
        return false;
    }

    private Vector2 YRot(Vector2 vec, float degree)
    {
        Vector3 rVec = vec;
        rVec.x = vec.x * Mathf.Sin(Mathf.Deg2Rad * (degree + 90)) - vec.y * Mathf.Cos(Mathf.Deg2Rad * (degree + 90));
        rVec.y = vec.x * Mathf.Cos(Mathf.Deg2Rad * (degree + 90)) + vec.y * Mathf.Sin(Mathf.Deg2Rad * (degree + 90));
        return rVec;
    }

}
