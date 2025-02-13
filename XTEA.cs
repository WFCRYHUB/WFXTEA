using System.Text;

namespace WFXTEA
{
    public static class XTEA
    {
        /// <summary>
        /// g_xml_tea_key, Game.pdb, 1.20600.1897.41200
        /// </summary>
        public static uint[] TeaKey = { 0x4DD87487, 0x0C15011B0, 0x5EDD6B3D, 0x43CF5892 };

        public static string Decrypt(byte[] data)
        {
            //^$x
            int length = data.Length - 3;

            byte[] buffer = new byte[length];
            Array.Copy(data, 3, buffer, 0, length);
            data = buffer;

            var decryptedData = new List<uint>();
            if (data.Count() % 8 != 0)
                data = data.Take(data.Length - (data.Length % 8)).ToArray();
            if (data.Count() % 8 == 0)
            {
                foreach (var packedbytes in data.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 8).Select(x => x.Select(v => v.Value).ToList()).ToList())
                {
                    decryptedData.AddRange(Decode(packedbytes.ToArray(), true));
                }

                if (buffer.Length > data.Length)
                {
                    var buf = new byte[8];
                    Array.Copy(buffer.Skip(data.Length).ToArray(), buf, buffer.Length - data.Length);
                    decryptedData.AddRange(Decode(buf, false));
                }

                return Encoding.UTF8.GetString(decryptedData.SelectMany(BitConverter.GetBytes).ToArray(), 0, buffer.Length);
            }
            return string.Empty;
        }

        public static byte[] Encrypt(string data)
        {
            var dataBuffer = Encoding.UTF8.GetBytes(data);
            var buffer = dataBuffer;
            var length = buffer.Length;
            var encryptedData = new List<uint>();
            if (buffer.Count() % 8 != 0)
                buffer = buffer.Take(buffer.Length - (buffer.Length % 8)).ToArray();
            //if (buffer.Count() % 8 == 0)
            //{
            foreach (var packedbytes in buffer.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 8).Select(x => x.Select(v => v.Value).ToList()).ToList())
            {
                encryptedData.AddRange(Encode(packedbytes.ToArray(), true));
            }

            if (length > buffer.Length)
            {
                var buf = new byte[8];
                Array.Copy(dataBuffer.Skip(buffer.Length).ToArray(), buf, length - buffer.Length);
                encryptedData.AddRange(Encode(buf, false));
            }

            byte[] res = new byte[length + 3];

            byte[] protect = [(byte)'^', (byte)'$', (byte)'x'];
            Array.Copy(protect, res, protect.Length);
            Array.Copy(encryptedData.SelectMany(BitConverter.GetBytes).ToArray(), 0, res, 3, length);
            return res;
        }

        private static uint[] Encode(byte[] data, bool isFull)
        {
            if (data.Length != 8)
                throw new InvalidDataException("Block must be 8 bytes");
            uint v0 = BitConverter.ToUInt32(data.Take(4).ToArray(), 0), v1 = BitConverter.ToUInt32(data.Skip(4).ToArray(), 0), sum = 0;
            if (!isFull)
                return [~v0, ~v1];
            for (var i = 0; i < 32; i++)
            {
                sum += 0x9e3779b9;
                v0 += ((v1 << 4) + TeaKey[0]) ^ (v1 + sum) ^ ((v1 >> 5) + TeaKey[1]);
                v1 += ((v0 << 4) + TeaKey[2]) ^ (v0 + sum) ^ ((v0 >> 5) + TeaKey[3]);
            }
            return [~v0, ~v1];
        }

        private static uint[] Decode(byte[] data, bool isFull)
        {
            if (data.Length != 8)
                throw new InvalidDataException("Block must be 8 bytes");
            uint v0 = ~BitConverter.ToUInt32(data.Take(4).ToArray(), 0), v1 = ~BitConverter.ToUInt32(data.Skip(4).ToArray(), 0), sum = 0xC6EF3720;
            if (!isFull)
                return [v0, v1];
            for (var i = 0; i < 32; i++)
            {
                v1 -= ((v0 << 4) + TeaKey[2]) ^ (v0 + sum) ^ ((v0 >> 5) + TeaKey[3]);
                v0 -= ((v1 << 4) + TeaKey[0]) ^ (v1 + sum) ^ ((v1 >> 5) + TeaKey[1]);
                sum -= 0x9e3779b9;
            }
            return [v0, v1];
        }
    }
}
