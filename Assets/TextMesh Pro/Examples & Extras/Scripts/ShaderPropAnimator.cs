using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{
    
    public class ShaderPropAnimator : MonoBehaviour
    {

        private Renderer m_Renderer;
        private MaterialPropertyBlock m_PropBlock;

        public AnimationCurve GlowCurve;

        public float m_frame;

        void Awake()
        {
            // Cache a reference to object's renderer
            m_Renderer = GetComponent<Renderer>();
            m_PropBlock = new MaterialPropertyBlock();
        }

        void Start()
        {
            StartCoroutine(AnimateProperties());
        }

        IEnumerator AnimateProperties()
        {
            //float lightAngle;
            float glowPower;
            m_frame = Random.Range(0f, 1f);

            while (true)
            {
                //lightAngle = (m_Material.GetFloat(ShaderPropertyIDs.ID_LightAngle) + Time.deltaTime) % 6.2831853f;
                //m_Material.SetFloat(ShaderPropertyIDs.ID_LightAngle, lightAngle);

                glowPower = GlowCurve.Evaluate(m_frame);
                m_Renderer.GetPropertyBlock(m_PropBlock);
                m_PropBlock.SetFloat(ShaderUtilities.ID_GlowPower, glowPower);
                m_Renderer.SetPropertyBlock(m_PropBlock);

                m_frame += Time.deltaTime * Random.Range(0.2f, 0.3f);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
