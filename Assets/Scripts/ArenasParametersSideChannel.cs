// using MLAgents.SideChannels;
using System;
using ArenasParameters;

public class ArenasParametersSideChannel : SideChannel
{

    public ArenasParametersSideChannel()
    {
        ChannelId = new Guid("9c36c837-cad5-498a-b675-bc19c9370072");
    }

    public override void OnMessageReceived(IncomingMessage msg)
    {
        // when a new message is received we trigger an event to signal the environment
        // configurations to check if they need to update

        ArenasParametersEventArgs args = new ArenasParametersEventArgs();
        args.Proto = msg.GetRawBytes();
        OnArenasParametersReceived(args);
    }

    protected virtual void OnArenasParametersReceived(ArenasParametersEventArgs arenasParametersEvent)
    {
        EventHandler<ArenasParametersEventArgs> handler = NewArenasParametersReceived;
        if (handler != null)
        {
            handler(this, arenasParametersEvent);
        }
    }

    public EventHandler<ArenasParametersEventArgs> NewArenasParametersReceived;


    // TODO: maybe add feedback on which items haven't been spawned ??

    // public void SendDebugStatementToPython(string logString, string stackTrace, LogType type)
    // {
    //     if (type == LogType.Error)
    //     {
    //         var stringToSend = type.ToString() + ": " + logString + "\n" + stackTrace;
    //         using (var msgOut = new OutgoingMessage())
    //         {
    //             msgOut.WriteString(stringToSend);
    //             QueueMessageToSend(msgOut);
    //         }
    //     }
    // }
}