using GiscardPunk77.Gameplay.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerHitscanWeapon))]
public sealed class PlayerHitscanWeaponInput : MonoBehaviour
{
    private PlayerHitscanWeapon weapon;
    private InputAction fireAction;
    private InputAction reloadAction;

    private void Awake()
    {
        weapon = GetComponent<PlayerHitscanWeapon>();
        fireAction = new InputAction("Fire", InputActionType.Button);
        fireAction.AddBinding("<Mouse>/leftButton");
        fireAction.AddBinding("<Gamepad>/rightTrigger");

        reloadAction = new InputAction("Reload", InputActionType.Button);
        reloadAction.AddBinding("<Keyboard>/r");
        reloadAction.AddBinding("<Gamepad>/buttonWest");
    }

    private void OnEnable()
    {
        fireAction.Enable();
        reloadAction.Enable();
    }

    private void OnDisable()
    {
        fireAction.Disable();
        reloadAction.Disable();
    }

    private void Update()
    {
        if (fireAction.WasPressedThisFrame())
        {
            weapon.TryFire();
        }

        if (reloadAction.WasPressedThisFrame())
        {
            weapon.TryStartReload();
        }
    }

    private void OnDestroy()
    {
        fireAction.Dispose();
        reloadAction.Dispose();
    }
}
