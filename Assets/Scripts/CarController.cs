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

        [Header("Settings")]
        [SerializeField] private float springStiffness;
        [SerializeField] private float damperstiffness;
        [SerializeField] private float restLength;
        [SerializeField] private float springTravel;
        [SerializeField] private float Wheelradius;


        [Header("Input")]
        [SerializeField] private float moveInput;
        [SerializeField] private float steerInput;

        [Header("Car Settings")]
        [SerializeField] private float maxSpeed;
        [SerializeField] private float acceleration;
        [SerializeField] private float deacceleration;


        private UnityEngine.Vector3 currentVelocity= UnityEngine.Vector3.zero;
        private float carVelocityRatio = 0;

        private int[] wheelisGrounded = new int[4];
        private bool isGrounded;

        private void Start()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();
        }
        private void FixedUpdate()
        {
            Suspension();
            GroundCheck();
            CalculateCarVelocity();
            HandleMovement();
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
                float maxLenght = restLength + springTravel;
                if (Physics.Raycast(raycastPoints[i].position, -raycastPoints[i].up, out hit, maxLenght, drivableLayer))
                {
                    wheelisGrounded[i] = 1;

                    float currentSpringLength = hit.distance- Wheelradius;
                    float springCompression = (restLength - currentSpringLength)/ springTravel;

                    float springVelocity = UnityEngine.Vector3.Dot(rb.GetPointVelocity(raycastPoints[i].position), raycastPoints[i].up);
                    float dampForce = springVelocity * damperstiffness;
                    float springForce = springCompression * springStiffness;
                    float netForce = springForce - dampForce;
                    rb.AddForceAtPosition(raycastPoints[i].up * netForce, raycastPoints[i].position);
                    Debug.DrawLine(raycastPoints[i].position, hit.point, Color.red);
                }
                else
                {
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
    #endregion
    }
}
