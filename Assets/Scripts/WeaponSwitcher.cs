// using System.Collections;
// using Photon.Pun;
// using Photon.Realtime;
// using UnityEngine;
// using Hashtable = ExitGames.Client.Photon.Hashtable;

// public class WeaponSwitcher : MonoBehaviourPunCallbacks
// {
//     public GameObject[] weapons;
//     // public UIManager uiManager;



//     PhotonView PV;

//     void Start()
//     {
//         if (PV.IsMine)
//         {
//             SwitchToWeapon(currentWeapon);
//         }
//     }

//     void Update()
//     {
//         for (int i = 0; i < weapons.Length; i++)
//         {
//             if (Input.GetKeyDown((i + 1).ToString()))
//             {
//                 SwitchToWeapon(i);
//             }
//         }

//         if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
//         {
//             if (currentWeapon >= weapons.Length - 1)
//             {
//                 SwitchToWeapon(0);
//             }
//             else
//             {
//                 SwitchToWeapon(currentWeapon + 1);
//             }
//         }
//         else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
//         {
//             if (currentWeapon <= 0)
//             {
//                 SwitchToWeapon(weapons.Length - 1);
//             }
//             else
//             {
//                 SwitchToWeapon(currentWeapon - 1);
//             }

//         }
//     }

//     void SwitchToWeapon(int index)
//     {
//         if (index >= weapons.Length) return;

//         for (int i = 0; i < weapons.Length; i++)
//         {
//             weapons[i].SetActive(i == index);
//         }

//         currentWeapon = index;

//         // GunController gc = weapons[index].GetComponent<GunController>();
//         // if (gc != null && uiManager != null)
//         // {
//         //     uiManager.gunStats = gc;
//         // }

//         if (PV.IsMine)
//         {
//             Hashtable hash = new Hashtable();
//             hash.Add("currentWeapon", currentWeapon);
//             PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
//         }
//     }

//     public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
//     {
//         if (!PV.IsMine && targetPlayer == PV.Owner)
//         {
//             SwitchToWeapon((int)changedProps["currentWeapon"]);
//         }
//     }
// }
