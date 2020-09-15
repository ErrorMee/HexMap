using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TextureArrayWizard : ScriptableWizard {

	public Texture2D[] textures;

	[MenuItem ("Assets/Create/Texture Array")]
	static void CreateWizard () {
		ScriptableWizard.DisplayWizard<TextureArrayWizard>(
			"Create Texture Array", "Create"
		);
	}

	private void OnEnable()
	{
		OnUpdateSelect();
	}

	private void OnSelectionChange()
	{
		OnUpdateSelect();
	}

	private void OnUpdateSelect()
	{
		UnityEngine.Object[] selObjs = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);

		textures = new Texture2D[selObjs.Length];

		List<UnityEngine.Object> selObjList = new List<UnityEngine.Object>();

		for (int i = 0; i < selObjs.Length; i++)
		{
			selObjList.Add(selObjs[i]);
		}
		selObjList.Sort((obj1,obj2) =>
		{
			return obj1.name.CompareTo(obj2.name);
		});

		for (int i = 0; i < selObjList.Count; i++)
		{
			textures[i] = selObjList[i] as Texture2D;
		}
	}

	void OnWizardCreate () {
		if (textures.Length == 0) {
			return;
		}

		string assetPath = AssetDatabase.GetAssetOrScenePath(textures[0]);
		string dirPath = Path.GetDirectoryName(assetPath);

		string path = EditorUtility.SaveFilePanelInProject(
			"Save Texture Array", "Texture Array", "asset", "Save Texture Array", dirPath);
		if (path.Length == 0) {
			return;
		}

		Texture2D t = textures[0];
		Texture2DArray textureArray = new Texture2DArray(
			t.width, t.height, textures.Length, t.format, t.mipmapCount > 1
		);
		textureArray.anisoLevel = t.anisoLevel;
		textureArray.filterMode = t.filterMode;
		textureArray.wrapMode = t.wrapMode;

		for (int i = 0; i < textures.Length; i++) {
			for (int m = 0; m < t.mipmapCount; m++) {
				Graphics.CopyTexture(textures[i], 0, m, textureArray, i, m);
			}
		}

		AssetDatabase.CreateAsset(textureArray, path);
	}
}