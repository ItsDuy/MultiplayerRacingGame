using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Kart
{
    public class CarController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform[] raycastPoints;
        [SerializeField] private LayerMask drivableLayer;
        [SerializeField]private Transform acclerationPoint;
        [SerializeField] private GameObject[] Tires = new GameObject[4];

        [Header("Settings")]
        [SerializeField] private float springStiffness;
        [SerializeField] private float damperstiffness;
        [SerializeField] private float restLength;
        [SerializeField] private float springTravel;
        [SerializeField] private float Wheelradius;
        [SerializeField] private AnimationCurve turningCurve;
        [SerializeField] private float dragCoefficient;


        [Header("Input")]
        [SerializeField] private float moveInput;
        [SerializeField] private float steerInput;

        [Header("Car Settings")]
        [SerializeField] private float maxSpeed;
        [SerializeField] private float acceleration;
        [SerializeField] private float deacceleration;
        [SerializeField] private float steerStrength;

        [Header("Visuals")]
        [SerializeField] private float tireRotationSpeed = 360f;
        private UnityEngine.Vector3 currentVelocity= UnityEngine.Vector3.zero;
        private float carVelocityRatio = 0;

        private int[] wheelisGrounded = new int[4];
        private bool isGrounded;

        private void Start()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();

            if (acclerationPoint != null)
            {
                acclerationPoint.position = rb.worldCenterOfMass;
            }
        }
        private void FixedUpdate()
        {
            Suspension();
            GroundCheck();
            CalculateCarVelocity();
            HandleMovement();
            Steer();
            SideWayDrag();
            // Visuals();
        }
        
        private void Update()
        {
            GetPlayerInput();
        }
    #region Car Suspesion
        private void Suspension()
        {
            for (int i = 0; i < raycastPoints.Length; i++)
            {
                RaycastHit hit;
                float maxLength = restLength + springTravel;
                if (Physics.Raycast(raycastPoints[i].position, -raycastPoints[i].up, out hit, maxLength, drivableLayer))
                {
                    wheelisGrounded[i] = 1;

                    float currentSpringLength = hit.distance- Wheelradius;
                    float springCompression = (restLength - currentSpringLength)/ springTravel;

                    float springVelocity = UnityEngine.Vector3.Dot(rb.GetPointVelocity(raycastPoints[i].position), raycastPoints[i].up);
                    float dampForce = springVelocity * damperstiffness;
                    float springForce = springCompression * springStiffness;
                    float netForce = springForce - dampForce;
                    rb.AddForceAtPosition(raycastPoints[i].up * netForce, raycastPoints[i].position);

                    //Visuals
                    // SetTirePosition(Tires[i], hit.point + raycastPoints[i].up * Wheelradius);                   
                    Debug.DrawLine(raycastPoints[i].position, hit.point, Color.red);
                }
                else
                {
                    wheelisGrounded[i] = 0;
                    // SetTirePosition(Tires[i], raycastPoints[i].position - raycastPoints[i].up * maxLength);
                    Debug.DrawLine(raycastPoints[i].position, raycastPoints[i].position - raycastPoints[i].up * (restLength + springTravel), Color.green);
                }
            }
        }
    #endregion
    #region Car status check
    private void GroundCheck()
    {
        isGrounded = false;
        for (int i = 0; i < wheelisGrounded.Length; i++)
        {
            if (wheelisGrounded[i] == 1)
            {
                isGrounded = true;
                break;
            }
        }

    }

    private void CalculateCarVelocity()
    {
        currentVelocity=transform.InverseTransformDirection(rb.velocity);
        carVelocityRatio = currentVelocity.z / maxSpeed;
    }
    #endregion
    #region Input Handling
    private void GetPlayerInput()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }
    #endregion
    
    #region Movement Handling
    private void HandleMovement()
    {
        if (isGrounded)
        {
            if (moveInput > 0)
            {
                Acceleration();
            }
            else if (moveInput < 0)
            {
                Deacceleration();
            }
        }
    }
    private void Acceleration()
    {
        rb.AddForceAtPosition(transform.forward * moveInput * acceleration, acclerationPoint.position,ForceMode.Acceleration);
    }
    private void Deacceleration()
    {
        rb.AddForceAtPosition(-transform.forward * deacceleration, acclerationPoint.position, ForceMode.Acceleration);
    }
    private void Steer()
    {
        rb.AddTorque(steerInput* steerStrength * turningCurve.Evaluate(carVelocityRatio) * Mathf.Sign(carVelocityRatio)*transform.up, ForceMode.Acceleration);
    }
    
    private void SideWayDrag()
        {
            float currentSidewaySpeed = currentVelocity.x;
            float dragForce = -currentSidewaySpeed * dragCoefficient;
            rb.AddForceAtPosition(dragForce*transform.right, rb.centerOfMass,ForceMode.Acceleration);
        }
    #endregion
    

    // #region Visuals
    // private void Visuals()
    // {
    //     TireVisuals();
    // }
    // private void TireVisuals()
    // {
    //     for (int i = 0; i < Tires.Length; i++)
    //     {
    //             // Rotate tires based on car's forward velocity
    //             if (i < 2)
    //             {
    //              Tires[i].transform.Rotate(UnityEngine.Vector3.right,tireRotationSpeed * carVelocityRatio * Time.deltaTime,Space.Self);
    //             }
    //             else
    //             {
    //                 Tires[i].transform.Rotate(UnityEngine.Vector3.right, tireRotationSpeed * moveInput* Time.deltaTime, Space.Self);
    //             }
    //     }
    // }
    // private void SetTirePosition(GameObject tire, UnityEngine.Vector3 targetPosition)
    // {
    //     tire.transform.position =  targetPosition;
    // }
    // }
    // #endregion
}
}
