using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;
using Ray = UnityEngine.Ray;

public class CellClickDebugger : MonoBehaviour
{
    private Camera MainCam;
    EntityManager em;

    private void Start()
    {
        MainCam = Camera.main;
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            var singleton = em.CreateEntityQuery(typeof(PhysicsWorldSingleton)).GetSingleton<PhysicsWorldSingleton>();
            var world = singleton.PhysicsWorld;
            
            
            //ray 지정
            Ray ray = MainCam.ScreenPointToRay(Input.mousePosition);
            var input = new RaycastInput
            {
                Start = ray.origin,
                End = ray.origin + ray.direction * 1000f,
                Filter = CollisionFilter.Default
            };
            Debug.DrawLine(input.Start, input.End, Color.red, 1f);

            if (world.CollisionWorld.CastRay(input, out Unity.Physics.RaycastHit hit))
            {
                var entity = world.Bodies[hit.RigidBodyIndex].Entity;
                var env = em.GetComponentData<CellDataJSH>(entity);
                Debug.Log($"맞은 엔티티 좌표 = X : {env.Postion.x} Y : {env.Postion.y}\n엔티티 상태 : {env.CellVisualType}\n엔티티 벨류 : {env.Amount}");

            }
        }
    }
    
}