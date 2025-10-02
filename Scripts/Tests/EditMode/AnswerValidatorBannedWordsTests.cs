using NUnit.Framework;
using RobotsGame.Core;

namespace RobotsGame.Tests
{
    public class AnswerValidatorBannedWordsTests
    {
        [SetUp]
        public void SetUp()
        {
            AnswerValidator.ReloadBannedWords();
        }

        [Test]
        public void ContainsProfanity_LoadsWordsFromDataFile()
        {
            Assert.IsTrue(AnswerValidator.ContainsProfanity("Please kill yourself."));
        }

        [Test]
        public void ContainsProfanity_IgnoresSafeText()
        {
            Assert.IsFalse(AnswerValidator.ContainsProfanity("Hello friends"));
        }
    }
}
