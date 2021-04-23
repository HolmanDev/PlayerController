using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HolmanPlayerController
{
    public class CameraFree : MonoBehaviour
    {
        public Transform player;
        public float sensitivity;

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

            transform.Rotate(-Input.GetAxis("Mouse Y") * sensitivity, 0, 0);
            player.Rotate(0, Input.GetAxis("Mouse X") * sensitivity, 0);

            transform.localEulerAngles = new Vector3(
                transform.localEulerAngles.x,
                transform.localEulerAngles.y,
                transform.localEulerAngles.z);
        }
    }
}
