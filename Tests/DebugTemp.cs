using System;
using Xunit;
using Lab1MAUI.Models.Schedule.Constraints;

namespace Lab1MAUI.Tests
{
    public class DebugTemp
    {
        [Fact]
        public void TestMask()
        {
            var mask = new bool[4] { true, true, true, true };
            var c = new SpecificTimeConstraint(new[] { 0, 2 }, 4);
            c.Apply(mask);
            Assert.True(mask[0]);
            Assert.False(mask[1]);
            Assert.True(mask[2]);
            Assert.False(mask[3]);
        }
    }
}
