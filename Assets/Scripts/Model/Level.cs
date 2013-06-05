using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;

/// <summary>
/// Level. Singleton Class
/// </summary>
public class Level : MonoBehaviour
{
	
	#region Variables
	
	/// <summary>
	/// The actual selected cube.
	/// </summary>
	private Cube selectedCube;
	private ArrayList sensors;
    private Dictionary<Vector3Int, GameEntity> entities;
	private Dictionary<Vector3Int, List<BasicSensor>> sensorSpaces; // Dictionary that determines which spaces activate the sensors
	public int sensorsLeft = 0;
	public int stepCount = 0;
    private static Level singleton;
	private Texture2D[] images = new Texture2D[4];
    private GUISkin skin;
    private bool showHint = false;
    private bool isMenu = false;
    private bool isExiting = false;
    private CameraDecubeLevel levelCamera;
	private static Vector2 dimension = new Vector2(0, 10);
	
    private TextReader tr;
	private List<string> hints;
	private string path;
    private bool askedForHint = false;
	#endregion

    private void restartLevel()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    private void ExitLevel()
    {
        Application.LoadLevel("MenuDecubeOnline");
    }

	#region Entities Dictionary Management
	
	public void AddEntity (GameEntity entity, Vector3 position)
	{
		AddEntity (entity, new Vector3Int (position));
	}
	
	public void AddEntity (GameEntity entity, Vector3Int position)
	{
		entities.Add (position, entity);
		if (entity is BasicSensor) {
			AddSensor ((BasicSensor)entity);
			sensorsLeft += 1;
		} else {
			if (sensorSpaces.ContainsKey (position)) {
				Cube c = (Cube)entity;
				c.setMood(Cube.Mood.Proud);
				foreach (BasicSensor s in sensorSpaces[position]) {
					s.NotifyPressed (position);
				}
			}
			if(position.y > 1 && ContainsElement(position.ToVector3 + Vector3.down) && Level.Singleton.getEntity(position.ToVector3 + Vector3.down) is Cube){
				Cube c = (Cube)Level.Singleton.getEntity(position.ToVector3 + Vector3.down);
				c.setMood(Cube.Mood.Angry);	
			}
		}
	}
	
	public void RemoveEntity (Vector3 position)
	{
		RemoveEntity (new Vector3Int (position));
			
	}
	
	public void RemoveEntity (Vector3Int position)
	{
		GameEntity temp = entities [position];
		entities.Remove (position);
	
		if (temp is BasicSensor) {
			RemoveSensor ((BasicSensor)temp);
		} else {
			if (sensorSpaces.ContainsKey (position)) {
				
				foreach (BasicSensor s in sensorSpaces[position]) {
					s.NotifyUnpressed (position);
				}
			}
			if(entities.ContainsKey(new Vector3Int( position.ToVector3+Vector3.down))){
			try{
				if(getEntity(position.ToVector3+Vector3.down)is Cube){
					Cube c = (Cube)getEntity(position.ToVector3+Vector3.down);
					c.setMood(Cube.Mood.Normal);
				}
			}catch(Exception e){}
			}
		}	
	}
	
	public GameEntity getEntity (Vector3 position)
	{
		return entities [new Vector3Int (position)];
	}
	
	///TODO: REvisar si borrar o no
	public bool ContainsElement (Vector3 position)
	{
		return entities.ContainsKey (new Vector3Int (position));
	}
	
	public bool ContainsElement (Vector3Int position)
	{
		return entities.ContainsKey (position);
	}
	
	#endregion
	
	#region Sensor Spaces Management
	
	public void AddSensor (BasicSensor sensor)
	{
		foreach (Vector3 direction in sensor.Directions) {
			Vector3Int pos = new Vector3Int (sensor.transform.position + direction);
			if (!sensorSpaces.ContainsKey (pos)) {
				sensorSpaces [pos] = new List<BasicSensor> ();
			}
			sensorSpaces [pos].Add (sensor);
		}
	}
	
	public void RemoveSensor (BasicSensor sensor)
	{
		foreach (Vector3 direction in sensor.Directions) {
			Vector3Int pos = new Vector3Int (sensor.transform.position + direction);
			sensorSpaces [pos].Remove (sensor);
		}
	}
	
	public bool ContainsSensor(Vector3 position){
		return SensorSpaces.ContainsKey (new Vector3Int (position));
	}
	
	public void SensorActivated ()
	{
		sensorsLeft --;
		if (sensorsLeft == 0 && !isExiting){  
			StartCoroutine(GoBack());
			levelCamera.PlayWin();
			
			isExiting = true;
			if (UserSettings.Instance.CurrentPlayer.Levels.ContainsKey(Application.loadedLevelName)){
				UserSettings.Instance.CurrentPlayer.Levels.Remove(Application.loadedLevelName);
			}
			UserSettings.Instance.CurrentPlayer.Levels.Add(Application.loadedLevelName,new LevelData(Application.loadedLevelName,stepCount));
			UserSettings.Instance.SavePlayerData();
		}
	}
	
	IEnumerator GoBack(){
		yield return new WaitForSeconds(3f);
		ExitLevel();
	}
	
	public void SensorDeactivated(){
		sensorsLeft++;
	}
	
	public void notifySwitches(Vector3Int position){
		if (sensorSpaces.ContainsKey (position)) {
			foreach (BasicSensor s in sensorSpaces[position]) {
				if(s is SensorSwitch){
					s.NotifyPressed (position);
				}
			}
		}
	}
	#endregion
	
	#region Monobehaviours Methods
	
	#region Networking
	
	public static string ip = "127.0.0.1";
	public static string port = "25000";
	ConnectionTesterStatus status;
	NetworkView networkView;
	public static bool server = true;
 
	void LaunchServer ()
	{
		Network.incomingPassword = "diaz";
		bool useNat = !Network.HavePublicAddress();
		Network.InitializeServer( 8,25000,false);
	}
	
	void ConnectServer ()
	{
		Network.Connect (ip, int.Parse(port),"diaz");
	}
	
	[RPC]
	void MoveCubeTo(Vector3 cubePosition, Vector3 cubeNextPosition){
		Vector3Int position  = new Vector3Int(cubePosition);
		if (entities.ContainsKey(position)){
			Cube cube = (Cube)entities[position];
			cube.MoveTo(new Vector3Int(cubeNextPosition));
		}else{
			Debug.Log("Wrong Movement");
		}
	}
	
	public void MoveCube(Vector3 cubePosition, Vector3 cubeNextPosition){
		networkView.RPC("MoveCubeTo",RPCMode.All,cubePosition,cubeNextPosition);
	}
	
	[RPC]
	void RemoveCube(Vector3 cubePosition){
		this.RemoveEntity(new Vector3Int(cubePosition));
	}
	
	[RPC]
	void PlayerConnected(){
		mngr.enabled = true;
	}
	
	#endregion
	
	public MouseInputManager mngr;
	public GameObject cam;
	
	void Awake ()
	{
		networkView = GetComponent<NetworkView>();
		cam = GameObject.Find("Camera");
		mngr = cam.GetComponent<MouseInputManager>();
		if (server){
			LaunchServer();
			mngr.enabled = false;
			StartCoroutine(checkPlayers());
		}else{
			ConnectServer();
		}
        if (Application.loadedLevelName == "MainMenu" || Application.loadedLevelName == "Options" || Application.loadedLevelName == "PlanetSelector" || Application.loadedLevelName == "ProfileSelector")
        {
            isMenu = true;
        }
		entities = new Dictionary<Vector3Int, GameEntity> (new Vector3EqualityComparer ());
		sensorSpaces = new Dictionary<Vector3Int, List<BasicSensor>> (new Vector3EqualityComparer ());
        images[0] = Resources.Load("Art/Textures/GUI/button_exit") as Texture2D;
        images[1] = Resources.Load("Art/Textures/GUI/button_hint") as Texture2D;
        images[2] = Resources.Load("Art/Textures/GUI/button_restart") as Texture2D;
        skin = Resources.Load("Art/Textures/GUI/ingame_skin") as GUISkin;
        foreach (GameObject go in FindObjectsOfType(typeof(GameObject))){
        	if (go.name == "Camera"){
        		CameraDecubeLevel c = go.GetComponent<CameraDecubeLevel>();
        		levelCamera = c;
        		break;
        	}
        }
		hints = new List<string>();
		path = "Assets/Resources/Txt/hints.txt";
		try{
        	tr = new StreamReader(path);
			string temp;
	        while((temp = tr.ReadLine()) != null)
	        {
				if(hints.Count == 12){
	            	hints.Add(temp);
				}
	            hints.Add(temp);
	        }
	 
	        // Close the stream
	        tr.Close();
		} 
		catch(FileLoadException e) {
			Debug.LogException(e);
		}
	}

    void OnGUI()
    {
        if (isMenu) return;

        if(skin != null){
            GUI.skin = skin;
        }

		if (!mngr.enabled){
			GUI.Label(new Rect(Screen.width/4,50,500,70),"Waiting for Player...");
		}

        float d = images[0].width * 0.75f;
        if (GUI.Button(new Rect(5, 5, d, d), images[0]))
        {
            ExitLevel();
        }

//        if (GUI.Button(new Rect(d + 10, 5, d, d), images[2]))
//        {
//            restartLevel();
//        }
//
//        if (GUI.Button(new Rect(Screen.width - d -5, Screen.height - d - 5, d, d), images[1])) 
//        {
//            showHint = !showHint;
//        }
//        
        float hintWidth = 512.0f * 0.75f , hintHeight = 128.0f * 0.75f;
        if (showHint)
        {
            if(!askedForHint){
                askedForHint = true;
            }
            //Quemo el tamaño de la textura de fondo
            GUI.TextArea(new Rect(Screen.width / 2 - hintWidth / 2 - 5, Screen.height - hintHeight - 5, hintWidth, hintHeight), hints[Int16.Parse(Application.loadedLevelName) - 1]);
        }

        Rect stepCountRect = new Rect(Screen.width - d - 5, 5, d, d);
        GUI.Box(stepCountRect, "");
        GUI.Label(stepCountRect, stepCount.ToString());

        if (sensorsLeft == 0)
        {
            GUI.Label(new Rect(0, 20, Screen.width, 80), "Ganaste!!");
			GUI.Label(new Rect(0, 70, Screen.width, 80), "Numero de pasos: " + stepCount);
        }
        
        GUI.Label(new Rect( 50, Screen.height - 100,200,70),"P." + Network.connections.Length);
    }
    
    IEnumerator checkPlayers(){
    	while (true){
        	if (Network.connections.Length > 0){
    			mngr.enabled = true;
	    	}else{
	    		mngr.enabled = false;
	    	}
	    	yield return new WaitForSeconds(3);
    	}
    }

	#endregion
	
	#region Steps Management
	
	public void addStep(){
		stepCount++;
	}
	
	#endregion
	
	#region Gets and Sets
	
    public static Vector2 Dimension
    {
        get { return Level.dimension; }
    }
	
	public static Level Singleton
    {
        get
        {
            if (singleton == null)
            {
            	GameObject lvl = new GameObject("Level");
                lvl.AddComponent<NetworkView>();
                singleton = lvl.AddComponent<Level>();
            }
            return singleton;
        }
    }
	
	/// <summary>
	/// The cube that is selected in the whole world.
	/// </summary>
	public Cube SelectedCube
    {
        set
        {
            if (selectedCube != value)
       	 	{
            	if(selectedCube != null){
            	    selectedCube.IsSelected = false;
            	}
            	if (value != null){
            		value.IsSelected = true;
            	}
            	selectedCube = value;
			}
        }	
		
		get { return selectedCube; }
    }
	
	
	

	public Dictionary<Vector3Int, List<BasicSensor>> SensorSpaces {
		get {
			return this.sensorSpaces;
		}
	}
	#endregion

	public bool IsInDimension (Vector3 position)
	{
		return position.x >= dimension.x && position.x <= dimension.y && // Check x
			position.z >= dimension.x && position.z <= dimension.y; // Check y
	}
}