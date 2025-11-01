using System.Collections;
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
        [SerializeField] private string _notConnectedMessage;
        [SerializeField] private float _hideDelaySeconds = 3f;

        private WaitForSeconds _hideDelay;
        private Coroutine? _currentDelay;

        private void Awake()
        {
            _hideDelay = new WaitForSeconds(_hideDelaySeconds);
        }

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

            StartHiding();
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
            StartHiding();
        }

        public void ShowNotConnected()
        {
            gameObject.SetActive(true);
            _text.color = _failedResponseColor;
            _text.text = _notConnectedMessage;
            StartHiding();
        }

		public void Hide()
		{
			gameObject.SetActive(false);
        }

        private void StartHiding()
        {
            if (_currentDelay != null)
            {
                StopCoroutine(_currentDelay);
            }

            _currentDelay = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return _hideDelay;
            Hide();
        }
    }
}