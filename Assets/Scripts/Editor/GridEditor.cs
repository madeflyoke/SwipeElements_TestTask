using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gameplay.Data;
using Gameplay.Enums;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class GridEditor : EditorWindow
    {
        private const int GRID_SIZE = 15; //maximum size of comfort field (mobiles)
        private const string LEVELS_DATA_NAME = "LevelsData";
        private const string LEVELS_DATA_PATH = "/Resources/Data/"+LEVELS_DATA_NAME+".json";
    
        private readonly Dictionary<Color, BlockType> _blockTypesByColors = new Dictionary<Color, BlockType>()
        {
            {Color.red, BlockType.FIRE},
            {Color.blue, BlockType.WATER},
        };
    
        private Color _currentColor = Color.red;
        private Color[,]_colors = new Color[GRID_SIZE, GRID_SIZE];
        private GridBlocksLevelData _currentSelectedLevelData;
        private LevelsData _levelsData;

        [MenuItem("Window/Levels Editor")]
        public static void ShowWindow()
        {
            GetWindow<GridEditor>("Levels Editor");
        }

        private void OnEnable()
        {
            LoadLevelsData();
        }

        private void OnGUI()
        {
            var rect = new Rect(position.width -25f, 0, 25, 25); //current color
            EditorGUI.DrawRect(rect, _currentColor);
            
            EditorGUILayout.BeginHorizontal();
            foreach (var kvp in _blockTypesByColors) //color select buttons
            {
                var guiStyle = new GUIStyle();
                guiStyle.normal.textColor = _currentColor == kvp.Key ? Color.green : Color.white;
                guiStyle.alignment = TextAnchor.MiddleCenter;
                
                if (GUILayout.Button(kvp.Value.ToString(), guiStyle, GUILayout.Height(25), GUILayout.Width(100)))
                {
                    _currentColor = kvp.Key;
                }
            }

            if (GUILayout.Button("AddLevel", GUILayout.Width(150), GUILayout.Height(25)))
            {
                _levelsData.AddNextLevelData();
                SelectLevel(_levelsData.Data.Count-1);
            }

            if (_levelsData.Data!=null)
            {
                foreach (var gridData in _levelsData.Data) //levels buttons
                {
                    if (GUILayout.Button(gridData.LevelId.ToString(), GUILayout.Height(25), GUILayout.Width(25)))
                    {
                        SelectLevel(gridData.LevelId);
                    }
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("ClearAll"))
            {
                ResetGrid();
                _currentSelectedLevelData?.ClearData();
            }

            if (_currentSelectedLevelData!=null)
            {
                DrawGrid();
            }
            
            if (GUILayout.Button("Save Levels Data"))
            {
                SaveLevelsData();
            }
        }

        private void DrawGrid()
        {
            var gridSize = GRID_SIZE - 1;
            for (int yPos = 1; yPos < gridSize; yPos++)
            {
                EditorGUILayout.BeginHorizontal(); //buttons draw from left-up point, while actual data start will be left-bottom point
                GUILayout.FlexibleSpace();

                for (int xPos = 1; xPos < gridSize; xPos++)
                {
                    GUI.backgroundColor = _colors[xPos, yPos];

                    var normalizedYPos = Mathf.Abs(yPos-gridSize);
                
                    if (GUILayout.Button($"{xPos}:{normalizedYPos}", GUILayout.MaxWidth(45), GUILayout.MaxHeight(45)))
                    {
                        _colors[xPos, yPos] = _currentColor;
                        _currentSelectedLevelData.AddData(xPos,normalizedYPos, _blockTypesByColors[_currentColor]);
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            GUI.backgroundColor = Color.white;
        }

        private void ResetGrid()
        {
            _colors = new Color[GRID_SIZE, GRID_SIZE];
        }
        
        private void LoadLevelsData()
        {
            TextAsset levelsDataJson = Resources.Load<TextAsset>($"Data/{LEVELS_DATA_NAME}");
            if (levelsDataJson==null)
            {
                LevelsData levelsData = new LevelsData();
                var json =JsonUtility.ToJson(levelsData);

                if (AssetDatabase.IsValidFolder("Assets/Resources/Data")==false)
                {
                    AssetDatabase.CreateFolder("Assets/Resources", "Data");
                    AssetDatabase.Refresh();

                }

                File.WriteAllText(Application.dataPath+LEVELS_DATA_PATH, json);
                AssetDatabase.Refresh();
                _levelsData = levelsData;
            }
            else
            {
                _levelsData = JsonUtility.FromJson<LevelsData>(levelsDataJson.text);
                SelectLevel(0);
            }
        }

        private void SelectLevel(int id)
        {
            ResetGrid();
            if (_levelsData.Data.Count>0)
            {
                _currentSelectedLevelData = _levelsData.Data[id];
            }

            for (int i = 0; i < _currentSelectedLevelData.BlocksDatas.GetLength(0); i++)
            {
                for (int j = 0; j < _currentSelectedLevelData.BlocksDatas.GetLength(1); j++)
                {
                    var colorKvp = _blockTypesByColors
                        .FirstOrDefault(x => x.Value == _currentSelectedLevelData.BlocksDatas[i, j]); //preserve that values are unique

                    if (colorKvp.Value!=BlockType.NONE)
                    {
                        var posX = Mathf.Abs(i);
                        var posY = Mathf.Abs(j-(GRID_SIZE - 1));
                        _colors[posX, posY] = colorKvp.Key;
                    }
                }
            }
        }

        private void CorrectLevelData()
        {
            if (_currentSelectedLevelData.BlocksDatas.Length!=0)
            {
                var maxX = _currentSelectedLevelData.BlocksDatas.GetLength(0);
                var maxY = _currentSelectedLevelData.BlocksDatas.GetLength(1);
            
                _currentSelectedLevelData.AddData(maxX, maxY-1, BlockType.NONE); //add right side 1 cell
            }
        }
        
        private void SaveLevelsData()
        {
            CorrectLevelData();
            string dataJson = JsonUtility.ToJson(_levelsData);
            System.IO.File.WriteAllText(Application.dataPath+LEVELS_DATA_PATH, dataJson);
            
            AssetDatabase.Refresh();
        }
    }
}


    
