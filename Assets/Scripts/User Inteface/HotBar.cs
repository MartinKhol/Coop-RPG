using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotBar : MonoBehaviour
{
    public Image ability1;
    public Image ability2;
    public Image ability1Cooldown;
    public Image ability2Cooldown;

    Ability ability1cache;
    Ability ability2cache;

    void Start()
    {
        ability1Cooldown.fillAmount = 0;
        ability2Cooldown.fillAmount = 0;
        StartCoroutine(WaitForPlayerSpawn());
    }

    IEnumerator WaitForPlayerSpawn()
    {
        yield return new WaitUntil(() => PlayerManager.LocalPlayer != null);
        PlayerAttack player = PlayerManager.LocalPlayer.GetComponent<PlayerAttack>();

        ability1.sprite = player.ability1.uiDisplay;
        ability2.sprite = player.ability2.uiDisplay;

        player.ability1.StartCooldown += Ability1Cooldown;
        player.ability2.StartCooldown += Ability2Cooldown;

        ability1cache = player.ability1;
        ability2cache = player.ability2;
    }

    void Ability1Cooldown(float time)
    {

        ability1Cooldown.fillAmount = 1;
        StartCoroutine(Cooldown(ability1Cooldown, time));
    }

    void Ability2Cooldown(float time)
    {
        ability2Cooldown.fillAmount = 1;
        StartCoroutine(Cooldown(ability2Cooldown, time));
    }

    IEnumerator Cooldown(Image image, float time)
    {
        var startTime = Time.time;
        float timeRemaining;
        while ((timeRemaining = Time.time - startTime) < time)
        {
            yield return new WaitForSeconds(0.1f);
            image.fillAmount = 1 - timeRemaining / time;
        }
        image.fillAmount = 0;
    }

    private void OnDestroy()
    {
        if (ability1cache != null)
            ability1cache.StartCooldown -= Ability1Cooldown;
        if (ability2cache != null)
            ability2cache.StartCooldown -= Ability2Cooldown;
    }
}
