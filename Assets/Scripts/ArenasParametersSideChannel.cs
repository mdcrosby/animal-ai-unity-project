using Unity.MLAgents.SideChannels;
using System;
using ArenasParameters;

public class ArenasParametersSideChannel : SideChannel
{

    public ArenasParametersSideChannel()
    {
        ChannelId = new Guid("9c36c837-cad5-498a-b675-bc19c9370072");
    }

    protected override void OnMessageReceived(IncomingMessage msg)
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
    
    //Currently no way to get feedback for which items spawned successfully

}