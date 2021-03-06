﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Game_Server.Controller;
using Common;

namespace Game_Server.Servers
{
    class Server
    {   
        //定义一个IP对象
        private IPEndPoint ipEndPoint;
        //socket类
        private Socket serverSocket;
        //客户端list，用于连接的客户端
        private List<Client> clientList = new List<Client>();
        //房间list，用于管理房间
        private List<Room> roomList = new List<Room>();
        //连接的管理类
        private ControllerManager controllerManager;

        public Server() { }
        //初始化Server
        public Server(string ipStr,int port)
        {
            //创建一个controllerManager管理类
            controllerManager = new ControllerManager(this);
            //设置服务器IP和端口
            SetIpAndPort(ipStr,port);
        }
        //设置需要连接的IP和端口
        public void SetIpAndPort(string ipStr,int port)
        {   
            //创建一个IP对象
            ipEndPoint = new IPEndPoint(IPAddress.Parse(ipStr), port);
        }

        public void Start()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);
            serverSocket.Bind(ipEndPoint);
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallBack,null);
        }

        private void AcceptCallBack(IAsyncResult ar)
        {
            Socket clientSocket = serverSocket.EndAccept(ar);
            Client client = new Client(clientSocket,this);
            client.Start();
            clientList.Add(client);
            serverSocket.BeginAccept(AcceptCallBack,null);
        }

        public void RemoveClient(Client client)
        {
            lock(clientList)
            {
                clientList.Remove(client);
            }
        }

        //这个函数是controllerManager处理完消息后，调用此方法，发送消息给客户端
        public void SendResponse(Client client,ActionCode actionCode,string data)
        {
            client.Send(actionCode, data);
        }

        public void HandleRequest(RequestCode requestCode, ActionCode actioncode, string data, Client client)
        {
            controllerManager.HandleRequest(requestCode, actioncode, data, client);
        }

        public void CreateRoom(Client client)
        {
            Room room = new Room(this);
            room.AddClient(client);
            roomList.Add(room);
        }

        public void RemoveRoom(Room room)
        {
            if (roomList != null && room != null) 
            {
                roomList.Remove(room);
            }
        }

        public List<Room> GetRoomList()
        {
            return roomList;
        }

        public Room GetRoomById(int id)
        {
            foreach(Room room in roomList)
            {
                if(room.GetId()==id)
                {
                    return room;
                }
            }
            return null;
        }

    }
}
