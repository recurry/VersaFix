//************************************************************************
// The VersaFix FIX Engine. 
// Copyright (c) 2009 Assimilate Technology, Inc.
//------------------------------------------------------------------------
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
//************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VfxEngine.Ipc
{
    /// <summary>
    /// The VfxMsgBlock class encapsulates a buffer of data
    /// and corresponding read/write indexes into it.
    /// </summary>
    public class VfxMsgBlock
    {
        /// <summary>
        /// The RdIndex property provides access to the current
        /// read index into the message block's buffer.
        /// </summary>
        public int RdIndex;
        /// <summary>
        /// The WrIndex property provides access to the current
        /// write index into the message block's buffer.
        /// </summary>
        public int WrIndex;

        // REC: The buffer that contains the data that is
        // associated with the message block:
        private byte[] _buffer;

        /// <summary>
        /// Initializes a new instance of a message block with
        /// the appropriate default settings.
        /// </summary>
        public VfxMsgBlock()
        {
            _buffer = new byte[1640];
            RdIndex = 0;
            WrIndex = 0;
        }

        /// <summary>
        /// Initializes a new instance of the class with the
        /// specified parameters.
        /// </summary>
        /// <param name="size">
        /// The initial size of the buffer that the message block
        /// is to maintain.
        /// </param>
        public VfxMsgBlock(int size)
        {
            _buffer = new byte[size];
            RdIndex = 0;
            WrIndex = 0;
        }

        /// <summary>
        /// Initializes a new instance of the class with the
        /// specified parameter(s).
        /// </summary>
        /// <param name="text">
        /// A string that is to be copied into the message block.
        /// </param>
        public VfxMsgBlock(string text)
        {
            _buffer = Encoding.ASCII.GetBytes(text);
            RdIndex = 0;
            WrIndex = text.Length;
        }

        /// <summary>
        /// Initializes a new instance of the class with the
        /// specified parameter(s).
        /// </summary>
        /// <param name="buffer">
        /// The buffer that the message block is to use for
        /// managing the data it contains.
        /// </param>
        public VfxMsgBlock(byte[] buffer)
        {
            _buffer = buffer;
            RdIndex = 0;
            WrIndex = buffer.Length;
        }

        /// <summary>
        /// The Buffer property provides access to the raw data
        /// buffer that the message block is managing.
        /// </summary>
        public byte[] Buffer
        {
            get { return _buffer; }
        }

        /// <summary>
        /// The Length property provide access to a value that indicates
        /// the current length of the data that is contained in an instance
        /// of a message block. 
        /// </summary>
        /// <returns></returns>
        public int Length()
        {
            return WrIndex - RdIndex;
        }

        /// <summary>
        /// The Remaining method is invoked to retrieve the number of bytes
        /// that are currently remaining (unread) in the message block buffer.
        /// </summary>
        /// <returns></returns>
        public int Remaining()
        {
            return _buffer.Length - WrIndex;
        }

        /// <summary>
        /// The Crunch method is invoked to move the contents of 
        /// the message block's buffer back to compensate for any
        /// bytes that have been consumed from the buffer.
        /// </summary>
        public void Crunch()
        {
            if ((WrIndex - RdIndex) > 0)
            {
                System.Array.Copy(_buffer, RdIndex, _buffer, 0, WrIndex - RdIndex);
            }

            WrIndex = WrIndex - RdIndex;
            RdIndex = 0;
        }

        /// <summary>
        /// The Append method is invoked to append the contents of a
        /// buffer to an instance of a message block.
        /// </summary>
        /// <param name="src">
        /// The buffer that is the source for the data that is to be
        /// appended to the message block instance.
        /// </param>
        public void Append(byte[] src)
        {
            System.Array.Copy(src, 0, _buffer, WrIndex, src.Length);
            WrIndex += src.Length;
        }

        /// <summary>
        /// The Append method is invoked to append the content of an
        /// message block to the content of the current instance.
        /// </summary>
        /// <param name="src">
        /// The message block that contains the data which is to be
        /// copied into the message block's buffer.
        /// </param>
        public void Append(VfxMsgBlock src)
        {
            System.Array.Copy(src.Buffer, src.RdIndex, _buffer, WrIndex, src.Length());
            WrIndex += src.Length();
        }


        /// <summary>
        /// The UnsafeAppend method is invoked to conduct an unsafe copy
        /// of the contents from an external messasge block into this one.
        /// </summary>
        /// <param name="src"></param>
        unsafe public void UnsafeAppend(VfxMsgBlock src)
        {
            fixed (byte* pDst = &_buffer[WrIndex])
            {
                fixed (byte* pSrc = &src._buffer[src.RdIndex])
                {
                    for (int i = 0; i != src.Length(); i++)
                    {
                        pDst[i] = pSrc[i];
                    }
                }
            }
            WrIndex += src.Length();
        }

        /// <summary>
        /// The UnsafeAppend method is invoked to conduct an unsafe
        /// copy of the data in an outside message block into the buffer
        /// that is being managed by this instance.
        /// </summary>
        /// <param name="src">
        /// The source of the data that is to be copied into the current
        /// message block instance.
        /// </param>
        /// <param name="len">
        /// The length of the data that is to be copied from the outside
        /// message block into this one's buffer.
        /// </param>
        unsafe public void UnsafeAppend(VfxMsgBlock src, int len)
        {
            fixed (byte* pDst = &_buffer[WrIndex])
            {
                fixed (byte* pSrc = &src._buffer[src.RdIndex])
                {
                    for (int i = 0; i != len; i++)
                    {
                        pDst[i] = pSrc[i];
                    }
                }
            }
            WrIndex += src.Length();
        }
    }
}
