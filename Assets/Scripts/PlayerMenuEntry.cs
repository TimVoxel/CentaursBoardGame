using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

namespace CentaursBoardGame
{
	[CreateAssetMenu(menuName = "CentaursBoardGame/Game Configuration")]
	public class GameConfig : ScriptableObject
	{
		//TODO: replace scriptableobject with file reading and parsing 

		[SerializeField] private Color _redPlayerColor = Color.red;
		[SerializeField] private Color _yellowPlayerColor = Color.yellow;
		[SerializeField] private Color _greenPlayerColor = Color.green;
		[SerializeField] private Color _bluePlayerColor = Color.blue;

		public Color RedPlayerColor => _redPlayerColor;
		public Color YellowPlayerColor => _yellowPlayerColor;
		public Color GreenPlayerColor => _greenPlayerColor;
		public Color BluePlayerColor => _bluePlayerColor;
	}

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
}