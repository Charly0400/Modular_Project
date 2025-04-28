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

        [Header("Probing Settings")]
        [SerializeField] protected int sizeX = 10;
        [SerializeField] protected int sizeZ = 10;

        #region Internal Data

        [SerializeField] protected List<Module> _allModulesInScene;

        #endregion



        #endregion



        #region Interfaces Methods

        #region DuringSceneGui
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
                if (raycastHit.collider.gameObject.layer == 3) //Layer -> Layout
                {
                    moduleInstance = raycastHit.collider.transform.parent.parent.parent.gameObject;
                    _allModulesInScene.Remove(moduleInstance.GetComponent<Module>());
                    DestroyImmediate(moduleInstance.gameObject);

                    Physics.SyncTransforms();
                    Invoke("CheckWallsAndPillars", 0.5f);
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
                if (raycastHit.collider.gameObject.layer == 6) //Layer -> Layout
                {
                    moduleInstance = Instantiate(_modulePrefab);
                    moduleInstance.transform.parent = transform;
                    modulePosition = raycastHit.point;
                    modulePosition.x = (int)modulePosition.x;
                    modulePosition.y = (int)modulePosition.y;
                    modulePosition.z = (int)modulePosition.z;
                    moduleInstance.transform.position = modulePosition;

                    _allModulesInScene.Add(moduleInstance.GetComponent<Module>());
                    moduleInstance.GetComponent<Module>()._levelBuilder = this;
                    moduleInstance.GetComponent<Module>().ModulePos = modulePosition;

                    Physics.SyncTransforms();
                    Invoke("CheckWallsAndPillars", 0.65f);

                }
            }
        }

        public void CheckWallsAndPillars()
        {
            foreach (Module module in _allModulesInScene)
            {
                module.GetComponent<Module>().CheckNeighbors();
            }
        }

        public void ChangeModuleStyle()
        {
            Debug.Log("Cambio de Modulo");
        }

        #endregion

        #region OnInspectorGUI

        public void ProbbingModules()
        {
            Vector3 startPosition = transform.position - new Vector3 (0f, 0f, 0f);
            for (int x= 0; x < sizeX; x++)
            {
                for (int z= 0; z < sizeX; z++)
                {
                    Vector3 moduleStartPosition = startPosition + new Vector3 (x, 0, z);

                    moduleInstance = Instantiate(_modulePrefab, moduleStartPosition, Quaternion.identity);
                    moduleInstance.transform.parent = transform;

                    modulePosition = moduleInstance.transform.position;
                    modulePosition.x = (int)modulePosition.x;
                    modulePosition.y = (int)modulePosition.y;
                    modulePosition.z = (int)modulePosition.z;

                    _allModulesInScene.Add(moduleInstance.GetComponent<Module>());
                    moduleInstance.GetComponent<Module>()._levelBuilder = this;
                    moduleInstance.GetComponent<Module>().ModulePos = modulePosition;

                }
            }
        }

        public void DeleteModules()
        {
            foreach (Module module in transform.GetComponentsInChildren<Module>())
            {
                DestroyImmediate(module.gameObject);
            }
            _allModulesInScene.Clear();
        }


        #endregion

        #region Gizmos


        Vector3 start;
        Vector3 end;
        Vector3 startPosition;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.gray;

            startPosition = transform.position - new Vector3(0f, 0, 0f);

            for (int x = 0; x <= sizeX; x++)
            {
                start = startPosition + (x * Vector3.right);
                end = start + (sizeZ * Vector3.forward);
                Gizmos.DrawLine(start, end);
            }

            for (int z = 0; z <= sizeZ; z++)
            {
                start = startPosition + (z * Vector3.forward);
                end = start + (sizeX * Vector3.right);
                Gizmos.DrawLine(start, end);
            }
        } 

        #endregion

        #endregion
    }
}