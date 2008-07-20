﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace ProtoSharp.Core
{
    public class MessageReader
    {
        public static T Read<T>(byte[] message) where T: class, new()
        { 
            return new MessageReader(message).Read<T>(); 
        }

        public MessageReader(IByteReader bytes)
        {
            _bytes = bytes;
        }

        public MessageReader(byte[] message) : this(new ByteArrayReader(message, 0, message.Length)) { }

        public event EventHandler FieldMissing;

        public MessageReader CreateSubReader(int length)
        {
            return new MessageReader(_bytes.GetByteReader(length));
        }

        public int ReadVarint32()
        {
            int value = 0;
            int shiftBits = 0;
            int bits;   
            do
            {
                bits = _bytes.GetByte();
                if(shiftBits < 31)
                {
                    value |= (bits & 0x7F) << shiftBits;
                    shiftBits += 7;
                }
            } while(bits > 0x7F);
            return value;
        }

        public Int64 ReadVarint64()
        {
            Int64 value = 0;
            int shiftBits = 0;
            Int64 bits;
            do
            {
                bits = _bytes.GetByte();
                value |= (bits & 0x7F) << shiftBits;
                shiftBits += 7;
            } while(bits > 0x7F);
            return value;
        }

        public int ReadFixedInt32()
        {
            return _bytes.GetByte()
                | _bytes.GetByte() << 8
                | _bytes.GetByte() << 16
                | _bytes.GetByte() << 24;
        }

        public uint ReadFixedUInt32()
        {
            return (uint)ReadFixedInt32();
        }

        public Int64 ReadFixedInt64()
        {
            Int64 lo = (long)ReadFixedUInt32(), hi = (long)ReadFixedUInt32();
            return lo | (hi << 32);
        }

        public UInt64 ReadFixedUInt64()
        {
            return (UInt64)ReadFixedInt64();
        }

        public float ReadFloat()
        {
            var bytes = _bytes.GetBytes(sizeof(float));
            var value = new BinaryReader(new MemoryStream(bytes.Array, bytes.Offset, bytes.Count)).ReadSingle();
            return value;
        }

        public double ReadDouble()
        {
            var bytes = _bytes.GetBytes(sizeof(double));
            var value = new BinaryReader(new MemoryStream(bytes.Array, bytes.Offset, bytes.Count)).ReadDouble();
            return value;
        }

        public string ReadString()
        {
            var bytes = _bytes.GetBytes(ReadVarint32());
            var value = Encoding.UTF8.GetString(bytes.Array, bytes.Offset, bytes.Count);
            return value;
        }

        public byte[] ReadBytes()
        {
            var bytes = _bytes.GetBytes(ReadVarint32());
            var value = new byte[bytes.Count];
            Array.Copy(bytes.Array, bytes.Offset, value, 0, bytes.Count);
            return value;
        }

        public MessageTag ReadMessageTag()
        {
            return new MessageTag(ReadVarint32());
        }

        public object ReadMessage(Type messageType)
        {
            return ReadMessage(CreateDefault(messageType));
        }

        object ReadMessage(object obj)
        {
            Type messageType = obj.GetType();
            var fields = new Dictionary<int, MessageField>();
            Message.ForEachField(messageType,
                field => fields.Add(field.Tag, field));

            while(!_bytes.EndOfStream)
            {
                var tag = ReadMessageTag();
                MessageField field;
                if(!fields.TryGetValue(tag.Tag, out field))
                {
                    if(FieldMissing != null)
                        FieldMissing(this, EventArgs.Empty);
                    continue;
                }
                field.Read(obj, this);
            }
            return obj;
        }

        public T Read<T>() where T : class, new()
        {
            var obj = CreateDefault(typeof(T));
            return Read<T>(obj as T);
        }

        public T Read<T>(T target) where T : class
        {
            return ReadMessage(target) as T;
        }

        static object CreateDefault(Type type)
        {
            return type.GetConstructor(Type.EmptyTypes).Invoke(null);
        }

        IByteReader _bytes;
    }
}
