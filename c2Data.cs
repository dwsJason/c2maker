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
    public class c2Data
    {
        List<c1Data> m_frames = new List<c1Data>();

        UInt32 m_ticks = 0;

        public c2Data()
        {
        }

        public void SetTicks(UInt32 ticks)
        {
            m_ticks = ticks;
        }

        public int NumFrames()
        {
            return m_frames.Count;
        }

        public void AddFrame( c1Data c1 )
        {
            m_frames.Add( c1 );
        }

        public void Save( String pathName )
        {
            // Save
            Console.WriteLine("Saving {0}", pathName);
            Console.WriteLine("Saving {0} frames", NumFrames());

            try
            {

                using (BinaryWriter b = new BinaryWriter(
                    File.Open(pathName, FileMode.Create)))
                {
                    c1Data lastFrame = m_frames[0];

                    UInt32 length = 0;

                    b.Write( lastFrame.GetBytes() );

                    b.Write(length);

                    b.Write(m_ticks);

                    b.Write((UInt32)4);

                    for (int frameIndex = 1; frameIndex < m_frames.Count; ++frameIndex)
                    {
                        c1Data currentFrame = m_frames[ frameIndex ];

                        // Encode the current frame
                        List<byte> bytes = Encode(ref lastFrame, ref currentFrame);

                        b.Write( bytes.ToArray() );

                        lastFrame = currentFrame;
                    }

                    int position = (int)b.Seek((int)0, System.IO.SeekOrigin.Current);  // Get current position in the Stream

                    length = (UInt32)position - 0x8008;

                    b.Seek((int)0x8000, System.IO.SeekOrigin.Begin);
                    b.Write(length);
                    b.Seek(0, System.IO.SeekOrigin.End);
                }
                
            } catch (Exception) {

                Console.WriteLine("Failed Saving {0}", pathName);
                System.Environment.Exit(-1);

            }


        }

        List<byte> Encode(ref c1Data prevFrame, ref c1Data nextFrame)
        {
            List<byte> bytes = new List<byte>();

            byte[] baseBytes = prevFrame.GetBytes();
            byte[] diffBytes = nextFrame.GetBytes();

            // Encode Pixels separate from palette, to reduce weirdness
            // if palettes get animated for some reason
            const int PixelBytes = 0x7DC8;
            const int PaletteOffset = 0x7E00;
            const int PaletteBytes = 0x200;

            // Step by 1, Pixelsn and SCB
            for (int idx = 0; idx < PixelBytes; ++idx)
            {
                if (diffBytes[ idx ] != baseBytes[ idx ])
                {
                    // We have a delta, save out the index
                    bytes.Add((byte)(idx & 0xFF));
                    bytes.Add((byte)((idx>>8) & 0xFF));

                    // Save out the, data
                    bytes.Add(diffBytes[ idx ]);
                    idx++;
                    bytes.Add(diffBytes[ idx ]);
                }
            }

            // skip the screen hole

            // Step by 2
            for (int idx = PaletteOffset; idx < (PaletteOffset + PaletteBytes); idx+=2)
            {
                if (diffBytes[ idx ] != baseBytes[ idx ])
                {
                    // We have a delta, save out the index
                    bytes.Add((byte)(idx & 0xFF));
                    bytes.Add((byte)((idx>>8) & 0xFF));

                    // Save out the, data
                    bytes.Add(diffBytes[ idx + 0 ]);
                    bytes.Add(diffBytes[ idx + 1 ]);
                }
            }

            bytes.Add(0x00);
            bytes.Add(0x00);
            bytes.Add(0xFF);
            bytes.Add(0xFF);

            return bytes;

        }
    }
}

