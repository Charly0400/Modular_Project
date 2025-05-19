using UnityEngine;
using UnityEditor;
using ProceduralLevelDesign;

[CustomEditor(typeof(LevelBuilder))]
public class Modular_Editor : Editor
{
	#region Variables

	[SerializeField] protected LevelBuilder _levelBuilder;

    #endregion

    #region InspectorGUI

    public override void OnInspectorGUI() {
        if (_levelBuilder == null) {
            _levelBuilder = (LevelBuilder)target;
        }

        DrawDefaultInspector();

        if (GUILayout.Button("Probbing")) {
            _levelBuilder.ProbbingModules();
        }

        if (GUILayout.Button("UpdateNeighbours")) {
            _levelBuilder.CheckWallsAndPillars();
        }

        if (GUILayout.Button("Binary Spacing")) {
            _levelBuilder.ClearLevel();
            _levelBuilder.ProbbingModules();

            Mazmorra mazmorra = new Mazmorra() {
                min_X = 0,
                min_Y = 0,
                max_X = _levelBuilder.sizeX,
                max_Y = _levelBuilder.sizeY,
            };

            _levelBuilder.BinarySpacePartition(mazmorra, -1, PreviousCut.NONE);
            _levelBuilder.CheckWallsAndPillars();

            //_levelBuilder.StartPartition();
        }

        if (GUILayout.Button("DelteModules")) {
            _levelBuilder.ClearLevel();
        }
    }

    #endregion
}
