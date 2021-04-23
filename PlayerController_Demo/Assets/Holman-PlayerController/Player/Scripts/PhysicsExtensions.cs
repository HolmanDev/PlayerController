using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HolmanPlayerController
{
    public static class PhysicsExtensions
    {
        public static RaycastHit[] RaySphere(Vector3 origin, Vector3 forward, int n, float radius, LayerMask layerMask)
        {
            Vector3[] directions = new Vector3[n];
            float phi = 2.39996322973f;

            // Create evenly spaced ray directions using the fibonacci sphere
            for (int i = 0; i < n; i++)
            {
                float y = 1f - (i / (n - 1f)) * 2f;
                float discRadius = Mathf.Sqrt(1 - y * y);

                float theta = phi * i;

                float x = Mathf.Cos(theta) * discRadius;
                float z = Mathf.Sin(theta) * discRadius;

                directions[i] = new Vector3(x, y, z); // !! Does this need to be normalized?
            }

            RaycastHit[] hits = new RaycastHit[n];

            // Cast the rays
            for (int i = 0; i < n; i++)
            {
                Physics.Raycast(origin, directions[i], out hits[i], radius, layerMask);

                if (PlayerController.debug)
                {
                    Debug.DrawLine(directions[i] + origin, directions[i] * 1.1f + origin, Color.cyan, Time.deltaTime);
                }
            }

            return hits;
        }
    }
}
