using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    HealthPoints healthPoints;

    Image image;

    void Start()
    {
        image = GetComponent<Image>();
        StartCoroutine(SetUp());
    }

    IEnumerator SetUp()
    {
        yield return new WaitUntil(() => PlayerManager.LocalPlayer);
        healthPoints = PlayerManager.LocalPlayer.GetComponent<HealthPoints>();

        healthPoints.OnHPChange += ChangeValue;
    }

    void ChangeValue(int val)
    {

        image.fillAmount = (float)val / (float)healthPoints.maxHP;
    }
    private void OnDestroy()
    {
        if(healthPoints)
            healthPoints.OnHPChange -= ChangeValue;
    }
}
