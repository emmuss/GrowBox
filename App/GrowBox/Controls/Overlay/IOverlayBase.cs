using Microsoft.AspNetCore.Components;

namespace GrowBox.Controls.Overlay;

public interface IOverlayBase : IComponent
{
    public Guid InstanceId { get; }
}
