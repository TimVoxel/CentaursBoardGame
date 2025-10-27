using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenuEntry : MonoBehaviour
{
	[SerializeField] private GameConfig _config;
	[SerializeField] private Image _colorImage;
	[SerializeField] private TextMeshProUGUI _nameText;

	private Player? _player;

	public Player Player => _player ?? throw new System.Exception($"Player menu entry does not have a bound player");
	public event Action<Player>? OnClicked;

	public void OnClick()
	{
		OnClicked?.Invoke(Player);
	}

	public void BindPlayer(Player player)
	{
		_player = player;

		_colorImage.color = player.Color switch
		{
			PlayerColor.Red => _config.RedPlayerColor,
			PlayerColor.Yellow => _config.YellowPlayerColor,
			PlayerColor.Green => _config.GreenPlayerColor,
			PlayerColor.Blue => _config.BluePlayerColor,
			_ => throw new Exception($"Unexpected player color: {player.Color}"),
		};
		_nameText.text = player.Name;
	}
}