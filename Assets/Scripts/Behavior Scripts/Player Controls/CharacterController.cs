using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour
{
    private PlayerInput playerInput;
    private int gatheredTotal;
    public PlayerStamina _playerStamina;
    public float tireLimit = 10f;
    public float speed = 2.5f;
    public bool sleep;
    public float tireRate = .01f;
    public float restoreRate = .5f;
    Rigidbody2D rigidbody2d;
    float horizontal;
    float vertical;
    Animator animator;
    Vector2 lookDirection = new Vector2(1,0);
    
    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sleep = false;
    }
    void Update()
    {
        if (GameController.instance.gamePlaying)
        {
            GetPlayerInput();
        }
    }

    void FixedUpdate()
    {
        if (GameController.instance.gamePlaying)
        {
            if (sleep == true)
            {
            ChangeStamina(restoreRate);
            } else if(sleep == false && _playerStamina.currentStamina <= tireLimit) {
                animator.SetTrigger("Tired");
                ChangeStamina(-tireRate);
                Movement(0.5f);
            } else if (_playerStamina.currentStamina == _playerStamina.maxStamina) {
                animator.SetTrigger("Wake");
                Wake();
                ChangeStamina(-tireRate);
                Movement(speed);
            }
        } else {
            ChangeStamina(0);
            Movement(0);
        }
    }

    private void ChangeStamina(float rate)
    {
        _playerStamina.currentStamina = Mathf.Clamp(_playerStamina.currentStamina + rate, 0, _playerStamina.maxStamina);
    }

    private void Movement(float quick)
    {
        Vector2 position = rigidbody2d.position;
        position.x = position.x + quick * horizontal * Time.deltaTime;
        position.y = position.y + quick * vertical * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }

    public void FoodStamina(int amount)
    {
        _playerStamina.currentStamina = Mathf.Clamp(_playerStamina.currentStamina + amount, 0, _playerStamina.maxStamina); 
    }

    void Sleep()
    {
        animator.SetTrigger("Sleep");
        sleep = true;
        Debug.Log("Player is asleep");
    }

    void Wake()
    {
        animator.SetTrigger("Awake");
        sleep = false;
        Debug.Log("Player is awake");
    }

    private void GetPlayerInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);

        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        if(Input.GetKeyDown(KeyCode.F))
        {
            sleep = true;
            Sleep();
        }
    }
}
