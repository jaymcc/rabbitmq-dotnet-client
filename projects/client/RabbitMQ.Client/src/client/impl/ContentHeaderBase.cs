// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007-2009 LShift Ltd., Cohesive Financial
//   Technologies LLC., and Rabbit Technologies Ltd.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//---------------------------------------------------------------------------
//
// The MPL v1.1:
//
//---------------------------------------------------------------------------
//   The contents of this file are subject to the Mozilla Public License
//   Version 1.1 (the "License"); you may not use this file except in
//   compliance with the License. You may obtain a copy of the License at
//   http://www.rabbitmq.com/mpl.html
//
//   Software distributed under the License is distributed on an "AS IS"
//   basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//   License for the specific language governing rights and limitations
//   under the License.
//
//   The Original Code is The RabbitMQ .NET Client.
//
//   The Initial Developers of the Original Code are LShift Ltd,
//   Cohesive Financial Technologies LLC, and Rabbit Technologies Ltd.
//
//   Portions created before 22-Nov-2008 00:00:00 GMT by LShift Ltd,
//   Cohesive Financial Technologies LLC, or Rabbit Technologies Ltd
//   are Copyright (C) 2007-2008 LShift Ltd, Cohesive Financial
//   Technologies LLC, and Rabbit Technologies Ltd.
//
//   Portions created by LShift Ltd are Copyright (C) 2007-2009 LShift
//   Ltd. Portions created by Cohesive Financial Technologies LLC are
//   Copyright (C) 2007-2009 Cohesive Financial Technologies
//   LLC. Portions created by Rabbit Technologies Ltd are Copyright
//   (C) 2007-2009 Rabbit Technologies Ltd.
//
//   All Rights Reserved.
//
//   Contributor(s): ______________________________________.
//
//---------------------------------------------------------------------------
using RabbitMQ.Client;
using RabbitMQ.Util;
using System;

namespace RabbitMQ.Client.Impl
{
    public abstract class ContentHeaderBase : IContentHeader
    {
        public static uint MaximumPermittedReceivableBodySize = 32 * 1048576; // FIXME: configurable

        public abstract int ProtocolClassId { get; }
        public abstract string ProtocolClassName { get; }
        public abstract void ReadPropertiesFrom(ContentHeaderPropertyReader reader);
        public abstract void WritePropertiesTo(ContentHeaderPropertyWriter writer);
        public abstract void AppendPropertyDebugStringTo(System.Text.StringBuilder sb);

        ///<summary>Fill this instance from the given byte buffer
        ///stream. Throws BodyTooLongException, which is the reason
        ///for the channelNumber parameter.</summary>
        ///<remarks>
        ///<para>
        /// It might be better to do the body length check in our
        /// caller, currently CommandAssembler, which would avoid
        /// passing in the otherwise unrequired channelNumber
        /// parameter.
        ///</para>
        ///</remarks>
        public ulong ReadFrom(int channelNumber, NetworkBinaryReader reader)
        {
            reader.ReadUInt16(); // weight - not currently used
            ulong bodySize = reader.ReadUInt64();
            if (bodySize > MaximumPermittedReceivableBodySize)
            {
                throw new BodyTooLongException(channelNumber, bodySize);
            }
            ReadPropertiesFrom(new ContentHeaderPropertyReader(reader));
            return bodySize;
        }

        public void WriteTo(NetworkBinaryWriter writer, ulong bodySize)
        {
            writer.Write((ushort)0); // weight - not currently used
            writer.Write((ulong)bodySize);
            WritePropertiesTo(new ContentHeaderPropertyWriter(writer));
        }

        public virtual object Clone()
        {
            throw new NotImplementedException();
        }
    }
}