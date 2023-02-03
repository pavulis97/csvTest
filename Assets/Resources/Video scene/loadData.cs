using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum CsvType { withGps, noGps};
        


public class loadData : MonoBehaviour
{
    public UDPSend udpSend;

    private enum SendThroughUDP { Yes, No }
        [SerializeField] SendThroughUDP useUDP = SendThroughUDP.Yes;    
    [SerializeField] CsvType csvType = CsvType.noGps;
    [SerializeField] private string dataFileNameNoGps = "impDataNoGPSShort";
    [SerializeField] private string dataFileNameWithGps = "impDataShort";

    CsvLoader csv;

    public float xAngle, yAngle, zAngle;
    public Rigidbody rb;
    public Rigidbody carRoll, carPitch;
    Quaternion rotation = Quaternion.Euler(30, 0, 0);
    Vector3 position = new Vector3(0f, 0f, 0f);

    public Vector3 carRollPosition = new Vector3(5.054374f, 4.371755f, 11.31f);
    public Vector3 carPitchPosition = new Vector3(4.897316f, 1.693563f, 11.31f);


    int currentIndex;
    
    [SerializeField] private string videoFilePath = "Video/POV_Sigulda_0097";


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //carRoll = GetComponent<Rigidbody>();
        //carPitch = GetComponent<Rigidbody>();

        csv = new CsvLoader(csvType, dataFileNameNoGps, dataFileNameWithGps);
        csv.loadCSV();

        GameObject camera = GameObject.Find("Main Camera");

        // VideoPlayer automatically targets the camera backplane when it is added
        // to a camera object, no need to change videoPlayer.targetCamera.
        var videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();

        videoPlayer.playOnAwake = false;

        // By default, VideoPlayers added to a camera will use the far plane.
        // Let's target the near plane instead.
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraFarPlane;

        // This will cause our Scene to be visible through the video being played.
        //videoPlayer.targetCameraAlpha = 0.5F;

        // Set the video to play. URL supports local absolute or relative paths.
        // Here, using absolute.
        videoPlayer.url = videoFilePath;

        videoPlayer.frame = 0;

        // Restart from beginning when done.
        videoPlayer.isLooping = true;


        // Start playback. This means the VideoPlayer may have to prepare (reserve
        // resources, pre-load a few frames, etc.). To better control the delays
        // associated with this preparation one can use videoPlayer.Prepare() along with
        // its prepareCompleted event.
        videoPlayer.Play();
    }

    void FixedUpdate()
    {
        currentIndex = csv.searchForClosestTimestamp((decimal)Time.time);

        rotation = csv.getOrientationQuaternion(currentIndex);
        rb.transform.SetPositionAndRotation(position, rotation);

        carRoll.transform.SetPositionAndRotation(carRollPosition, csv.getRoll(currentIndex));
        carPitch.transform.SetPositionAndRotation(carPitchPosition, csv.getPitch(currentIndex, -90f));
        //Debug.Log(csv.getOrientationQuaternion(currentIndex));
        //Debug.Log(csv.getGravityFreeAcceleration(currentIndex)+ "      " +csv.getZacc(currentIndex));

        if (useUDP == SendThroughUDP.Yes) {
            Debug.Log(csv.getGravityFreeXacc(currentIndex) + "      " +csv.getXacc(currentIndex));
            udpSend.sendString(csv.getGravityFreeXacc(currentIndex), csv.getGravityFreeYacc(currentIndex), csv.getZacc(currentIndex));
        } else if (useUDP == SendThroughUDP.No) {
           // Debug.Log(csv.getGravityFreeXacc(currentIndex) + "      " +csv.getGravityFreeYacc(currentIndex));
        }     
    }
    
}
