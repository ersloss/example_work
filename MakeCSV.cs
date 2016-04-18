using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace MakeCSV
{
    class MakeCSV
    {
        static int Main(string[] args)
        {
            // Test if input arguments were supplied: 
            if (args.Length < 2 )
            {
                Console.WriteLine("Invalid input parameters.  Please enter base path and output path.");
                Console.WriteLine("Usage: MakeCSV <base path> , <output path> , <optional recursive flag -R>");
                return 1; /* Exit input variables error */
            }

            /* Args[0] is the input location of the (JPEG, PDF) files to analyze */
            string filePath = args[0];
            /* Args[1] is the location of the csv output file which will be named Pictures.csv */
            string fileName = args[1] + "\\Pictures.csv";
            StringBuilder sb = new StringBuilder();
            /* Args[2] is the recursive flag, and 1 is the non-recursive state */
            int isItTheFlag = 1;

            /* Process base folder first */
            sb = ProcessFiles(filePath);

            /* Make the output file unless it exists already */
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();
            }

            /* Write out pertinent data to CSV for base directory */
            File.AppendAllText(fileName, sb.ToString());

            /* Catch if the flag is passed to our application */
            if (args.Length > 2 )
            { 
                int num;
                int.TryParse(args[2], out num);

                isItTheFlag = String.Compare(args[2], "-R");
            }

            /* Check if we should do this recursively */
            if (isItTheFlag == 0)
            {
                // Recurse into subdirectories of this directory. 
                string[] subdirectoryEntries = Directory.GetDirectories(filePath);

                foreach (string subdirectory in subdirectoryEntries)
                {
                    sb = ProcessFiles(subdirectory);
                    File.AppendAllText(fileName, sb.ToString());
                }
            }

            return 0; /* Exit no error */
        }

        static StringBuilder ProcessFiles(string filePath)
        {
            /* CSV Output Variables */
            StringBuilder sb = new StringBuilder();
            string delimiter = ",";
            MD5 md5Hash = MD5.Create();

            int fileCount = Directory.GetFiles(filePath, "*", SearchOption.TopDirectoryOnly).Length;
            string[] fileNames = Directory.GetFiles(filePath, "*", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < fileCount; i++)
            {
                byte[] input = File.ReadAllBytes(fileNames[i]);
                StringBuilder sBuilder = new StringBuilder();
                byte[] data = md5Hash.ComputeHash(input);
                string fileType = "Unknown";

                /* If input file is fewer than 4 bytes we may get an index out of range error */
                if (input.Length < 4)
                    continue;

                /* This is a JPEG */
                if ((input[0] == 255) && (input[1] == 216))
                {
                    fileType = "JPEG";
                }
                /* This is a PDF */
                else if ((input[0] == 37) && (input[1] == 80) && (input[2] == 68) && (input[3] == 70))
                {
                    fileType = "PDF";
                }
                // Loop through each byte of the hashed data  
                // and format each one as a hexadecimal string. 
                for (int ii = 0; ii < data.Length; ii++)
                {
                    sBuilder.Append(data[ii].ToString("x2"));
                }

                string[] output = new string[] { fileNames[i], fileType, sBuilder.ToString() };
                sb.AppendLine(string.Join(delimiter, output));
            }

            return sb;
        }
    }
}
