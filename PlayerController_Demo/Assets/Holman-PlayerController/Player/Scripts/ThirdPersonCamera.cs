using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HolmanPlayerController
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform Target;
        public float sensitivity = 2f;
        private float rotationZ;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }

            Target.Rotate(0, Input.GetAxis("Mouse X") * sensitivity, 0);
            transform.RotateAround(Target.position, Target.right, -Input.GetAxis("Mouse Y") * sensitivity);
        }
    }
}
