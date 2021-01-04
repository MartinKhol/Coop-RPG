using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingHPBarUI : MonoBehaviour
{
    public Image healthImage;
    public HealthPoints healthPoints;
    public Vector3 offset;
    public string chracterName;
    public Text nameText;
    public Text hpValueText;
    private GameObject healthBar;

    void Start()
    {
        transform.position = healthPoints.transform.position + offset;
        healthPoints.OnHPChange += UpdateHPBar;
        healthBar = healthImage.transform.parent.gameObject;
        healthBar.SetActive(healthPoints.HP < healthPoints.maxHP);
        myTransform = transform;

        if (chracterName.Length == 0)
        {
            nameText.enabled = false;
        }
        else
        {
            nameText.text = chracterName;
        }


        UpdateHPBar(healthPoints.HP);
    }

    void UpdateHPBar(int hp)
    {
        healthImage.fillAmount = (float)hp / (float)healthPoints.maxHP;
        healthBar.SetActive(hp < healthPoints.maxHP);

        hpValueText.text = string.Concat(hp, "/", healthPoints.maxHP);
    }

    Vector3 scale = new Vector3(0.01f, 0.01f, 0.01f);
    Transform myTransform;

    private void Update()
    {
        if (Mathf.Sign(healthPoints.transform.localScale.x) != Mathf.Sign(myTransform.localScale.x))
        {
            scale.x = 0.01f * healthPoints.transform.localScale.x;
            myTransform.localScale = scale;
        }
    }
}
