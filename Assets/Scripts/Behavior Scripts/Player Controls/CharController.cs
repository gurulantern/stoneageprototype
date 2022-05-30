using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class CharController : MonoBehaviour
{
    [SerializeField] private PlayerControls playerControls;
    [SerializeField] private GameEvent _observeDone;
    public PlayerStamina _playerStamina;
    
    private Vector2 moveInput;
    public float tireLimit = 10f;
    private bool sleep;
    private bool observing;
    public float tireRate = .01f;
    public float restoreRate = .5f;
    private float speed;
    Rigidbody2D rb;
    Animator animator;
    Vector2 lookDirection = new Vector2(1,0);
    
    private void Awake() {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        _playerStamina.currentStamina = 100;
    }

    void Start()
    {
        sleep = false;
    }
    void FixedUpdate()
    {
        if (GameController.instance.gamePlaying)
        {
            if (_playerStamina.currentStamina == _playerStamina.maxStamina && sleep == true) {
                Wake();
            } else if (sleep == true) {
                ChangeStamina(restoreRate);
                playerControls.Disable();
                Debug.Log("Sleeping and restoring stamina");
            } else if (animator.GetBool("Observe") == true || animator.GetBool("Gather") == true && sleep == false) {
                playerControls.Disable();
                ChangeStamina(-tireRate);
            } else if (sleep == false && _playerStamina.currentStamina <= tireLimit) {
                animator.SetBool("Tired", true);
                ChangeStamina(-tireRate);
                speed = 0.5f;
                rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
            } else if (sleep == false) {
                ChangeStamina(-tireRate);
                speed = 2.5f;
                rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
            }
        } else {
            playerControls.Disable();
        }
    }

    private void ChangeStamina(float rate)
    {
        _playerStamina.currentStamina = Mathf.Clamp(_playerStamina.currentStamina + rate, 0, _playerStamina.maxStamina);
    }

    public void FoodStamina(int amount)
    {
        _playerStamina.currentStamina = Mathf.Clamp(_playerStamina.currentStamina + amount, 0, _playerStamina.maxStamina); 
    }

    private void OnSleep()
    {
        if(sleep == false) {
            sleep = true;
            animator.SetTrigger("Sleep");
            Debug.Log("Player is asleep");
        }
    }

    void Wake()
    {
        animator.SetBool("Tired", false);
        animator.SetTrigger("Awake");
        sleep = false;
        Debug.Log("Player is awake");
    }

    public void OnObserve()
    {
        animator.SetBool("Observe", true);
        Debug.Log("Player observes");
    }

    public void StopObserve()
    {
        animator.SetBool("Observe", false);
        _observeDone?.Invoke();
    }

    private void OnMove(InputValue value) {
        moveInput = value.Get<Vector2>();

        if(!Mathf.Approximately(moveInput.x, 0.0f) || !Mathf.Approximately(moveInput.y, 0.0f))
        {
            lookDirection.Set(moveInput.x, moveInput.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", moveInput.magnitude);

        Debug.Log("Moving");
    }
}
