using System;

namespace SteamAuthCore;

public static class Util
{
    public static byte[] HexStringToByteArray(string hex)
    {
        int hexLen = hex.Length;
        byte[] ret = new byte[hexLen / 2];

        for (int i = 0; i < hexLen; i += 2)
        {
            ret[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return ret;
    }
}