using System.IO;

namespace TestProject
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void DoorLock_ChangesStatusWhenLockedAndUnlocked()
        {
            DoorLock door = new DoorLock("door-test", "TEST DOOR");

            Assert.IsTrue(door.IsLocked);
            Assert.AreEqual("LOCKED", door.StatusText);

            Assert.AreEqual(CommandResult.Accepted, door.Lock());
            Assert.IsTrue(door.IsLocked);
            Assert.AreEqual("LOCKED", door.StatusText);

            Assert.AreEqual(CommandResult.Rejected, door.Unlock(confirmed: false));
            Assert.IsTrue(door.IsLocked);
            Assert.AreEqual("LOCKED", door.StatusText);

            Assert.AreEqual(CommandResult.Accepted, door.Unlock(confirmed: true));
            Assert.IsFalse(door.IsLocked);
            Assert.AreEqual("UNLOCKED", door.StatusText);
        }

        [TestMethod]
        public void SensorCsvFiles_AdvanceOneRowPerRead()
        {
            string dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
            (string FileName, double FirstValue, double SecondValue)[] expectedRows =
            {
                ("motion_backhall.csv", 0, 0),
                ("thermal_backhall.csv", 21.5, 21.6),
                ("glassbreak_backhall.csv", 32, 35),
                ("camera_lobby.csv", 1, 1),
                ("sos_teller.csv", 0, 0)
            };

            foreach ((string fileName, double firstValue, double secondValue) in expectedRows)
            {
                FileDataSource source = new FileDataSource(Path.Combine(dataDirectory, fileName));
                Assert.IsTrue(source.IsAvailable, $"Could not load {fileName}: {source.Error}");

                Assert.IsTrue(CsvSampleParser.TryParse(source.NextLine(), out DataSample first));
                Assert.AreEqual(1, first.Tick, $"Unexpected first tick in {fileName}.");
                Assert.AreEqual(firstValue, first.Value, 0.0001, $"Unexpected first value in {fileName}.");

                Assert.IsTrue(CsvSampleParser.TryParse(source.NextLine(), out DataSample second));
                Assert.AreEqual(2, second.Tick, $"Unexpected second tick in {fileName}.");
                Assert.AreEqual(secondValue, second.Value, 0.0001, $"Unexpected second value in {fileName}.");
            }
        }
    }
}
