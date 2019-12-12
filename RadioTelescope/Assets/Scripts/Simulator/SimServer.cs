﻿using System;
using System.IO;
using System.Net; 
using System.Net.Sockets; 
using System.Text; 
using System.Threading; 
using UnityEngine;
using Valve.Newtonsoft.Json;
using Modbus.Data;
using Modbus.Device;
using System.Linq;
using UnityEngine.UI;

public class SimServer : MonoBehaviour {  	
	#region private members 	
	/// <summary> 	
	/// TCPListener to listen for incomming TCP connection 	
	/// requests. 	
	/// </summary> 	
	private TcpListener tcpListener; 
	/// <summary> 
	/// Background thread for TcpServer workload. 	
	/// </summary> 	
	private Thread tcpListenerThread;  	
	/// <summary> 	
	/// Create handle to connected tcp client. 	
	/// </summary> 	
	private TcpClient connectedTcpClient;

	
	#endregion

	public int port;
    private TcpListener MCU_TCPListener;
    private ModbusSlave MCU_Modbusserver;
    public String ip;
    public TelescopeController tc;
    public Text text;
    public float speed = 0.1f;

    private float azDeg = -42069;

    private float elDeg = -42069;
	// Use this for initialization
	//void Start () { 		
		// Start TcpServer background thread 		
	//	tcpListenerThread = new Thread (new ThreadStart(Run_MCU_server_thread)); 		
	//	tcpListenerThread.IsBackground = true; 		
	//	tcpListenerThread.Start(); 	
	//}
    private string textString = "";
	void ExitServer()
	{
		tcpListener.Server.Close();
	}

    private TcpClient PLCTCPClient;
    public ModbusIpMaster PLCModbusMaster;

    private Thread MCU_emulator_thread;
    private Thread PLC_emulator_thread;

    public string PLC_ip;
    public int PLC_port;
    public string MCU_ip;

    private bool runsimulator = true, mooving = false, jogging = false, isconfigured = false, isTest = false;

    private int acc, distAZ, distEL, currentAZ, currentEL, AZ_speed, EL_speed;

    void Start()
    {
        tc.speed = speed;
        tc.targetY = 0.0f;
        tc.targetZ = 0.0f;
        tc.SetY(0);
        tc.SetZ(0.0f + 15.0f);
        try
        {
            //MCU_TCPListener = new TcpListener(new IPEndPoint(IPAddress.Parse("127.0.0.2"), 8080));
            //Debug.Log(MCU_ip,)
            MCU_TCPListener = new TcpListener(new IPEndPoint(IPAddress.Parse(MCU_ip), PLC_port+1));
            MCU_emulator_thread = new Thread(new ThreadStart(Run_MCU_server_thread));
        }
        catch (Exception e)
        {
            if ((e is ArgumentNullException) || (e is ArgumentOutOfRangeException))
            {
                Debug.Log(e);
                return;
            }
            else { throw e; }// Unexpected exception
        }
        try
        {
            MCU_TCPListener.Start(1);
        }
        catch (Exception e)
        {
            if ((e is SocketException) || (e is ArgumentOutOfRangeException) || (e is InvalidOperationException))
            {
                Debug.Log(e);
                return;
            }
        }
        runsimulator = true;
        MCU_emulator_thread.Start();
    }
    
    // Update is called once per frame
    void Update () { 		
        //if (Input.GetKeyDown(KeyCode.Space)) {             
        //	SendMessage();         
        //}
        text.text = textString;
        if (azDeg != -42069)
        {
            
            tc.SetY(azDeg);
            azDeg = -42069;

        }

        if (elDeg != -42069)
        {
            tc.SetZ(elDeg);
            elDeg = -42069;
        }
        if (Input.GetKeyDown((KeyCode.Escape)))
        {
            ExitServer();
            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }
    }
    
    public void startPLC()
    {
        try
        {
            PLC_emulator_thread = new Thread(new ThreadStart(Run_PLC_emulator_thread));
            PLC_emulator_thread.Start();
        }
        catch (Exception e)
        {
            if ((e is ArgumentNullException) || (e is ArgumentOutOfRangeException))
            {
                Debug.Log(e);
                return;
            }

        }
    }
    public void Bring_down()
    {
        runsimulator = false;
        PLC_emulator_thread.Join();
        PLCTCPClient.Dispose();
        PLCModbusMaster.Dispose();
        MCU_emulator_thread.Join();
        MCU_TCPListener.Stop();
        MCU_Modbusserver.Dispose();
    }

    private void Run_PLC_emulator_thread()
    {
        while (runsimulator)
        {
            try
            {
                PLCTCPClient = new TcpClient(this.PLC_ip, this.PLC_port);
                PLCModbusMaster = ModbusIpMaster.CreateIp(PLCTCPClient);
            }
            catch
            {//no server setup on control room yet 
                Debug.Log("________________PLC sim awaiting control room");

                //Thread.Sleep(1000);
            }
            Debug.Log("________________PLC sim running");
            //PLCModbusMaster.WriteMultipleRegisters((ushort)PLC_modbus_server_register_mapping.Safty_INTERLOCK - 1, new ushort[] { 1 });
            while (runsimulator)
            {
                if (isTest)
                {
                   // PLCModbusMaster.WriteMultipleRegisters((ushort)PLC_modbus_server_register_mapping.Safty_INTERLOCK - 1, new ushort[] { 1 });
                    Thread.Sleep(5);
                    continue;
                }
                Thread.Sleep(50);
            }
        }
    }

    private void Server_Written_to_handler(object sender, DataStoreEventArgs e)
    {
        MCU_Modbusserver.DataStore.HoldingRegisters[1] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[1] & 0xff7f);
        MCU_Modbusserver.DataStore.HoldingRegisters[11] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[11] & 0xff7f);
        //Debug.Log("plcdriver data writen 1 reg "+ e.Data.B[0]+" start adr "+ e.StartAddress);
        ushort[] data = new ushort[e.Data.B.Count];
        for (int i = 0; i < e.Data.B.Count; i++)
        {
            data[i] = e.Data.B[i];
        }
        handleTestCMD(data);
    }

    private void Run_MCU_server_thread()
    {
        byte slaveId = 1;
        // create and start the TCP slave
        MCU_Modbusserver = ModbusTcpSlave.CreateTcp(slaveId, MCU_TCPListener);
        //coils, inputs, holdingRegisters, inputRegisters
        MCU_Modbusserver.DataStore = DataStoreFactory.CreateDefaultDataStore(0, 0, 1054, 0);
        // PLC_Modbusserver.DataStore.SyncRoot.ToString();

        //MCU_Modbusserver.ModbusSlaveRequestReceived += new EventHandler<ModbusSlaveRequestEventArgs>(Server_Read_handler);
        if (isTest)
        {
            MCU_Modbusserver.DataStore.DataStoreWrittenTo += new EventHandler<DataStoreEventArgs>(Server_Written_to_handler);
        }

        MCU_Modbusserver.Listen();

        // prevent the main thread from exiting
        ushort[] previos_out, current_out;
        previos_out = Copy_modbus_registers(1025, 20);
        while (runsimulator)
        {
            if (isTest)
            {
                Thread.Sleep(5);
                continue;
            }
            Thread.Sleep(50);
            current_out = Copy_modbus_registers(1025, 20);
            if (!current_out.SequenceEqual(previos_out))
            {
                handleCMD(current_out);
                //Debug.Log("data changed");
            }
            if (mooving)
            {
                if (distAZ != 0 || distEL != 0)
                {
                    int travAZ = (distAZ < -AZ_speed) ? -AZ_speed : (distAZ > AZ_speed) ? AZ_speed : distAZ;
                    int travEL = (distEL < -EL_speed) ? -EL_speed : (distEL > EL_speed) ? EL_speed : distEL;
                    move(travAZ, travEL);
                }
                else
                {
                    mooving = false;
                    MCU_Modbusserver.DataStore.HoldingRegisters[1] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[1] | 0x0080);
                    MCU_Modbusserver.DataStore.HoldingRegisters[11] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[11] | 0x0080);
                }
            }
            if (jogging)
            {
                move(AZ_speed, EL_speed);
            }
            previos_out = current_out;
        }


    }

    private bool move(int travAZ, int travEL)
    {
        distAZ -= travAZ;
        distEL -= travEL;
        currentAZ += travAZ;
        currentEL += travEL;
        //   Debug.Log("offset: az" + currentAZ + " el " + currentEL);
        MCU_Modbusserver.DataStore.HoldingRegisters[3] = (ushort)((currentAZ & 0xffff0000) >> 16);
        MCU_Modbusserver.DataStore.HoldingRegisters[4] = (ushort)(currentAZ & 0xffff);
        MCU_Modbusserver.DataStore.HoldingRegisters[13] = (ushort)((currentEL & 0xffff0000) >> 16);
        MCU_Modbusserver.DataStore.HoldingRegisters[14] = (ushort)(currentEL & 0xffff);
        return true;
    }


    private bool handleCMD(ushort[] data)
    {
        string outstr = "";
        for(int v = 0; v < data.Length; v++)
        {
            outstr += Convert.ToString(data[v], 16).PadLeft(5) + ",";
        }
        // Debug.Log(outstr);
        string[] packetInfo = outstr.Split(',');
        //Debug.Log(packetInfo[0]);
        if (Int32.Parse(packetInfo[0]) == 2)
        {
            
            //Debug.Log("Az 1: " + packetInfo[2]);
            //Debug.Log("Az 2: " + packetInfo[3]);
            //byte[] iHateThis = StringToByteArray(packetInfo[2]);
            //uint iReallyHateThis = uint.Parse(packetInfo[2], System.Globalization.NumberStyles.AllowHexSpecifier);
            //Debug.Log("DMOWEFMWEOFMWEOF: " + iReallyHateThis);
            int frontAz = (Convert.ToInt32(packetInfo[2].Trim(), 16))  << 16;
            //Debug.Log("Front az before: " + packetInfo[2] + ", Front az after: " + frontAz);
            int backAz = (Convert.ToInt32(packetInfo[3].Trim(), 16));

            //Debug.Log("El 1: " + packetInfo[12]);
            //Debug.Log("El 2: " + packetInfo[13]);
            int frontEl = Convert.ToInt32(packetInfo[12].Trim(), 16) << 16;
            int backEl = Convert.ToInt32(packetInfo[13].Trim(), 16);
            int az = frontAz + backAz;
            int el = frontEl + backEl;
            
            //Debug.Log("Az hex: " + az);
            //Debug.Log("El hex: " + el);
            
            azDeg = az * 360.0f / (20000.0f * 500.0f);
            Debug.Log("The degree azimuth is: " + azDeg); 
            elDeg = el * 360.0f / (20000.0f * 50.0f);
            Debug.Log("The degree elevation is: " + elDeg);
            
            
            
            textString = "Move Packet\n" + "Pos Az: " + azDeg + "\nPos El: " + elDeg;
            
            //speed
            //Debug.Log(packetInfo[4]);
            //Debug.Log(packetInfo[5]);
            //Debug.Log(packetInfo[14]);
            //Debug.Log(packetInfo[15]);
        }
        //Debug.Log("===========================================================================================================================");
        jogging = false;
        if (data[0] == 0x8400)
        {//if not configured dont move

            isconfigured = true;
        }
        else if (!isconfigured)
        {
            return true;
        }

        if (data[1] == 0x0403)
        {//move cmd
            mooving = true;
            MCU_Modbusserver.DataStore.HoldingRegisters[1] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[1] & 0xff7f);
            MCU_Modbusserver.DataStore.HoldingRegisters[11] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[11] & 0xff7f);
            AZ_speed = (data[2] << 16) + data[3];
            AZ_speed /= 5;
            EL_speed = AZ_speed;
            acc = data[4];
            distAZ = (data[6] << 16) + data[7];
            distEL = (data[12] << 16) + data[13];
            return true;
        }
        else if (data[0] == 0x0080 || data[0] == 0x0100 || data[10] == 0x0080 || data[10] == 0x0100)
        {
            jogging = true;
            MCU_Modbusserver.DataStore.HoldingRegisters[1] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[1] & 0xff7f);
            MCU_Modbusserver.DataStore.HoldingRegisters[11] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[11] & 0xff7f);
            if (data[0] == 0x0080)
            {
                AZ_speed = ((data[4] << 16) + data[5]) / 20;
            }
            else if (data[0] == 0x0100)
            {
                AZ_speed = -((data[4] << 16) + data[5]) / 20;
            }
            else
            {
                AZ_speed = 0;
            }
            if (data[10] == 0x0080)
            {
                EL_speed = ((data[14] << 16) + data[15]) / 20;
            }
            else if (data[10] == 0x0100)
            {
                EL_speed = -((data[14] << 16) + data[15]) / 20;
            }
            else
            {
                EL_speed = 0;
            }
            return true;
        }
        else if (data[0] == 0x0002 || data[0] == 0x0002)
        {//move cmd
            mooving = true;
            MCU_Modbusserver.DataStore.HoldingRegisters[1] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[1] & 0xff7f);
            MCU_Modbusserver.DataStore.HoldingRegisters[11] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[11] & 0xff7f);
            AZ_speed = ((data[4] << 16) + data[5]) / 5;
            EL_speed = ((data[14] << 16) + data[15]) / 5;
            acc = data[6];
            distAZ = (data[2] << 16) + data[3];
            distEL = (data[12] << 16) + data[13];
            return true;
        }
        return false;
    }


    private bool handleTestCMD(ushort[] data)
    {
        string outstr = " inreg";
        for (int v = 0; v < data.Length; v++)
        {
            outstr += Convert.ToString(data[v], 16).PadLeft(5) + ",";
        }
        // Debug.Log(outstr);
        if (data[1] == 0x0403)//move cmd
        {
            distAZ = (data[6] << 16) + data[7];
            distEL = (data[12] << 16) + data[13];
            Debug.Log("Move command");
            Console.WriteLine("AZ_22 {0,16} EL_22 {1,16}", (MCU_Modbusserver.DataStore.HoldingRegisters[3] << 16) + MCU_Modbusserver.DataStore.HoldingRegisters[4], (MCU_Modbusserver.DataStore.HoldingRegisters[13] << 16) + MCU_Modbusserver.DataStore.HoldingRegisters[14]);
            int data1 = MCU_Modbusserver.DataStore.HoldingRegisters[3] + MCU_Modbusserver.DataStore.HoldingRegisters[4];
            int data2 = MCU_Modbusserver.DataStore.HoldingRegisters[13] + MCU_Modbusserver.DataStore.HoldingRegisters[14];
            Debug.Log("AZ_22 {" + data1 + "} EL_22 {" + data2 +"}");
            data1 = MCU_Modbusserver.DataStore.HoldingRegisters[3] + MCU_Modbusserver.DataStore.HoldingRegisters[4];
            data2 = MCU_Modbusserver.DataStore.HoldingRegisters[13] + MCU_Modbusserver.DataStore.HoldingRegisters[14];
            move(distAZ, distEL);
            MCU_Modbusserver.DataStore.HoldingRegisters[1] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[1] | 0x0080);
            MCU_Modbusserver.DataStore.HoldingRegisters[11] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[11] | 0x0080);

            Debug.Log("AZ_finni1 {"+data1+"} EL_finni1 {"+data2+"}");

            return true;
        }
        else if (data[0] == 0x0002 || data[0] == 0x0002)
        {//move cmd
            Debug.Log("Move command");
            MCU_Modbusserver.DataStore.HoldingRegisters[1] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[1] & 0xff7f);
            MCU_Modbusserver.DataStore.HoldingRegisters[11] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[11] & 0xff7f);
            distAZ = (data[2] << 16) + data[3];
            distEL = (data[12] << 16) + data[13];

            move(distAZ, distEL);
            MCU_Modbusserver.DataStore.HoldingRegisters[1] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[1] | 0x0080);
            MCU_Modbusserver.DataStore.HoldingRegisters[11] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[11] | 0x0080);

            return true;
        }
        return false;
    }

    private ushort[] Copy_modbus_registers(int start_index, int length)
    {
        ushort[] data = new ushort[length];
        for (int i = 0; i < length; i++)
        {
            data[i] = MCU_Modbusserver.DataStore.HoldingRegisters[i + start_index];
        }
        return data;
    }

    public static byte[] StringToByteArray(string hex) {
        return Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
    }
}
