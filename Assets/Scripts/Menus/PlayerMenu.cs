using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable enable

namespace CentaursBoardGame
{
    public class PlayerMenu : MonoBehaviour
	{
		[SerializeField] private PlayerMenuEntry _prefab;
		[SerializeField] private GridLayoutGroup _layoutGroup;
		[SerializeField] private Button _commitButton;
		
        [SerializeField] private UnityEvent<Player>? _onPlayerSelected;
		[SerializeField] private UnityEvent? _onShow;
		[SerializeField] private UnityEvent? _onHide;

        private PlayerMenuEntry?[] _entries = new PlayerMenuEntry[GameFacts.MaxPlayers];
		private bool _isShown;

		private Player? _selectedPlayer;
		private Player? SelectedPlayer
		{
			get => _selectedPlayer;
			set
			{
				_commitButton.enabled = value != null;
				_selectedPlayer = value;
			}
		}
	
		public event Action<Player>? OnCommitedPlayer;

        private void OnEnable()
        {
            foreach (var entry in _entries)
			{
				if (entry != null)
				{
                    entry.OnClicked += SelectEntry;
                }
			}
        }

        private void OnDisable()
        {
            foreach (var entry in _entries)
            {
                if (entry != null)
                {
                    entry.OnClicked -= SelectEntry;
                }
            }
        }

        public void BuildFromPlayers(IList<Player> players)
		{
			var parentTransform = _layoutGroup.transform;

			for (var i = 0; i < _entries.Length; i++)
			{
				var entry = _entries[i];

				if (entry != null)
                {
                    DeleteEntry(i);
                }
            }

			_entries = new PlayerMenuEntry[players.Count];

            for (var i = 0; i < players.Count; i++)
			{
				var entry = Instantiate(_prefab, parentTransform);
				var player = players[i];
				entry.BindPlayer(player);
				entry.OnClicked += SelectEntry;
				_entries[i] = entry;
			}
		}

        private void DeleteEntry(int i)
        {
			var entry = _entries[i];

			if (entry == null)
			{
				throw new Exception("Trying to delete null entry");
			}

            entry.OnClicked -= SelectEntry;
            _entries[i] = null;
            Destroy(entry.gameObject);
        }

        public void Show()
		{
			SelectedPlayer = null;
			_isShown = true;
            _onShow?.Invoke();
        }

		public void Hide()
		{
			SelectedPlayer = null;
			_isShown = false;
			_onHide?.Invoke();
        }

		public void Toggle()
		{
            if (!_isShown)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

		public void Commit()
		{
			OnCommitedPlayer?.Invoke(SelectedPlayer ?? throw new Exception("No player selected, button should not be clickable"));
		}

		public void RemoveEntry(Player player)
		{
			for (int i = 0; i < _entries.Length; i++)
			{
				var entry = _entries[i];

				if (entry != null && entry.Player == player)
				{
					DeleteEntry(i);
					break;
				}
			}
		}

		private void SelectEntry(Player player)
		{
			SelectedPlayer = player;
			_onPlayerSelected?.Invoke(player);
		}
    }
}