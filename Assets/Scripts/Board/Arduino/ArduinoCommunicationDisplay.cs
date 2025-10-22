using TMPro;
using UnityEngine;

#nullable enable

namespace CentaursBoardGame
{
	public class ArduinoCommunicationDisplay : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Color _successfulResponseColor;
        [SerializeField] private Color _failedResponseColor;

        public void ShowRequest(IArduinoRequest request)
		{
            gameObject.SetActive(true);

            switch (request.Type)
            {
                case ArduinoRequestType.Rotate:
                    _text.text = "Rotating rings...";
                    break;

                case ArduinoRequestType.Stop:
                    _text.text = "Stopped rings";
                    break;

                default:
                    throw new System.Exception($"Unexpected request type: {request.Type}");
            }
		}

		public void ShowResponse(ArduinoResponse response)
		{
            gameObject.SetActive(true);
            _text.color = response.Status switch
            {
                ArduinoResponseStatus.Successful => _successfulResponseColor,
                ArduinoResponseStatus.Failed => _failedResponseColor,
                _ => throw new System.Exception($"Trying to display unexpected response status: {response.Status}")
            };
            _text.text = response.Message;
        }

		public void Hide()
		{
			gameObject.SetActive(false);
		}

        private void DisplayArduinoRepsonse(ArduinoResponse response)
        {
            
        }

    }
}