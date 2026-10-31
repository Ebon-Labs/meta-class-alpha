using UnityEngine;

public static class Pcm16Codec
{
    public static byte[] FloatToPCM16(float[] samples)
    {
        byte[] bytes = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short value = (short)Mathf.Clamp(samples[i] * short.MaxValue, short.MinValue, short.MaxValue);
            bytes[i * 2] = (byte)(value & 0xff);
            bytes[i * 2 + 1] = (byte)((value >> 8) & 0xff);
        }

        return bytes;
    }

    public static float[] PCM16ToFloat(byte[] bytes)
    {
        int sampleCount = bytes.Length / 2;
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            short value = (short)(bytes[i * 2] | (bytes[i * 2 + 1] << 8));
            samples[i] = value / (float)short.MaxValue;
        }

        return samples;
    }
}
