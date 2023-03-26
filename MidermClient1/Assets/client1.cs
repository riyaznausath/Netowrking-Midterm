using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using TMPro;


public class client1 : MonoBehaviour
{

    public GameObject myCube;
    public GameObject otherCube;
    public GameObject loginPanel;
    public GameObject loginIPText;
    public GameObject msgPanel;

    public bool loggedIn = false;

    public static bool msgReceived = false;

    public static string msg = "";

    private static byte[] buffer = new byte[512];
    private static Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public float x = 0.0f;
    public float y = 0.0f;
    public float z = 0.0f;

    private static byte[] outBuffer = new byte[512];
    private static IPEndPoint remoteEP;
    private static Socket clientSoc;
    private float interval = 0.0f; // interval at which next packet will be send
    public float delay = 0.1f; //delay amount 0.1 seconds
    private static EndPoint remoteServ;



    void Start()
    {
        myCube = GameObject.Find("Cube");
    }

    // Update is called once per frame
    void Update()
    {
        

        if (loggedIn == true)
        {
            /*string position = "(" + myCube.transform.position.x.ToString() + "," +
                myCube.transform.position.y.ToString() + ", " +
                myCube.transform.position.z.ToString() + ")";
            */
            string position = myCube.transform.position.x.ToString() + "," +
                myCube.transform.position.y.ToString() + ", " +
                myCube.transform.position.z.ToString();
            // outBuffer = Encoding.ASCII.GetBytes(myCube.transform.position.ToString());
            outBuffer = Encoding.ASCII.GetBytes(position);

            //check if other sockets are open. to receive positions of 'other cube' from other clients
            //update the positon of other cube if found
            if (clientSoc.Available > 0)
            {
                int recv = clientSoc.ReceiveFrom(buffer, ref remoteServ);
                //split buffer positions from recived position. outbuffer didnt want and get index out of range

                float[] serverMsg = new float[recv / 4];
                Buffer.BlockCopy(buffer, 0, serverMsg, 0, recv);
                Debug.Log("Recv from: " + remoteServ.ToString() + "Data: " + serverMsg[0] + " " + serverMsg[1] + " " + serverMsg[2]);
                otherCube.transform.position = new Vector3(serverMsg[0], serverMsg[1], serverMsg[2]);



            }
            //if msg from other client is received, it will create a new panel and textbox and output int chatbox
            if (msgReceived == true)
            {

                ChatBox.GetInstance().CreateMessage(msg);
                msgReceived = false;

            }

            //make movement of cube in current client send its position in intervals so it doesnt spam it with every signle tick of movement
            if (Time.time > interval)
            {

                interval += delay;
                //if the cube has moved, it will send positon to other client to mimic
                if (myCube.transform.hasChanged)
                {
                  //  Debug.Log("moving");

                    float[] floatPos = new float[] { myCube.transform.position.x, myCube.transform.position.y, myCube.transform.position.z };
                    byte[] bufferPosition = new byte[floatPos.Length * 4];
                    Buffer.BlockCopy(floatPos, 0, bufferPosition, 0, bufferPosition.Length);
                    clientSoc.SendTo(bufferPosition, remoteEP); 
                    myCube.transform.hasChanged = false;

                }
            }



        }
       
    }

    public static void StartClientUDP(string serverIP)
    {
        //this 2nd client will set it to 888 UD Pport
        try
        {
            IPAddress ip = IPAddress.Parse(serverIP);
            remoteEP = new IPEndPoint(ip, 8888);
            remoteServ = new IPEndPoint(ip, 8888);
            clientSoc = new Socket(AddressFamily.InterNetwork,
                           SocketType.Dgram, ProtocolType.Udp);

        }
        catch (Exception e)
        {
            Debug.Log("Exception: " + e.ToString());
        }



    }

    //connecting to 888 tcp port // issue with this area as it either makes movement or chatbox work. couldnt get both to work together
    public static void StartClientTCP(string serverIP)
    {
        client.Connect(IPAddress.Parse(serverIP), 8888);
        Debug.Log("connected to server(TCP)");

        client.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallBack), client);


    }
    //login function for the button in UI
    public void Login()
    {
        StartClientUDP(loginIPText.GetComponent<TMP_InputField>().text);
        StartClientTCP(loginIPText.GetComponent<TMP_InputField>().text);
        loggedIn = true;

        loginPanel.SetActive(false);


    }

    // function for the msg send button
    public void SendButton()
    {
        string sendMessage = msgPanel.GetComponent<TMP_InputField>().text;
        byte[] buffer = Encoding.ASCII.GetBytes(sendMessage);
        client.Send(buffer);
    }


    private static void ReceiveCallBack(IAsyncResult results)
    {
        Socket socket = (Socket)results.AsyncState;
        int rec = socket.EndReceive(results);

        byte[] data = new byte[rec];
        Array.Copy(buffer, data, rec);

        string serverMessage = Encoding.ASCII.GetString(data);
        Debug.Log("Received from Server: " + serverMessage);
        msgReceived = true;

        msg = serverMessage;
        socket.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallBack), socket);
    }

  
}
