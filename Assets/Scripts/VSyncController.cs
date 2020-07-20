using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VSyncController : MonoBehaviour
{
	public TMPro.TextMeshProUGUI uiONOFF;

    public bool enableVSync {
		get { return IsVSyncEnabled(); }
		set { EnableVSync(value); }
	}

	private void OnEnable() {

		UpdateUI();
	}

	public void EnableVSync(bool enable) {

		QualitySettings.vSyncCount = enable ? 1 : 0;

		UpdateUI();
	}

	public void ToggleVSync() {

		EnableVSync(!(QualitySettings.vSyncCount > 0));
	}

	bool IsVSyncEnabled() {

		return QualitySettings.vSyncCount > 0;
	}

	void UpdateUI() {

		uiONOFF.text = QualitySettings.vSyncCount > 0 ? "VSync\nON" : "VSync\nOFF";
	}
}
