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
	public string filePath;
	public Image img;
	//TODO la liste n'est pas permanente, sauvegarder
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

		//On ne doit pas appeler get_persistentDataPath dans une declaration de variable
		filePath = Application.persistentDataPath + "/Textures/";

		//PlayerPrefs.DeleteAll ();

		//D'abord on regarde si on a déjà une variable pour savoir si on a téléchargé ou pas, si non, on la définit
		if (PlayerPrefs.GetInt ("Telecharge") == null) {
			//Si le dossier textures n'exise pas, on le cŕee
			if (!System.IO.Directory.Exists(filePath)) 
				System.IO.Directory.CreateDirectory (filePath);
			
			//Si le fichier de sauvegarde des images n'existe pas, on le crée
			if (!System.IO.File.Exists (Application.persistentDataPath + @"/ImagesList.txt")) {
				System.IO.File.WriteAllText (Application.persistentDataPath + @"/ImagesList.txt", "");
			}

			//On déclare la variable dans les sauvegardes
			PlayerPrefs.SetInt ("Telecharge", 0);
		} else if (PlayerPrefs.GetInt ("Telecharge") == 0) { //Si on a pas encore téléchargé
			StartCoroutine(Download(imgURL));
			PlayerPrefs.SetInt("Telecharge", 1);
		} else { //Si on a déjà téléchargé
			LoadTexture();
		}
	}

	void Start() {
		
	}

	/*Coroutine qui nous permet de télécharger les images la prémière fois qu'on lance l'application.
	 * Idéalement, elle est accompagne d'un temps de chargement pendant lequel on peut faire d'autres
	 * calculs au même temps, ou bien montrer des choses au joueur.*/
	IEnumerator Download(string url) {
		Texture2D texture = new Texture2D (1, 1);
		WWW www = new WWW (url);
		yield return www;
		www.LoadImageIntoTexture (texture);
		SaveTexture (texture);
	}

	/*Fonction qui permet de sauvegarder la texture que l'on vient de télécharger afin de l'utiliser
	 * les prochaines fois que l'on lance l'application sans devoir les retelecharger*/
	void SaveTexture(Texture2D newImg) {
		//Le nom du fichier est celui avec lequel l'image a été uploadé
		string fileName = imgURL.Substring (20);

		//On ajoute le nom du fichier à notre fichier de sauvegarde des noms
		System.IO.File.AppendAllText(Application.persistentDataPath + @"/ImagesList.txt", fileName+",");

		//Création du fichier avec notre texture dans notre terminal
		string path = filePath + fileName;
		byte[] bytes = newImg.EncodeToPNG ();
		System.IO.File.WriteAllBytes (path, bytes);

		//Une fois téléchargé et stocké
		LoadTexture();
	}

	/*Si les textures dont nous avons besoin sont déjà dans le stockage interne de notre appareil,
	 * alors nous devons seulement les charger en lançant l'application*/
	void LoadTexture() {
		//Avant tout, on ajoute à notre liste les noms de toutes les images qu'on doit charger
		string[] files = System.IO.File.ReadAllText(Application.persistentDataPath + @"/ImagesList.txt").Split(',');
		for (int i = 0; i < files.Length-1; i++) {
			/*On préfère une liste car on a pas besoin de déterminer sa taille en la déclarant, et elle peut varier
			 * de taille entre les sessions de jeu sans problème*/
			fileNames.Add (files [i]);
		}

		print (fileNames.Count);
		//Mainteant, on cherche et charge toutes les images d'après les noms de fichier stockés dans la liste
		for (int i=0; i<fileNames.Count; i++) {
			Texture2D texture = new Texture2D (256, 256, TextureFormat.DXT5, false);
			byte[] imgBytes = System.IO.File.ReadAllBytes (filePath + fileNames [i]);
			texture.LoadImage (imgBytes);
			Sprite image = Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height), Vector2.zero);
			img.sprite = image;
		}
	}
}