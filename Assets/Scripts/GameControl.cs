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

	private byte[][] allImgs;
	private byte[] imgBytes;
	private int imagesI;
	private string[] imgURL; 

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

		//Ligne laissée en place en commentaire pour vous permettre de tester plus facilement si vous voulez
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
		} else { //Si on a déjà téléchargé
			LoadingThreads();
		}
	}

	void Update() {
		//Quand toutes les images ont été chargées et transformées en bytes[] et que donc on peut en faire des textures
		if (imagesI != 0 && imagesI == fileNames.Count) {
			StartCoroutine (LoadOnTexture ());
			imagesI = 0;
		}
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

	/*Coroutine pour le chargement des images en parallèle (mais pas aussi optimisé qu'un thread), qui permet
	 * l'usage de l'API Unity*/
	IEnumerator LoadOnTexture() {
		for (int i = 0; i < fileNames.Count; i++) {
			Texture2D texture = new Texture2D (256, 256, TextureFormat.DXT5, false);
			texture.LoadImage (allImgs[i]);
			/*Il n'y a qu'une image pour cet exemple, mais il serait très simple d'avoir une liste, ou un tableau, 
			 * avec toutes les images dans notre jeu que nous avons besoin de charger et les prendre de là pour 
			 * charger chaque texture couplée à chaque sprite*/
			Sprite image = Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height), Vector2.zero);
			img.sprite = image;
		}
		yield return null;
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

		LoadingThreads ();
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

		//Le tableau de tableaus de bytes est initialísé une fois que l'on sait combien d'images on a
		allImgs = new byte[fileNames.Count][];

		/*Mainteant, on cherche et charge toutes les images d'après les noms de fichier stockés dans la liste. Chaque
		 * image est chargée dans un thread separé pour pouvoir toutes les charger au même temps*/
		for (int i=0; i<fileNames.Count; i++) {
			Thread t = new Thread(() => LoadTexture(i));
			t.Start ();
		}
	}

	/*Ne pouvant pas utiliser les fonctions de l'API de Unity dans un thread qui n'est pas le principal, j'ai decidé de
	 * stocker les bytes de chaque image dans un tableau que l'on parcourt une fois qu'on a fini de tout charger pour les
	 * convertir en Texture2D exploitables par Unity*/
	void LoadTexture(int index) {
		imgBytes = System.IO.File.ReadAllBytes (filePath + fileNames[index-1]);
		allImgs [imagesI] = imgBytes;
		imagesI++;
	}
}