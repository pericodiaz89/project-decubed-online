using UnityEngine;
using System.Collections.Generic;

public class IceCube : Cube {
	
//	private Vector3Int nextPosition;
	private bool breakIce = false;
	private Vector3Int endPosition;
	private Vector3Int nextPosition;
	public bool comandEnded = false;
	
	public override void MoveTo (Vector3Int endPosition)
	{
		this.endPosition = endPosition;
		Level.Singleton.RemoveEntity(new Vector3Int(transform.position));
        Level.Singleton.AddEntity(this, endPosition);
		if(NextPosition.y == new Vector3Int(transform.position).y){
			CubeAnimations.AnimateSlide(gameObject,endPosition.ToVector3);
		}else{
			CubeAnimations.AnimateMove (gameObject, Vector3.down, nextPosition.ToVector3);
		}
	}
	
	public override Command[] GetOptions(){ 
		List<Command> options = new List<Command>();
			Vector3Int pos;
			if (CubeHelper.CheckAvailablePosition(transform.position + Vector3.forward,out pos,GetJumpHeight())){
				options.Add(new Slide(this,pos,new Vector3Int(Vector3.forward)));
			}
			if (CubeHelper.CheckAvailablePosition(transform.position + Vector3.back,out pos,GetJumpHeight())){
				options.Add(new Slide(this,pos,new Vector3Int(Vector3.back)));
			}
			if (CubeHelper.CheckAvailablePosition(transform.position + Vector3.right,out pos,GetJumpHeight())){
				options.Add(new Slide(this,pos,new Vector3Int(Vector3.right)));
			}
			if (CubeHelper.CheckAvailablePosition(transform.position + Vector3.left,out pos,GetJumpHeight())){
				options.Add(new Slide(this,pos,new Vector3Int(Vector3.left)));
			}
            return options.ToArray();
	}
	
	public void Break(){
		breakIce = true;
	}
	
	public override void EndExecution ()
	{
		OrganizeTransform();
		if(Command != null && endPosition.ToVector3 ==  new Vector3Int(transform.position).ToVector3){
			//fix
			Command.EndExecution();
			comandEnded = false;
		}
		OnEndExecution();
	}
	
	public override void OnEndExecution ()
	{
		if(breakIce &&  endPosition.ToVector3 == new Vector3Int(transform.position).ToVector3){
			Level.Singleton.RemoveEntity(endPosition);
			Destroy(this.gameObject);
		}else if( endPosition.z != transform.position.z || endPosition.x != transform.position.x){
			CubeAnimations.AnimateSlide(gameObject, new Vector3Int(endPosition.x,Mathf.RoundToInt(transform.position.y),endPosition.z).ToVector3);
		}else if( endPosition.y < transform.position.y){
			CubeAnimations.AnimateMove (gameObject, Vector3.down, endPosition.ToVector3);
		}
		Vector3Int next = new Vector3Int(transform.position);
		if(!breakIce && next.x > 10 || next.x < 0 || next.z > 10 || next.z < 0){
			Level.Singleton.RemoveEntity(new Vector3Int(transform.position));
			Destroy(gameObject);
		}
		
	}
	
	public Vector3Int NextPosition {
		get {
			return this.nextPosition;
		}set{
			this.nextPosition = value;
		}
	}
}
