using OfxParser.Models;
using OfxParser.Core;

namespace OfxParser.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            OfxData ofxData= Parser.LoadFromFile ("E:\\arquivo.ofx");
        }
    }
}