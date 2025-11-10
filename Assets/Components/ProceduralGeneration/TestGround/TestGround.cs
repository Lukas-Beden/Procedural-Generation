using Cysharp.Threading.Tasks;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Ground")]
    public class TestGround : ProceduralGenerationMethod
    {
        [Range(0f, 1f)]
        [SerializeField] private float _noiseDensity = 0.5f;

        [SerializeField] List<bool> _noiseValue;

        [SerializeField] GameObject _prefabGrass;
        [SerializeField] float _grassY = 0.5f;
        [SerializeField] float _offsetX = -0.5f;
        [SerializeField] float _offsetZ = -0.5f;

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();

            BuildGround();
            CreateNoiseObject();
            InstantiateObject();

            // Waiting between steps to see the result.
            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
        }

        private void BuildGround()
        {

            // Instantiate ground blocks
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell, out var _))
                    {
                        continue;
                    }
                    AddTileToCell(chosenCell, GRASS_TILE_NAME, false, true);
                }
            }
        }

        private void CreateNoiseObject()
        {
            _noiseValue.Clear();
            for (int i = 0; i < Grid.Width; i++)
            {
                for (int j = 0; j < Grid.Lenght; j++)
                {
                    _noiseValue.Add(RandomService.Chance(_noiseDensity));
                }
            }
        }

        private void InstantiateObject()
        {
            for (int i = 0; i < Grid.Width; i++)
            {
                for (int j = 0; j < Grid.Lenght; j++)
                {
                    if (Grid.TryGetCellByCoordinates(i, j, out var chosenCell, out var _))
                    {
                        if (_noiseValue[i * Grid.Width + j] == true)
                        {
                            Instantiate(_prefabGrass, new Vector3(chosenCell.Coordinates.x + _offsetX, _grassY, chosenCell.Coordinates.y + _offsetZ), Quaternion.identity);
                        }
                    }
                }
            }
        }
    }
}
