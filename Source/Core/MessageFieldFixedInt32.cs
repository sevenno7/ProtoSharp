﻿using System.Reflection;

namespace ProtoSharp.Core
{
    class MessageFieldFixedInt32 : MessageField
    {
        public MessageFieldFixedInt32(int tag, IFieldIO fieldIO) : base(tag, fieldIO, WireType.Fixed32) { }

        protected override object DoRead(MessageReader reader)
        {
            return reader.ReadFixedInt32();
        }
        protected override void DoWrite(object value, MessageWriter writer)
        {
            writer.WriteFixed32((int)value);
        }
    }
}
