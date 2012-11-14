using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace CAGA.Dialogue
{
    class Utility
    {
        public static string GetUniqueKey(int size=8)
        {
            char[] chars = new char[62];
            string a = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            chars = a.ToCharArray();
            byte[] data = new byte[1];
            RNGCryptoServiceProvider  crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(size) ;
            foreach(byte b in data )
		    { 
                result.Append(chars[b % (chars.Length - 1)]); 
            }
			return result.ToString();
        }
    }
}
