using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;

public class MyUDPClient : MonoBehaviour
{
  private UdpClient udpClient;
  private IPEndPoint serverEndPoint;

    private void Start()
    {
        IPAddress ipAddress = IPAddress.Parse("192.168.54.119"); // IP address of the destination server
        int port = 1234; // Port number of the destination server

        udpClient = new UdpClient();
        serverEndPoint = new IPEndPoint(ipAddress, port);
    }

    private void OnDestroy()
    {
        udpClient.Close();
    }

    private void SendUDPMessage(string message)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, serverEndPoint);
    }

    // Example usage
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMessage("Hello, UDP server!"); // Replace with your desired message
        }
    }
}