using NUnit.Framework;

public class Pcm16CodecTests
{
    [Test]
    public void SilenceAndFullScale_RoundTrip()
    {
        float[] samples = { 0f, 1f, -1f };
        float[] decoded = Pcm16Codec.PCM16ToFloat(Pcm16Codec.FloatToPCM16(samples));

        Assert.AreEqual(0f, decoded[0], 0.0001f);
        Assert.AreEqual(1f, decoded[1], 0.0001f);
        Assert.AreEqual(-1f, decoded[2], 0.001f);
    }

    [Test]
    public void ValuesOutsideTheRange_AreClamped()
    {
        float[] decoded = Pcm16Codec.PCM16ToFloat(Pcm16Codec.FloatToPCM16(new[] { 2f, -2f }));

        Assert.AreEqual(1f, decoded[0], 0.0001f);
        Assert.AreEqual(-1f, decoded[1], 0.001f);
    }

    [Test]
    public void OddTrailingByte_IsDropped()
    {
        float[] decoded = Pcm16Codec.PCM16ToFloat(new byte[] { 0, 0, 7 });

        Assert.AreEqual(1, decoded.Length);
        Assert.AreEqual(0f, decoded[0], 0.0001f);
    }
}
