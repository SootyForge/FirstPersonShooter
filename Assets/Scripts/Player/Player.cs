using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class Player : MonoBehaviour, IKillable
{
    [Header("Mechanics")]
    public int health = 100;
    public float runSpeed = 7.5f;
    public float walkSpeed = 6f;
    public float gravity = 10f;
    public float crouchSpeed = 4f;
    public float jumpHeight = 20f;
    public int maxJumps = 2;
    public float interactRange = 10f;
    public float groundRayDistance = 1.1f;

    [Header("UI")]
    public GameObject interactUIPrefab; // Prefab of text to show up when interacting
    public Transform interactUIParent; // Tranform (Panel) to attach it to on start

    [Header("References")]
    public Camera attachedCamera;
    public Transform hand;

    //Mechanics
    private int jumps = 0;

    //Animation
    private Animator anim;

    // Movement
    private CharacterController controller;
    private Vector3 movement; // Movement for current frame

    // Weapons
    public Weapon currentWeapon; // Public for testing
    private List<Weapon> weapons = new List<Weapon>();
    private int currentWeaponIndex = 0;

    // UI
    private GameObject interactUI;
    private TextMeshProUGUI interactText;

    /*
    private int score = 0;
    void UpdateUI(int score)
    {

    }
    // Update's UI when score changes
    public int Score
    {
        get { return Score; }
        set
        {
            UpdateUI(value);
            Score = value;
        }
    }
    */

    void DrawRay (Ray ray, float distance)
    {
        Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * interactRange);
    }

    void OnDrawGizmosSelected()
    {
        Ray interactRay = attachedCamera.ViewportPointToRay(new Vector2(.5f, .5f));
        Gizmos.color = Color.blue;
        DrawRay(interactRay, interactRange);

        Gizmos.color = Color.red;
        Ray groundRay = new Ray(transform.position, -transform.up);
        DrawRay(groundRay, groundRayDistance);
    }

    #region Initialisation
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        CreateUI();
        RegisterWeapons();
    }
    void Start()
    {
        // Select current weapon at start
        SelectWeapon(0);
    }
    void CreateUI()
    {
        interactUI = Instantiate(interactUIPrefab, interactUIParent);
        interactText = interactUI.GetComponentInChildren<TextMeshProUGUI>();
    }
    void RegisterWeapons()
    {
        weapons = new List<Weapon>(GetComponentsInChildren<Weapon>());
    }
    #endregion

    #region Controls
    /// <summary>
    /// Moves the Character Controller in direction of input
    /// </summary>
    /// <param name="inputH">Horizontal Input</param>
    /// <param name="inputV">Vertical Input</param>
    void Move(float inputH, float inputV)
    {
        // Create direction from input
        Vector3 input = new Vector3(inputH, 0, inputV);
        // Localise direction to player transform
        input = transform.TransformDirection(input);
        // Set Move Speed Note(Manny): Add speed mechanic here
        float moveSpeed = walkSpeed;
        // Apply movement
        movement.x = input.x * moveSpeed;
        movement.z = input.z * moveSpeed;
    }
    #endregion

    #region Combat
    /// <summary>
    /// Switches between weapons with given direction
    /// </summary>
    /// <param name="direction">-1 to 1 number for list selection</param>
    void SwitchWeapons(int direction)
    {

    }
    /// <summary>
    /// Disables GameObjects of every attached weapon
    /// </summary>
    void DisableAllWeapons()
    {

    }
    /// <summary>
    /// Adds weapon to list and attaches to player's hand
    /// </summary>
    /// <param name="weaponToPickup">Weapon to place in Hand</param>
    void Pickup(Weapon weaponToPickup)
    {

    }
    /// <summary>
    /// Removes weapon to list and removes from player's hand
    /// </summary>
    /// <param name="weaponToDrop">Weapon to remove from hand</param>
    void Drop(Weapon weaponToDrop)
    {

    }
    /// <summary>
    /// Sets currentWeapon to weapon at given index
    /// </summary>
    /// <param name="index">Weapon Index</param>
    void SelectWeapon(int index)
    {

    }
    #endregion
    
    #region Actions
    /// <summary>
    /// Player movement using CharacterController
    /// </summary>
    void Movement()
    {
        // Get Input from User
        float inputH = Input.GetAxis("Horizontal");
        float inputV = Input.GetAxis("Vertical");
        Move(inputH, inputV);
        // Is the controller grounded?
        Ray groundRay = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        bool isGrounded = Physics.Raycast(groundRay, out hit, groundRayDistance);
        bool isJumping = Input.GetButtonDown("Jump");
        bool canJump = jumps < maxJumps; // jumps = int, maxJumps = int

        // Is grounded?
        if (isGrounded)
        {
            // If jump is pressed
            if (isJumping)
            {
                jumps = 1;
                // Move controller up
                movement.y = jumpHeight;
            } 
        }
        // Is NOT grounded?
        else
        {
            if (isJumping && canJump)
            {
                movement.y = jumpHeight * jumps;
                jumps++;
            }
        }
        
        // Apply gravity
        movement.y -= gravity * Time.deltaTime;
        // Limit the gravity
        movement.y = Mathf.Max(movement.y, -gravity);
        // Move the controller
        controller.Move(movement * Time.deltaTime);
    }
    /// <summary>
    /// Interaction with items in the world
    /// </summary>
    void Interact()
    {
        // Disable interact UI
        interactUI.SetActive(false);
        
        // Create ray from centre of screen
        Ray interactRay = attachedCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f));
        RaycastHit hit;
        // Shoot ray in a range
        if (Physics.Raycast(interactRay, out hit, interactRange))
        {
            // Try getting IInteractable Object
            IInteractable interact = hit.collider.GetComponent<IInteractable>();
            // Is Interactable
            if (interact != null)
            {
                // Enable the UI
                interactUI.SetActive(true);
                // Change the text to item's title
                interactText.text = interact.GetTitle();
                // Get input from user
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // Check if the thing we hit is a weapon (now that we know it's an interactable)
                    Weapon weapon = hit.collider.GetComponent<Weapon>();
                    if (weapon)
                    {
                        // Pick it up!
                        Pickup(weapon);
                    } 
                }
            }
        }

    }
    /// <summary>
    /// Using the current weapon to fire a bullet
    /// </summary>
    void Shooting()
    {

    }
    /// <summary>
    /// Cycling through available weapons
    /// </summary>
    void Switching()
    {

    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        Movement();
        Interact();
        Shooting();
        Switching();
    }

    public void Kill()
    {
        throw new System.NotImplementedException();
    }
    public void TakeDamage(int damage)
    {
        throw new System.NotImplementedException();
    }
}
