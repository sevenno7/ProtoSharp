﻿using ProtoSharp.Core;
using ProtoSharp.Performance.Messages;

namespace ProtoSharp.Performance
{
    class MessageWriterRawAdapter : BenchmarkAdapterBase
    {
        public MessageWriterRawAdapter(byte[] memory)
            : base(memory)
        {
            _writer = new MessageWriter(Memory);
        }

        public override void Serialize(MessageWithInt32 item)
        {
            _writer.WriteVarint(item.Value);
        }

        MessageWriter _writer;
    }
}