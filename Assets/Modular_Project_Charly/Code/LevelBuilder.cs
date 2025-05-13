using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace ProceduralLevelDesign {

    #region Interface

    public interface ILevelEditor {
        public void ClearLevel();

        public void DeleteModule(Vector2 value);

        public void CreateModule(Vector2 value);
    }

    #endregion

    #region Structs
    [System.Serializable]
    public struct Mazmorra {
        [Header("X")]
        public int min_X;
        public int max_X;

        [Header("Y")]
        public int min_Y;
        public int max_Y;

        [Header("Bools")]
        public bool isSlisableX;
        public bool isSlisableY;

        public int Width() {
            return max_X - min_X;
        }
        public int Height() {
            return max_Y - min_Y;
        }

        public string PrintMazmorra() {
            return "Min_X: " + min_X + ",    Max_X: " + max_X + ",    Min_Y: " + min_Y + ",    Max_Y: " + max_Y;
        }
    }

    #endregion

    public class LevelBuilder : MonoBehaviour, ILevelEditor {
        #region Parameters

        [SerializeField] GameObject[] _modulePrefab;

        #endregion

        #region RuntimeVariables

        protected Ray rayFromSceneCamera;
        protected RaycastHit raycastHit;
        protected GameObject moduleInstance;
        [SerializeField] protected Vector3 modulePosition;

        [SerializeField] protected bool _isRecursivityDone;

        [Header("Probing Settings")]
        [SerializeField] public int sizeX = 10;
        [SerializeField] public int sizeY = 10;

        [SerializeField] protected int minDoungeonX = 1;
        [SerializeField] protected int minDoungeonY = 1;

        #region Internal Data
        [Header("Dungeons")]


        [Header("Modules")]
        [SerializeField] protected List<Module> _allModulesInScene;
        [SerializeField] protected List<Mazmorra> _DungeonsOnScene;

        protected List<GameObject> _dungeonsParents;

        #endregion

        #endregion

        #region Interfaces Methods

        #region DuringSceneGui
        public void ClearLevel() {
            //Debug.Log(this.name + " - " + gameObject.name + " ClearLevel() ");
            foreach (Module module in _allModulesInScene) {
                module.gameObject.SetActive(true);
            }

            foreach (Module module in transform.GetComponentsInChildren<Module>()) {
                DestroyImmediate(module.gameObject);
            }

            foreach (GameObject go in _dungeonsParents) {
                DestroyImmediate(go);
            }

            _allModulesInScene.Clear();
            _DungeonsOnScene.Clear();
            _dungeonsParents.Clear();
        }

        public void DeleteModule(Vector2 value) {
            //Debug.Log(this.name + " - " + gameObject.name + " DeleteModule( " + value.ToString() + ")", gameObject);
            rayFromSceneCamera = HandleUtility.GUIPointToWorldRay(value); //Camera.main.ScreenPointToRay(value);
            Debug.DrawRay(rayFromSceneCamera.origin, rayFromSceneCamera.direction * 10000f, Color.red, 5f);
            if (Physics.Raycast(rayFromSceneCamera, out raycastHit, 100000f)) {
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

        public void CreateModule(Vector2 value) {
            GameObject prefab = _modulePrefab[0];
            Debug.Log(this.name + " - " + gameObject.name + " CreateModule( " + value.ToString() + ")", gameObject);
            rayFromSceneCamera = HandleUtility.GUIPointToWorldRay(value); //Camera.main.ScreenPointToRay(value);
            Debug.DrawRay(rayFromSceneCamera.origin, rayFromSceneCamera.direction * 10000f, Color.magenta, 5f);
            if (Physics.Raycast(rayFromSceneCamera, out raycastHit, 10000f)) {
                if (raycastHit.collider.gameObject.layer == 6) //Layer -> Layout
                {
                    moduleInstance = Instantiate(prefab);
                    moduleInstance.transform.parent = transform;
                    modulePosition = raycastHit.point;
                    modulePosition.x = (int)modulePosition.x;
                    modulePosition.y = (int)modulePosition.y;
                    modulePosition.z = (int)modulePosition.z;
                    moduleInstance.transform.position = modulePosition;

                    Module module = moduleInstance.GetComponent<Module>();
                    if (!_allModulesInScene.Contains(module)) {
                        _allModulesInScene.Add(module);
                    }
                    module._levelBuilder = this;
                    module.ModulePos = modulePosition;

                    Physics.SyncTransforms();
                    Invoke("CheckWallsAndPillars", 0.65f);

                }
            }
        }

        #endregion

        #region public Methods
        public void CheckWallsAndPillars() {
            foreach (Module module in _allModulesInScene) {
                module.GetComponent<Module>().CheckNeighbors();
            }
        }

        public void ChangeModuleStyle() {
            Debug.Log("Cambio de Modulo");
        } 
        #endregion

        #endregion

        #region Private Methods
        private void GroupModulesInDungeon(Mazmorra mazmorra) {
            GameObject dungeonGroup = new GameObject("Mazmorra_" + _DungeonsOnScene.Count);
            dungeonGroup.transform.parent = this.transform;

            GameObject selectedPrefab = _modulePrefab[Random.Range(0, _modulePrefab.Length)];

            foreach (Module module in _allModulesInScene) {
                Vector3 pos = module.ModulePos;
                if (pos.x >= mazmorra.min_X && pos.x <= mazmorra.max_X &&
                    pos.z >= mazmorra.min_Y && pos.z <= mazmorra.max_Y) {

                    module.isInDungeon = true;
                    module.transform.parent = dungeonGroup.transform;
                }
            }
            _dungeonsParents.Add(dungeonGroup);
        }
        private void GroupUnusedModules() {
            GameObject existing = GameObject.Find("ModulosSinMazmorra");
            GameObject trashGroup = existing != null ? existing : new GameObject("ModulosSinMazmorra");

            trashGroup.transform.parent = this.transform;

            foreach (Module module in _allModulesInScene) {
                if (!module.isInDungeon) {
                    module.transform.parent = trashGroup.transform;
                }
            }
            if (!_dungeonsParents.Contains(trashGroup)) {
                _dungeonsParents.Add(trashGroup);
            }

        }
        private void ReplaceModulesInDungeon(Mazmorra mazmorra) {
            GameObject dungeonGroup = new GameObject("Mazmorra_" + _DungeonsOnScene.Count);
            dungeonGroup.transform.parent = this.transform;

            GameObject selectedPrefab = _modulePrefab[Random.Range(0, _modulePrefab.Length)];

            foreach (Module oldModule in _allModulesInScene.ToArray()) {
                Vector3 pos = oldModule.ModulePos;
                if (pos.x >= mazmorra.min_X && pos.x <= mazmorra.max_X &&
                    pos.z >= mazmorra.min_Y && pos.z <= mazmorra.max_Y) {

                    // Reemplazar el módulo con uno de diseño distinto
                    GameObject newInstance = Instantiate(selectedPrefab, pos, Quaternion.identity);
                    newInstance.transform.parent = dungeonGroup.transform;

                    Module newModule = newInstance.GetComponent<Module>();
                    newModule._levelBuilder = this;
                    newModule.ModulePos = pos;

                    _allModulesInScene.Remove(oldModule);
                    DestroyImmediate(oldModule.gameObject);

                    _allModulesInScene.Add(newModule);
                }
            }
        }
        #endregion

        #region Recursivity
        public void BinarySpacePartition(Mazmorra mazmorra) //, Vector2 cut, bool previousAxisCut)
        {
            Debug.Log("BinarySpacePartition() " + mazmorra.PrintMazmorra());
            //Tenemos que determinar si se puede hacer un corte horizontal
            if (mazmorra.Width() > minDoungeonX * 2) {
                mazmorra.isSlisableX = true;
            }
            //y validar si se puede hacer un corte vertical
            if (mazmorra.Height() > minDoungeonY * 2) {
                mazmorra.isSlisableY = true;
            }

            if (!mazmorra.isSlisableY && !mazmorra.isSlisableX) {
                _DungeonsOnScene.Add(mazmorra);
                GroupModulesInDungeon(mazmorra);
                //ReplaceModulesInDungeon(mazmorra);
                _isRecursivityDone = true;
                GroupUnusedModules();
                CheckWallsAndPillars();
                return;
            }

            //IF else para saber si rebanas en horizontal o en vertical
            // y si en ambos se puede un random entre uno u otro

            if (mazmorra.isSlisableY && mazmorra.isSlisableX) {
                int randomBool = Random.Range(0, 1);

                if (randomBool == 0) {
                    mazmorra.isSlisableX = true;
                    mazmorra.isSlisableY = false;
                }
                else if (randomBool == 1) {
                    mazmorra.isSlisableX = false;
                    mazmorra.isSlisableY = true;
                }

            }

            #region Check On Width and Cuts On Height 

            if (mazmorra.isSlisableX && !mazmorra.isSlisableY) {
                int RandomCut = Random.Range(mazmorra.min_X + minDoungeonX + 1,
                                mazmorra.max_X - minDoungeonX - 1);

                //for (int i = mazmorra.min_Y; i <= mazmorra.max_Y; i++) {
                //    matrix[RandomCut, i].gameObject.SetActive(false);
                //}
                foreach (Module module in _allModulesInScene) {
                    if (module.ModulePos.x == RandomCut) {
                        if (module.ModulePos.z >= mazmorra.min_Y && module.ModulePos.z <= mazmorra.max_Y) {
                            module.gameObject.SetActive(false);
                        }
                    }
                }

                int connectionRoomIndex = Random.Range(mazmorra.min_Y, mazmorra.max_Y);
                foreach (Module module in _allModulesInScene) {
                    if (module.ModulePos.x == RandomCut && module.ModulePos.z == connectionRoomIndex) {
                        module.gameObject.SetActive(true);
                    }
                }

                CheckWallsAndPillars();

                Mazmorra DungeonA = new Mazmorra() {
                    min_X = mazmorra.min_X,
                    max_X = RandomCut - 1,
                    min_Y = mazmorra.min_Y,
                    max_Y = mazmorra.max_Y
                };

                Mazmorra DungeonB = new Mazmorra() {
                    min_X = RandomCut + 1,
                    max_X = mazmorra.max_X,
                    min_Y = mazmorra.min_Y,
                    max_Y = mazmorra.max_Y
                };

                BinarySpacePartition(DungeonA);
                BinarySpacePartition(DungeonB);
            }
            #endregion

            #region Check On Height and Cuts On Width 

            else if (!mazmorra.isSlisableX && mazmorra.isSlisableY) {
                int RandomCut = Random.Range(mazmorra.min_Y + minDoungeonY + 1,
                                mazmorra.max_Y - minDoungeonY - 1);

                //for (int i = mazmorra.min_Y; i <= mazmorra.max_Y; i++) {
                //    matrix[RandomCut, i].gameObject.SetActive(false);
                //}
                foreach (Module module in _allModulesInScene) {
                    if (module.ModulePos.z == RandomCut) {
                        if (module.ModulePos.x >= mazmorra.min_X && module.ModulePos.x <= mazmorra.max_X) {
                            module.gameObject.SetActive(false);
                        }
                    }
                }

                int connectionRoomIndex = Random.Range(mazmorra.min_X, mazmorra.max_X);
                foreach (Module module in _allModulesInScene) {
                    if (module.ModulePos.z == RandomCut && module.ModulePos.x == connectionRoomIndex) {
                        module.gameObject.SetActive(true);
                    }
                }

                CheckWallsAndPillars();

                Mazmorra DungeonA = new Mazmorra() {
                    min_X = mazmorra.min_X,
                    max_X = mazmorra.max_X,
                    min_Y = mazmorra.min_Y,
                    max_Y = RandomCut - 1
                };

                Mazmorra DungeonB = new Mazmorra() {
                    min_X = mazmorra.min_X,
                    max_X = mazmorra.max_X,
                    min_Y = RandomCut + 1,
                    max_Y = mazmorra.max_Y
                };

                BinarySpacePartition(DungeonA);
                BinarySpacePartition(DungeonB);

            }

            #endregion

        }

        #endregion

        #region OnInspectorGUI

        public void ProbbingModules() {

            Vector3 startPosition = transform.position - new Vector3(0f, 0f, 0f);
            GameObject prefab = _modulePrefab[0]; 
            for (int x = 0; x < sizeX; x++) {
                for (int z = 0; z < sizeX; z++) {
                    Vector3 moduleStartPosition = startPosition + new Vector3(x, 0, z);

                    moduleInstance = Instantiate(prefab, moduleStartPosition, Quaternion.identity);
                    moduleInstance.transform.parent = transform;

                    modulePosition = moduleInstance.transform.position;
                    modulePosition.x = (int)modulePosition.x;
                    modulePosition.y = (int)modulePosition.y;
                    modulePosition.z = (int)modulePosition.z;

                    Module module = moduleInstance.GetComponent<Module>();
                    if (!_allModulesInScene.Contains(module)) {
                        _allModulesInScene.Add(module);
                    }
                    module._levelBuilder = this;
                    module.ModulePos = modulePosition;

                }
            }
        }

        #endregion

        #region Gizmos


        Vector3 start;
        Vector3 end;
        Vector3 startPosition;

        private void OnDrawGizmos() {
            Gizmos.color = Color.gray;

            startPosition = transform.position - new Vector3(0f, 0, 0f);

            for (int x = 0; x <= sizeX; x++) {
                start = startPosition + (x * Vector3.right);
                end = start + (sizeY * Vector3.forward);
                Gizmos.DrawLine(start, end);
            }

            for (int z = 0; z <= sizeY; z++) {
                start = startPosition + (z * Vector3.forward);
                end = start + (sizeX * Vector3.right);
                Gizmos.DrawLine(start, end);
            }
        }

        #endregion

    }
}