﻿using ProtoSharp.Core;
using ProtoSharp.Performance.Messages;
using System;
using ProtoSharp.Performance.Benchmarks;

namespace ProtoSharp.Performance
{
    class MessageWriterRawAdapter : BenchmarkAdapterBase
    {
        public MessageWriterRawAdapter(byte[] memory)
            : base(memory)
        {
            _writer = new MessageWriter(Memory);
            _reader = new MessageReader(new ByteReader(Memory));
        }

        public override void Serialize(MessageWithInt32 item){ _writer.WriteVarint(item.Value); }
        public override void Serialize(MessageWithUInt32 item) { _writer.WriteVarint(item.Value); }
        public override void Serialize(MessageWithSInt32 item) { _writer.WriteZigZag(item.Value); }
        public override void Serialize(MessageWithFixed32 item) { _writer.WriteFixed(item.Signed); _writer.WriteFixed(item.Unsigned); _writer.WriteFixed(item.Float); }
        public override void Serialize(MessageWithFixed64 item) { _writer.WriteFixed(item.Signed); _writer.WriteFixed(item.Unsigned); _writer.WriteFixed(item.Double); }
        public override void Serialize(MessageWithString item) { _writer.WriteString(item.Value); }
        public override void Serialize(MessageWithBytes item) { _writer.WriteBytes(item.Value); }
        public override void Serialize(MessageWithRepeatedItem item) { item.Value.ForEach(x => _writer.WriteVarint(x)); }
        public override void Serialize(Person item) { throw new NotSupportedException(); }

        public override void Deserialize(out MessageWithInt32 item) { item = new MessageWithInt32() { Value = _reader.ReadInt32() }; }
        public override void Deserialize(out MessageWithUInt32 item) { item = new MessageWithUInt32() { Value = (uint)_reader.ReadInt32() }; }
        public override void Deserialize(out MessageWithSInt32 item) { item = new MessageWithSInt32() { Value = _reader.ReadZigZag32() }; }
        public override void Deserialize(out MessageWithFixed32 item) { item = new MessageWithFixed32() { Signed = _reader.ReadFixedInt32(), Unsigned = _reader.ReadFixedUInt32(), Float = _reader.ReadFixedSingle() }; }
        public override void Deserialize(out MessageWithFixed64 item) { item = new MessageWithFixed64() { Signed = _reader.ReadFixedInt32(), Unsigned = _reader.ReadFixedUInt32(), Double = _reader.ReadFixedDouble() }; }
        public override void Deserialize(out MessageWithString item) { item = new MessageWithString() { Value = _reader.ReadString() }; }
        public override void Deserialize(out MessageWithBytes item) { throw new NotSupportedException(); }
        public override void Deserialize(out MessageWithRepeatedItem item) 
        {
            item = new MessageWithRepeatedItem();
            for(int i = 0; i != RepeatedItemDeserializationBenchmark.Items; ++i)
                item.Value.Add(_reader.ReadInt32());
        }
        public override void Deserialize(out Person item) { throw new NotSupportedException(); }

        MessageWriter _writer;
        MessageReader _reader;
    }
}
