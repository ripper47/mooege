﻿/*
 * Copyright (C) 2011 D3Sharp Project
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using D3Sharp.Net;
using D3Sharp.Utils;
using Google.ProtocolBuffers;

namespace D3Sharp.Core.Games
{
    public class Game
    {
        protected static readonly Logger Logger = LogManager.CreateLogger();

        /// <summary>
        /// Actual game id.
        /// </summary>
        public ulong ID { get; private set; }

        /// <summary>
        /// Bnet EntityID encoded id.
        /// </summary>
        public bnet.protocol.EntityId BnetEntityId { get; private set; }

        public bnet.protocol.game_master.GameHandle GameHandle {get; private set;}

        public ulong RequestID { get; private set; }
        public ulong FactoryID { get; private set; }

        public static ulong RequestIdCounter = 0;

        public Game(ulong id, ulong factoryId)
        {
            this.ID = ++id;
            this.RequestID = ++RequestIdCounter;
            this.FactoryID = factoryId;

            this.BnetEntityId = bnet.protocol.EntityId.CreateBuilder().SetHigh(433661094641971304).SetLow(this.ID).Build();
            this.GameHandle = bnet.protocol.game_master.GameHandle.CreateBuilder().SetFactoryId(this.FactoryID).SetGameId(this.BnetEntityId).Build();
        }

        public void ListenForGame(Client client)
        {
            var connectionInfo =
                bnet.protocol.game_master.ConnectInfo.CreateBuilder().SetToonId(client.CurrentToon.BnetEntityID).SetHost
                    ("127.0.0.1").SetPort(Net.GameServer.Config.Instance.Port).SetToken(ByteString.CopyFrom(new byte[] {0x07, 0x34, 0x02, 0x60, 0x91, 0x93, 0x76, 0x46, 0x28, 0x84}))
                    .AddAttribute(bnet.protocol.attribute.Attribute.CreateBuilder().SetName("SGameId").SetValue(bnet.protocol.attribute.Variant.CreateBuilder().SetIntValue(2014314530).Build())).Build();
                 
            var builder = bnet.protocol.game_master.GameFoundNotification.CreateBuilder();
            builder.AddConnectInfo(connectionInfo);
            builder.SetRequestId(this.RequestID);
            builder.SetGameHandle(this.GameHandle);

            Logger.Trace("Game spawned: {0}:{1}", connectionInfo.Host, connectionInfo.Port);
            client.CallMethod(bnet.protocol.game_master.GameFactorySubscriber.Descriptor.FindMethodByName("NotifyGameFound"), builder.Build(), this.ID);
        }
    }
}