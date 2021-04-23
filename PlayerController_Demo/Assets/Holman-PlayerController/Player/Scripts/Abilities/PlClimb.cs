using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HolmanPlayerController
{
    public class PlClimb : MonoBehaviour
    {
        private PlayerController _playerController;

        [SerializeField] private LayerMask _grabbableMask = default;
        [SerializeField] private float _grabRange = 1f;
        [SerializeField] private float _climbSpeedHorizontal = 5f;
        [SerializeField] private float _climbSpeedVertical = 2f;
        [SerializeField] private int _climbDetectionSphereVertices = 200;

        public GameObject GrabbedObject { get; private set; }
        public bool Grabbing { get; private set; }
        private GameObject _anchorConnection;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        public void ClimbMovement(Vector3 inputDir)
        {
            LayerMask layerMask = LayerMask.GetMask(LayerMask.LayerToName(GrabbedObject.layer));
            // Create a fibonacci sphere of rays around the player.
            RaycastHit[] hits = PhysicsExtensions.RaySphere(transform.position, transform.forward, _climbDetectionSphereVertices, _grabRange, layerMask);
            RaycastHit normalHit = new RaycastHit();
            float highestDotProduct = -2f;
            float highestYPosition = -999f;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider != null)
                {
                    // If the surface is too steep to walk on
                    if (Vector3.Angle(Vector3.up, hits[i].normal) > _playerController.MaxAngle)
                    {
                        if (PlayerController.debug)
                        {
                            Debug.DrawRay(hits[i].point, hits[i].normal * 0.2f, Color.black, Time.deltaTime);
                        }

                        float value = Vector3.Dot(hits[i].normal, transform.position - hits[i].point);
                        // Store the largest dot product, i.e the normal most towards the player
                        if (value > highestDotProduct)
                        {
                            highestDotProduct = value;
                            normalHit = hits[i];
                        }
                    }

                    // Store the highest y-position
                    if (hits[i].point.y > highestYPosition)
                    {
                        highestYPosition = hits[i].point.y;
                    }
                }
            }

            // If a point was found
            if (highestDotProduct > -1.5f)
            {
                // If the highest point is over the player's feet
                if (highestYPosition > _playerController.Feet.position.y)
                {
                    Vector3 right = Vector3.Cross(transform.up, normalHit.normal).normalized;
                    Vector3 up = Vector3.Cross(right, normalHit.normal).normalized;
                    // Force the desired solution to the cross-product
                    if (Vector3.Dot(up, Vector3.up) < 0)
                    {
                        up = -up;
                    }

                    float wallDistance = 1f;
                    // How much does the player need to be pushed out to be {wallDistance} units away from the wall?
                    float wallOffset = Vector3.Dot(normalHit.point + normalHit.normal * wallDistance - transform.position, normalHit.normal);

                    if (PlayerController.debug)
                    {
                        Debug.DrawRay(transform.position, up * 2, Color.green, Time.deltaTime);
                        Debug.DrawRay(transform.position, right * 2, Color.red, Time.deltaTime);
                    }

                    _playerController.CharacterController.Move(((-right * inputDir.x) * _climbSpeedHorizontal +
                                                                (up * inputDir.y) * _climbSpeedVertical +
                                                                normalHit.normal * wallOffset) * Time.deltaTime);
                }
                else
                {
                    ReleaseGrab();
                }
            }
        }

        /// <summary>
        /// Store a reference to the grabbed object. THIS DOES NOT GRAB THE OBJECT! It is, however, called when the object is grabbed.
        /// </summary>
        private void SetGrabbedObject(GameObject objectToGrab)
        {
            GrabbedObject = objectToGrab;
        }

        /// <summary>
        /// Anchors the player to a transform.
        /// </summary>
        public void Anchor(Transform anchor)
        {
            if (anchor == null)
            {
                transform.parent = null;
                if (_anchorConnection != null)
                {
                    Destroy(_anchorConnection);
                    _anchorConnection = null;
                }
            }
            else
            {
                // Connect the player to the anchor via a connection object (a chain, if you will)
                _anchorConnection = new GameObject();
                AnchorConnection anchorConnection = _anchorConnection.AddComponent<AnchorConnection>();
                anchorConnection.Anchor = anchor;
                _anchorConnection.transform.position = anchor.position;
                transform.SetParent(_anchorConnection.transform);
            }
        }

        /// <summary>
        /// Grab hold of a grabbable surface in the player's proximity.
        /// </summary>
        public void Grab()
        {
            RaycastHit[] hits = PhysicsExtensions.RaySphere(transform.position, transform.forward, _climbDetectionSphereVertices, _grabRange, _grabbableMask);
            RaycastHit selectedHit = new RaycastHit();
            float highestDotProduct = -2f;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider != null)
                {
                    // If the surface is too steep to walk on
                    float surfaceAngle = Vector3.Angle(Vector3.up, hits[i].normal);
                    if (surfaceAngle > _playerController.MaxAngle)
                    {
                        float value = Vector3.Dot(hits[i].normal, transform.position - hits[i].point);
                        // Store the largest dot product, i.e the normal most towards the player
                        if (value > highestDotProduct)
                        {
                            highestDotProduct = value;
                            selectedHit = hits[i];
                        }
                    }
                }
            }

            // If a hit has been selected
            if (selectedHit.collider != null)
            {
                Grabbing = true;
                _playerController.SetState(new PlClimbing(_playerController));
                SetGrabbedObject(selectedHit.collider.gameObject);
                Anchor(GrabbedObject.transform);
            }
        }

        /// <summary>
        /// Release the grip on the grabbed surface.
        /// </summary>
        public void ReleaseGrab()
        {
            Grabbing = false;
            SetGrabbedObject(null);
            Anchor(null);
            Vector3 forward = _playerController.transform.forward;
            forward.y = 0;
            forward.Normalize();
            _playerController.transform.forward = forward;
            _playerController.SetState(new PlWalking(_playerController));
        }
    }
}
