using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

namespace CentaursBoardGame
{
	public class PlayerDefinitionMenu : MonoBehaviour
	{
		[SerializeField] private GridLayoutGroup _layoutGroup;
		[SerializeField] private PlayerDefinitionMenuEntry _prefab;
		[SerializeField] private Button _addEntryButton;
		[SerializeField] private Player _emptyPlayer;

		private List<PlayerDefinitionMenuEntry> _entries = new List<PlayerDefinitionMenuEntry>();

		public void Initialize(IEnumerable<Player> players)
		{
			for (int i = _entries.Count - 1; i >= 0; i--)
			{
				var entry = _entries[i];
				ForceRemoveEntry(entry);
			}

			foreach (var player in players)
			{
				AddEntry(player);
			}
		}

		private void OnEnable()
		{
			foreach (var entry in _entries)
			{
				SubToEvents(entry);
            }
		}

		private void OnDisable()
		{
			foreach (var entry in _entries)
			{
				UnsubFromEvents(entry);
            }
		}

		public void AddEntry(Player player)
		{
			if (_entries.Count >= 4)
			{
				return;
			}

			var entry = Instantiate(_prefab, _layoutGroup.transform);
			entry.transform.SetSiblingIndex(Mathf.Max(0, _layoutGroup.transform.childCount - 2));
			entry.Color = player.Color;
			entry.Name = player.Name;
            SubToEvents(entry);
            _entries.Add(entry);

			if (_entries.Count >= 4)
			{
				_addEntryButton.gameObject.SetActive(false);
			}
		}

		public void AddEmptyEntry()
			=> AddEntry(new Player(_emptyPlayer.Name, GetNextAvailableColor()));

		public void RemoveEntry(PlayerDefinitionMenuEntry playerDefinitionMenuEntry)
		{
			if (_entries.Count <= 2)
			{
				return;
			}

			ForceRemoveEntry(playerDefinitionMenuEntry);
			_addEntryButton.gameObject.SetActive(true);
		}

		private void ForceRemoveEntry(PlayerDefinitionMenuEntry entry)
		{
			UnsubFromEvents(entry);
			Destroy(entry.gameObject);
			_entries.Remove(entry);
		}

		public IEnumerable<Player> GetPlayers()
		{
			foreach (var entry in _entries)
			{
				yield return entry.GetPlayer();
			}
		}

		private PlayerColor GetNextAvailableColor()
		{
			foreach (var value in GameFacts.PlayerColors)
			{
				var taken = false;

				foreach (var entry in _entries)
				{
					if (entry.Color == value)
					{
						taken = true;
						break;
					}
                }

				if (!taken)
				{
                    return value;
                }
            }
			throw new System.Exception("No free color, should not be possible");
		}

		private void SwapColors(PlayerDefinitionMenuEntry entry, PlayerColor oldColor, PlayerColor newColor)
		{
			foreach (var e in _entries)
			{
				if (e != entry && e.Color == newColor)
				{
					e.Color = oldColor;
					return;
				}
            }
        }

		private void SubToEvents(PlayerDefinitionMenuEntry entry)
		{
            entry.OnRemoveRequested += RemoveEntry;
            entry.ColorChanged += SwapColors;
        }

		private void UnsubFromEvents(PlayerDefinitionMenuEntry entry)
		{
			entry.OnRemoveRequested -= RemoveEntry;
			entry.ColorChanged -= SwapColors;
        }
	}
}