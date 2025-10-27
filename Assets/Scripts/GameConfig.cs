using System;
using UnityEngine;

#nullable enable

[CreateAssetMenu(menuName = "CentaursBoardGame/Game Configuration")]
public class GameConfig : ScriptableObject
{
	//TODO: replace scriptableobject with file reading and parsing 

	[SerializeField] private Color _redPlayerColor = Color.red;
	[SerializeField] private Color _yellowPlayerColor = Color.yellow;
	[SerializeField] private Color _greenPlayerColor = Color.green;
	[SerializeField] private Color _bluePlayerColor = Color.blue;
	[SerializeField] private int _boardSectorCount = GameFacts.DefaultBoardSectorCount;
	public Color RedPlayerColor => _redPlayerColor;
	public Color YellowPlayerColor => _yellowPlayerColor;
	public Color GreenPlayerColor => _greenPlayerColor;
	public Color BluePlayerColor => _bluePlayerColor;

	public int BoardSectorCount => _boardSectorCount;

	public Color PlayerColorToColor(PlayerColor playerColor)
	{
		return playerColor switch
		{
			PlayerColor.Red => _redPlayerColor,
			PlayerColor.Yellow => _yellowPlayerColor,
			PlayerColor.Green => _greenPlayerColor,
			PlayerColor.Blue => _bluePlayerColor,
			_ => throw new Exception($"Unexpected player color: {playerColor}")
		};
	}
}
