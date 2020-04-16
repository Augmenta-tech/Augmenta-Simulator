using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TextHyperlink : MonoBehaviour, IPointerClickHandler
{
    public Canvas canvas;
    public TMP_Text text;

    private new Camera camera;

    void Awake() {
        // Get a reference to the camera if Canvas Render Mode is not ScreenSpace Overlay.
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            camera = null;
        else
            camera = canvas.worldCamera;
    }

    public void OnPointerClick(PointerEventData eventData) {

        // Check if Mouse intersects any words and if so assign a random color to that word.
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, camera);

        if (linkIndex != -1) {

            Application.OpenURL("https://www.augmenta-tech.com");

        }
    }
}
