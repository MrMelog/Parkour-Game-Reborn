using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PersonMove;

public class SpearThrowing : MonoBehaviour
{
    [SerializeField] private PersonMoveSc moveSc;
    [SerializeField] private LineRenderer line;
    [SerializeField] private Material lineMat;
    [SerializeField] private float Power;
    [SerializeField] private float Delay;
    [SerializeField] private float MaxDistance;
    [SerializeField] private Camera camera;


    private bool canThrow;
    private float delay;

    private void Awake()
    {
        InputManagerScr.MouseLeftDown.AddListener(Click);
        camera = Camera.main;
        delay = -5;
    }


    private void Click()
    {
        if (Time.time - delay > Delay)
            canThrow = true;
        if (!canThrow)
            return;
        Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, MaxDistance))
        {
            moveSc.velocity += new Vector2(ray.direction.x, ray.direction.z) * Power;
            moveSc.YSpeed += ray.direction.y * Power;
            moveSc.jmpDelay = Time.time;
            canThrow = false;
            delay = Time.time;
            StartCoroutine(IELine(hit.point));
        }
        
    }

    private IEnumerator IELine(Vector3 end)
    {
        lineMat.SetFloat("_Cutoff", Mathf.Clamp01(0.5f));
        Vector3[] positions = new Vector3[2];
        line.enabled = true;
        float timer = Time.time;
        float cutoff = 0.5f;
        while (Time.time - timer < 0.5f)
        {
            positions[0] = transform.position;
            positions[1] = end;
            line.SetPositions(positions);
            cutoff += Time.deltaTime;
            lineMat.SetFloat("_Cutoff", Mathf.Clamp01(cutoff));
            yield return true;
        }
        line.enabled = false;   
    }
}
