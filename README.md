# TestFeerik
Gestionnaire de resource - telechargement et chargement d'une texture

Le gestionnaire télécharge les fichiers image en format PNG dans un dossier du persistentDataPath de la machine dans lequel on l'utilise. Ensuite, il les sauvegarde et charge en tant que textures exploitables par Unity. À chaque fois que l'application sera ouverte après ça, le chargement des images se fera automatiquement à partir des fichiers sauvegardés dans l'appareil.

Par contre, il n'est pas possible d'utiliser l'API de Unity dans un thread qui n'est pas le main. Pour le chargement, toutes les images sont ouvertes et stockées dans des tableaux de bytes, par contre, le changement de celles-là en textures et/ou sprites exploitables par Unity doit être fait dans la boucle de jeu principale, et entraine donc un temps de "freeze", mais supportable et qui peut être comblé par un écran de chargement comme dans la plupart des jeux. Ce chargement est fait dans une coroutine pour permettre tout de même la réalisation d'autres actions en parallèle.

Je suis consciente que ma façon de faire ce chargement à la fin des threads n'est pas du tout optimale, mais je n'ai pas réussi à trouver une meilleure en tenant compte du fait que je ne peux pas utiliser l'Unity API dans un thread qui n'est pas le main. Si vous avez une meilleure solution, je vous prie de me la montrer car ça m'intrigue énormement.