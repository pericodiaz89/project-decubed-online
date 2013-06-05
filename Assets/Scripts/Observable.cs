using UnityEngine;
using System.Collections;
 
public class Observable: MonoBehaviour {
    private ArrayList observers;
    private bool changed = false;
 
    void Awake() {
        observers = new ArrayList();
    }
 
    public void addObserver(IObserver observer) {
        observers.Add(observer);
    }
 
    protected void setChanges(){
        changed = true;
    }
 
    protected bool isChanged() {
        return changed;
    }
 
    protected void notifyAllObservers() {
        if(isChanged()){
            changed = false;
            foreach (IObserver observer in observers) {
                observer.UpdateObserver(this);
            }
        }
    }
 
    protected void notifyObserver(IObserver observer) {
        if (isChanged())
        {
            changed = false;
            observer.UpdateObserver(this);
        }
    }
    public void removeObserver(IObserver observer) {
        observers.Remove(observer);
    }
}