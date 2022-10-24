using System;
using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Settings _settings;
    private Player _player;
    private World _world;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private void Start()
    {
        Debug.Log($"Start Game, time since start: {Time.realtimeSinceStartup}s");

        VoxelData.seed = new System.Random().Next(0, 100);
        VoxelData.seed = 75;
        Debug.Log($"Seed: {VoxelData.seed}");


        _settings = GameObject.Find("GameManager").GetComponent<Settings>();
        _player = GameObject.Find("Player").GetComponent<Player>();

        WorldGenerator worldGenerator = new(_settings.WorldGenerationSettings);
        ChunkVoxelMapRepository repo = new(worldGenerator);

        ChunkVoxelMap.GameSettings = _settings.GameSettings;

        Transform worldTransform = GameObject.Find("World").GetComponent<Transform>();
        _world = new World(
            _settings.MenuSettings.ViewDistance
            , _settings.MenuSettings.LoadDistance
            , repo
            , _settings.GameSettings
            , worldTransform);
        _world.WorldIsReadyEvent += GameWorld_IsReady;
        _world.Start();

        _player.PlayerMovedChunks += _world.Player_Moved;
        _player.LoadedWorld = new WorldPlayerInteractons(_world);
        _player.Settings = _settings.PlayerSettings;
        _player.MenuSettings = _settings.MenuSettings;
    }
    private void Update()
    {
        _world.PrintRenderedChunkToScreen();
        if (!_world.WorldIsReady)
        {
            return;
        }
    }

    public void GameWorld_IsReady(object sender, EventArgs args)
    {


        _player.SetActive(true);
        Debug.Log($"World is Ready, time since start: {Time.realtimeSinceStartup}s");
    }



    private void OnDestroy()
    {
        _cancellationTokenSource.Cancel();
    }
}

