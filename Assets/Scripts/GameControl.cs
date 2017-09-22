/* GameControl contient une classe statique qui est accessible depuis tous les objets et scripts du jeu, et qui 
 * reste la même même entre les différentes scènes et ne peut pas être détruite. Ainsi, c'est ici que l'on
 * aura toutes les variables et classes qui doivent rester joignables et constante entre tous les objets*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine.UI;

public class GameControl : MonoBehaviour {

	//Juste un objet GameControl, et toujours le même
	public static GameControl control;

	//Variables globales accesibles depuis tous les scripts et objets
	public string filePath, textPath;
	public Image img;
	public List<String> fileNames = new List<String>();
	public TextAsset urls;

	private byte[] imgBytes;
	public string[] imgURL; 

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
		textPath = Application.persistentDataPath + @"/ImagesList.txt";

		/*imgURL est un tableau qui contient toutes les adresses sur lesquelles on va télécharger
		 * des images. Elles sont stockées dans un fichier texte*/
		imgURL = urls.text.Split ('\n');

		//PlayerPrefs.DeleteAll ();

		//D'abord on regarde si on a déjà une variable pour savoir si on a téléchargé ou pas, si non, on la définit
		if (!PlayerPrefs.HasKey("Telecharge")) {
			//Si le dossier textures n'exise pas, on le cŕee
			if (!System.IO.Directory.Exists(filePath)) 
				System.IO.Directory.CreateDirectory (filePath);
			
			//Si le fichier de sauvegarde des images n'existe pas, on le crée
			if (!System.IO.File.Exists (textPath)) {
				System.IO.File.WriteAllText (textPath, "");
			}

			//On déclare la variable dans les sauvegardes
			PlayerPrefs.SetInt ("Telecharge", 0);
		} 

		if (PlayerPrefs.GetInt ("Telecharge") == 0) { //Si on a pas encore téléchargé
			for (int i = 0; i < imgURL.Length; i++) {
				StartCoroutine (Download (i));
			}
			PlayerPrefs.SetInt("Telecharge", 1);
		} 
		if (PlayerPrefs.GetInt ("Telecharge") == 1) { //Si on a déjà téléchargé
			LoadingThreads();
		}
	}

	void Update() {
		
	}

	/*Coroutine qui nous permet de télécharger les images la prémière fois qu'on lance l'application.
	 * Idéalement, elle est accompagne d'un temps de chargement pendant lequel on peut faire d'autres
	 * calculs au même temps, ou bien montrer des choses au joueur.*/
	IEnumerator Download(int index) {
		Texture2D texture = new Texture2D (1, 1);
		WWW www = new WWW (imgURL[index]);
		yield return www;
		www.LoadImageIntoTexture (texture);
		byte[] newBytes = texture.EncodeToPNG ();

		//Pour chaque image téléchargée, on commence alors un thread pour la sauvegarder
		Thread t = new Thread(() => SaveTexture (newBytes, index));
		t.Start ();
	}

	/*Fonction qui permet de sauvegarder la texture que l'on vient de télécharger afin de l'utiliser
	 * les prochaines fois que l'on lance l'application sans devoir les retelecharger*/
	void SaveTexture(byte[] bytes, int index) {
		//Le nom du fichier est celui avec lequel l'image a été uploadé
		string fileName = imgURL[index].Substring (20);

		//On ajoute le nom du fichier à notre fichier de sauvegarde des noms
		System.IO.File.AppendAllText(textPath, fileName+",");

		//Création du fichier avec notre texture dans notre terminal
		string path = filePath + fileName;
		System.IO.File.WriteAllBytes (path, bytes);
	}

	/*Si les textures dont nous avons besoin sont déjà dans le stockage interne de notre appareil,
	 * alors nous devons seulement les charger en lançant l'application*/
	void LoadingThreads() {
		//Avant tout, on ajoute à notre liste les noms de toutes les images qu'on doit charger
		string[] files = System.IO.File.ReadAllText(textPath).Split(',');
		for (int i = 0; i < files.Length-1; i++) {
			/*On préfère une liste car on a pas besoin de déterminer sa taille en la déclarant, et elle peut varier
			 * de taille entre les sessions de jeu sans problème*/
			fileNames.Add (files [i]);
		}

		//print (fileNames.Count);
		/*Mainteant, on cherche et charge toutes les images d'après les noms de fichier stockés dans la liste. Chaque
		 * image est chargée dans un thread separé pour pouvoir toutes les charger au même temps*/
		for (int i=0; i<fileNames.Count; i++) {
			Thread t = new Thread(() => LoadTexture(i));
			t.Start ();
			Texture2D texture = new Texture2D (256, 256, TextureFormat.DXT5, false);
			texture.LoadImage (imgBytes);
			Sprite image = Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height), Vector2.zero);
			img.sprite = image;
		}
	}

	void LoadTexture(int index) {
		imgBytes = System.IO.File.ReadAllBytes (filePath + fileNames[index-1]);
	}
}