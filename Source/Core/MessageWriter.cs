﻿using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;


namespace ProtoSharp.Core
{
    public class MessageWriter
    {
        public static byte[] Write(object message)
        {
            MemoryStream output = new MemoryStream();
            new MessageWriter(output).WriteMessage(message);
            return output.ToArray();
        }
        public MessageWriter(Stream output)
        {
            _writer = new BinaryWriter(output);
        }

        public void WriteVarint(int value)
        {
            if(value < 0)
                WriteVarint((UInt64)value);
            else
                WriteVarint((uint)value);
        }
        public void WriteVarint(uint value)
        {
            do
            {
                byte bits = (byte)(value & 0x7f);
                value >>= 7;
                if(value > 0)
                    bits |= 0x80;
                _writer.Write(bits);
            } while(value != 0);
        }

        public void WriteVarint(Int64 value) { WriteVarint((UInt64)value); }
        public void WriteVarint(UInt64 value)
        {
            do
            {
                byte bits = (byte)(value & 0x7f);
                value >>= 7;
                if(value > 0)
                    bits |= 0x80;
                _writer.Write(bits);
            } while(value != 0);
        }

        public void WriteString(string value)
        {
            WriteVarint(value.Length);
            _writer.Write(Encoding.UTF8.GetBytes(value));
        }

        public void WriteBytes(byte[] value)
        {
            WriteBytes(value, value.Length);
        }

        public void WriteBytes(byte[] value, int length)
        {
            WriteVarint(length);
            _writer.Write(value, 0, length);
        }

        public void WriteMessage(object obj)
        {
            List<MessageField> fields;
            if(!_fieldCache.TryGetValue(obj.GetType(), out fields))
            {
                fields = new List<MessageField>();
                Message.ForEachField(obj.GetType(), fields.Add);
                _fieldCache.Add(obj.GetType(), fields);
            }
            fields.ForEach(item => item.Write(obj, this));
        }

        public void WriteFixed32(int value) { _writer.Write(value); }

        public void WriteFixed32(uint value) { _writer.Write(value); }

        public void WriteFixed32(float value) { _writer.Write(value); }

        public void WriteFixed64(long value) { _writer.Write(value); }

        public void WriteFixed64(ulong value) { _writer.Write(value); }

        public void WriteFixed64(double value) { _writer.Write(value); }

        public void WriteObject(object obj)
        {
            var embedded = new MemoryStream();
            var writer = new MessageWriter(embedded);
            writer.WriteMessage(obj);
            WriteBytes(embedded.GetBuffer(), (int)embedded.Length);
        }

        public void WriteHeader(int tag, WireType wireType)
        {
            WriteVarint(tag << 3 | (int)wireType);
        }

        BinaryWriter _writer;
        Dictionary<Type, List<MessageField>> _fieldCache = new Dictionary<Type, List<MessageField>>();
    }
}
