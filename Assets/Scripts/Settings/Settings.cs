using UnityEngine;

public class Settings : MonoBehaviour
{
    [SerializeField]
    private GameSettings _gameSettings;
    public GameSettings GameSettings => _gameSettings;

    [SerializeField]
    private MenuSettings _menuSettings;
    public MenuSettings MenuSettings => _menuSettings;

    [SerializeField]
    private PlayerSettings _playerSettings;
    public PlayerSettings PlayerSettings => _playerSettings;

    [SerializeField]
    private WorldGenerationSettings _worldGenerationSettings;
    public WorldGenerationSettings WorldGenerationSettings => _worldGenerationSettings;

    [SerializeField]
    private CloudsSettings _cloudsSettings;
    public CloudsSettings CloudsSettings => _cloudsSettings;
}

