using System;
using FFS.Libraries.StaticEcs;

namespace Code.EcsUi.Mvc
{
    public sealed class MvcManagerResource : IResource
    {
        public MvcManagerResource(IMvcManager manager)
        {
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public IMvcManager Manager { get; }
    }
}
