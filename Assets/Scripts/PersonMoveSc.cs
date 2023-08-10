using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


namespace PersonMove
{
    public struct Force 
    {
        private Vector2 _direction;
        public Vector2 direction 
        { 
            get { return _direction; } 
            set { _direction = value.normalized; } 
        }

        public float amplitude;


        public Vector2 Vector
        {
            get { return direction * amplitude; }
            set
            {
                direction = value.normalized;
                amplitude = value.sqrMagnitude;
            }
        }

        public Force ZRot(float degree)
        {
            Vector2 rVec = direction;
            rVec.x = direction.x * Mathf.Sin(Mathf.Deg2Rad * (degree + 90)) - direction.y * Mathf.Cos(Mathf.Deg2Rad * (degree + 90));
            rVec.y = direction.x * Mathf.Cos(Mathf.Deg2Rad * (degree + 90)) + direction.y * Mathf.Sin(Mathf.Deg2Rad * (degree + 90));
            direction = rVec;
            return this;
        }


        public Force SetDir(float x, float y)
        {
            _direction.x = x; _direction.y = y;
            return this;
        }


        public Force(Vector2 Force)
        {
            _direction = Force.normalized;
            amplitude = Force.magnitude;
        }
        public Force(Vector2 dir, float amp)
        {
            _direction = dir;
            amplitude = amp;
        }

        public override string ToString()
        { 
            return _direction.ToString() + ", " + amplitude.ToString();
        }
    }

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

        public float jmpDelay;
        public float YSpeed;

        public float _sprintPoints;

        public float SprintPoints
        {
            get { return _sprintPoints; }
            private set
            {
                _sprintPoints = Mathf.Clamp(value, 0, MaxSprintPoints);
                st.localScale = new Vector3(_sprintPoints / MaxSprintPoints, 1, 1);
            }
        }

        private RaycastHit Hit;

        public Vector2 velocity;
        public float jmpVelocity;

        public Force Velocity;


        public Vector2 keysVector;

        private enum State { Ground, Air, Slide }

        public float GrLerpSpeed;
        public float AirLerpSpeed;
        public float SlideLerpSpeed;
        public float SlideStrenght;
        public float AirMaxSpeed;

        private float lerpSpeed;

        private float speed;

        private bool _isRunning;
        [DoNotSerialize]
        public bool IsRunning
        {
            get
            {
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
        [SerializeField]
        private State lastState;



        private void Start()
        {
            cc = GetComponent<CharacterController>();
            cm = GetComponent<CameraMove>();
            Cursor.lockState = CursorLockMode.Locked;
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
                        else if (CheckStep())
                        {
                            YSpeed = 0.4f;
                        }
                    }
                    else
                        lerpSpeed = 0.2f;
                    lastState = State.Ground;
                }
                else
                {
                    lerpSpeed = SlideLerpSpeed;
                    Vector3 slideN = Hit.normal * SlideStrenght * Time.deltaTime;
                    velocity.x += slideN.x;
                    velocity.y += slideN.z;
                    if (lastState != State.Slide)
                        velocity += keysVector * -YSpeed;
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
                Force force = new(velocity);
                force.SetDir(
                    Mathf.Lerp(force.direction.x, keysVector.x, Time.deltaTime * AirLerpSpeed),
                    Mathf.Lerp(force.direction.y, keysVector.y, Time.deltaTime * AirLerpSpeed));
                if (keysVector.sqrMagnitude > 0.4f) force.amplitude = Mathf.Lerp(force.amplitude, AirMaxSpeed, Time.deltaTime);
                velocity = force.Vector;
                lastState = State.Air;
            }
            if (_isRunning)
                SprintPoints -= Time.deltaTime;
            else
                SprintPoints += Time.deltaTime;
            cc.Move(new Vector3(velocity.x, YSpeed, velocity.y) * Time.deltaTime);
            cm.IsRunning = IsRunning;
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
            hits = Physics.SphereCastAll(transform.position, 0.41f, -transform.up, 0.7f);
            if (hits.Length > 1)
            {
                RaycastHit maxhit = hits[0];
                foreach (RaycastHit hit in hits)
                {
                    if (hit.normal.y > maxhit.normal.y && hit.transform.tag != "Player")
                        maxhit = hit;
                }
                _hit = maxhit;
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


        private bool CheckStep()
        {
            return Physics.Raycast(new Ray(transform.position + -Vector3.up, new Vector3(keysVector.x, 0, keysVector.y)), 0.4f) 
                && !Physics.Raycast(new Ray(transform.position + -Vector3.up * 0.6f, new Vector3(keysVector.x, 0, keysVector.y)), 0.5f);
        }

    }

}
