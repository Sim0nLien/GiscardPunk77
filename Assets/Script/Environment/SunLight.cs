using UnityEngine;

/// <summary>
/// Configures a directional Unity light as the scene sun.
/// The distinct class name avoids hiding UnityEngine.Light in other scripts.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(UnityEngine.Light))]
public sealed class SunLight : MonoBehaviour
{
    [SerializeField]
    private Color sunlightColour = new Color(1f, 0.88f, 0.68f, 1f);

    [SerializeField, Min(0f)]
    private float intensity = 1.1f;

    [SerializeField]
    private LightShadows shadows = LightShadows.Soft;

    [SerializeField, Range(0f, 1f)]
    private float shadowStrength = 0.85f;

    [SerializeField]
    [Tooltip("Registers this directional light as the sun used by the skybox.")]
    private bool useAsSceneSun = true;

    private UnityEngine.Light cachedLight;

    public UnityEngine.Light UnityLight
    {
        get
        {
            if (cachedLight == null)
            {
                cachedLight = GetComponent<UnityEngine.Light>();
            }

            return cachedLight;
        }
    }

    private void Reset()
    {
        transform.rotation = Quaternion.Euler(35f, -30f, 0f);
        ApplySettings();
    }

    private void Awake()
    {
        ApplySettings();
    }

    private void OnEnable()
    {
        ApplySettings();
    }

    private void OnValidate()
    {
        intensity = Mathf.Max(0f, intensity);
        shadowStrength = Mathf.Clamp01(shadowStrength);
        ApplySettings();
    }

    private void OnDisable()
    {
        if (cachedLight != null && RenderSettings.sun == cachedLight)
        {
            RenderSettings.sun = null;
        }
    }

    private void ApplySettings()
    {
        UnityEngine.Light sun = UnityLight;

        if (sun == null)
        {
            return;
        }

        sun.type = LightType.Directional;
        sun.color = sunlightColour;
        sun.intensity = intensity;
        sun.shadows = shadows;
        sun.shadowStrength = shadowStrength;

        if (useAsSceneSun && isActiveAndEnabled)
        {
            RenderSettings.sun = sun;
        }
    }
}
