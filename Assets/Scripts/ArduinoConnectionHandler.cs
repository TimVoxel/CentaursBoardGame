using UnityEngine;
using System.Thread;
using System.IO.SerialPort;

public class ArduinoConnectionHandler : MonoBehaviour
{
    SerialPort serialPort;
    Thread readThread;
    bool isRunning = false;
    string receivedData;

    void Start()
    {
        // Adjust COM port to match your Arduino’s port
        serialPort = new SerialPort("COM3", 9600);
        serialPort.ReadTimeout = 100;
        serialPort.Open();

        isRunning = true;
        readThread = new Thread(ReadSerial);
        readThread.Start();
    }

    void ReadSerial()
    {
        while (isRunning)
        {
            try
            {
                string data = serialPort.ReadLine();
                receivedData = data;
            }
            catch (System.Exception) { }
        }
    }

    void Update()
    {
        // Show data received from Arduino
        if (!string.IsNullOrEmpty(receivedData))
        {
            Debug.Log("Arduino says: " + receivedData);
        }

        // Send data to Arduino
        if (Input.GetKeyDown(KeyCode.O))
        {
            SendData("LED_ON");
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            SendData("LED_OFF");
        }
    }

    void SendData(string message)
    {
        if (serialPort.IsOpen)
        {
            serialPort.WriteLine(message);
            serialPort.BaseStream.Flush();
            Debug.Log("Sent to Arduino: " + message);
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (readThread != null && readThread.IsAlive)
            readThread.Join();

        if (serialPort != null && serialPort.IsOpen)
            serialPort.Close();
    }
}
