using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerControl : MonoBehaviourPunCallbacks
{
    [Header("MOVEMENTS_CAMERA")]
    [SerializeField] public Transform viewPoint;// parent of camera
    [SerializeField] public float mouseSensitivity = 1f;
    private float verticalRotStore;  //store value for not have quirk with Quaternion movements
    private Vector2 mouseInput;
    [SerializeField] public bool invertLook;

    [Header("MOVEMENTS")]
    public float moveSpeed = 5f, runSpeed = 8f;
    private float activeMoveSpeed;
    private Vector3 moveDir, movement;

    [Header("JUMPING")]
    public float jumpForce = 12f, gravityMode = 2.5f;

    public Transform groundCheckPoint; //N13 need to create child object with y-1 and drop 
    private bool isGrounded;
    public LayerMask groundLayers; //N13 need to create new layerMask(Like Tag) and do this layer for teren and select in player

    [Header("JUMPING")]


    public CharacterController characterController; // like rigitbody need to add and droop

    private Camera Mainamera;




    [Header("EFFECTS")]
    public GameObject buletImpact;// need to create and droop

    [Header("SHOTING")]
    //public float timeBetwenShoots = .1f; andheatPerShot  handle those from Gun script
    private float shootCounter;

    public float maxHeat = 10f,/* heatPerShot = 1f,*/ coolRate = 4f, overheatCoolRate = 5f; //N20 maxHeat-maxLevel when heat can go to/  coolRate-how fast meter go back to 0/ overheatCoolRate-when overheated
    private float heatCounter; //N20  current value overheating
    private bool overHeated;

    [Header("GUNS")]
    public Gun[] allGuns;
    private int selectedGun;

    [Header("GUNS EFFECTS")]
    public float muzzleDisplayTime = 1/60;
    private float muzzleConter;


   




    public GameObject playerHitImpact;

    public int maxHealth = 100;
    private int currentHealth;

    public Animator animator;
    public GameObject playerModel;
    public Transform modelGunPoint, gunHolder;


    public Material[] allSkins;

    private float adsSpeed = 5f;
    public Transform adsOutPoint, adsInPoint;

    public AudioSource footStepSlow, footStepFast;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //for lock mouse in window


        Mainamera = Camera.main;

        UIController.instance.weaponTempSlider.maxValue = maxHeat; //N22 for handle temp slider

        photonView.RPC("SetGun", RpcTarget.All, selectedGun);

        currentHealth = maxHealth;


        //SwitchGun();

        //Transform newTrans = SpownManager.instance.GetSpawnPoint(); All that stuff i handle in SpownManager for working corect in network
        //transform.position = newTrans.position;  All that stuff i handle in SpownManager for working corect in network
        //transform.rotation = newTrans.rotation; All that stuff i handle in SpownManager for working corect in network


        if (photonView.IsMine)
        {
            playerModel.SetActive(false);

            UIController.instance.healthSlider.maxValue = maxHealth;
            UIController.instance.healthSlider.value = currentHealth;
        }
        else
        {
            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }

        playerModel.GetComponent<Renderer>().material = allSkins[photonView.Owner.ActorNumber % allSkins.Length];
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if (!UIController.instance.optionScreen.activeInHierarchy)
            {

                //-----------------------------------------------------------------Movements-----------------------------------------------------
                mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity; //created references to mouse movements
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);//move player in y vector

                verticalRotStore += mouseInput.y; // for not have quirk with Quaternion movements
                verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);// for limits movements up/down


                if (invertLook)
                {
                    viewPoint.rotation = Quaternion.Euler(verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);

                }
                else
                {
                    viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
                }


                moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));  //created references to forwards/left/ mpvements


                if (Input.GetKey(KeyCode.LeftShift))
                {
                    activeMoveSpeed = runSpeed;
                    if (!footStepFast.isPlaying && moveDir != Vector3.zero)
                    {
                        footStepFast.Play();
                        footStepSlow.Stop();
                    }
                }
                else
                {
                    activeMoveSpeed = moveSpeed;
                    if (!footStepSlow.isPlaying && moveDir != Vector3.zero)
                    {
                        footStepFast.Stop();
                        footStepSlow.Play();
                    }
                }
                if (moveDir == Vector3.zero || !isGrounded)
                {
                    footStepFast.Stop();
                    footStepSlow.Stop();
                }
                //-----------------------------------------------------------------Movements-----------------------------------------------------





                //-----------------------------------------------------------------Jumping----------------------------------------------------------
                float yVelocity = movement.y;   //N11 for store gravity val
                movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * activeMoveSpeed; // were i actualy move player   .normalized - for not moving x2 speed on diagonal
                movement.y = yVelocity; //N11 aply gravity
                if (characterController.isGrounded)
                {
                    movement.y = 0;  //N11 for not going constantly down
                }

                isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayers); // N13 Invisible line from,howlong,whatCheck

                if (Input.GetButtonDown("Jump") && isGrounded)
                {
                    movement.y = jumpForce;  //N12 add force to jump
                }


                movement.y += Physics.gravity.y * Time.deltaTime * gravityMode;//N11 Handle gravity

                //-----------------------------------------------------------------Jumping----------------------------------------------------------





                //-----------------------------------------------------------------Movements_All-----------------------------------------------------
                //transform.position += movement * moveSpeed * Time.deltaTime;
                characterController.Move(movement * Time.deltaTime);  // were i actualy move player currently

                //-----------------------------------------------------------------Movements_All-----------------------------------------------------

                //-----------------------------------------------------------------MuzzleFlash Shoting-----------------------------------------------------
                if (allGuns[selectedGun].muzzleFlash.activeInHierarchy)// 27 For not have problem with visualisation
                {
                    muzzleConter -= Time.deltaTime;
                    if (muzzleConter <= 0)//Checking OFF befor shoting
                    {
                        allGuns[selectedGun].muzzleFlash.SetActive(false);
                    }
                }
                //-----------------------------------------------------------------MuzzleFlash Shoting-----------------------------------------------------

                //-----------------------------------------------------------------Shoting-----------------------------------------------------
                if (!overHeated)
                {              
                    if (Input.GetMouseButtonDown(0))
                    {
                        Shoot();
                    }
                    //-----------------------------------------------------------------Automating_Shoting-----------------------------------------------------
                    if (Input.GetMouseButton(0) && allGuns[selectedGun].isAutomatic)
                    {
                        shootCounter -= Time.deltaTime;

                        if (shootCounter <= 0)
                        {
                            Shoot();
                        }
                    }
                    //-----------------------------------------------------------------Automating_Shoting-----------------------------------------------------

                    heatCounter -= coolRate * Time.deltaTime;
                }
                else
                {
                    heatCounter -= overheatCoolRate * Time.deltaTime;
                    if (heatCounter <= 0)
                    {
                        heatCounter = 0;
                        overHeated = false;
                        UIController.instance.overheatedMessage.gameObject.SetActive(false);
                    }
                }
                if (heatCounter < 0)// exteacheck
                {
                    heatCounter = 0f;
                }

                UIController.instance.weaponTempSlider.value = heatCounter; //N22 for handle temp slider

                //-----------------------------------------------------------------Shoting-----------------------------------------------------




                //-----------------------------------------------------------------Swithcing Weapons-----------------------------------------------------
                if (Input.GetAxisRaw("MouseScrollWheel") > 0f)//N25 if mouse scroll up 
                {
                    selectedGun++;
                    if (selectedGun >= allGuns.Length)//N25 for switching in cycle
                    {
                        selectedGun = 0;
                    }
                    photonView.RPC("SetGun", RpcTarget.All, selectedGun);
                }
                else if (Input.GetAxisRaw("MouseScrollWheel") < 0f)//N25 if mouse scroll down
                {
                    selectedGun--;
                    if (selectedGun < 0)//N25 for switching in cycle
                    {
                        selectedGun = allGuns.Length - 1;
                    }
                    photonView.RPC("SetGun", RpcTarget.All, selectedGun);
                }


                for (int i = 0; i < allGuns.Length; i++)//N25 for switching from num of keybord
                {
                    if (Input.GetKeyDown((i + 1).ToString()))
                    {
                        selectedGun = i;
                        photonView.RPC("SetGun", RpcTarget.All, selectedGun);
                    }
                }
                //-----------------------------------------------------------------Swithcing Weapons-----------------------------------------------------

                animator.SetBool("grounded", isGrounded);
                animator.SetFloat("speed", moveDir.magnitude);

                if (Input.GetMouseButton(1))
                {
                    Mainamera.fieldOfView = Mathf.Lerp(Mainamera.fieldOfView, allGuns[selectedGun].adsZoom, adsSpeed * Time.deltaTime);
                    gunHolder.position = Vector3.Lerp(gunHolder.position, adsInPoint.position, adsSpeed * Time.deltaTime);
                }
                else
                {
                    Mainamera.fieldOfView = Mathf.Lerp(Mainamera.fieldOfView, 60f, adsSpeed * Time.deltaTime);
                    gunHolder.position = Vector3.Lerp(gunHolder.position, adsOutPoint.position, adsSpeed * Time.deltaTime);
                }

                //-----------------------------------------------------------------Handle Cursor-----------------------------------------------------
                if (Input.GetKeyDown(KeyCode.Escape))  //N15 For unlock cursor IMPORTANT
                {
                    Cursor.lockState = CursorLockMode.None;//N15 For unlock cursor IMPORTANT
                }
                else if (Cursor.lockState == CursorLockMode.None)
                {
                    if (Input.GetMouseButtonDown(0) && !UIController.instance.optionScreen.activeInHierarchy)
                    {
                        Cursor.lockState = CursorLockMode.Locked;//N15 For lock cursor again after click IMPORTANT
                    }
                }
                //-----------------------------------------------------------------Handle Cursor-----------------------------------------------------
            }
        }
    }


    private void Shoot ()
    {
        Ray ray = Mainamera.ViewportPointToRay(new Vector3(.5f, .5f, 0f)); //N16 Center point of screen Camera
        ray.origin = Mainamera.transform.position; //N16 for declare positions of ray the same with camera

        if (Physics.Raycast(ray, out RaycastHit hit))//N16 in hit we store all information about shoting line
        {
            //Debug.Log("We hit " + hit.collider.gameObject.name);

            if (hit.collider.gameObject.tag == "Player") 
            {
                Debug.Log("We hit " + hit.collider.gameObject.GetPhotonView().Owner.NickName);
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);

                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, allGuns[selectedGun].shotDamage, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                GameObject buleImpactObject = Instantiate(buletImpact, hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));// N18 for to appear effect 
                Destroy(buleImpactObject, 10f);
            }
        }


        shootCounter = allGuns[selectedGun].timeBetwenSots;

        //-----------------------------------------------------------------Handle Overheating-----------------------------------------------------
        heatCounter += allGuns[selectedGun].heatPerShot;
        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;

            UIController.instance.overheatedMessage.gameObject.SetActive(true); //UIController always on Canvas
        }
        //-----------------------------------------------------------------Handle Overheating-----------------------------------------------------

        //-----------------------------------------------------------------MuzzleFlash Shoting-----------------------------------------------------
        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleConter = muzzleDisplayTime;
        //-----------------------------------------------------------------MuzzleFlash Shoting-----------------------------------------------------

        allGuns[selectedGun].shotSound.Stop();
        allGuns[selectedGun].shotSound.Play();
    }

    [PunRPC]
    public void DealDamage(string damager, int damegeAmont, int actor)
    {
        TakeDamage(damager, damegeAmont, actor);
    }

    public void TakeDamage(string damager, int damegeAmont, int actor)
    {
        //Debug.Log(photonView.Owner.NickName + " has been hit " + damager);
        //gameObject.SetActive(false);
        if (photonView.IsMine)
        {
            currentHealth -= damegeAmont;            

            if (currentHealth <= 0)
            {
                PlayerSpawner.instance.Die(damager);

                MatchManeger.instance.UpdateStatSend(actor, 0, 1);
            }

            UIController.instance.healthSlider.value = currentHealth;
        }
    }



    private void LateUpdate()
    {

        if (photonView.IsMine)
        {
            if (MatchManeger.instance.state == MatchManeger.GameState.Playing)
            {
                Mainamera.transform.position = viewPoint.position;// N10 for make cam independs from player must be outside the player and each frame move to viewPoint
                Mainamera.transform.rotation = viewPoint.rotation;// N10 for make cam independs from player must be outside the player and each frame move to viewPoint
            }
            else
            {
                Mainamera.transform.position = MatchManeger.instance.mapCamPoint.position;
                Mainamera.transform.rotation = MatchManeger.instance.mapCamPoint.rotation;
            }
        }
    }
    //-----------------------------------------------------------------Swithcing Weapons-----------------------------------------------------
    void SwitchGun()
    {
        foreach (Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }
        allGuns[selectedGun].gameObject.SetActive(true);

        allGuns[selectedGun].muzzleFlash.SetActive(false);// make shure in switching not remain muzzle
    }

    [PunRPC]
    public void SetGun(int gunToSwitchTo)
    {
        if(gunToSwitchTo < allGuns.Length)
        {
            selectedGun = gunToSwitchTo;
            SwitchGun();
        }
    }
    //-----------------------------------------------------------------Swithcing Weapons-----------------------------------------------------
}
