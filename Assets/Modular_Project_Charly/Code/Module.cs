using UnityEngine;

namespace ProceduralLevelDesign {
    [System.Serializable]
    public struct ModulesPrefabs {
        public GameObject[] pillars;
        public GameObject[] walls;
    }

    public class Module : MonoBehaviour {
        #region Variables
        public LevelBuilder _levelBuilder;
        public Vector3 ModulePos;

        public ModulesPrefabs modulesPrefabs;

        public float rayDistanceWalls = 1.1f;
        public float rayDistancePillars = 1.1f;
        public LayerMask moduleLayer;
        public LayerMask pillarLayer;

        private Vector3[] directions;

        [Header("Center Offset Settings")]
        public Vector3 centerOffset = new Vector3(0.5f, 0, 0.5f);
        public float verticalPillarOffset = 0.5f;
        #endregion

        #region Unity Methods
        private void Start() {
            CheckNeighbors();
        }
        #endregion

        #region Private Methods
        public void CheckNeighbors() {

            directions = new Vector3[]
            {
                transform.right,    //Derecha
                -transform.right,   //Izquierda
                transform.forward,  //Arriba
                -transform.forward  //Abajo
            };

            CheckPillars();
            CheckWalls();
        }
        #endregion

        #region Walls & Pillars
        private void CheckWalls() {
            Vector3 rayOrigin = transform.position + centerOffset;
            for (int i = 0; i < directions.Length; i++) {
                if (Physics.Raycast(rayOrigin, directions[i], out RaycastHit hit, rayDistanceWalls, moduleLayer)) {
                    if (modulesPrefabs.walls[i] != null)
                        modulesPrefabs.walls[i].SetActive(false);
                }
                else {
                    modulesPrefabs.walls[i].SetActive(true);
                }
            }
        }

        private void CheckPillars() {

            for (int i = 0; i < modulesPrefabs.pillars.Length; i++) {

                Vector3 rayOrigin = modulesPrefabs.pillars[i].transform.position + Vector3.up * verticalPillarOffset;
                bool foundAnyWAll = false;

                // Lanzar rayos en las 4 direcciones desde el pilar
                for (int dir = 0; dir < directions.Length; dir++) {
                    if (Physics.Raycast(rayOrigin, directions[dir], rayDistancePillars, pillarLayer)) { 
                        foundAnyWAll = true;
                        break; // Si cualquier rayo no golpea, desactivamos
                    }
                }

                modulesPrefabs.pillars[i].SetActive(foundAnyWAll);
            }
        }

        #endregion

        #region Gizmos
        private void OnDrawGizmos() {
            if (directions == null || directions.Length == 0) {
                directions = new Vector3[]
                {
                    transform.right,
                    -transform.right,
                    transform.forward,
                    -transform.forward
                };
            }

            Gizmos.color = Color.green;
            Vector3 rayOrigin = transform.position + centerOffset;

            // Dibujar rayos de paredes
            foreach (var dir in directions) {
                Gizmos.DrawRay(rayOrigin, dir.normalized * rayDistanceWalls);
            }

            // Dibujar rayos de pilares

            for (int i = 0; i < modulesPrefabs.pillars.Length; i++) {


                Vector3 pillarPos = modulesPrefabs.pillars[i].transform.position + Vector3.up * verticalPillarOffset;

                // Verificar si detecta paredes para cambiar color
                foreach (var dir in directions) {
                    if (Physics.Raycast(pillarPos, dir, rayDistancePillars, pillarLayer)) {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawRay(pillarPos, dir * rayDistancePillars);
                    }
                    else {
                        Gizmos.color = Color.red;
                        Gizmos.DrawRay(pillarPos, dir * rayDistancePillars);
                    }
                }
            }
        }
        #endregion

    }
}