﻿using Analyzer.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AnalyzerTests.Pipeline
{
    [TestClass()]
    public class TestParsedMonoCecil
    {
        [TestMethod()]
        public void TestMonoCecilTypes()
        {
            string dllFile = Assembly.GetExecutingAssembly().Location;
            ParsedDLLFile parsedDLL = new(dllFile);
            List<ParsedClassMonoCecil> classObjList = parsedDLL.classObjListMC;
            foreach(ParsedClassMonoCecil cls in classObjList)
            {
                if (cls.Name == "TestNullNamespaceClassMC" || cls.TypeObj.Namespace == null)
                {
                    Assert.AreEqual(null,cls.TypeObj.Namespace);
                }
                //Console.WriteLine(cls.Name);
                //Console.WriteLine(cls.TypeObj.Namespace);
                if (cls.Name == "BMW" && cls.TypeObj.Namespace == "TestParsedMonoCecil")
                {
                    Assert.AreEqual(1,cls.Constructors.Count);
                    Assert.AreEqual("TestParsedMonoCecil.Car", cls.ParentClass.FullName);
                    Assert.AreEqual("TestParsedMonoCecil.IBMWSpec", cls.Interfaces[0].InterfaceType.FullName);
                    Assert.AreEqual(2,cls.MethodsList.Count);
                    Assert.AreEqual(2,cls.FieldsList.Count);
                    Assert.AreEqual(1,cls.PropertiesList.Count);
                }
            }
        }
    }
}

class TestNullNamespaceClassMC
{
    TestNullNamespaceClassMC()
    {
        Console.WriteLine("This class has no namespace");
    }
}
namespace TestParsedMonoCecil
{
    public interface IVehicle
    {

    }
    public interface ICar : IVehicle
    {

    }

    public interface IBMWSpec
    {

    }
    public class Car: ICar
    {
        public Car() { }
    }

    public class BMW: Car,IBMWSpec
    {
        private string _name = "BMW";
        public int seatCapacity;
        public BMW() {
            Console.WriteLine("BMW Car");
        }

        public string Name
        {
            get { return _name; }
        }
        public void Drive()
        {
            Console.WriteLine("Driving ");
        }
    }
}