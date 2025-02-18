using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using Singleton;
using System.Text.RegularExpressions;

public class MyUDPClient : Singleton<MyUDPClient>
{
    private UdpClient udpClient;
    private IPEndPoint serverEndPoint;
    private int port = 1234;

    public string oculusIP = "192.168.54.119";

    private void Start()
    {
        IPAddress ipAddress = IPAddress.Parse(oculusIP); // IP address of the destination server

        udpClient = new UdpClient();
        serverEndPoint = new IPEndPoint(ipAddress, port);
    }

    private void OnDestroy()
    {
        udpClient.Close();
    }

    public void SendUDPMessage(string message)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, serverEndPoint);
    }

    public void SetOculusIp(string ip)
    {

        string trimmedString = ip.Trim();
        string noNull = trimmedString.Replace("\0", "");
        string cleanedString = Regex.Replace(noNull, @"[^\u0020-\u007E]", "");

        Debug.Log("Set Oculus Ip: " + cleanedString);

        oculusIP = cleanedString;
        IPAddress ipAddress = IPAddress.Parse(cleanedString);
        serverEndPoint = new IPEndPoint(ipAddress, port);
    }

    // Example usage
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Send UDP Message");
            SendUDPMessage("Hello, UDP server!"); // Replace with your desired message
        }
    }
}
