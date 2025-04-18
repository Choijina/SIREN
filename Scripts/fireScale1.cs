using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.ParticleSystemModule;
#endif

public class fireScale1 : MonoBehaviour
{
    public GameObject fire;
    public ParticleSystem extinguisherParticle; // 소화기 파티클 시스템을 지정할 변수

    [SerializeField, Range(0f, 1f)] private float currentIntensity = 0.1f; // 초기 강도를 낮게 설정
    private float startIntensity = 0f;
    public float GetCurrentIntensity() => currentIntensity;

    [SerializeField] private ParticleSystem firePS;

    private float spreadSpeed = 0.001f; // 불 번지는 속도
    private float extinguishSpeed = 0.1f; // 소화 속도
    private bool isExtinguishing = false; // 소화 중인지 여부
    private bool isExtinguished = false; // 소화 완료 여부

    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.MainModule mainModule;
    private Collider fireCollider; // 불 파티클 오브젝트의 콜라이더

    private void Start()
    {
        if (firePS == null)
        {
            Debug.LogError("firePS가 연결되지 않았습니다.");
            return;
        }

        emissionModule = firePS.emission;
        mainModule = firePS.main;
        fireCollider = GetComponent<Collider>(); // 현재 오브젝트의 Collider를 가져옴

        startIntensity = emissionModule.rateOverTime.constant;
        Debug.Log("초기 강도: " + startIntensity);

        mainModule.startSize = new ParticleSystem.MinMaxCurve(0.2f); // 시작 크기를 작게 설정
        firePS.Play(); // 파티클 시스템을 활성화
    }

    private void Update()
    {
        if (firePS == null || isExtinguished) return;

        if (!isExtinguishing) // 확산이 가능한 상태에서만 SpreadFire 호출
        {
            SpreadFire();
        }
        else // 소화 중이라면 currentIntensity를 감소시킴
        {
            currentIntensity -= extinguishSpeed * Time.deltaTime;
            currentIntensity = Mathf.Max(currentIntensity, 0f); // 최소 0으로 유지

            if (currentIntensity <= 0) // 강도가 0이면 소화 완료로 설정
            {
                isExtinguished = true;
                firePS.Stop(); // 불을 완전히 끔
                if (fireCollider != null) 
                {
                    fireCollider.enabled = false; // 불 파티클 오브젝트의 콜라이더 비활성화
                }
                Debug.Log("불이 완전히 꺼졌습니다. 콜라이더가 비활성화되었습니다.");
            }
        }

        ChangeIntensity();
        IncreaseParticleSize();
    }

    private void ChangeIntensity()
    {
        emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(currentIntensity * startIntensity);
    }

    // 불이 점점 번지는 효과를 주기 위한 메서드
    private void SpreadFire()
    {
        if (currentIntensity < 1f)
        {
            currentIntensity += spreadSpeed * Time.deltaTime; // 점진적으로 강도 증가
        }
        else
        {
            currentIntensity = 1f; // 최대 강도 제한
        }
    }

    // 파티클 크기를 점점 증가시키는 메서드, 최대 크기인 4에 도달하면 유지
    private void IncreaseParticleSize()
    {
        if (mainModule.startSize.constant < 4f)
        {
            float newSize = Mathf.Min(mainModule.startSize.constant + 0.5f * Time.deltaTime, 4f);
            mainModule.startSize = new ParticleSystem.MinMaxCurve(newSize);
        }
    }

    // 소화기 파티클과의 충돌 시 소화 중 상태로 설정
    private void OnParticleCollision(GameObject other)
    {
        if (isExtinguished) return; // 소화 완료된 경우 충돌 무시

        if (other.gameObject == extinguisherParticle.gameObject)
        {
            Debug.Log("소화기 파티클과 충돌하여 불 강도 감소 중");
            isExtinguishing = true; // 소화 중 상태로 전환
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == extinguisherParticle.gameObject)
        {
            Debug.Log("소화기 파티클과의 충돌이 끝났습니다.");
            // isExtinguishing을 여기서 끄지 않음 - 소화 중 상태 유지
        }
    }
}
