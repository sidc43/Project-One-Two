using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Attributes")]
    public float moveSpeed;
    public float jumpForce;
    public Vector2 spawnPos;
    public bool onGround;
    public Vector2Int mousePos;
    public int attackRange;
    public int placeRange;
    public Inventory inventory;

    [SerializeField] private TerrainGenerator terrainGenerator;
    [SerializeField] private CinemachineVirtualCamera vCam;
    [SerializeField] private TileClass selectedTile;

    private Rigidbody2D rb;
    private Animator animator;
    private float horizontal;
    private float vertical;
    private Vector2 movement;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        inventory = GetComponent<Inventory>();
    }
    private void FixedUpdate()
    {
        GetAxes();   
        movement = new(horizontal * moveSpeed, rb.velocity.y);

        Jump(vertical);
        FlipSprite();
        rb.velocity = movement;
    }
    private void Update()
    {
        SetMousePos();

        HandleAnimations();
        HandleHit();
        HandlePlace();
    }

    #region GENERAL
    public void Spawn()
    {
        GetComponent<Transform>().position = spawnPos;
        
        vCam.gameObject.SetActive(true);
    }
    private void GetAxes()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxisRaw("Jump");
    }
    private void SetMousePos()
    {
        mousePos.x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 0.5f);
        mousePos.y = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y - 0.5f);
    }
    #endregion

    #region MOVEMENT
    private void HandleHit()
    {
        if (Vector2.Distance(transform.position, mousePos) <= attackRange && Utility.LMB)
            terrainGenerator.RemoveTile(mousePos.x, mousePos.y);
    }
    private void HandlePlace()
    {
        if (Vector2.Distance(transform.position, mousePos) <= placeRange && Utility.RMB)
        {
            terrainGenerator.PlaceTile(selectedTile, mousePos.x, mousePos.y, this);
        }
    }
    private void Jump(float jump)
    {
        if (jump > 0.1f)
        {
            if (onGround)
                movement.y = jumpForce;
        }
    }
    #endregion

    #region ANIMATIONS
    private void HandleAnimations()
    {
        animator.SetFloat("Horizontal", horizontal);

        if (Utility.LMB || Utility.RMB)
            animator.SetBool("Hit", true);
    }
    private void FlipSprite()
    {
        if (horizontal < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (horizontal > 0)
            transform.localScale = new Vector3(1, 1, 1);
    }
    public void StopHitAnimation()
    {
        animator.SetBool("Hit", false);
    }
    #endregion

    #region COLLISIONS
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
            onGround = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
            onGround = false;
    }
    #endregion
}