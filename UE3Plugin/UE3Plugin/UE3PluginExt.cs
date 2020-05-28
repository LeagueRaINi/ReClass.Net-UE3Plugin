using UE3Plugin.Target;
using UE3Plugin.Utils;

using ReClassNET.Extensions;
using ReClassNET.Plugins;
using ReClassNET.Memory;
using ReClassNET.Nodes;

using System.Collections.Generic;
using System;

namespace UE3Plugin
{
    public class UE3PluginExt : Plugin
    {
        /// <summary>
        /// Enum of supported games.
        /// </summary>
        public enum GameType
        {
            RocketLeague,
            KillingFloor2
        }

        /// <summary>
        /// The game
        /// </summary>
        public GameType Game;

        /// <summary>
        /// Contains the names from the GNames array + their index.
        /// </summary>
        public Dictionary<int, string> NameDump = new Dictionary<int, string>();

        /// <summary>
        /// Gets called when reclass loads our plugin.
        /// </summary>
        /// <param name="host">The plugin host.</param>
        public override bool Initialize(IPluginHost host)
        {
            host.Process.ProcessAttached += OnProcessAttached;

            return true;
        }

        /// <summary>
        /// Gets called once to receive all node info readers the plugin provides.
        /// </summary>
        public override IReadOnlyList<INodeInfoReader> GetNodeInfoReaders()
        {
            return new[] { new UObjectInfoNodeReader(this) };
        }

        /// <summary>
        /// Called when [process attached].
        /// </summary>
        /// <param name="sender">The process instance.</param>
        void OnProcessAttached(RemoteProcess sender)
        {
            sender.UpdateProcessInformations();

            var mainModule = sender.GetModuleByName(sender.UnderlayingProcess.Name);
            if (mainModule is null)
                return;

            dynamic gnames = null;
            switch (sender.UnderlayingProcess.Name)
            {
                case "RocketLeague.exe":
                {
                    Game = GameType.RocketLeague;

                    var namesPtr = PatternScanner.Search(sender, mainModule, "E8 [....] 48 83 CF FF 45 85 FF", (bytes, address) =>
                    {
                        address += BitConverter.ToInt32(bytes, address) + 0x4  + 0x2c;
                        address += BitConverter.ToInt32(bytes, address  + 0x3) + 0x7;
                        return address;
                    });
                    if (namesPtr.IsNull())
                        return;

                    gnames = sender.ReadRemoteObject<RocketLeague.GNames>(namesPtr);
                    break;
                }
                case "KFGame.exe":
                {
                    Game = GameType.KillingFloor2;

                    var namesPtr = PatternScanner.Search(sender, mainModule, "E8 [....] 48 83 CB FF 45 85 F6", (bytes, address) =>
                    {
                        address += BitConverter.ToInt32(bytes, address) + 0x4  + 0x7a;
                        address += BitConverter.ToInt32(bytes, address  + 0x3) + 0x7;
                        return address;
                    });
                    if (namesPtr.IsNull())
                        return;

                    gnames = sender.ReadRemoteObject<KillingFloor2.GNames>(namesPtr);
                    break;
                }
                default:
                    Terminate();
                    return;
            }

            if (gnames.Names.Num is 0)
                return;

            NameDump.Clear();

            for (var i = 0; i < gnames.Names.Num; i++)
            {
                var nameEntry = gnames.Names.Read(sender, i, true);
                if (nameEntry.Name is null)
                    continue;

                NameDump.Add(i, nameEntry.Name);
            }
        }
    }

    public class UObjectInfoNodeReader : INodeInfoReader
    {
        /// <summary>
        /// A reference to our base class.
        /// </summary>
        UE3PluginExt _base;

        /// <summary>
        /// Initializes a new instance of the <see cref="UObjectInfoNodeReader"/> class.
        /// </summary>
        /// <param name="baseClass">The base class.</param>
        public UObjectInfoNodeReader(UE3PluginExt baseClass)
        {
            _base = baseClass;
        }

        /// <summary>
        /// Used to print custom informations about a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="reader">The current <see cref="T:ReClassNET.Memory.IRemoteMemoryReader" />.</param>
        /// <param name="memory">The current <see cref="T:ReClassNET.Memory.MemoryBuffer" />.</param>
        /// <param name="nodeAddress">The absolute address of the node.</param>
        /// <param name="nodeValue">The memory value of the node as <see cref="T:System.IntPtr" />.</param>
        public string ReadNodeInfo(BaseHexCommentNode node, IRemoteMemoryReader reader, MemoryBuffer memory, IntPtr nodeAddress, IntPtr nodeValue)
        {
            if (nodeValue.IsNull() || _base.NameDump.Count is 0)
                return null;

            dynamic uobject = null;
            switch (_base.Game)
            {
                case UE3PluginExt.GameType.KillingFloor2:
                {
                    uobject = reader.ReadRemoteObject<KillingFloor2.UObject>(nodeValue);
                    break;
                }
                case UE3PluginExt.GameType.RocketLeague:
                {
                    uobject = reader.ReadRemoteObject<RocketLeague.UObject>(nodeValue);
                    break;
                }
            }

            if (uobject.Name.Index < 0 || uobject.Name.Index >= _base.NameDump.Count)
                return null;

            if (_base.NameDump.TryGetValue(uobject.Name.Index, out string name))
                return name;

            return null;
        }
    }
}
