using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(BoxCollider))]
public class PlayerController : MonoBehaviour
{
    public Transform startPosition;

    public Transform cameraTransform;

    public float playerViewYOffset = 0.6f;
    public float yMouseSensitivity = 30.0f;
    public float xMouseSensitivity = 30.0f;
    private float sensitivityMultiplier = .2f;


    public float gravity = 20.0f;

    public float friction = 6;

    public float moveSpeed = 7.0f;
    public float runAcceleration = 14.0f;
    public float runDeacceleration = 10.0f;

    public float airAcceleration = 2.0f;
    public float airDecceleration = 2.0f;
    public float airControl = 0.3f;
    public float sideStrafeAcceleration = 50.0f;
    public float sideStrafeSpeed = 1.0f;
    public float jumpSpeed = 8.0f;

    public GameState GameState;

    private float moveForward;
    private float moveSideways;
    private CharacterController characterController;

    private float rotY = 0.0f;
    private float rotX = 0.0f;

    private Vector3 moveDirectionNorm = Vector3.zero;
    private Vector3 playerVelocity = Vector3.zero;
    private bool wishJump = false;


    private void Start()
    {
        transform.position = startPosition.position;

        cameraTransform.position = new Vector3(
                transform.position.x,
                transform.position.y + playerViewYOffset,
                transform.position.z);

        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        RotateCamera();

        QueueJump();

        HandleMovement();

        UpdateCamera();

        CheckOutOfBounds();
    }

    private void UpdateCamera()
    {
        cameraTransform.position = new Vector3(
                        transform.position.x,
                        transform.position.y + playerViewYOffset,
                        transform.position.z);
    }

    private void HandleMovement()
    {
        if (characterController.isGrounded)
            GroundMove();
        else if (!characterController.isGrounded)
            AirMove();

        characterController.Move(playerVelocity * Time.deltaTime);
    }

    private void CheckOutOfBounds()
    {
        if (transform.position.y < -20)
        {
            transform.position = new Vector3(startPosition.position.x, startPosition.position.y, startPosition.position.z);
            playerVelocity = Vector3.zero;
            GameState.OnPlayerDeath();
        }
    }

    private void RotateCamera()
    {
        rotY += Input.GetAxisRaw("Mouse Y") * yMouseSensitivity * sensitivityMultiplier;
        rotX += Input.GetAxisRaw("Mouse X") * xMouseSensitivity * sensitivityMultiplier;

        rotY = Mathf.Clamp(rotY, -90, 90);

        this.transform.rotation = Quaternion.Euler(0, rotX, 0);
        cameraTransform.rotation = Quaternion.Euler(rotY, rotX, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickup"))
        {
            GameState.OnPickupCollision(other.gameObject);
        }
        else if (other.CompareTag("Finish"))
        {
            GameState.OnNextLevel();
            startPosition = GameState.GetStartPoint();
            transform.position = startPosition.position;
        }

    }

    private void SetMovementDir()
    {
        moveForward = Input.GetAxisRaw("Vertical");
        moveSideways = Input.GetAxisRaw("Horizontal");
    }

    private void QueueJump()
    {
        wishJump = Input.GetButton("Jump");
    }

    /**
	 * Execs when the player is in the air
	 */
    private void AirMove()
    {
        Vector3 wishdir;
        float wishvel = airAcceleration;
        float accel;

        SetMovementDir();

        wishdir = new Vector3(moveSideways, 0, moveForward);
        wishdir = transform.TransformDirection(wishdir);

        float wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        wishdir.Normalize();
        moveDirectionNorm = wishdir;


        float wishspeed2 = wishspeed;
        if (Vector3.Dot(playerVelocity, wishdir) < 0)
            accel = airDecceleration;
        else
            accel = airAcceleration;

        if (moveForward == 0 && moveSideways != 0)
        {
            if (wishspeed > sideStrafeSpeed)
                wishspeed = sideStrafeSpeed;
            accel = sideStrafeAcceleration;
        }

        Accelerate(wishdir, wishspeed, accel);
        if (airControl > 0)
            AirControl(wishdir, wishspeed2);



        playerVelocity.y -= gravity * Time.deltaTime;
    }


    private void AirControl(Vector3 wishdir, float wishspeed)
    {
       
       
        // Can't control movement if not moving forward or backward
        if (Mathf.Abs(moveForward) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
            return;

        float zspeed = playerVelocity.y;
        playerVelocity.y = 0;

        float speed = playerVelocity.magnitude;
        playerVelocity.Normalize();

        float dot = Vector3.Dot(playerVelocity, wishdir);
        float k = 32;
        k *= airControl * dot * dot * Time.deltaTime;

        if (dot > 0)
        {
            playerVelocity.x = playerVelocity.x * speed + wishdir.x * k;
            playerVelocity.y = playerVelocity.y * speed + wishdir.y * k;
            playerVelocity.z = playerVelocity.z * speed + wishdir.z * k;

            playerVelocity.Normalize();
            moveDirectionNorm = playerVelocity;
        }

        playerVelocity.x *= speed;
        playerVelocity.y = zspeed;
        playerVelocity.z *= speed;
    }

    /**
	 * Called every frame when the engine detects that the player is on the ground
	 */
    private void GroundMove()
    {
        Vector3 wishdir;

        if (wishJump)
            ApplyFriction(0);
        else
            ApplyFriction(1.0f);

        SetMovementDir();

        wishdir = new Vector3(moveSideways, 0, moveForward);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        var wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        Accelerate(wishdir, wishspeed, runAcceleration);

        // Reset the gravity velocity
        playerVelocity.y = -gravity * Time.deltaTime;

        if (wishJump)
        {
            playerVelocity.y = jumpSpeed;
            wishJump = false;
        }
    }

    private void ApplyFriction(float t)
    {
        Vector3 vec = playerVelocity;
        vec.y = 0.0f;
        float speed = vec.magnitude;
        float drop = 0.0f;


        if (characterController.isGrounded)
        {
            float control = speed < runDeacceleration ? runDeacceleration : speed;
            drop = control * friction * Time.deltaTime * t;
        }

        float newspeed = speed - drop;

        if (newspeed < 0)
            newspeed = 0;
        if (speed > 0)
            newspeed /= speed;

        playerVelocity.x *= newspeed;
        playerVelocity.z *= newspeed;
    }

    private void Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        float currentspeed = Vector3.Dot(playerVelocity, wishdir);
        float addspeed = wishspeed - currentspeed;
        if (addspeed <= 0)
            return;
        float accelspeed = accel * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
    }


}

