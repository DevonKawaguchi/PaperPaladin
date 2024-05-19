using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;
    [SerializeField] AudioClip sceneMusic;

    public bool IsLoaded { get; private set; }

    List<SaveableEntity> saveableEntities;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log($"Entered {gameObject.name}");

            LoadScene();
            GameController.Instance.SetCurrentScene(this); //Reference of current and previous scene

            if (sceneMusic != null)
            {
                AudioManager.i.PlayMusic(sceneMusic, fade: true); //Plays music. 2nd parameter not passed as loop = True by default
            }

            //Load all connected scenes
            foreach (var scene in connectedScenes)
            {
                scene.LoadScene();
            }

            //Unload the scenes that are no longer connected
            var prevScene = GameController.Instance.PrevScene;

            if (prevScene!= null)
            {
                var previouslyLoadedScenes = GameController.Instance.PrevScene.connectedScenes;
                foreach (var scene in previouslyLoadedScenes)
                {
                    if (!connectedScenes.Contains(scene) && scene != this) //If scene is not currently loaded scene and not in the connected scene of the current scene, unload the scene(s)
                    {
                        scene.UnloadScene();
                    }
                }

                if (!connectedScenes.Contains(prevScene)) //Scene will be unloaded if not connected scene
                {
                    prevScene.UnloadScene();
                }
            }
        }
    }

    public void LoadScene()
    {
        if (!IsLoaded)
        {
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive); //"LoadSceneAsync" means the line is being run in the background
            IsLoaded = true;

            operation.completed += (AsyncOperation op) =>
            {
                saveableEntities = GetSaveableEntitiesInScene(); //Restores entity states after loading the scene
                SavingSystem.i.RestoreEntityStates(saveableEntities);
            }; //Function will only be called once this function is complete/when scene loading is complete
        }
    }

    public void UnloadScene()
    {
        if (IsLoaded)
        {
            SavingSystem.i.CaptureEntityStates(saveableEntities);

            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    List<SaveableEntity> GetSaveableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var saveableEntities = FindObjectsOfType<SaveableEntity>().Where(x => x.gameObject.scene == currScene).ToList();
        return saveableEntities;
    }

    public AudioClip SceneMusic => sceneMusic;
}
