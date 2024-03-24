using UnityEngine;

public class ParticleEffectHandler : MonoBehaviour
{
    public ObjectPool objectPool;
    public int particleEffectID;
    private void OnDisable()
    {
        objectPool.ReturnParticleEffectToPool(gameObject, particleEffectID);
    }
}
