using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gameplay.Blocks.Enums;
using Gameplay.Levels.Data;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Utility;

namespace Editor
{
    public class LevelsEditor : EditorWindow
    {
        private const int GRID_SIZE = 15; //maximum size of comfort field (mobiles)
    
        private readonly Dictionary<Color, BlockType> _blockTypesByColors = new Dictionary<Color, BlockType>()
        {
            {Color.red, BlockType.FIRE},
            {Color.blue, BlockType.WATER},
        };
    
        private Color _currentColor = Color.red;
        private Color[,]_colors = new Color[GRID_SIZE, GRID_SIZE];
        private LevelData _currentSelectedLevelData;
        private LevelsDataContainer _levelsDataContainer;

        [MenuItem("Window/Levels Editor")]
        public static void ShowWindow()
        {
            GetWindow<LevelsEditor>("Levels Editor");
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
                _levelsDataContainer.AddNextLevelData();
                SelectLevel(_levelsDataContainer.Data.Count-1);
            }

            if (_levelsDataContainer.Data!=null)
            {
                foreach (var gridData in _levelsDataContainer.Data) //levels buttons
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
            for (int yPos = 0; yPos <= gridSize; yPos++)
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
            LevelsDataContainer _levelsDataContainer = EditorResourcesManager.LoadJsonDataAsset<LevelsDataContainer>();
            if (_levelsDataContainer==null)
            {
                _levelsDataContainer = new LevelsDataContainer();
                var json =JsonConvert.SerializeObject(_levelsDataContainer);
                EditorResourcesManager.CreateJson<LevelsDataContainer>(json);
            }
            this._levelsDataContainer = _levelsDataContainer;
            if (this._levelsDataContainer!=null && this._levelsDataContainer.Data!=null)
            {
                SelectLevel(0);
            }
        }

        private void SelectLevel(int id)
        {
            ResetGrid();
            if (_levelsDataContainer.Data.Count==0)
            {
                return;
               
            }
            _currentSelectedLevelData = _levelsDataContainer.Data[id];

            for (int i = 0; i < _currentSelectedLevelData.GridWidth; i++)
            {
                for (int j = 0; j < _currentSelectedLevelData.GridHeight; j++)
                {
                    var colorKvp = _blockTypesByColors
                        .FirstOrDefault(x => x.Value == _currentSelectedLevelData.BlocksData[i, j]); //preserve that values are unique

                    if (colorKvp.Value!=BlockType.NONE)
                    {
                        var posX = Mathf.Abs(i);
                        var posY = Mathf.Abs(j-(GRID_SIZE - 1));
                        _colors[posX, posY] = colorKvp.Key;
                    }
                }
            }
        }

        private void CorrectLevelData() //make grid corrected to fit game conditions
        {
            if (_currentSelectedLevelData.BlocksData.Length!=0)
            {
                var maxX = _currentSelectedLevelData.GridWidth;
                var maxY = _currentSelectedLevelData.GridHeight;
            
                _currentSelectedLevelData.AddData(maxX, maxY-1, BlockType.NONE); //add right side 1 cell
            }
        }
        
        private void SaveLevelsData()
        {
            CorrectLevelData();
            string dataJson = JsonConvert.SerializeObject(_levelsDataContainer);
            EditorResourcesManager.CreateJson<LevelsDataContainer>(dataJson);
        }

        private void OnDisable()
        {
            Resources.UnloadUnusedAssets();
        }
    }
}


    
