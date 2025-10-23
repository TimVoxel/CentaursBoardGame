using TMPro;
using UnityEngine;

#nullable enable

namespace CentaursBoardGame
{
    public class PlayerLabel : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _text;
		[SerializeField] private string? _prefix;
		[SerializeField] private string? _suffix;

		public void DisplayPlayer(Player player)
		{
			_text.text = $"{_prefix ?? string.Empty}{player.Name}{_suffix ?? string.Empty}";
		}

		public void ResetText()
		{
			_text.text = $"{_prefix ?? string.Empty}{_suffix ?? string.Empty}";
		}
	}
}