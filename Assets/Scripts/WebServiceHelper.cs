using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Collections;
public class WebServiceHelper: Observable {

    private WWW requestedWWW;
    private IObserver currentObserver;
    private Dictionary<IObserver, Queue<string>> responses;

    public Dictionary<IObserver, Queue<string>> Responses
    {
        get { return responses; }

    }
    private Queue<Request> pendingRequests;
  
    void Awake() {
        responses = new Dictionary<IObserver, Queue<string>>();
        pendingRequests = new Queue<Request>();
    }

    void Update() { 
        if(requestedWWW != null && requestedWWW.isDone){
            string responseString = Encoding.UTF8.GetString(requestedWWW.bytes);
            if (!responses.ContainsKey(currentObserver)) {
                responses.Add(currentObserver, new Queue<string>());
            }
            responses[currentObserver].Enqueue(Encoding.UTF8.GetString(requestedWWW.bytes));
            setChanges();
            notifyObserver(currentObserver);
            currentObserver = null;
            requestedWWW = null;
        }
        if(pendingRequests.Count>0 && requestedWWW==null){
            StartCoroutine(makeRequest(pendingRequests.Dequeue()));
        }
    }

    public void callServiceGet(string serviceURL, Dictionary<string, object> parameters, IObserver observer)
    {
        string getData = "?";
        foreach (KeyValuePair<string, object> kv in parameters)
        {
            getData += kv.Key + "=" + kv.Value + "&";
        }
        getData = getData.Substring(0, getData.Length - 1);

        /*WebRequest request = WebRequest.Create(serviceURL + getData);
        WebResponse response = request.GetResponse();*/
        pendingRequests.Enqueue(new Request(observer, serviceURL + getData, null));
    }

    private IEnumerator makeRequest(Request data) {
        if (data.PostData == null)
        {
            requestedWWW = new WWW(data.Url);
        }
        else
        {
            requestedWWW = new WWW(data.Url, data.PostData);
        }
        currentObserver = data.Observer;
        yield return requestedWWW;
    }

    public void callServicePost(string serviceURL, Dictionary<string, object> parameters, IObserver observer)
    {
        //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceURL);
        string postData = "";
        foreach (KeyValuePair<string, object> kv in parameters)
        {
            postData += kv.Key + "=" + kv.Value + "&";
        }
        postData = postData.Substring(0, postData.Length - 1);


        UTF8Encoding encoding = new UTF8Encoding();
        byte[] byteArray = encoding.GetBytes(postData);

        /*
        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = byteArray.Length;

        using (Stream stream = request.GetRequestStream())
        {
            stream.Write(byteArray, 0, byteArray.Length);
        }

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();*/
        pendingRequests.Enqueue(new Request(observer, serviceURL, byteArray));
    }
}

public class Request {
    private IObserver observer;

    public IObserver Observer
    {
        get { return observer; }
        set { observer = value; }
    }
    private string url;

    public string Url
    {
        get { return url; }
        set { url = value; }
    }
    private byte[] postData;

    public byte[] PostData
    {
        get { return postData; }
        set { postData = value; }
    }

    public Request(IObserver observer, string url, byte[] postData) {
        this.observer = observer;
        this.url = url;
        this.postData = postData;
    }
}

public enum WebServiceType
{
    Get, Post
}