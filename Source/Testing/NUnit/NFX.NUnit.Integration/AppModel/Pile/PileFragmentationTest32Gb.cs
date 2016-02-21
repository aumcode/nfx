using NUnit.Framework;

namespace NFX.NUnit.Integration.AppModel.Pile
{
  public class PileFragmentationTest32Gb : HighMemoryLoadTest32RAM
  {
    [TestCase(100000, 30, true,  2, 1000,  3, true)]
    [TestCase(100000, 30, false, 2, 1000, 10, true)]
    public void Put_RandomDelete_ByteArray(int cnt, int durationSec, bool speed, int payloadSizeMin, int payloadSizeMax, int deleteFreq, bool isParallel)
    {
      PileFragmentationTest.Put_RandomDelete_ByteArray(cnt, durationSec, speed, payloadSizeMin, payloadSizeMax, deleteFreq, isParallel);
    }

    [TestCase(true,  30, 2, 1000, 3, true)]
    [TestCase(false, 30, 2, 1000, 3, true)]
    public void DeleteOne_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, int deleteFreq, bool isParallel)
    {
      PileFragmentationTest.DeleteOne_ByteArray(speed, durationSec, payloadSizeMin, payloadSizeMax, deleteFreq, isParallel);
    }

    [TestCase(true,  30, 2, 1000, true)]
    [TestCase(false, 30, 2, 1000, true)]
    public void Chessboard_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, bool isParallel)
    {
      PileFragmentationTest.Chessboard_ByteArray(speed, durationSec, payloadSizeMin, payloadSizeMax, isParallel);
    }

    [TestCase(true,  30, 100, 200, 4, 2, 1000, true)]
    [TestCase(false, 30, 100, 200, 4, 2, 1000, true)]
    public void DeleteSeveral_ByteArray(bool speed, int durationSec, int putMin, int putMax, int delFactor, int payloadSizeMin, int payloadSizeMax, bool isParallel)
    {
      PileFragmentationTest.DeleteSeveral_ByteArray(speed, durationSec, putMin, putMax, delFactor, payloadSizeMin, payloadSizeMax, isParallel);
    }

    [TestCase(true,  30, 2, 1000,  100, 2000)]
    [TestCase(false, 30, 2, 1000,  100, 2000)]
    [TestCase(true,  30, 2, 1000, 1000, 2000)]
    [TestCase(false, 30, 2, 1000, 1000, 2000)]
    public void NoGrowth_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, int countMin, int countMax)
    {
      PileFragmentationTest.NoGrowth_ByteArray(speed, durationSec, payloadSizeMin, payloadSizeMax, countMin, countMax);
    }
  }
}
