using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HolmanPlayerController
{
    public class AnchorConnection : MonoBehaviour
    {
        public Transform Anchor;

        // Update is called once per frame
        void Update()
        {
            transform.position = Anchor.position;
        }
    }
}
