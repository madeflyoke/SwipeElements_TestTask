#if UNITY_EDITOR
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
    public partial class LevelsEditor : EditorWindow
    {
        private const int MAX_GRID_SIZE = 30; //i dont think will be more, especially on mobiles 
        
        private readonly Dictionary<Color, BlockType> _blockTypesByColors = new Dictionary<Color, BlockType>()
        {
            {Color.clear, BlockType.NONE},
            {Color.red, BlockType.FIRE},
            {Color.blue, BlockType.WATER},
        };

        private Color[,]_colors = new Color[MAX_GRID_SIZE, MAX_GRID_SIZE];
        private Color _selectedColor = Color.red;
        
        private Vector2 _currentScrollPosition;
        
        private BlockType _selectedBlockType;
        private LevelData _selectedLevelData;
        private SectionDataContainer _selectedSectionDataContainer;
        
        private List<SectionDataContainer> _allLevelsSectionsDataContainers;

        private LevelSection _selectedLevelSection;
        private int _selectedLevelId;

        private Color[,] _copiedLevelDataColors;

        private int _currentGridWidth =3;
        private int _currentGridHeight = 3;

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

            GUILayout.Label("Block Type", GUILayout.Width(100));
            _selectedBlockType = (BlockType)EditorGUILayout.EnumPopup(_selectedBlockType, GUILayout.Width(100),
                GUILayout.Height(25));
            _selectedColor =_blockTypesByColors.FirstOrDefault(x => x.Value == _selectedBlockType).Key;
            
            GUILayout.Label("Level Section", GUILayout.Width(100));
            SelectSection((LevelSection)EditorGUILayout.EnumPopup(_selectedLevelSection, GUILayout.Width(150),
            GUILayout.Height(25)));

            if (GUILayout.Button("+Level", GUILayout.Width(75), GUILayout.Height(25)))
            {
                _selectedSectionDataContainer.AddNextLevelData_Editor();
                SelectLevel(_selectedSectionDataContainer.Data.Count-1);
            }
            
            if (GUILayout.Button("-Level", GUILayout.Width(75), GUILayout.Height(25)))
            {
                if (_selectedLevelData!=null)
                {
                    _selectedSectionDataContainer.RemoveLevelData_Editor(_selectedLevelData);
                    if (_selectedSectionDataContainer.Data.Count>0)
                    {
                        SelectLevel(0);
                    }
                    else
                    {
                        ResetGrid();
                        _selectedLevelData = null;
                    }
                }
            }
            
            if (GUILayout.Button("Copy", GUILayout.Width(100), GUILayout.Height(25)))
            {
                _copiedLevelDataColors = _colors;
            }
            
            if (GUILayout.Button("Paste", GUILayout.Width(100), GUILayout.Height(25)))
            {
                if (_copiedLevelDataColors!=null)
                {
                    _currentGridWidth = _copiedLevelDataColors.GetLength(0);
                    _currentGridHeight = _copiedLevelDataColors.GetLength(1);
                    _colors = _copiedLevelDataColors;
                }
            }
            
            if (GUILayout.Button("Normalize", GUILayout.Width(100), GUILayout.Height(25)))
            {
                CorrectLevelDataColors();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("GridWidth", GUILayout.Width(100));
            _currentGridWidth =
                EditorGUILayout.IntField(Mathf.Clamp(_currentGridWidth, 3, MAX_GRID_SIZE), GUILayout.Width(50), GUILayout.Height(25));
            
            GUILayout.Label("GridHeight", GUILayout.Width(100));
            _currentGridHeight =
                EditorGUILayout.IntField(Mathf.Clamp(_currentGridHeight, 3, MAX_GRID_SIZE), GUILayout.Width(50), GUILayout.Height(25));
            
            EditorGUILayout.EndHorizontal();
            
            if (_selectedSectionDataContainer.Data!=null)
            {
                EditorGUILayout.BeginHorizontal();
                
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
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Clear"))
            {
                ResetGrid();
            }

            if (_selectedLevelData!=null)
            {
                DrawGrid();
            }
            
            if (GUILayout.Button("Save Levels Data (click it every time, all changes that was made without click is visual only"))
            {
                SaveLevelsData();
            }
        }

        private void DrawGrid()
        {
            if (_colors.GetLength(0)<_currentGridWidth || _colors.GetLength(1)<_currentGridHeight)
            {
                ExpandColorsData_Editor();
            }

            for (var yPos = _currentGridHeight-1; yPos >=0; yPos--)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (var xPos =0; xPos < _currentGridWidth; xPos++)
                {
                    var blockedButton = xPos == 0 || xPos == _currentGridWidth - 1;
                    
                    GUI.backgroundColor = blockedButton ? Color.black : _colors[xPos, yPos];
                    
                    if (GUILayout.Button($"{xPos}:{yPos}", GUILayout.MaxWidth(45), GUILayout.MaxHeight(45)))
                    {
                        if (blockedButton)
                        {
                            continue;
                        }
                        _colors[xPos, yPos] = _selectedColor;
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            GUI.backgroundColor = Color.white;
        }

        private void ResetGrid()
        {
            _colors = new Color[_currentGridWidth, _currentGridHeight];
            _colors.ForEach((e, x, y) =>
            {
                _colors[x, y] = Color.clear;
            });
        }

        private void SelectSection(LevelSection section)
        {
            if (_selectedLevelSection!=section)
            {
                _selectedLevelSection = section;
                _selectedSectionDataContainer =
                    _allLevelsSectionsDataContainers.FirstOrDefault(x => x.Section == section);
                ResetGrid();
                SelectLevel(0);
            }
        }
        
        private void SelectLevel(int id)
        {
            if (_selectedSectionDataContainer.Data==null ||
                _selectedSectionDataContainer.Data.Count==0 )
            {
                return;
            }
            
            _selectedLevelData = _selectedSectionDataContainer.Data[id];
            _currentGridWidth = _selectedLevelData.GridWidth;
            _currentGridHeight = _selectedLevelData.GridHeight;
            
            ResetGrid();
           
            for (var i = 0; i < _selectedLevelData.GridWidth; i++)
            {
                for (var j = 0; j < _selectedLevelData.GridHeight; j++)
                {
                    var colorKvp = _blockTypesByColors
                        .FirstOrDefault(x => x.Value == _selectedLevelData.BlocksData[i, j]); //preserve that values are unique

                    var posX = Mathf.Abs(i);
                    var posY = Mathf.Abs(j);
                    _colors[posX, posY] = colorKvp.Key;
                }
            }

            _selectedLevelId = id;
        }

        private void CorrectLevelDataColors() //make grid corrected to fit game conditions
        {
            var yOffset = 0 - FindLowestCellY();
            var xOffset = 1 - FindLowestCellX(); //1 because visible grid starts from (1,1) coords 

            ShiftBlocks_Editor(xOffset,yOffset);
            ShrinkData_Editor();
        }
        
        private int FindLowestCellY()
        {
            var blocksColors = _colors;
            var lowestBlockY = -1;
            
            blocksColors.ForEach((e,x,y) =>
            {
                if (e!=Color.clear)
                {
                    if (lowestBlockY==-1)
                    {
                        lowestBlockY = y;
                    }
                    else if (y<lowestBlockY)
                    { 
                        lowestBlockY = y;
                    }
                    
                }
            });
            
            return lowestBlockY;
        }
        
        private int FindLowestCellX()
        {
            var blocksColors = _colors;
            var lowestBlockX = -1;
            
            blocksColors.ForEach((e,x,y) =>
            {
                if (e!= Color.clear)
                {
                    if (lowestBlockX==-1)
                    {
                        lowestBlockX = x;
                    }
                    else if (x<lowestBlockX)
                    { 
                        lowestBlockX = x;
                    }
                }
            });
            
            return lowestBlockX;
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
            BlockType[,] newBlockData = new BlockType[_currentGridWidth, _currentGridHeight];
            
            _colors.ForEach((e,x,y) =>
            {
                newBlockData[x, y] = _blockTypesByColors[e];
            });
            
            SetBlocksData_Editor(newBlockData);
            SelectLevel(_selectedLevelId);
            var targetName = StringHelpers.BuildDataNameByType<SectionDataContainer>(
                _selectedSectionDataContainer.Section.ToString());
            
            var dataJson = JsonConvert.SerializeObject(_selectedSectionDataContainer);
            EditorResourcesManager.CreateJson(targetName, dataJson);
        }

        private void OnDisable()
        {
            Resources.UnloadUnusedAssets();
        }
    }

    /// <summary>
    /// Editor level data
    /// </summary>
    public partial class LevelsEditor
    {
        private void SetBlocksData_Editor(BlockType[,] blockData)
        {
            _selectedLevelData.SaveDataFromEditor(blockData);
        }

        private void ShiftBlocks_Editor(int shiftX, int shiftY)
        {
            var newBlocksColorsData = new Color[_currentGridWidth, _currentGridHeight];

            for (var x = 0; x < _currentGridWidth; x++)
            {
                for (var y = 0; y <  _currentGridHeight; y++)
                {
                    var newX = x + shiftX;
                    var newY = y + shiftY;

                    if (newX >= 0 && newX < _currentGridWidth && newY >= 0 && newY <  _currentGridHeight)
                    {
                        newBlocksColorsData[newX, newY] = _colors[x, y];
                    }
                }
            }

            _colors = newBlocksColorsData;
        }
        
        private void ShrinkData_Editor()
        {
            var gridWidth = _colors.GetLength(0);
            var gridHeight = _colors.GetLength(1);

            var newWidth = 0;
            var newHeight = 0;

            _colors.ForEach((e,x,y) =>
            {
                if (e != Color.clear)
                {
                    if (x + 1 > newWidth)
                    {
                        newWidth = x + 1;
                    }
                    if (y + 1 > newHeight)
                    {
                        newHeight = y + 1;
                    }
                }
            });
            
            if (newWidth == gridWidth && newHeight == gridHeight)
            {
                return;
            }
            
            var newArray = new Color[newWidth+1, newHeight];
            
            newArray.ForEach((e,x,y) =>
            {
                newArray[x, y] = _colors[x, y];
            });

            _currentGridWidth = newArray.GetLength(0); //right empty cell
            _currentGridHeight = newArray.GetLength(1);
            _colors = newArray;
        }
        
        private void ExpandColorsData_Editor()
        {
            var newArray = new Color[_currentGridWidth, _currentGridHeight];
            
            _colors.ForEach((e,x,y) =>
            {
                newArray[x, y] = e;
            });
            _colors = newArray;
        }
    }
}
#endif

    
