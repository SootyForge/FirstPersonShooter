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
    }
    void Start()
    {
        RegisterWeapons();
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
        foreach (Weapon weapon in weapons)
        {
            weapon.Pickup();
        }
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

    void AttachWeapon(Weapon weaponToAttach)
    {
        // Call Pickup on the weapon
        weaponToAttach.Pickup();
        // Get transform
        Transform weaponTranform = weaponToAttach.transform;
        // Attach weapon to hand
        weaponTranform.SetParent(hand);
        // Zero rotation and position
        weaponTranform.localRotation = Quaternion.identity;
        weaponTranform.localPosition = Vector3.zero;
    }
    void DetachWeapon(Weapon weaponToDetach)
    {
        // Drop weapon
        weaponToDetach.Drop();
        // Get the transform
        Transform weaponTransform = weaponToDetach.transform;
        weaponTransform.SetParent(null);
    }

    #region Combat
    /// <summary>
    /// Switches between weapons with given direction
    /// </summary>
    /// <param name="direction">-1 to 1 number for list selection</param>
    void SwitchWeapon(int direction)
    {
        // Offset weapon index with direction
        currentWeaponIndex += direction;
        // Check if index is below zero
        if (currentWeaponIndex < 0)
        {
            // Loop back to end
            currentWeaponIndex = weapons.Count - 1;
        }
        // Check if index is exceeding length
        if (currentWeaponIndex >= weapons.Count)
        {
            // Reset back to zero
            currentWeaponIndex = 0;
        }
        // Select weapon
        SelectWeapon(currentWeaponIndex);
    }
    /// <summary>
    /// Disables GameObjects of every attached weapon
    /// </summary>
    void DisableAllWeapons()
    {
        // Loop through all weapons
        foreach (Weapon item in weapons)
        {
            // Deactivate it!
            item.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Adds weapon to list and attaches to player's hand
    /// </summary>
    /// <param name="weaponToPickup">Weapon to place in Hand</param>
    void Pickup(Weapon weaponToPickup)
    {
        AttachWeapon(weaponToPickup);
        // Add to list
        weapons.Add(weaponToPickup);
        // Select new weapon
        SelectWeapon(weapons.Count - 1);
    }
    /// <summary>
    /// Removes weapon to list and removes from player's hand
    /// </summary>
    /// <param name="weaponToDrop">Weapon to remove from hand</param>
    void Drop(Weapon weaponToDrop)
    {
        // Drop weapon
        weaponToDrop.Drop();
        // Get the transform
        Transform weaponTransform = weaponToDrop.transform;
        weaponTransform.SetParent(null);
        // Remove weapon from list
        weapons.Remove(weaponToDrop);
    }
    /// <summary>
    /// Sets currentWeapon to weapon at given index
    /// </summary>
    /// <param name="index">Weapon Index</param>
    void SelectWeapon(int index)
    {
        // Is index in range?
        if (index >= 0 && index < weapons.Count)
        {
            // Disable all weapons
            DisableAllWeapons();
            // Select weapon
            currentWeapon = weapons[index];
            // Enable the current weapon (using index)
            currentWeapon.gameObject.SetActive(true);
            // Update the current index
            currentWeaponIndex = index;
        }
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
        // Is a current weapon selected
        if (currentWeapon)
        {
            // Is the fire button pressed?
            if (Input.GetButton("Fire1"))
            {
                // Shoot the current weapon
                currentWeapon.Shoot();
            }
        }
    }
    /// <summary>
    /// Cycling through available weapons
    /// </summary>
    void Switching()
    {
        // If there is more than one weapon
        if (weapons.Count > 1)
        {
            float inputScroll = Input.GetAxis("Mouse ScrollWheel");
            // If scroll input has been made
            if (inputScroll != 0)
            {
                int direction = inputScroll > 0 ?
                                    Mathf.CeilToInt(inputScroll)
                                :
                                    Mathf.FloorToInt(inputScroll);
                // Switch Weapons up or down
                SwitchWeapon(direction);
            }
        }
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
