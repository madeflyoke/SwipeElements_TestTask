using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Blocks.Enums;
using Gameplay.Levels.Data;
using Gameplay.Levels.Enums;
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
            {Color.clear, BlockType.NONE},
            {Color.red, BlockType.FIRE},
            {Color.blue, BlockType.WATER},
        };

        private Color[,]_colors = new Color[GRID_SIZE, GRID_SIZE];
        private Color _selectedColor = Color.red;
        
        private BlockType _selectedBlockType;
        private LevelData _selectedLevelData;
        private SectionDataContainer _selectedSectionDataContainer;
        
        private List<SectionDataContainer> _allLevelsSectionsDataContainers;

        private LevelSection _selectedLevelSection;
        private int _selectedLevelId;

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
            EditorGUI.DrawRect(rect, _selectedColor);
            
            EditorGUILayout.BeginHorizontal();

            _selectedBlockType = (BlockType)EditorGUILayout.EnumPopup("Block Type", _selectedBlockType, GUILayout.Width(250),
                GUILayout.Height(25));
            _selectedColor =_blockTypesByColors.FirstOrDefault(x => x.Value == _selectedBlockType).Key;
            
            SelectSection((LevelSection)EditorGUILayout.EnumPopup("Level Section", _selectedLevelSection, GUILayout.Width(250),
            GUILayout.Height(25)));

            if (GUILayout.Button("AddLevel", GUILayout.Width(150), GUILayout.Height(25)))
            {
                _selectedSectionDataContainer.AddNextLevelData_Editor();
                SelectLevel(_selectedSectionDataContainer.Data.Count-1);
            }

            if (_selectedSectionDataContainer.Data!=null)
            {
                for (var index = 0; index < _selectedSectionDataContainer.Data.Count; index++)
                {
                    var gridData = _selectedSectionDataContainer.Data[index];
                    GUI.backgroundColor = _selectedLevelId == index ? Color.green : Color.gray;
                    
                    if (GUILayout.Button(gridData.LevelId.ToString(), GUILayout.Height(25), GUILayout.Width(25)))
                    {
                        SelectLevel(gridData.LevelId);
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("ClearAll"))
            {
                ResetGrid();
                _selectedLevelData?.ClearData_Editor();
            }

            if (_selectedLevelData!=null)
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
                        _colors[xPos, yPos] = _selectedColor;
                        _selectedLevelData.AddData_Editor(xPos,normalizedYPos, _blockTypesByColors[_selectedColor]);
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

        private void SelectSection(LevelSection section)
        {
            if (_selectedLevelSection!=section)
            {
                _selectedLevelSection = section;
                _selectedSectionDataContainer =
                    _allLevelsSectionsDataContainers.FirstOrDefault(x => x.Section == section);
                SelectLevel(0);
            }
        }
        
        private void SelectLevel(int id)
        {
            ResetGrid();

            if (_selectedSectionDataContainer.Data==null ||
                _selectedSectionDataContainer.Data.Count==0 )
            {
                return;
            }
            
            _selectedLevelData = _selectedSectionDataContainer.Data[id];

            for (int i = 0; i < _selectedLevelData.GridWidth; i++)
            {
                for (int j = 0; j < _selectedLevelData.GridHeight; j++)
                {
                    var colorKvp = _blockTypesByColors
                        .FirstOrDefault(x => x.Value == _selectedLevelData.BlocksData[i, j]); //preserve that values are unique

                    var posX = Mathf.Abs(i);
                    var posY = Mathf.Abs(j-(GRID_SIZE - 1));
                    _colors[posX, posY] = colorKvp.Key;
                }
            }

            _selectedLevelId = id;
        }

        private void CorrectLevelData() //make grid corrected to fit game conditions
        {
            if (_selectedLevelData.BlocksData.Length!=0)
            {
                var maxX = _selectedLevelData.GridWidth;
                var maxY = _selectedLevelData.GridHeight;

                var lastColumnEmpty = true;
                
                for (int yCoord = 0; yCoord < _selectedLevelData.GridHeight; yCoord++)
                {
                    if (_selectedLevelData.BlocksData[_selectedLevelData.GridWidth-1, yCoord]!=BlockType.NONE)
                    {
                        lastColumnEmpty = false;
                        break;
                    }
                }

                if (lastColumnEmpty==false)
                {
                    _selectedLevelData.AddData_Editor(maxX, maxY-1, BlockType.NONE); //add right side 1 cell
                }
            }
        }
        
        private void LoadLevelsData()
        {
            _allLevelsSectionsDataContainers = new List<SectionDataContainer>();

            foreach (var value in Enum.GetNames(typeof(LevelSection)).Skip(1))
            {
                var targetName = StringHelpers.BuildDataNameByType<SectionDataContainer>(value);
                
                var levelsSectionDataContainer = EditorResourcesManager.LoadJsonDataAsset<SectionDataContainer>(targetName);
                if (levelsSectionDataContainer==null)
                {
                    levelsSectionDataContainer = new SectionDataContainer
                    {
                        Section = Enum.Parse<LevelSection>(value)
                    };
                    Debug.LogWarning("<color=green>Its expected, creating new json data...</color>");
                    var json =JsonConvert.SerializeObject(levelsSectionDataContainer);
                    EditorResourcesManager.CreateJson(targetName,json);
                }
                _allLevelsSectionsDataContainers.Add(levelsSectionDataContainer);
            }

            if (_allLevelsSectionsDataContainers!=null && _allLevelsSectionsDataContainers.Count>0 
                                                       && _allLevelsSectionsDataContainers[0].Data!=null)
            {
                SelectSection((LevelSection)1);
                SelectLevel(0);
            }
        }
        
        private void SaveLevelsData()
        {
            CorrectLevelData();
            var targetName = StringHelpers.BuildDataNameByType<SectionDataContainer>(
                _selectedSectionDataContainer.Section.ToString());
            
            string dataJson = JsonConvert.SerializeObject(_selectedSectionDataContainer);
            EditorResourcesManager.CreateJson(targetName, dataJson);
        }

        private void OnDisable()
        {
            Resources.UnloadUnusedAssets();
        }
    }
}


    
