using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour
{
    public TMPro.TextMeshProUGUI text;

    public KeyCode shortcutKey = KeyCode.F;

	private void OnEnable() {

        text.gameObject.SetActive(false);
    }

	// Update is called once per frame
	void Update()
    {
        if (Input.GetKeyDown(shortcutKey))
            ToggleFPS();

        text.text = "FPS\n" + (1.0f / Time.smoothDeltaTime).ToString("F0");
    }

    void ToggleFPS() {

        text.gameObject.SetActive(!text.gameObject.activeSelf);
	}
}
