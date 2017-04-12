using System;

namespace AzureBootCampTickets.Common
{
    /// <summary>
    /// Genera un string Alfanumérico
    /// </summary>
    public static class CodeGenerator
    {
        
        private static readonly Random Random = new Random(DateTime.UtcNow.Millisecond);
        private static readonly char[] Characters = "ABCDEFGHJKMNPQRSTUVWXYZ123456789".ToCharArray();

        public static string Generate(int length)
        {
            var result = new char[length];
            lock (Random)
                for (int i = 0; i < length; i++)
                    result[i] = Characters[Random.Next(0, Characters.Length)];


            return new string(result);
        }
    }
}