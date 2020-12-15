using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPun
{
    public KeyCode up = KeyCode.W;
    public KeyCode down = KeyCode.S;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    [Space]
    public float movementSpeed = 1f;

    private Animator animator;
    private Rigidbody2D rb;

    Vector2 networkPosition;
    Vector2 movement;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (photonView.IsMine)
            ProcessInput();
    }

    bool moved;

    private void FixedUpdate()
    {
        if (photonView.IsMine && !Ability.abilityInUse)
        {
            if (moved = (movement != Vector2.zero))
            {
                rb.MovePosition((Vector2)transform.position + movement.normalized * movementSpeed * Time.fixedDeltaTime);
            }
            animator.SetBool("walk", moved);
        }
    }

    private void ProcessInput()
    {
        movement = Vector2.zero;

        if (Input.GetKey(up))
        {
            movement += Vector2.up;
        }
        if (Input.GetKey(down))
        {
            movement -= Vector2.up;
        }
        if (Input.GetKey(right))
        {
            movement += Vector2.right;
        }
        if (Input.GetKey(left))
        {
            movement -= Vector2.right;
        }
    }
}
