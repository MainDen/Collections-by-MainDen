using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MainDen.Collections.Generic
{
    [TestClass]
    public class KeyBundleTest
    {
        [TestMethod]
        public void TestKeyBundle()
        {
            AssertKeyBundle("Default", 1, Int32.MaxValue, 0, Array.Empty<int>(), new ());
        }

        [TestMethod]
        public void TestKeyBundle_WithMinValueMaxValue()
        {
            AssertKeyBundle("With single element", 1, 1, 0, Array.Empty<int>(), new (1, 1));
            AssertKeyBundle("With range", 1, 5, 0, Array.Empty<int>(), new (1, 5));
            AssertKeyBundle("With range including negatives", -100, 100, 0, Array.Empty<int>(), new (-100, 100));

            AssertException<ArgumentException>("With invalid range", () => { _ = new KeyBundle(5, 1); });
            AssertException<ArgumentException>("With overloaded range", () => { _ = new KeyBundle(-2000000000, 2000000000); });
        }

        [TestMethod]
        public void TestKeyBundle_WithKeys()
        {
            AssertKeyBundle("With collection", 1, Int32.MaxValue, 5, new int[] { 1, 2, 3, 4, 5 }, new(new int[] { 1, 2, 3, 4, 5 }));

            AssertException<ArgumentOutOfRangeException>("With out of range collection", () => { _ = new KeyBundle(new int[] { 0, 2, 3, 4, 5 }); });
        }

        [TestMethod]
        public void TestKeyBundle_WithMinValueMaxValueKeys()
        {
            AssertKeyBundle("With range and collection", 0, 10, 5, new int[] { 0, 2, 3, 4, 5 }, new(0, 10, new int[] { 0, 2, 3, 4, 5 }));

            AssertException<ArgumentException>("With invalid range", () => { _ = new KeyBundle(5, 1, new int[] { 1, 2, 3, 4, 5 }); });
            AssertException<ArgumentException>("With overloaded range", () => { _ = new KeyBundle(-2000000000, 2000000000, new int[] { 1, 2, 3, 4, 5 }); });
            AssertException<ArgumentOutOfRangeException>("With out of range collection", () => { _ = new KeyBundle(2, 4, new int[] { 1, 2, 3, 4, 5 }); });
        }

        [TestMethod]
        public void TestCreateFull()
        {
            AssertKeyBundle("With range", 1, 5, 5, new int[] { 1, 2, 3, 4, 5 }, KeyBundle.CreateFull(1, 5));

            AssertException<ArgumentException>("With invalid range", () => { _ = KeyBundle.CreateFull(5, 1); });
            AssertException<ArgumentException>("With overloaded range", () => { _ = KeyBundle.CreateFull(-2000000000, 2000000000); });
        }

        [TestMethod]
        public void TestAdd()
        {
            var keyBundle = new KeyBundle();

            keyBundle.Add(1);
            AssertKeyBundle("Add(1)", 1, Int32.MaxValue, 1, new int[] { 1 }, keyBundle);
            keyBundle.Add(5);
            AssertKeyBundle("Add(5)", 1, Int32.MaxValue, 2, new int[] { 1, 5 }, keyBundle);
            keyBundle.Add(2);
            AssertKeyBundle("Add(2)", 1, Int32.MaxValue, 3, new int[] { 1, 2, 5 }, keyBundle);
            keyBundle.Add(4);
            AssertKeyBundle("Add(4)", 1, Int32.MaxValue, 4, new int[] { 1, 2, 4, 5 }, keyBundle);
            keyBundle.Add(3);
            AssertKeyBundle("Add(3)", 1, Int32.MaxValue, 5, new int[] { 1, 2, 3, 4, 5 }, keyBundle);

            AssertException<ArgumentOutOfRangeException>("Add(0)", () => { keyBundle.Add(0); });
            AssertException<ArgumentException>("Add(1) (repeated)", () => { keyBundle.Add(1); });
        }

        [TestMethod]
        public void TestPut()
        {
            var keyBundle = new KeyBundle();

            Assert.IsTrue(keyBundle.Put(1));
            AssertKeyBundle("Put(1)", 1, Int32.MaxValue, 1, new int[] { 1 }, keyBundle);
            Assert.IsFalse(keyBundle.Put(1));
            AssertKeyBundle("Put(1) (repeated)", 1, Int32.MaxValue, 1, new int[] { 1 }, keyBundle);
            Assert.IsTrue(keyBundle.Put(2));
            AssertKeyBundle("Put(2)", 1, Int32.MaxValue, 2, new int[] { 1, 2 }, keyBundle);
            Assert.IsTrue(keyBundle.Put(5));
            AssertKeyBundle("Put(5)", 1, Int32.MaxValue, 3, new int[] { 1, 2, 5 }, keyBundle);
            Assert.IsTrue(keyBundle.Put(4));
            AssertKeyBundle("Put(4)", 1, Int32.MaxValue, 4, new int[] { 1, 2, 4, 5 }, keyBundle);
            Assert.IsTrue(keyBundle.Put(3));
            AssertKeyBundle("Put(3)", 1, Int32.MaxValue, 5, new int[] { 1, 2, 3, 4, 5 }, keyBundle);

            AssertException<ArgumentOutOfRangeException>("Put(0)", () => { _ = keyBundle.Put(0); });
        }

        [TestMethod]
        public void TestRemove()
        {
            var keyBundle = KeyBundle.CreateFull(1, 5);

            Assert.IsTrue(keyBundle.Remove(2));
            AssertKeyBundle("Remove(2)", 1, 5, 4, new int[] { 1, 3, 4, 5 }, keyBundle);
            Assert.IsFalse(keyBundle.Remove(2));
            AssertKeyBundle("Remove(2) (repeated)", 1, 5, 4, new int[] { 1, 3, 4, 5 }, keyBundle);
            Assert.IsTrue(keyBundle.Remove(1));
            AssertKeyBundle("Remove(1)", 1, 5, 3, new int[] { 3, 4, 5 }, keyBundle);
            Assert.IsTrue(keyBundle.Remove(4));
            AssertKeyBundle("Remove(4)", 1, 5, 2, new int[] { 3, 5 }, keyBundle);
            Assert.IsTrue(keyBundle.Remove(3));
            AssertKeyBundle("Remove(3)", 1, 5, 1, new int[] { 5 }, keyBundle);
            Assert.IsTrue(keyBundle.Remove(5));
            AssertKeyBundle("Remove(5)", 1, 5, 0, new int[] { }, keyBundle);

            AssertException<ArgumentOutOfRangeException>("Remove(0)", () => { _ = keyBundle.Remove(0); });
        }

        [TestMethod]
        public void TestTake()
        {
            var keyBundle = KeyBundle.CreateFull(1, 5);
            int key;

            Assert.IsTrue(keyBundle.Take(out key));
            Assert.AreEqual(1, key, "Take(out 1)");
            AssertKeyBundle("Take(out 1)", 1, 5, 4, new int[] { 2, 3, 4, 5 }, keyBundle);
            Assert.IsTrue(keyBundle.Take(out key));
            Assert.AreEqual(2, key, "Take(out 2)");
            AssertKeyBundle("Take(out 2)", 1, 5, 3, new int[] { 3, 4, 5 }, keyBundle);
            Assert.IsTrue(keyBundle.Take(out key));
            Assert.AreEqual(3, key, "Take(out 3)");
            AssertKeyBundle("Take(out 3)", 1, 5, 2, new int[] { 4, 5 }, keyBundle);
            Assert.IsTrue(keyBundle.Take(out key));
            Assert.AreEqual(4, key, "Take(out 4)");
            AssertKeyBundle("Take(out 4)", 1, 5, 1, new int[] { 5 }, keyBundle);
            Assert.IsTrue(keyBundle.Take(out key));
            Assert.AreEqual(5, key, "Take(out 5)");
            AssertKeyBundle("Take(out 5)", 1, 5, 0, new int[] { }, keyBundle);
            Assert.IsFalse(keyBundle.Take(out key));
            Assert.AreEqual(0, key, "Take(out 0) (empty)");
            AssertKeyBundle("Take(out 0) (empty)", 1, 5, 0, new int[] { }, keyBundle);
        }

        public static void AssertKeyBundle(string testCase, int expectedMinValue, int expectedMaxValue, int expectedCount, int[] expectedKeys, KeyBundle keyBundle)
        {
            Assert.IsNotNull(keyBundle, testCase);
            Assert.AreEqual(expectedMinValue, keyBundle.MinValue, testCase);
            Assert.AreEqual(expectedMaxValue, keyBundle.MaxValue, testCase);
            Assert.AreEqual(expectedCount, keyBundle.Count, testCase);

            var actualKeys = keyBundle.ToArray();

            Assert.AreEqual(expectedKeys.Length, actualKeys.Length, testCase);

            for (var i = 0; i < expectedKeys.Length; ++i)
            {
                Assert.AreEqual(expectedKeys[i], actualKeys[i], testCase);
            }
        }

        public static void AssertException<T>(string testCase, Action action) where T : Exception
        {
            Assert.ThrowsException<T>(action, testCase);
        }
    }
}
