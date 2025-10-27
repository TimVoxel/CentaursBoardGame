using CentaursBoardGame;
using System.Collections.Immutable;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable enable

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameConfig _config;
    [SerializeField] private PlayerDefinitionMenu _playerDefinitionMenu;
    [SerializeField] private GameObject _continueButton;
    [SerializeField] private string _mainScene;

    [Space(20)]
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _newGamePanel;
    [SerializeField] private GameObject _settingsPanel;

    [Space(20)]
    [SerializeField] private Player[] _placeholderPlayers;

    private GameObject? _activePanel;
    
    private void Awake()
    {
        var context = GameContext.Load();
        _continueButton.SetActive(context != null);
       
        _playerDefinitionMenu.Initialize(context != null
            ? context.Players
            : _placeholderPlayers);

        ShowPanel(_mainPanel);

        Debug.Log("AWDKOWAKDLAWKDL");
        Debug.Log(_mainPanel.name);
    }

    public void ShowPanel(GameObject panel)
    {
        _activePanel?.SetActive(false);
        _activePanel = panel;
        _activePanel.SetActive(true);
    }

    public void ContinueExistingGame()
    {
        SceneManager.LoadScene(_mainScene);
    }

    public void StartGame()
    {
        var players = _playerDefinitionMenu.GetPlayers().ToImmutableArray();
        var context = new GameContext(players, _config.BoardSectorCount);
        StartGame(context);
    }

    private void StartGame(GameContext context)
    {
        context.Save();
        SceneManager.LoadScene(_mainScene);
    }
}