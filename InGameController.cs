public partial class InGameController : ViewController
{
    private static InGameController _instance;
    public static InGameController Instance { get { return _instance; } }

    private Dictionary<int, CObject> ObjectManager { get; set; }

    public InGameController(View controlledView, byte subOperationCode = 0)
        : base(controlledView, subOperationCode)
    {
        _instance = this;
        ObjectManager = new Dictionary<int, CObject>();
        EventSubCodeHandlers.Add((int)MessageSubCode.UserInfo, new UserInfoHandler(this));
        EventSubCodeHandlers.Add((int)MessageSubCode.CharInfo, new CharInfoHandler(this));
        EventSubCodeHandlers.Add((int)MessageSubCode.MoveToLocation, new MoveToLocationHandler(this));
        EventSubCodeHandlers.Add((int)MessageSubCode.DeleteObject, new DeleteObjectHandler(this));
        InitializeChat();
        AddChatHandlers();
        InitializeMovement();
        AddMovementHandlers();
    }

    // LOCAL PLAYER
    public void CreateOrUpdateUser(int objectId, UserInfo userInfo)
    {
        GameObject user = ObjectManager.ContainsKey(objectId) ? ObjectManager[objectId].gameObject : null;
        if (user != null)
        {
            //UPDATE USER INFO WITH THE PACKET DATA
            User.Health = userInfo.Health;
        }
        else
        {
            ControlledView.LogInfo("Instantiate User");
            var userPrefab = Resources.Load("User");

            var obj = (GameObject)Object.Instantiate(userPrefab, new Vector3(userInfo.Position.X, userInfo.Position.Y, userInfo.Position.Z),
                                                        new Quaternion());
            
            obj.name = "" + objectId;
            User = obj.GetComponent<CObject>();
            User.ObjectId = objectId;

            ///////////////////
            // DEBUG INFO
            ControlledView.LogInfo("Health: " + User.Health.ToString());
            ///////////////////

            ObjectManager.Add(objectId, User);
        }
    }

    // NETWORK PLAYER
    public void CreateOrUpdatePlayer(int objectId, CharInfo userInfo)
    {
        GameObject player = ObjectManager.ContainsKey(objectId) ? ObjectManager[objectId].gameObject : null;
        if (player != null)
        {
            //UPDATE USER INFO WITH THE PACKET DATA
        }
        else
        {
            ControlledView.LogInfo("Instantiate Player");
            var playerPrefab = Resources.Load("Player");
            var obj = (GameObject)Object.Instantiate(playerPrefab, new Vector3(userInfo.Position.X, userInfo.Position.Y, userInfo.Position.Z),
                                                        new Quaternion());
            obj.name = "" + objectId;
            var cobj = obj.GetComponent<CObject>();
            cobj.FinalPosition = new Vector3(userInfo.Position.X, userInfo.Position.Y, userInfo.Position.Z);
            ObjectManager.Add(objectId, cobj);
        }
    }

    public void SendPlayerInGame()
    {
        Dictionary<byte, object> parameters = new Dictionary<byte,object>();

        parameters.Add((byte)ClientParameterCode.SubOperationCode, MessageSubCode.PlayerInGame);

        OperationRequest request = new OperationRequest { OperationCode = (byte)ClientOperationCode.Region, Parameters = parameters };

        SendOperation(request, true, 0, true);
    }

    public void UpdatePosition(int objectId, MoveTo moveTo)
    {
        var cobj = ObjectManager.ContainsKey(objectId) ? ObjectManager[objectId] : null;
        if (cobj != null)
        {
            cobj.UpdatePosition(new Vector3(moveTo.CurrentPosition.X, moveTo.CurrentPosition.Y, moveTo.CurrentPosition.Z), 
                moveTo.Speed, moveTo.Moving, moveTo.Direction, moveTo.Facing, User.ObjectId == objectId);
        }
        ControlledView.LogInfo("UPDATEPOSITION BEING CALLED!");
        ControlledView.LogInfo("Facccing: " + moveTo.Facing.ToString());
        ControlledView.LogInfo("isUser: " + (User.ObjectId == objectId).ToString());
    }

    public void DeleteObject(int i)
    {
        //Add fade out alpha then delete object

        if (ObjectManager.ContainsKey(i))
        {
            ControlledView.LogInfo("Deleting Object: " + i);
            Object.Destroy(ObjectManager[i].gameObject);
            ObjectManager.Remove(i);
        }
    }
}
