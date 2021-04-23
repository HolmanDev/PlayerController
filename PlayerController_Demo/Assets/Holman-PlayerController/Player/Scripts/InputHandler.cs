using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HolmanPlayerController
{
    public class InputHandler : MonoBehaviour
    {
        private PlayerController plCtrl;
        private Vector2 _inputDir;
        public Vector2 InputDir => _inputDir;

        // Using JSON files for settings is a good alternative to this
        public KeyBindings KeyBindingSettings;
        [System.Serializable]
        public class KeyBindings
        {
            public KeyCode Jump;
            public KeyCode Run;
            public KeyCode Grab;
            public KeyCode Interact;
        }

        private void Awake()
        {
            plCtrl = GetComponent<PlayerController>();
        }

        private void Update()
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            _inputDir = input.normalized;

            if (plCtrl.State.ToggleGrabCheck())
            {
                plCtrl.State.ToggleGrab();
            }

            if (plCtrl.State.RunSpeedCheck())
            {
                plCtrl.State.RunSpeed();
            }

            if (plCtrl.State.WalkSpeedCheck())
            {
                plCtrl.State.WalkSpeed();
            }
            
            if (plCtrl.State.JumpCheck())
            {
                plCtrl.State.Jump();
            }
        }
    }
}
