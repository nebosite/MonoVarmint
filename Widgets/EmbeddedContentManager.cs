using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MonoVarmint.Widgets
{
    //-----------------------------------------------------------------------------------------------
    // EmbeddedContentManager- A version of content manager that can tries to load content from 
    // embedded resources first.
    //
    // The resources can be located anywhere in the assembly.  The search algorithm is case insensitive
    // and the first matching resource will be used, so be careful of name collisions.
    //-----------------------------------------------------------------------------------------------
    class EmbeddedContentManager : ContentManager
    {
        Assembly _localAssembly;

        public EmbeddedContentManager(GraphicsDevice graphicsDevice) : base(new EmbeddedContentServiceManager(graphicsDevice))
        {
            _localAssembly = Assembly.GetExecutingAssembly();
        }

        protected override Stream OpenStream(string assetName)
        {
            // Look in the embedded resources first
            var searchTerm = "." + assetName.ToLower() + ".xnb";
            foreach(var resourceName in _localAssembly.GetManifestResourceNames())
            {
                if(resourceName.ToLower().EndsWith(searchTerm))
                {
                    return _localAssembly.GetManifestResourceStream(resourceName);
                }
            }

            return base.OpenStream(assetName);
        }


    }

    //-----------------------------------------------------------------------------------------------
    // EmbeddedContentServiceManager - Needed by EmbeddedContentManager
    //-----------------------------------------------------------------------------------------------
    class EmbeddedContentServiceManager : IServiceProvider, IGraphicsDeviceService
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;

        public EmbeddedContentServiceManager(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }
        public object GetService(Type serviceType)
        {
            if (serviceType.Name != "IGraphicsDeviceService")
            {
                throw new ApplicationException("Don't know how to prived a " + serviceType.Name);
            }

            return this;
        }
    }
}
