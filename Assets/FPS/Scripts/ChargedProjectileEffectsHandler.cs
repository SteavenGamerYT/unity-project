using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargedProjectileEffectsHandler : MonoBehaviour
{
    [Tooltip("Object that will be affected by charging scale & color changes")]
    public GameObject chargingObject;
    [Tooltip("Scale of the charged object based on charge")]
    public MinMaxVector3 scale;
    [Tooltip("Color of the charged object based on charge")]
    public MinMaxColor color;

    MeshRenderer[] m_AffectedRenderers;
    ProjectileBase m_ProjectileBase;
    static MaterialPropertyBlock s_PropertyBlock;
    static readonly int s_BaseColorProp = Shader.PropertyToID("_BaseColor");
    static readonly int s_ColorProp = Shader.PropertyToID("_Color");

    private void OnEnable()
    {
        m_ProjectileBase = GetComponent<ProjectileBase>();
        DebugUtility.HandleErrorIfNullGetComponent<ProjectileBase, ChargedProjectileEffectsHandler>(m_ProjectileBase, this, gameObject);

        m_ProjectileBase.onShoot += OnShoot;

        m_AffectedRenderers = chargingObject.GetComponentsInChildren<MeshRenderer>();
    }

    void OnShoot()
    {
        chargingObject.transform.localScale = scale.GetValueFromRatio(m_ProjectileBase.initialCharge);

        if (s_PropertyBlock == null)
            s_PropertyBlock = new MaterialPropertyBlock();

        Color chargeColor = color.GetValueFromRatio(m_ProjectileBase.initialCharge);
        foreach (var ren in m_AffectedRenderers)
        {
            ren.GetPropertyBlock(s_PropertyBlock);
            s_PropertyBlock.SetColor(s_BaseColorProp, chargeColor);
            s_PropertyBlock.SetColor(s_ColorProp, chargeColor);
            ren.SetPropertyBlock(s_PropertyBlock);
        }
    }
}
