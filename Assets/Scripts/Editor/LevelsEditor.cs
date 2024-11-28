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
    //Very-very raw
    public partial class LevelsEditor : EditorWindow
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
        
        private Vector2 _currentScrollPosition;
        
        private BlockType _selectedBlockType;
        private LevelData _selectedLevelData;
        private SectionDataContainer _selectedSectionDataContainer;
        
        private List<SectionDataContainer> _allLevelsSectionsDataContainers;

        private LevelSection _selectedLevelSection;
        private int _selectedLevelId;

        private LevelData _copiedLevelData;

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
            
            if (GUILayout.Button("RemoveCurrentLevel", GUILayout.Width(150), GUILayout.Height(25)))
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
                        _editor_expandedBlocksData = null;
                    }
                }
            }
            
            if (GUILayout.Button("CopyLevel", GUILayout.Width(150), GUILayout.Height(25)))
            {
                _copiedLevelData = _selectedLevelData;
            }
            
            if (GUILayout.Button("PasteLevel", GUILayout.Width(150), GUILayout.Height(25)))
            {
                AddData_Editor(_copiedLevelData.BlocksData);
                SelectLevel(_selectedLevelId);
            }
            
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
            
            if (GUILayout.Button("ClearAll"))
            {
                ResetGrid();
                ClearData_Editor();
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
            var gridSize = GRID_SIZE - 1;
            for (var yPos = 0; yPos <= gridSize; yPos++)
            {
                EditorGUILayout.BeginHorizontal(); //buttons draw from left-up point, while actual data start will be left-bottom point
                GUILayout.FlexibleSpace();

                for (var xPos = 1; xPos < gridSize; xPos++)
                {
                    GUI.backgroundColor = _colors[xPos, yPos];

                    var normalizedYPos = Mathf.Abs(yPos-gridSize);
                
                    if (GUILayout.Button($"{xPos}:{normalizedYPos}", GUILayout.MaxWidth(45), GUILayout.MaxHeight(45)))
                    {
                        _colors[xPos, yPos] = _selectedColor;
                        AddData_Editor(xPos,normalizedYPos, _blockTypesByColors[_selectedColor]);
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
            UpdateExpandedEditorBlocksData();

            for (var i = 0; i < _selectedLevelData.GridWidth; i++)
            {
                for (var j = 0; j < _selectedLevelData.GridHeight; j++)
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
                var yOffset = 0 - FindLowestCellY();
                var xOffset = 1 - FindLowestCellX(); //1 because visible grid starts from (1,1) coords 

                ShiftBlocks_Editor(xOffset,yOffset);
                ShrinkData_Editor();
                
                var lastColumnEmpty = true;
                
                for (var yCoord = 0; yCoord < _selectedLevelData.GridHeight; yCoord++)
                {
                    if (_selectedLevelData.BlocksData[_selectedLevelData.GridWidth-1, yCoord]!=BlockType.NONE)
                    {
                        lastColumnEmpty = false;
                        break;
                    }
                }

                if (lastColumnEmpty==false)
                {
                    AddData_Editor(_selectedLevelData.GridWidth, _selectedLevelData.GridWidth-1, BlockType.NONE); //add right side 1 cell
                }
                SelectLevel(_selectedLevelId);
            }
        }
        
        private int FindLowestCellY()
        {
            var blocks = _selectedLevelData.BlocksData;
            var lowestBlockY = -1;
            
            blocks.ForEach((e,x,y) =>
            {
                if (e!=BlockType.NONE)
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
            var blocks = _selectedLevelData.BlocksData;
            var lowestBlockX = -1;
            
            blocks.ForEach((e,x,y) =>
            {
                if (e!=BlockType.NONE)
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
            CorrectLevelData();
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
        //use list in editor for easily expand-shrink and then assign it to 2d array
        private BlockType[,] _editor_expandedBlocksData;
        
        private void AddData_Editor(int xCoord, int yCoord, BlockType type)
        {
            int GridWidth() => _editor_expandedBlocksData.GetLength(0);
            int GridHeight() => _editor_expandedBlocksData.GetLength(1);
            
            BlockType[,] newArray = null;
            
            void CopyData()
            {
                _editor_expandedBlocksData.ForEach((x, y) =>
                {
                    newArray[x, y] = _editor_expandedBlocksData[x, y];
                });
                
                _editor_expandedBlocksData = newArray;
            }
            
            if (xCoord >= GridWidth()) //flexible expand
            {
                newArray = new BlockType[xCoord + 1, GridHeight()];
                CopyData();
            }

            if (yCoord >= GridHeight())
            {
                newArray = new BlockType[GridWidth(), yCoord + 1];
                CopyData();
            }

            _editor_expandedBlocksData[xCoord, yCoord] = type;
            _selectedLevelData.SaveDataFromEditor(_editor_expandedBlocksData);
        }

        private void AddData_Editor(BlockType[,] blockData)
        {
            _editor_expandedBlocksData = blockData;
            _selectedLevelData.SaveDataFromEditor(blockData);
        }

        private void ShiftBlocks_Editor(int shiftX, int shiftY)
        {
            var newBlocksData = new BlockType[_selectedLevelData.GridWidth, _selectedLevelData.GridHeight];

            for (var x = 0; x < _selectedLevelData.GridWidth; x++)
            {
                for (var y = 0; y <  _selectedLevelData.GridHeight; y++)
                {
                    var newX = x + shiftX;
                    var newY = y + shiftY;

                    if (newX >= 0 && newX < _selectedLevelData.GridWidth && newY >= 0 && newY <  _selectedLevelData.GridHeight)
                    {
                        newBlocksData[newX, newY] = _selectedLevelData.BlocksData[x, y];
                    }
                }
            }
            _selectedLevelData.SaveDataFromEditor(newBlocksData);
        }
        
        private void ShrinkData_Editor()
        {
            var gridWidth = _editor_expandedBlocksData.GetLength(0);
            var gridHeight = _editor_expandedBlocksData.GetLength(1);

            var newWidth = 0;
            var newHeight = 0;

            _editor_expandedBlocksData.ForEach((e,x,y) =>
            {
                if (e != BlockType.NONE)
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
            
            var newArray = new BlockType[newWidth, newHeight];
            
            newArray.ForEach((e,x,y) =>
            {
                newArray[x, y] = _editor_expandedBlocksData[x, y];
            });
            
            _editor_expandedBlocksData = newArray;
            _selectedLevelData.SaveDataFromEditor(_editor_expandedBlocksData);
        }
        
        private void ClearData_Editor()
        {
            _editor_expandedBlocksData = new BlockType[0, 0];
            _selectedLevelData?.ClearDataFromEditor();
        }

        private void UpdateExpandedEditorBlocksData()
        {
            _editor_expandedBlocksData = _selectedLevelData.BlocksData;
        }
    }
}
#endif

    
