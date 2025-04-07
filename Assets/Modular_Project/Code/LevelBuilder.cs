using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace ProceduralLevelDesign
{

    #region Interface

    public interface ILevelEditor
    {
        public void ClearLevel();

        public void DeleteModule(Vector2 value);

        public void CreateModule(Vector2 value);
    }

    #endregion

    public class LevelBuilder : MonoBehaviour, ILevelEditor
    {
        #region Parameters

        [SerializeField] GameObject _modulePrefab;

        #endregion

        #region RuntimeVariables

        protected Ray rayFromSceneCamera;
        protected RaycastHit raycastHit;
        protected GameObject moduleInstance;
        public Vector3 modulePosition;

        #endregion

        #region Internal Data

        [SerializeField] protected List<Module> _allModulesInScene;

        #endregion

        #region Interfaces Methods

        public void ClearLevel()
        {
            //Debug.Log(this.name + " - " + gameObject.name + " ClearLevel() ");
            foreach (Module module in transform.GetComponentsInChildren<Module>())
            {
                DestroyImmediate(module.gameObject);
            }
            _allModulesInScene.Clear();
        }

        public void DeleteModule(Vector2 value)
        {
            //Debug.Log(this.name + " - " + gameObject.name + " DeleteModule( " + value.ToString() + ")", gameObject);
            rayFromSceneCamera = HandleUtility.GUIPointToWorldRay(value); //Camera.main.ScreenPointToRay(value);
            Debug.DrawRay(rayFromSceneCamera.origin, rayFromSceneCamera.direction * 10000f, Color.red, 5f);
            if (Physics.Raycast(rayFromSceneCamera, out raycastHit, 100000f))
            {
                if (raycastHit.collider.gameObject.layer == 6) //Layer -> Layout
                {
                    moduleInstance = raycastHit.collider.transform.parent.parent.gameObject;
                    _allModulesInScene.Remove(moduleInstance.GetComponent<Module>());
                    DestroyImmediate(moduleInstance);
                }
            }
        }

        public void CreateModule(Vector2 value)
        {
            Debug.Log(this.name + " - " + gameObject.name + " CreateModule( " + value.ToString() + ")", gameObject);
            rayFromSceneCamera = HandleUtility.GUIPointToWorldRay(value); //Camera.main.ScreenPointToRay(value);
            Debug.DrawRay(rayFromSceneCamera.origin, rayFromSceneCamera.direction * 10000f, Color.magenta, 5f);
            if (Physics.Raycast(rayFromSceneCamera, out raycastHit, 10000f))
            {
                Debug.Log("Ray");
                if (raycastHit.collider.gameObject.layer == 6) //Layer -> Layout
                {
                    Debug.Log("hola");
                    moduleInstance = Instantiate(_modulePrefab);
                    moduleInstance.transform.parent = transform;
                    modulePosition = raycastHit.point;
                    modulePosition.x = (int)modulePosition.x;
                    modulePosition.y = (int)modulePosition.y;
                    modulePosition.z = (int)modulePosition.z;
                    moduleInstance.transform.position = modulePosition;

                    _allModulesInScene.Add(moduleInstance.GetComponent<Module>());
                }
            }
        }

        #endregion
    }
}