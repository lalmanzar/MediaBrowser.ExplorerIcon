using System.Threading.Tasks;
using MediaBrowser.Common.Security;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.ExplorerIcon.Configuration;
using MediaBrowser.Model.Logging;

namespace MediaBrowser.ExplorerIcon
{
    /// <summary>
    ///     Class ServerEntryPoint
    /// </summary>
    public class ServerEntryPoint : IServerEntryPoint, IRequiresRegistration
    {
        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ServerEntryPoint Instance { get; private set; }

        /// <summary>
        ///     Access to the LibraryManager of MB Server
        /// </summary>
        public ILibraryManager LibraryManager { get; private set; }

        /// <summary>
        ///     Access to the SecurityManager of MB Server
        /// </summary>
        public ISecurityManager PluginSecurityManager { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServerEntryPoint" /> class.
        /// </summary>
        /// <param name="libraryManager"></param>
        /// <param name="logManager"></param>
        /// <param name="securityManager"></param>
        public ServerEntryPoint(ILibraryManager libraryManager, ILogManager logManager,
            ISecurityManager securityManager)
        {
            LibraryManager = libraryManager;
            PluginSecurityManager = securityManager;
            Plugin.Logger = logManager.GetLogger(Plugin.Instance.Name);

            Instance = this;
        }

        /// <summary>
        ///     Runs this instance.
        /// </summary>
        public void Run()
        {
        }

        /// <summary>
        ///     Called when [configuration updated].
        /// </summary>
        /// <param name="oldConfig">The old config.</param>
        /// <param name="newConfig">The new config.</param>
        public void OnConfigurationUpdated(PluginConfiguration oldConfig, PluginConfiguration newConfig)
        {
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        ///     Loads our registration information
        /// </summary>
        /// <returns></returns>
        public async Task LoadRegistrationInfoAsync()
        {
            //Plugin.Instance.Registration = await PluginSecurityManager.GetRegistrationStatus("MediaBrowser.ExplorerIcon", "[**MB2CompatibleFeature**]").ConfigureAwait(false);
        }
    }
}
