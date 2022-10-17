using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class DebugScreen : MonoBehaviour
{

    [SerializeField]
    private Player _player;
    private Text text;

    private float _frameRate;
    private float _timer;

    private int _halfWorldSizeInVoxels;
    private int _halfWorldSizeInChunks;

    void Start()
    {

        _player = GameObject.Find("Player").GetComponent<Player>();
        text = GetComponent<Text>();

        _halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;
        _halfWorldSizeInChunks = VoxelData.WorldSizeInChunks / 2;

    }

    void Update()
    {

        string debugText = "b3agz' Code a Game Like Minecraft in Unity";
        debugText += "\n";
        debugText += _frameRate + " fps";
        debugText += "\n\n";
        debugText += "XYZ: " + (Mathf.FloorToInt(_player.transform.position.x)) 
            + " / " + Mathf.FloorToInt(_player.transform.position.y) 
            + " / " + (Mathf.FloorToInt(_player.transform.position.z));
        debugText += "\n";
        debugText += "Chunk: " + (_player.PlayerChunkCoord.X) + " / " + (_player.PlayerChunkCoord.Z);
        debugText += "\n";
        debugText += "PlaceBlock XYZ: " + (Mathf.FloorToInt(_player.PlaceBlock.position.x))
            + " / " + Mathf.FloorToInt(_player.PlaceBlock.position.y)
            + " / " + (Mathf.FloorToInt(_player.PlaceBlock.position.z));
        debugText += "\n";
        debugText += "HighlightBlock XYZ: " + (Mathf.FloorToInt(_player.HighlightBlock.position.x))
            + " / " + Mathf.FloorToInt(_player.HighlightBlock.position.y)
            + " / " + (Mathf.FloorToInt(_player.HighlightBlock.position.z));

        text.text = debugText;

        if (_timer > 1f)
        {

            _frameRate = (int)(1f / Time.unscaledDeltaTime);
            _timer = 0;

        }
        else
            _timer += Time.deltaTime;

    }
}
