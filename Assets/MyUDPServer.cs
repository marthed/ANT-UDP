using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using System.Net.NetworkInformation;
using UnityEngine.Events;

public class MyUDPServer : MonoBehaviour
{
  #region "Inspector Members"

  [SerializeField] string oculusIpAdress = "192.168.231.218";
  [SerializeField] int port = 1234;
  [SerializeField] bool hideLogs = false;

  [Tooltip("Event is triggered when UDPServer receivs the external server ip")]
  public UnityEvent<string> OnIpReceived;

  #endregion

  #region "Private Members"

  private float moveDistance = 1f;
  private int frameWait = 3;
  private int maxClients = 3;
  Socket udp = null;
  int idAssignIndex = 0;
  Dictionary<EndPoint, UDPClient2> clients;
  #endregion

  void Start()
  {
    clients = new Dictionary<EndPoint, UDPClient2>();

    string wifiIpAddress = GetLocalIPAddress();
    Debug.Log("Wifi Ip Adress: " + wifiIpAddress);

    string testThisIpAdress = !string.IsNullOrEmpty(wifiIpAddress) ? wifiIpAddress : oculusIpAdress;  // Oculus Quest Ip

    IPAddress ipAddress = IPAddress.Parse(testThisIpAdress);
    IPEndPoint endPoint = new IPEndPoint(ipAddress, port);


    Debug.Log("Wi-Fi IP Address: " + testThisIpAdress);
    Debug.Log("Server IP Address: " + endPoint.Address);
    Debug.Log("Port: " + port);
    udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    udp.Bind(endPoint);
    udp.Blocking = false;
  }

  string GetLocalIPAddress()
  {
    string localIP = "";
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
      if (ip.AddressFamily == AddressFamily.InterNetwork)
      {
        localIP = ip.ToString();
        break;
      }
    }
    return localIP;
  }

  void Update()
  {
    if (Time.frameCount % frameWait == 0 && udp != null && udp.Available != 0)
    {
      byte[] packet = new byte[64];
      EndPoint sender = new IPEndPoint(IPAddress.Any, port);

      int rec = udp.ReceiveFrom(packet, ref sender);
      string info = Encoding.Default.GetString(packet);

      if (!hideLogs)
      {
        Debug.Log("Server received: " + info);
      }


      HandleUDPMessage(info);


      if (info[0] == 'n' && clients.Count < maxClients)
        HandleNewClient(sender, info);
      else if (info[0] == 'e')
        DisconnectClient(sender, info);
      else if (rec > 0)
      {
        string id = Parser.ParseID(info);
        int seqNumber = Parser.ParseSequenceNumber(info);
        if (id == "" || seqNumber == -1) return;

        string userInput = Parser.ParseInput(info);
        if (userInput != "")
          HandleUserMoveInput(sender, userInput, seqNumber);
      }
    }
  }

  void HandleUDPMessage(string msg)
  {
    Debug.Log("UDP message received: " + msg);
    switch (msg.Substring(0, 2))
    {
      case "ip":
        OnIpReceived?.Invoke(msg.Substring(3));
        break;
    }
  }

  void HandleNewClient(EndPoint addr, string data)
  {
    string id = "c" + idAssignIndex++ + "t";
    Debug.Log("Handling new client with id " + id);
    SendPacket("a " + id, addr);

    Vector3 pos = Parser.ParseInitialPosition(data);
    clients.Add(addr, new UDPClient2(id, pos));
    SendPositionToAllClients();
  }

  void DisconnectClient(EndPoint sender, string data)
  {
    if (clients.ContainsKey(sender))
      clients.Remove(sender);
    Broadcast(data);
  }

  void Broadcast(string data)
  {
    foreach (KeyValuePair<EndPoint, UDPClient2> p in clients)
      SendPacket(data, p.Key);
  }

  void HandleUserMoveInput(EndPoint client, string userInput, int seqNumber)
  {
    if (!clients.ContainsKey(client) || clients[client].lastSeqNumber > seqNumber)
      return;

    if (!clients[client].history.ContainsKey(seqNumber))
    {
      clients[client].UpdateStateHistory(seqNumber);
      clients[client].lastSeqNumber = seqNumber;
    }
    UpdatePosition(client, userInput);
    /* so that clients see newly connected clients */
    SendPositionToAllClients();
  }

  void UpdatePosition(EndPoint addr, string input)
  {
    if (input.Equals("a"))
      clients[addr].position.x -= moveDistance;
    else if (input.Equals("d"))
      clients[addr].position.x += moveDistance;
    else if (input.Equals("w"))
      clients[addr].position.y += moveDistance;
    else if (input.Equals("s"))
      clients[addr].position.y -= moveDistance;
  }

  void SendPositionToAllClients()
  {
    foreach (KeyValuePair<EndPoint, UDPClient2> p in clients)
      foreach (KeyValuePair<EndPoint, UDPClient2> p2 in clients)
        SendPacket(p2.Value.ToString(), p.Key);
  }

  Vector3 ParsePosition(Match match)
  {
    float x, y, z;
    float.TryParse(match.Groups["x"].Value, out x);
    float.TryParse(match.Groups["y"].Value, out y);
    float.TryParse(match.Groups["z"].Value, out z);
    return new Vector3(x, y, z);
  }

  void SendPacket(string str, EndPoint addr)
  {
    byte[] arr = Encoding.ASCII.GetBytes(str);
    udp.SendTo(arr, addr);
  }
}