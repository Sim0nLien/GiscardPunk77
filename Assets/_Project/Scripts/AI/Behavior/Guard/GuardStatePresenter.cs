using UnityEngine;

namespace GiscardPunk77.AI.Behavior.Guard
{
    /// <summary>Temporary capsule colors. It observes state and owns no guard decision.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GuardContext))]
    public sealed class GuardStatePresenter : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private GuardContext context;
        [SerializeField] private Renderer[] renderers;

        private MaterialPropertyBlock propertyBlock;

        private void Reset()
        {
            context = GetComponent<GuardContext>();
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void OnEnable()
        {
            context ??= GetComponent<GuardContext>();
            renderers ??= GetComponentsInChildren<Renderer>(true);
            propertyBlock ??= new MaterialPropertyBlock();
            if (context != null)
            {
                context.StateChanged += OnStateChanged;
                Apply(context.CurrentState);
            }
        }

        private void OnDisable()
        {
            if (context != null)
            {
                context.StateChanged -= OnStateChanged;
            }
        }

        public void Configure(GuardContext guardContext, params Renderer[] stateRenderers)
        {
            if (isActiveAndEnabled && context != null)
            {
                context.StateChanged -= OnStateChanged;
            }

            context = guardContext;
            renderers = stateRenderers;

            if (isActiveAndEnabled && context != null)
            {
                context.StateChanged += OnStateChanged;
                Apply(context.CurrentState);
            }
        }

        private void OnStateChanged(GuardState previous, GuardState next)
        {
            Apply(next);
        }

        private void Apply(GuardState state)
        {
            if (context == null || context.Config == null || renderers == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            var color = context.Config.GetColor(state);
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(ColorId, color);
            foreach (var stateRenderer in renderers)
            {
                if (stateRenderer != null)
                {
                    stateRenderer.SetPropertyBlock(propertyBlock);
                }
            }
        }
    }
}
