using System;
using GiscardPunk77.Core;
using UnityEngine;

namespace GiscardPunk77.Gameplay.Weapons
{
    [DisallowMultipleComponent]
    public sealed class PlayerHitscanWeapon : MonoBehaviour
    {
        public const int DefaultMagazineCapacity = 8;
        public const float DefaultReloadDuration = 1.6f;

        [Header("Aim")]
        [SerializeField] private Camera aimCamera;
        [SerializeField] private Transform shooterRoot;
        [SerializeField] private LayerMask hitLayers = ~0;
        [SerializeField, Min(0.01f)] private float maxDistance = 100f;

        [Header("Damage")]
        [SerializeField, Min(0f)] private float damage = 34f;
        [SerializeField] private ActorKind sourceKind = ActorKind.Player;
        [SerializeField] private int sourceTeamId;

        [Header("Ammunition")]
        [SerializeField, Min(1)] private int magazineCapacity = DefaultMagazineCapacity;
        [SerializeField, Min(0)] private int startingReserve = 24;
        [SerializeField, Min(0.01f)] private float roundsPerSecond = 4f;
        [SerializeField, Min(0.01f)] private float reloadDuration = DefaultReloadDuration;

        private readonly HitscanResolver resolver = new HitscanResolver();
        private SemiAutomaticWeaponState state;

        public event Action<HitscanResult> Fired;
        public event Action ReloadStarted;
        public event Action ReloadCompleted;
        public event Action<int, int> AmmoChanged;

        public int MagazineAmmo => State.MagazineAmmo;
        public int ReserveAmmo => State.ReserveAmmo;
        public bool IsReloading => State.IsReloading;

        private SemiAutomaticWeaponState State => state ?? (state = CreateState());

        private void Awake()
        {
            state = CreateState();
            ResolveReferences();
        }

        private void Start()
        {
            PublishAmmo();
        }

        private void Update()
        {
            CompleteReloadIfReady(Time.time);
        }

        private void OnValidate()
        {
            magazineCapacity = Mathf.Max(1, magazineCapacity);
            startingReserve = Mathf.Max(0, startingReserve);
            roundsPerSecond = Mathf.Max(0.01f, roundsPerSecond);
            reloadDuration = Mathf.Max(0.01f, reloadDuration);
            maxDistance = Mathf.Max(0.01f, maxDistance);
            damage = Mathf.Max(0f, damage);
        }

        public bool TryFire()
        {
            ResolveReferences();
            var currentTime = Time.time;
            CompleteReloadIfReady(currentTime);

            if (aimCamera == null || !State.TryConsumeShot(currentTime))
            {
                return false;
            }

            var aimTransform = aimCamera.transform;
            var request = new HitscanRequest(
                aimTransform.position,
                aimTransform.forward,
                maxDistance,
                hitLayers,
                shooterRoot,
                damage,
                new ActorIdentity(sourceKind, new TeamId(sourceTeamId)));
            var result = resolver.Resolve(request);

            Fired?.Invoke(result);
            PublishAmmo();
            return true;
        }

        public bool TryStartReload()
        {
            var currentTime = Time.time;
            CompleteReloadIfReady(currentTime);

            if (!State.TryStartReload(currentTime))
            {
                return false;
            }

            ReloadStarted?.Invoke();
            return true;
        }

        public void ConfigureAim(Camera camera, Transform root)
        {
            aimCamera = camera;
            shooterRoot = root;
        }

        private SemiAutomaticWeaponState CreateState()
        {
            return new SemiAutomaticWeaponState(
                magazineCapacity,
                startingReserve,
                1f / roundsPerSecond,
                reloadDuration);
        }

        private void ResolveReferences()
        {
            if (shooterRoot == null)
            {
                shooterRoot = transform.root;
            }

            if (aimCamera == null)
            {
                aimCamera = GetComponentInChildren<Camera>(true);
            }
        }

        private void PublishAmmo()
        {
            AmmoChanged?.Invoke(State.MagazineAmmo, State.ReserveAmmo);
        }

        private void CompleteReloadIfReady(float time)
        {
            if (!State.CompleteReloadIfReady(time))
            {
                return;
            }

            ReloadCompleted?.Invoke();
            PublishAmmo();
        }
    }
}
