using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent.Common.Scheduling
{
    [TestClass]
    public class SimpleScheduleTests
    {
        [TestMethod]
        public void TestOffsets()
        {
            var schedule = new SchedulingTable(new bool[24], DateTime.UtcNow, 0);
            var schedule4 = new SchedulingTable(new bool[24], DateTime.UtcNow, 4);

            Assert.AreNotEqual(schedule.Hour, schedule4.Hour, "Hours should not match");
            schedule.Current = DateTime.UtcNow.AddHours(-1 * 4);
            Assert.AreEqual(schedule.Hour, schedule4.Hour, "Hours should match");

            schedule.Current = DateTime.UtcNow.AddHours(-1);
            Assert.IsTrue(schedule.IsNextHour, "Current should be offset by an hour");
            schedule.Current = DateTime.UtcNow.AddDays(-1);
            Assert.IsTrue(schedule.IsNextDay, "Should be next day");

            schedule4.Current = DateTime.UtcNow.AddHours(-1);
            Assert.IsTrue(schedule4.IsNextHour, "Current should be offset by an hour");
            schedule4.Current = schedule4.Current.AddDays(-1);
            Assert.IsTrue(schedule4.IsNextDay, "Should be next day");

            var clone = schedule4.Clone();

            Assert.IsTrue(clone.IsNextHour, "Current should be offset by an hour");
            Assert.IsTrue(clone.IsNextDay, "Should be next day");
        }
    }
}