using GiscardPunk77.Gameplay;
using GiscardPunk77.Gameplay.Weapons;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerHitscanWeapon))]
public sealed class HitscanSandboxDebugDisplay : MonoBehaviour
{
    private PlayerHitscanWeapon weapon;
    private string lastResult = "Aucun tir";

    private void Awake()
    {
        weapon = GetComponent<PlayerHitscanWeapon>();
    }

    private void OnEnable()
    {
        if (weapon == null)
        {
            weapon = GetComponent<PlayerHitscanWeapon>();
        }

        if (weapon == null)
        {
            return;
        }

        weapon.Fired += OnFired;
        weapon.ReloadStarted += OnReloadStarted;
        weapon.ReloadCompleted += OnReloadCompleted;
    }

    private void OnDisable()
    {
        if (weapon == null)
        {
            return;
        }

        weapon.Fired -= OnFired;
        weapon.ReloadStarted -= OnReloadStarted;
        weapon.ReloadCompleted -= OnReloadCompleted;
    }

    private void OnGUI()
    {
        if (weapon == null)
        {
            return;
        }

        var status = weapon.IsReloading ? "RECHARGEMENT" : "PRÊTE";
        GUI.Box(new Rect(12f, 12f, 430f, 82f), "P03 — Diagnostic hitscan");
        GUI.Label(new Rect(24f, 38f, 400f, 22f), $"Arme {status} | Chargeur {weapon.MagazineAmmo} | Réserve {weapon.ReserveAmmo}");
        GUI.Label(new Rect(24f, 62f, 400f, 22f), lastResult);
    }

    private void OnFired(HitscanResult result)
    {
        if (!result.HasHit)
        {
            lastResult = "Tir : aucun obstacle touché";
            return;
        }

        var hitName = result.Hit.collider != null ? result.Hit.collider.name : "inconnu";
        var rootHealth = result.Hit.collider != null
            ? result.Hit.collider.GetComponentInParent<Health>()
            : null;
        var healthText = rootHealth != null ? $" | santé {rootHealth.CurrentHealth:0.#}" : string.Empty;
        lastResult = $"Impact : {hitName} | dégâts appliqués : {result.DamageApplied}{healthText}";
    }

    private void OnReloadStarted()
    {
        lastResult = "Recharge commencée — tir bloqué pendant 1,6 s";
    }

    private void OnReloadCompleted()
    {
        lastResult = "Recharge terminée";
    }
}
