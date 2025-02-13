using System.Text;
using System.Xml.Linq;

namespace WFXTEA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;

            string filename = args[0];

            if (string.IsNullOrEmpty(filename))
                End("File is not specified or not found!");

            if ((!File.Exists(filename) || Path.GetExtension(filename) != ".xml"))
                End(@"File format must be "".xml""!");

            string result;
            byte[] data = File.ReadAllBytes(filename);

            if (data[0] == '<')
            {
                result = XElement.Parse(Encoding.UTF8.GetString(data)).ToString();
            }
            else if (data[0] == '^' && data[1] == '$' && data[2] == 'x')
            {
                result = XTEA.Decrypt(data);
            }
            else
            {
                End("Unknown file format!");
                return;
            }

            File.Move(filename, filename + ".backup");
            File.WriteAllText(args![0], result);
            Console.WriteLine("Complete!");
        }

        static void End(string text)
        {
            Console.WriteLine(text);
            Thread.Sleep(5000);
            Environment.Exit(0);
        }
    }
}
