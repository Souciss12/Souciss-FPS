using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] Image healthbarImage;
    [SerializeField] GameObject UI;
    [SerializeField] Item[] items;
    [SerializeField] GameObject cameraHolder;

    public int currentItemIndex = 0;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public const float maxHealth = 100;
    public float currentHealth = maxHealth;

    public float lookSpeed = 2f;
    public float lookXLimit = 75f;

    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;

    CharacterController characterController;

    PlayerManager playerManager;

    PhotonView PV;

    void Awake()
    {
        PV = GetComponent<PhotonView>();

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    void Start()
    {
        if (PV.IsMine)
        {
            SwitchToWeapon(currentItemIndex);
        }
        if (!PV.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(UI);
        }
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!PV.IsMine)
        {
            return;
        }

        #region Handles Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        #endregion

        #region Handles Jumping
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        characterController.Move(moveDirection * Time.deltaTime);

        #endregion

        #region Handles Rotation
        if (canMove)
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

            cameraHolder.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, mouseX, 0);
        }
        #endregion

        #region Handles Items
        for (int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                SwitchToWeapon(i);
            }
        }

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if (currentItemIndex >= items.Length - 1)
            {
                SwitchToWeapon(0);
            }
            else
            {
                SwitchToWeapon(currentItemIndex + 1);
            }
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (currentItemIndex <= 0)
            {
                SwitchToWeapon(items.Length - 1);
            }
            else
            {
                SwitchToWeapon(currentItemIndex - 1);
            }

        }

        Gun currentGun = items[currentItemIndex] as Gun;
        if (currentGun != null)
        {
            if (currentGun.isAutomatic)
            {
                if (Input.GetMouseButton(0))
                {
                    items[currentItemIndex].Use();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    items[currentItemIndex].Use();
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            items[currentItemIndex].Use();
        }

        items[currentItemIndex].Aim();

        if (transform.position.y < -10f)
        {
            Die();
        }
    }
    #endregion

    void SwitchToWeapon(int index)
    {
        if (index >= items.Length) return;

        for (int i = 0; i < items.Length; i++)
        {
            items[i].gameObject.SetActive(i == index);
        }

        currentItemIndex = index;

        // GunController gc = weapons[index].GetComponent<GunController>();
        // if (gc != null && uiManager != null)
        // {
        //     uiManager.gunStats = gc;
        // }

        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("currentWeapon", currentItemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!PV.IsMine && targetPlayer == PV.Owner)
        {
            SwitchToWeapon((int)changedProps["currentWeapon"]);
        }
    }

    public void TakeDamage(float damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        if (!PV.IsMine)
        {
            return;
        }

        if (currentHealth > 0)
        {
            currentHealth -= damage;
            Debug.Log("Took damage" + damage.ToString());
            healthbarImage.fillAmount = currentHealth / maxHealth;
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        else if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        playerManager.Die();
    }

    public Item GetCurrentItem()
    {
        if (items != null && currentItemIndex >= 0 && currentItemIndex < items.Length)
        {
            return items[currentItemIndex];
        }
        return null;
    }
}
