using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Settings _settings;
    private IPlayer _player;
    private GameWorld _world;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private void Start()
    {     
        _settings = GameObject.Find("GameManager").GetComponent<Settings>();
        _player = GameObject.Find("Player").GetComponent<Player>();

        WorldGenerator worldGenerator = new(_settings.WorldGenerationSettings);
        ChunkVoxelMapRepository repo = new ChunkVoxelMapRepository(worldGenerator);

        ChunkVoxelMap.GameSettings = _settings.GameSettings;

        var worldTransform = GameObject.Find("World").GetComponent<Transform>();
        _world = new GameWorld(
            _settings.MenuSettings.ViewDistance 
            ,_settings.MenuSettings.LoadDistance
            ,repo
            ,_settings.GameSettings
            , worldTransform);
        _world.Start();
        _player.PlayerMovedChunks += _world.Player_Moved;

    }
    private void Update()
    {
        _world.Update();
    }

    private void OnDestroy()
    {
        _cancellationTokenSource.Cancel();
    }
}

