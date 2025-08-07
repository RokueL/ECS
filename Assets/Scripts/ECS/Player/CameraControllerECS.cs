using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class CameraControllerECS : MonoBehaviour
{
    // 오프셋
    public Vector3 offset = new Vector3(0, 2, -5);     // 뒤에서 바라보는 위치
    // 마우스 감도
    public float mouseSensitivity = 3f;
    // 상하 각도 제한
    public float pitchMin = -30f;
    public float pitchMax = 60f;
    // 카메라 부드럽게 처리
    public float smoothSpeed = 10f;

    // 카메라 위치
    private float yaw = 0f;
    private float pitch = 10f;

    // 모든 엔티티를 관리
    private EntityManager entityManager;
    // Entity 전부 뽑아서 담아두는 곳
    private EntityQuery playerQuery;
    // 카메라 방향 정보가 담긴 엔티티 ( 플레이어 쿼리에 전달용 )
    private Entity cameraDirectionEntity;

    void Start()
    {
        // 일반 월드를 참조하는 것
        var world = World.DefaultGameObjectInjectionWorld;
        // 엔티티 매니저 할당
        entityManager = world.EntityManager;
        
        // 플레이어 조작하는 Entity 찾아 할당
        playerQuery = entityManager.CreateEntityQuery(new EntityQueryDesc
        {
            // 플레이어태그를 가지고 있으며 로컬 트랜스폼도 가지고 있는 것
            All = new ComponentType[] { typeof(PlayerTag), typeof(LocalTransform) }
        });
        
        // 카메라 디렉션 아키텍쳐 생성
        var archetype = entityManager.CreateArchetype(typeof(CameraDirection));
        // 만들어진 아키텍쳐를 바탕으로 엔티티 생성
        cameraDirectionEntity = entityManager.CreateEntity(archetype);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        // 플레이어 없으면 처리 안함
        if (playerQuery.IsEmpty) return;

        //LocalTransform 컴포넌트를 가진 엔티티가 딱 하나라고 가정하고,
        //그 컴포넌트 값을 가져오는 코드입니다.
        var playerTransform = playerQuery.GetSingleton<LocalTransform>();
        Vector3 playerPos = playerTransform.Position;

        // 마우스 입력
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        // 상하 제한
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // 회전 생성
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // 카메라 위치: 회전된 offset 적용
        // offset 길이만큼 떨어진 위치 계산 (항상 같은 거리 유지)
        Vector3 targetOffset = rotation * offset.normalized * offset.magnitude;
        Vector3 desiredPos = playerPos + targetOffset;
        // 바로 위치 적용 (Lerp 제거)
        transform.position = desiredPos;
        //transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);

        // 카메라는 항상 플레이어 바라보게
        transform.LookAt(playerPos + Vector3.up * 1.5f);

        // ECS에 카메라 방향 전달 (수평 방향만)
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        var camDir = new CameraDirection
        {
            Forward = new float3(forward.x, 0, forward.z),
            Right = new float3(right.x, 0, right.z)
        };
        
        // 값 지정
        entityManager.SetComponentData(cameraDirectionEntity, camDir);
    }
}

public struct CameraDirection : IComponentData
{
    public float3 Forward;
    public float3 Right;
}
