using System;

namespace SharpShell.ServerRegistration
{
    /// <summary>
    /// Represents registration info for a server.
    /// </summary>
    public class ServerRegistrationInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRegistrationInfo"/> class.
        /// </summary>
        /// <param name="serverRegistationType">Type of the server registation.</param>
        /// <param name="serverCLSID">The server CLSID.</param>
        public ServerRegistrationInfo(ServerRegistationType serverRegistationType, Guid serverCLSID)
        {
            ServerRegistationType = serverRegistationType;
            ServerCLSID = serverCLSID;
        }

        /// <summary>
        /// Gets the server CLSID.
        /// </summary>
        public Guid ServerCLSID { get; internal set; }

        /// <summary>
        /// Gets the server path.
        /// </summary>
        public string ServerPath { get; internal set; }

        /// <summary>
        /// Gets the threading model.
        /// </summary>
        public string ThreadingModel { get; internal set; }

        /// <summary>
        /// Gets the assembly version.
        /// </summary>
        public string AssemblyVersion { get; internal set; }

        /// <summary>
        /// Gets the assembly.
        /// </summary>
        public string Assembly { get; internal set; }

        /// <summary>
        /// Gets the class.
        /// </summary>
        public string Class { get; internal set; }

        /// <summary>
        /// Gets the runtime version.
        /// </summary>
        public string RuntimeVersion { get; internal set; }

        /// <summary>
        /// Gets the codebase path.
        /// </summary>
        public string CodeBase { get; internal set; }

        /// <summary>
        /// Gets the type of the server registation.
        /// </summary>
        /// <value>
        /// The type of the server registation.
        /// </value>
        public ServerRegistationType ServerRegistationType { get; internal set; }
    }
}
