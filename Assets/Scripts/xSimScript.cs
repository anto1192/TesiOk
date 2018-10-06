
using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

public struct Vector
    {
        public Single X;
        public Single Y;
        public Single Z;
    }
    public struct Vec
    {
        public Int32 X;
        public Int32 Y;
        public Int32 Z;
    }
    public struct Si
    {
        public uint Time;
        public Vector AngVel;
        public Single Heading; //z di Orientation
        public Single Pitch; //y di Orientation
        public Single Roll; //x di Orientation
        public Vector Accel;
        public Vector Vel;
        public Vector Pos;
        public int gameId;
    }
	
public class xSimScript : MonoBehaviour
{
    IPEndPoint remoteEndPoint;
    String IP = "127.0.0.1";
    int    port = 4123;
    UdpClient client;

    byte[] getBytes(Si str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    // sendData
    public  void sendString(Si sim)
    {
        try
        {
            client.Send(getBytes(sim), getBytes(sim).Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            Console.WriteLine(err.ToString());
        }
    }

    VehicleController vehicleController;
    PIDPars _pidPars;
    
    // Use this for initialization
    void Start () {
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();
        tempo = 0;
        vehicleController = gameObject.GetComponent<VehicleController>();
        _pidPars = Resources.Load<PIDPars>("PIDPars_steeringWheel");
    }



   private static int accelQueueLen = 10;
   uint tempo;
    double lastLong;
    double lastLat;
    private Vector3 accelerazioneCorrente = Vector3.zero;
   private Si sendData()
    {
        Si sim = new Si();
        Rigidbody rigidBody = GetComponent<Rigidbody>();

        // Acceleration (requires smoothing due to positional rounding errors).
        Vector3 accelerazione;
        LinearAcceleration(out accelerazione, transform.position, accelQueueLen);
        accelerazione *= accelQueueLen - 1;
        Vector3 accelerazioneOk = ConvertRightHandedToLeftHandedVector(accelerazione);
        
        
        //se sto per fermarmi evito strani valori di accelerazione, la porto a zero con un'interpolazione lineare, evitando picchi improvvisi e movimenti strani della piattaforma
        if (rigidBody.velocity.magnitude < 1f && vehicleController.accellInput <= 0) //l0accelerazione sta scendendo perchè sto frenando o non accelerando
        {
            if (accelerazioneCorrente == Vector3.zero)
            {
                accelerazioneCorrente = accelerazioneOk;
            }
            accelerazioneOk.x = Mathf.Lerp(accelerazioneCorrente.x, 0f, Time.fixedDeltaTime * _pidPars.violenzaPiattaforma);
            accelerazioneOk.y = Mathf.Lerp(accelerazioneCorrente.y, 0f, Time.fixedDeltaTime * _pidPars.violenzaPiattaforma);
            accelerazioneCorrente = accelerazioneOk;
        } else
        {
            if (accelerazioneCorrente != Vector3.zero)
            {
                accelerazioneCorrente = Vector3.zero;
            }
        }

        //setto i valori giusti di accelerazione nella struttura
        sim.Accel.X = accelerazioneOk.x;
        sim.Accel.Y = accelerazioneOk.y;
        sim.Accel.Z = accelerazioneOk.z;
        
        
        //Velocity
        Vector3 velocityOk = ConvertRightHandedToLeftHandedVector(rigidBody.velocity);
        sim.Vel.X = velocityOk.x;
        sim.Vel.Y = velocityOk.y;
        sim.Vel.Z = velocityOk.z;

        //AngularVelocity
        Vector3 angularVelocityOk = ConvertRightHandedToLeftHandedVector(rigidBody.angularVelocity);       
        sim.AngVel.X = angularVelocityOk.x;
        sim.AngVel.Y = angularVelocityOk.y;
        sim.AngVel.Z = angularVelocityOk.z;

        //Time
        //c += (uint) Time.fixedDeltaTime;        
        sim.Time = tempo;
        tempo++;

        //Position
        Vector3 posizioneOk = ConvertRightHandedToLeftHandedVector(transform.position);    
        sim.Pos.X = posizioneOk.x;
        sim.Pos.Y = posizioneOk.y;
        sim.Pos.Z = posizioneOk.z;

        //orientation: heading, roll, pitch
        Vector3 angoliEulero = ConvertRightHandedToLeftHandedQuaternion(transform.rotation);
        sim.Heading = angoliEulero.z * 3.14159f / 180f;
        sim.Roll = angoliEulero.x * 3.14159f / 180f;
        sim.Pitch = angoliEulero.y * 3.14159f / 180f;


        double accelerazioneLaterale = ((Math.Cos(angoliEulero.x) * accelerazioneOk.x + Math.Sin(angoliEulero.x) * accelerazioneOk.y)); // / 9.81f);
        double accelerazioneLongitudinale = ((-Math.Sin(angoliEulero.x) * accelerazioneOk.x + Math.Cos(angoliEulero.x) * accelerazioneOk.y)); // / 9.81f);

        lastLong = accelerazioneLongitudinale;
        lastLat = accelerazioneLaterale;

        sim.gameId = 0;

        return sim;
    }

    private Vector3 ConvertRightHandedToLeftHandedQuaternion(Quaternion rightHandedQuaternion)
    {
        //restituisce gli angoli di Eulero del quaternion convertiti
        Quaternion ok = new Quaternion(-rightHandedQuaternion.x,
                               -rightHandedQuaternion.z,
                               -rightHandedQuaternion.y,
                                 rightHandedQuaternion.w);
        Vector3 angoli = ok.eulerAngles;
        return new Vector3(-angoli.x, -angoli.y, angoli.z);
    }

    private Vector3 ConvertRightHandedToLeftHandedVector(Vector3 rightHandedVector)
    {
        return new Vector3(-rightHandedVector.x, -rightHandedVector.z, rightHandedVector.y);
    }

    // Update is called once per frame
    void FixedUpdate () {
        try
        {
            sendString(sendData());
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    private static Vector3[] positionRegister;
    private static float[] posTimeRegister;
    private static int positionSamplesTaken = 0;

    private static bool LinearAcceleration(out Vector3 vector, Vector3 position, int samples)
    {
        Vector3 averageSpeedChange = Vector3.zero;
        vector = Vector3.zero;
        Vector3 deltaDistance;
        float deltaTime;
        Vector3 speedA;
        Vector3 speedB;

        if (positionRegister == null)
        {
            positionRegister = new Vector3[samples];
            posTimeRegister = new float[samples];
        }

        for (int i = 0; i < positionRegister.Length - 1; i++)
        {
            positionRegister[i] = positionRegister[i + 1];
            posTimeRegister[i] = posTimeRegister[i + 1];
        }

        positionRegister[positionRegister.Length - 1] = position;
        posTimeRegister[posTimeRegister.Length - 1] = Time.fixedTime;
        positionSamplesTaken++;

        if (positionSamplesTaken >= samples)
        {
            for (int i = 0; i < positionRegister.Length - 2; i++)
            {
                deltaDistance = positionRegister[i + 1] - positionRegister[i];
                deltaTime = posTimeRegister[i + 1] - posTimeRegister[i];

                if (deltaTime == 0)
                {
                    return false;
                }

                speedA = deltaDistance / deltaTime;
                deltaDistance = positionRegister[i + 2] - positionRegister[i + 1];
                deltaTime = posTimeRegister[i + 2] - posTimeRegister[i + 1];

                if (deltaTime == 0)
                {
                    return false;
                }

                speedB = deltaDistance / deltaTime;
                averageSpeedChange += speedB - speedA;
            }

            averageSpeedChange /= positionRegister.Length - 2;
            float deltaTimeTotal = posTimeRegister[posTimeRegister.Length - 1] - posTimeRegister[0];
            vector = averageSpeedChange / deltaTimeTotal;
            return true;
        }

        return false;
    }

    void OnDestroy()
    {
        client.Close();
        client = null;
        remoteEndPoint = null;
    }
}
