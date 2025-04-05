using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    public GameObject[] weapons;
    public UIManager uiManager;

    public int currentWeapon = 0;

    void Start()
    {
        SwitchToWeapon(currentWeapon);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchToWeapon(3);
    }

    void SwitchToWeapon(int index)
    {
        if (index >= weapons.Length) return;

        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(i == index);
        }

        currentWeapon = index;

        GunController gc = weapons[index].GetComponent<GunController>();
        if (gc != null && uiManager != null)
        {
            uiManager.gunStats = gc;
        }
    }
}
