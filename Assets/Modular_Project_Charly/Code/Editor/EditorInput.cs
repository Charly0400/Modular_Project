using UnityEngine;
using UnityEditor;

namespace ProceduralLevelDesign
{
    public class EditorInput
    {
        #region Local Methods

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void ScriptsHasBeenReloaded()
        {
            SceneView.duringSceneGui += DuringSceneGui;
        }

        #region DelegateMethods

        private static void DuringSceneGui(SceneView sceneView) // equivalente OnDrawGizmos()
        {
            Event e = Event.current; //Equivaltenta al InputAction.CallbackContext / InputValue
            //Event stores data input from the level designer / programmer

            //Debug.Log("EditorInput - DuringSceneGui(): " + e);
            LevelBuilder levelBuilder = GameObject.FindAnyObjectByType<LevelBuilder>();

            if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Delete) //Delete all object on scene
            {
                //Method to clean all the level
                levelBuilder?.ClearLevel();
            }

            if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Backspace)
            {
                //Method to delte a tile / module from scene
                levelBuilder?.DeleteModule(e.mousePosition);
            }

            if (e.type == EventType.MouseUp && e.button == 0)
            {
                //Method to instantiate a tile / module in the scene
                levelBuilder?.CreateModule(e.mousePosition);
            }

        }

        #endregion
        #endregion
    }
}

