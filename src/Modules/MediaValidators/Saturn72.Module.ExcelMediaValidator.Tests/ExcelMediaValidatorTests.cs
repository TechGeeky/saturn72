﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Saturn72.Core.Services.File;
using Shouldly;

namespace Saturn72.Module.ExcelMediaValidator.Tests
{
    public class ExcelMediaValidatorTests
    {
        [Test]
        public void ExcelMediaValidator_Validate_Unsupportedtypes()
        {
            new ExcelMediaValidator().Validate(null, "ddd").ShouldBe(FileStatusCode.Unsupported);
        }
        [Test]
        public void ExcelMediaValidator_Validate_ReturnsCorruptedFileError()
        {
            var nv = new ExcelMediaValidator();
            var cossuptedXlsFile = new byte[] {1, 1, 1, 1, 1};
            nv.Validate(cossuptedXlsFile, "xls").ShouldBe(FileStatusCode.Corrupted);
        }

        [Test]
        public void ExcelMediaValidator_Validate_ReturnsValid()
        {
            var resourcesPath = Path.Combine(GetCurrentAssemblyFolder(),"Resources");
            var allGoodFiles = Directory.GetFiles(resourcesPath, "good.*");
            var nv = new ExcelMediaValidator();
            foreach (var f in allGoodFiles)
            {
                var ext = Path.GetExtension(f).Replace(".", string.Empty);
                var fs = File.ReadAllBytes(f);
                nv.Validate(fs, ext).ShouldBe(FileStatusCode.Valid);
            }
        }

        public string GetCurrentAssemblyFolder()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            return Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
        }
    }
}