using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace c2maker
{
    class Program
    {
        static void Main(string[] args)
        {
            if (3 != args.Length)
            {
                Console.WriteLine("c2maker - encode c2 files from c1 files");
                Console.WriteLine("Usage:");
                Console.WriteLine("       c2maker <tick> <input>0001 <output>");
                System.Environment.Exit(0);
            }
            string tickString = args[0];
            String c1Path = args[1];
            String c2Path = args[2];

            // Validate Arguments            
            UInt32 tick;
            if (!UInt32.TryParse(tickString, out tick))
            {
                Console.WriteLine("tick must be integer value, invalid input \"{0}\"", tickString);
                System.Environment.Exit(-1);
            }

            if (c1Path==c2Path)
            {
                Console.WriteLine("input must not equal output \"{0}\",\"{1}\"", c1Path, c2Path);
                System.Environment.Exit(-1);
            }

            c2Data c2 = new c2Data();
            c2.SetTicks( tick );

            while ( true )
            {
                c1Data c1 = new c1Data( c1Path );

                if (c1.LooksGood())
                {
                    Console.WriteLine("imported {0}", c1Path);
                    c2.AddFrame(c1);

                    NextFrame( ref c1Path );
                }
                else
                {
                    if (c2.NumFrames() < 1)
                    {
                        Console.WriteLine("Unable to Read C1({0})", c1Path);
                        System.Environment.Exit(-1);
                    }

                    c2.Save( c2Path );

                    break;
                }
            }

            Console.WriteLine("{0} - Completed", c2Path);
            System.Environment.Exit(0);
        }

        // Take the pathname, and increment the number
        // of the frame

        static void NextFrame( ref String c1Path )
        {                   
            string dir = Path.GetDirectoryName(c1Path);
            string ext = Path.GetExtension(c1Path);
            string fileName = Path.GetFileNameWithoutExtension(c1Path);

            String[] iiext = c1Path.Split(new char[] {'#'});

            if (iiext.Length > 1)
            {
                if (iiext.Length > 2)
                {
                    Console.WriteLine("I only support filenames with a single # [{0}]", c1Path);
                    System.Environment.Exit(-1);
                }
                ext = @"#" + iiext[1];
                fileName = Path.GetFileName(iiext[0]);
            }

            //Console.WriteLine("dir = {0}", dir);
            //Console.WriteLine("ext = {0}", ext);
            //Console.WriteLine("filename = {0}", fileName);

            // Take the fileName, and increment the number by 1
            byte[] bytes = Encoding.ASCII.GetBytes(fileName);

            int index = bytes.Length - 1;

            while(index >= 0)
            {
                byte c = bytes[index];
                c++;
                if (c <= 0x39)  // c < '9'
                {
                    bytes[index] = c; // and we're done
                    break;
                }
                else
                {
                    c = 0x30;
                    bytes[index] = c; // '0'
                }

                index--;
            }

            char[] charsAscii = new char[bytes.Length];
            Encoding.ASCII.GetChars(bytes, 0, bytes.Length, charsAscii, 0);

            fileName = new string(charsAscii);

            // Put it back together
            c1Path = dir + fileName + ext;

            //Console.WriteLine("new c1Path = {0}", c1Path);
        }
    }
}
