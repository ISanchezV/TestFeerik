/* GameControl contient une classe statique qui est accessible depuis tous les objets et scripts du jeu, et qui 
 * reste la même même entre les différentes scènes et ne peut pas être détruite. Ainsi, c'est ici que l'on
 * aura toutes les variables et classes qui doivent rester joignables et constante entre tous les objets*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class GameControl : MonoBehaviour {

	//Juste un objet GameControl, et toujours le même
	public static GameControl control;

	//Variables globales accesibles depuis tous les scripts et objets
	public string imgURL = "https://i.imgur.com/HQuzSER.jpg";
	public string filePath = Application.persistentDataPath + "/Textures/";
	public Image img;
	public List<String> fileNames = new List<String>();

	void Awake () {
		//Si l'objet permanent n'existe pas
		if (control == null) {

			//Ne pas détruire l'objet en
			//changeant de scène
			DontDestroyOnLoad (gameObject);

			//L'objet permanent est celui
			//qui contient ce script
			control = this;

			//Si l'objet permanent n'est pas
			//celui qui contient ce script
		} else if (control != this) {

			//Detruire cet objet
			Destroy (gameObject);
		}
		print (PlayerPrefs.GetInt ("Telecharge"));
		//D'abord on regarde si on a déjà une variable pour savoir si on a téléchargé ou pas, si non, on la définit
		if (PlayerPrefs.GetInt ("Telecharge") == null) {
			PlayerPrefs.SetInt ("Telecharge", 0);
			System.IO.Directory.CreateDirectory (filePath);
		} else if (PlayerPrefs.GetInt ("Telecharge") == 0) { //Si on a pas encore téléchargé
			StartCoroutine(Download(imgURL));
			PlayerPrefs.SetInt("Telecharge", 1);
		} else { //Si on a déjà téléchargé
			LoadTexture();
		}
	}

	void Start() {
		
	}

	IEnumerator Download(string url) {
		Texture2D texture = new Texture2D (1, 1);
		WWW www = new WWW (url);
		yield return www;
		print ("blep");
		www.LoadImageIntoTexture (texture);
		SaveTexture (texture);
	}

	void SaveTexture(Texture2D newImg) {
		string fileName = imgURL.Substring (20);
		fileNames.Add (fileName);
		string path = filePath + fileName;
		byte[] bytes = newImg.EncodeToPNG ();
		System.IO.File.WriteAllBytes (path, bytes);
		//Une fois téléchargé et stocké
		LoadTexture();
	}

	void LoadTexture() {
		for (int i=0; i<fileNames.Count; i++) {
		Texture2D texture = new Texture2D (256, 256, TextureFormat.DXT5, false);
			byte[] imgBytes = System.IO.File.ReadAllBytes (filePath + fileNames [i]);
			texture.LoadImage (imgBytes);
			Sprite image = Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height), Vector2.zero);
			img.sprite = image;
		}
	}
}