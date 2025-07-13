using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;

namespace c2maker
{
    public class c1Data
    {
        bool m_bLooksGood = false;
        byte[] m_data_bytes;

        public byte[] GetBytes()
        {
            return m_data_bytes;
        }

        public c1Data(string pathName)
        {
            try
            {
                using (BinaryReader b = new BinaryReader(
                    File.Open(pathName, FileMode.Open)))
                {
                    long fileLength = b.BaseStream.Length;

                    if (32768 != fileLength)
                    {
                        // This doesn't look like a C1
                        // so bail
                        return;
                    }

                    // Read in the bytes
                    m_data_bytes = b.ReadBytes((int)fileLength);

                    // Mark it good
                    m_bLooksGood = true;
                }
            }
            catch (Exception)
            {
                // Nothing to do here, as object defaults to bad
            }
        }

        public bool LooksGood()
        {
            return m_bLooksGood;
        }
    }
}
