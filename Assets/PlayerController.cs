using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Transform _transform;
    private Camera _camera;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("click");
            _transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

    }
}
