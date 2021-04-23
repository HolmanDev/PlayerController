using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public float speed = 1f;
    public float height = 1f;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        Vector3 pos = startPos;
        pos.y += (Mathf.Sin(Time.time * speed) + 1) * height;
        transform.position = pos;
    }
}
