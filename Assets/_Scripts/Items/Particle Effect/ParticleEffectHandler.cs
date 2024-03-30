using UnityEngine;

public class ParticleEffectHandler : MonoBehaviour
{
    public int particleEffectID;
    private void OnDisable()
    {
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnParticleEffectToPool(gameObject, particleEffectID);
        }
        
    }
}
