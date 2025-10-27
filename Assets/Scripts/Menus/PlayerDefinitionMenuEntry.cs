using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

namespace CentaursBoardGame
{
	public class PlayerDefinitionMenuEntry : MonoBehaviour
	{
		[SerializeField] private GameConfig _config;
		[SerializeField] private Image _colorImage;
		[SerializeField] private TMP_InputField _nameInputField;
		[SerializeField] private TMP_Dropdown _colorSelectionDropdown;
 		
		public string Name
		{
			get => _nameInputField.text;
			set => _nameInputField.text = value;
        }

		public event Action<PlayerDefinitionMenuEntry, PlayerColor, PlayerColor>? ColorChanged;
		public event Action<PlayerDefinitionMenuEntry>? OnRemoveRequested;

        private PlayerColor _playerColor;
		public PlayerColor Color
		{
			get => _playerColor;
			set
			{
				var oldColor = _playerColor;
				_playerColor = value;
				_colorImage.color = _config.PlayerColorToColor(value);
				Debug.Log(_playerColor);
				ColorChanged?.Invoke(this, oldColor, _playerColor);
            }
        }

        public void SwitchColor(int color)
		{
            Color = (PlayerColor) _colorSelectionDropdown.value;
        }

		public void RequestRemove() => OnRemoveRequested?.Invoke(this);

        public Player GetPlayer()
		{
			Debug.Log(Color);
			return new Player(Name, Color);
        }

	}
}