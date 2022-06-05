using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Fading_text : MonoBehaviour
{
    [SerializeField]
    private float time_to_fade = 8f;
    TextMeshProUGUI text_mesh;

    void Awake() {
        text_mesh = GetComponent<TextMeshProUGUI>();
    }

    public void MakeTextInvisible() {
        text_mesh = GetComponent<TextMeshProUGUI>();
        text_mesh.color = new Color(text_mesh.color.r, text_mesh.color.g, text_mesh.color.b, 0.0f);
    }

    public void MakeTextVisible() {
        text_mesh = GetComponent<TextMeshProUGUI>();
        text_mesh.color = new Color(text_mesh.color.r, text_mesh.color.g, text_mesh.color.b, 1.0f);
    }

    public void FadeText() {
        text_mesh.color = new Color(text_mesh.color.r, text_mesh.color.g, text_mesh.color.b, 1.0f);

        IEnumerator coruotine = FadeOutCR();
        StartCoroutine(coruotine);
    }

    private IEnumerator FadeOutCR() {
        float currentTime = 0f;
        while(currentTime < time_to_fade) {
            float alpha = Mathf.Lerp(1f, 0f, currentTime/time_to_fade);
            text_mesh.color = new Color(text_mesh.color.r, text_mesh.color.g, text_mesh.color.b, alpha);
            currentTime += Time.deltaTime;
            yield return null;
        }
        yield break;
    }
}
