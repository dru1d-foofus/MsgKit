﻿using System;
using System.Collections.Generic;
using System.IO;
using MsgWriter.Exceptions;
using MsgWriter.Helpers;

namespace MsgWriter.PropertiesStream
{
    /// <summary>
    ///     A helper class to generate a __properties_version1.0 stream
    /// </summary>
    /// <remarks>
    ///     https://msdn.microsoft.com/en-us/library/ee178759%28v=exchg.80%29.aspx
    /// </remarks>
    internal class Properties : List<FixedLengthProperty>
    {
        #region AddProperty
        /// <summary>
        ///     Adds a property to the property stream
        /// </summary>
        /// <param name="id">The id of the property</param>
        /// <param name="type">The <see cref="PropertyType" /></param>
        /// <param name="data"></param>
        /// <param name="flags">
        ///     the flags to set on the property, default <see cref="PropertyFlag.PROPATTR_READABLE"/> 
        ///     and <see cref="PropertyFlag.PROPATTR_WRITABLE"/>
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Raised when <paramref name="data"/> is not 8 bytes</exception>
        internal void AddProperty(ushort id, 
                                  PropertyType type, 
                                  byte[] data,
                                  PropertyFlag flags = PropertyFlag.PROPATTR_READABLE & PropertyFlag.PROPATTR_WRITABLE)
        {
            if (data.Length != 8)
                throw new ArgumentOutOfRangeException("The data should always have an 8 byte size");

            Add(new FixedLengthProperty(id, type, flags, data));
        }
        #endregion

        #region ReadProperties
        /// <summary>
        ///     Reads all the <see cref="FixedLengthProperty" /> objects from the given <paramref name="binaryReader" />
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <exception cref="MWInvalidProperty">Raised when a property is invalid, e.g. not 16 bytes long</exception>
        internal void ReadProperties(BinaryReader binaryReader)
        {
            // The data inside the property stream (1) MUST be an array of 16-byte entries. The number of properties, 
            // each represented by one entry, can be determined by first measuring the size of the property stream (1), 
            // then subtracting the size of the header from it, and then dividing the result by the size of one entry.
            // The structure of each entry, representing one property, depends on whether the property is a fixed length 
            // property or not.

            while (!binaryReader.Eof())
            {
                // property tag: A 32-bit value that contains a property type and a property ID. The low-order 16 bits 
                // represent the property type. The high-order 16 bits represent the property ID.
                var type = (PropertyType) binaryReader.ReadUInt16();
                var id = binaryReader.ReadUInt16();
                var flags = binaryReader.ReadUInt16();
                // 8 bytes for the data
                var data = binaryReader.ReadBytes(10);

                Add(new FixedLengthProperty(id, type, flags, data));
            }
        }
        #endregion

        #region WriteProperties
        /// <summary>
        ///     Writes all the <see cref="FixedLengthProperty" /> objects to the given <paramref name="binaryWriter" />
        /// </summary>
        /// <param name="binaryWriter"></param>
        internal void WriteProperties(BinaryWriter binaryWriter)
        {
            // The data inside the property stream (1) MUST be an array of 16-byte entries. The number of properties, 
            // each represented by one entry, can be determined by first measuring the size of the property stream (1), 
            // then subtracting the size of the header from it, and then dividing the result by the size of one entry.
            // The structure of each entry, representing one property, depends on whether the property is a fixed length 
            // property or not.

            foreach (var property in this)
            {
                // property tag: A 32-bit value that contains a property type and a property ID. The low-order 16 bits 
                // represent the property type. The high-order 16 bits represent the property ID.
                binaryWriter.Write(property.Id);
                binaryWriter.Write(Convert.ToUInt16(property.Type));
                binaryWriter.Write(Convert.ToUInt32(property.Flags));
                binaryWriter.Write(property.Data);
            }
        }
        #endregion
    }
}