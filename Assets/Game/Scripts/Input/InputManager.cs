using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public Action<Vector2> OnMoveInput;
    public Action<bool> OnSprintInput;
    public Action OnJumpInput;
    public Action OnClimbInput;
    public Action OnCancelClimb;
    public Action OnChangePOV;

    private void Update()
    {
        CheckMovementInput();
        CheckInputSprint();
        CheckJumpInput();
        CheckCrouchInput();
        CheckChangePOVInput();
        CheckClimbInput();
        CheckGlidInput();
        CheckCancelInput();
        CheckPuchInput();
        CheckMainMenuInput();
    }

    private void CheckMovementInput()
    {
        float verticalAxis = Input.GetAxis("Vertical");
        float horizontalAxis = Input.GetAxis("Horizontal");
        // Debug.Log("Vertical Axis: "+verticalAxis);
        // Debug.Log("Horizontal Axis: "+horizontalAxis);
        Vector2 inputAxis = new Vector2(horizontalAxis, verticalAxis);
        if (OnMoveInput != null) {
            OnMoveInput(inputAxis);
        }
    }

    private void CheckInputSprint()
    {
        bool isHoldSprintInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (isHoldSprintInput)
        {
            // Debug.Log("Sprinting");
            if (OnSprintInput != null)
            {
                OnSprintInput(true);
            }
        }
        else
        {
            // Debug.Log("Not Sprinting");
            if (OnSprintInput != null)
            {
                OnSprintInput(false);
            }
        }
    }

    private void CheckJumpInput()
    {
        bool isPressJumpInput = Input.GetKeyDown(KeyCode.Space);
        if (isPressJumpInput)
        {
            // Debug.Log("Jump");
            if (OnJumpInput != null)
            {
                OnJumpInput();
            }
        }
    }

    private void CheckCrouchInput()
    {
        bool isPressCrouchInput = Input.GetKeyDown(KeyCode.LeftControl);
        if (isPressCrouchInput)
        {
            // Debug.Log("Crouch");
        }
    }

    private void CheckChangePOVInput()
    {
        bool isPressChangePOVInput =Input.GetKeyDown(KeyCode.Q);
         if (isPressChangePOVInput)
        {
            if (OnChangePOV != null)
            {
                OnChangePOV();
            }
        }
    }

     private void CheckClimbInput()
    {
        bool isPressClimbInput =Input.GetKeyDown(KeyCode.E);
         if (isPressClimbInput)
        {
            // Debug.Log("Climb");
            OnClimbInput();
        }
    }

     private void CheckGlidInput()
    {
        bool isCheckGlideInput =Input.GetKeyDown(KeyCode.G);
         if (isCheckGlideInput)
        {
            // Debug.Log("Glide");
        }
    }

     private void CheckCancelInput()
    {
        bool isCancelInput =Input.GetKeyDown(KeyCode.C);
         if (isCancelInput)
        {
            // Debug.Log("Cancel");
            if (OnCancelClimb != null)
            {
                OnCancelClimb();
            }
        }
    }

     private void CheckPuchInput()
    {
        bool isPuchInput =Input.GetKeyDown(KeyCode.Mouse0);
         if (isPuchInput)
        {
            // Debug.Log("Punch");
        }
    }

     private void CheckMainMenuInput()
    {
        bool isMainMenuInput =Input.GetKeyDown(KeyCode.Escape);
         if (isMainMenuInput)
        {
            Debug.Log("Main Menu");
        }
    }

   
}
