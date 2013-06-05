using UnityEngine;
using System.Collections;
using System.Net;

public class DecubeOnlineMenu : MonoBehaviour {
	
	private string gameFinderServer = "127.0.0.1";
	private bool checking = false;
	private string waitingText = "";
	
	void OnGUI(){
		
		GUI.BeginGroup(new Rect(Screen.width/2 - 100,Screen.height/2 - 100,200,200));
		if (!checking){
		
			GUILayout.Label("Decube Online");
			
			GUILayout.Label("Game Finder Server");
			
			gameFinderServer = GUILayout.TextArea(gameFinderServer);
			
			if (GUILayout.Button("Start Decube Game")){
				//Level.server = true;
				//Application.LoadLevel("1");
				Level.server = true;
				StartCoroutine(RegisterServer());
				waitingText = "Waiting for another player";
				checking = true;
				
				//callServiceGet();
			}
			
			if (GUILayout.Button("Find Decube Game")){
				Level.server = false;
				StartCoroutine(GetServer());
				waitingText = "Looking for a Match...";
				checking = true;
			}
			
		}else{
			GUILayout.Label(waitingText);
			if (GUILayout.Button("Cancel Request")){
				checking = false;
			}
		}
		GUI.EndGroup();
		
	}
	
	public IEnumerator RegisterServer()
    {
    	WWW access = new WWW("http://" + gameFinderServer + "/DecubeGameFinder/?action=registrar");
    	while (!access.isDone){
    		yield return new WaitForSeconds(0.1f);
    	}
    	if (access.text.Equals("Success")){
    	Application.LoadLevel("1");
    	}else{
    		checking = false;
    	}
    }
    
	public IEnumerator GetServer()
    {
    	WWW access = new WWW("http://" + gameFinderServer + "/DecubeGameFinder/?action=buscar");
    	while (!access.isDone){
    		yield return new WaitForSeconds(0.1f);
    	}
    	if (!access.text.Equals("empty")){
    		Level.ip = access.text;
    		Application.LoadLevel("1");
    	}else{
    		checking = false;
    	}
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
