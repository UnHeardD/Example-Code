public class MoveToLocationHandler : PhotonEventHandler
{
    public MoveToLocationHandler(ViewController controller)
        : base(controller)
    {

    }

    public override byte Code
    {
        get { return (byte)ClientEventCode.ServerPacket; }
    }

    public override int? SubCode
    {
        get { return (int)MessageSubCode.MoveToLocation; }
    }

    public override void OnHandleEvent(EventData eventData)
    {
        //////////
        // DEBUG INFO
        //////////
        _controller.ControlledView.LogInfo("MoveToLocationHandler - Handled MoveTo");
        InGameController controller = _controller as InGameController;
        if (controller != null)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MoveTo));
            StringReader inStream = new StringReader((string)eventData.Parameters[(byte)ClientParameterCode.Object]);
            controller.UpdatePosition((int)eventData.Parameters[(byte)ClientParameterCode.ObjectId],
                ((MoveTo)serializer.Deserialize(inStream)));
        }
    }
}
