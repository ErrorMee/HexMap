﻿using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FPSCounter))]
public class FPSDisplay : MonoBehaviour {

	[System.Serializable]
	private struct FPSColor {
		public Color color;
		public int minimumFPS;
	}

	public Text highestFPSLabel, averageFPSLabel, lowestFPSLabel;

	[SerializeField]
	private FPSColor[] coloring;

	FPSCounter fpsCounter;

	void Awake () {
		fpsCounter = GetComponent<FPSCounter>();
	}

	void Update () {
		Display(highestFPSLabel, fpsCounter.HighestFPS);
		Display(averageFPSLabel, fpsCounter.AverageFPS);
		Display(lowestFPSLabel, fpsCounter.LowestFPS);
	}

	void Display (Text label, int fps) {
		label.text = StringUtil.StringsFrom00To100[Mathf.Clamp(fps, 0, 99)];
		for (int i = 0; i < coloring.Length; i++) {
			if (fps >= coloring[i].minimumFPS) {
				label.color = coloring[i].color;
				break;
			}
		}
	}
}