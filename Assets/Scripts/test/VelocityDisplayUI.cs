using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class VelocityDisplayUI : MonoBehaviour
{
    public TextMeshProUGUI velocityText; // Unity UI Text (또는 TMP_Text)
    public TextMeshProUGUI FPSText; // Unity UI Text (또는 TMP_Text)

    private EntityQuery query;
    private EntityManager entityManager;
    float deltaTime = 0.0f;

    void Start()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        entityManager = world.EntityManager;

        // 플레이어 엔티티를 찾기 위한 쿼리
        query = entityManager.CreateEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(PlayerTag),
                typeof(Velocity)
            }
        });
    }

    void Update()
    {
        if (query.IsEmpty) return;

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        
        var velocityComponent = query.GetSingleton<Velocity>();
        float3 v = velocityComponent.Value;
        float fps = 1.0f / deltaTime;

        velocityText.text = $"Velocity: {v.x:F2}, {v.y:F2}, {v.z:F2}";
        FPSText.text = string.Format("FPS: {0:0.}  (ms: {1:0.0})", fps, deltaTime * 1000);
    }
}