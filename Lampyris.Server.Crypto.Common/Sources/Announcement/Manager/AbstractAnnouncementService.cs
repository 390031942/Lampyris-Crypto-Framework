namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;

[Component]
public abstract class AbstractAnnouncementService
{
    public delegate void OnAnnouncementReceivedDelegate(AnnouncementInfo announcementInfo);

    public OnAnnouncementReceivedDelegate OnAnnouncementReceived;
}