# FishNetReqRes
A Request and response addon for FishNet

## How to use it

Attach `RequestResponseManager` to any game object, but it must be able to access to network manager, then set ref of network manager to it.

Register requests by uses functions from `RequestResponseManager` class, example:

```
// Request Data
public struct TestReq
{
    public int a;
    public int b;
}

public static partial class CustomSerializer
{
    public static void WriteTestReq(this Writer writer, TestReq value)
    {
        writer.WriteInt32(value.a);
        writer.WriteInt32(value.b);
    }

    public static TestReq ReadTestReq(this Reader reader)
    {
        return new TestReq()
        {
            a = reader.ReadInt32(),
            b = reader.ReadInt32(),
        };
    }
}
// Response Data
public struct TestRes
{
    public int c;
}

public static partial class CustomSerializer
{
    public static void WriteTestRes(this Writer writer, TestRes value)
    {
        writer.WriteInt32(value.c);
    }

    public static TestRes ReadTestRes(this Reader reader)
    {
        return new TestRes()
        {
            c = reader.ReadInt32(),
        };
    }
}
// Register class, attach this to the same game object with `RequestResponseManager`
public class TestRequestResponse : MonoBehaviour
{
    private RequestResponseManager reqResMgr;

    public int a = 0;
    public int b = 0;
    public bool sendUpdate;

    private void Start()
    {
        reqResMgr = GetComponent<RequestResponseManager>();
        reqResMgr.RegisterRequestToServer<TestReq, TestRes>(1, RequestHandler, ResponseHandler);
    }
    public void Update()
    {
        if (sendUpdate)
        {
            sendUpdate = false;
            reqResMgr.ClientSendRequest(1, new TestReq()
            {
                a = a,
                b = b,
            });
        }
    }

    void RequestHandler(RequestHandlerData requestHandler, TestReq request, RequestProceedResultDelegate<TestRes> result)
    {
        Debug.Log("Request: " + request.a + ", " + request.b);
        result.Invoke(AckResponseCode.Success, new TestRes()
        {
            c = request.a + request.b
        });
    }

    void ResponseHandler(ResponseHandlerData requestHandler, AckResponseCode responseCode, TestRes response)
    {
        Debug.Log("Response: " + responseCode + " = " + response.c);
    }
}
```
