using AktieHandelRepoLib;

namespace AktiehandelRepositoryLib.Tests
{
    [TestClass]
    public class AktieHandelTests
    {
        [TestMethod]
        public void Validate_GyldigAktiehandel_KasterIkkeException()
        {
            AktieHandel handel = new AktieHandel
            {
                Navn = "Tesla",
                Antal = 10,
                HandelsPris = 250m
            };

            handel.Validate();
        }

        [TestMethod]
        public void Validate_NegativtAntal_ErGyldigt()
        {
            AktieHandel handel = new AktieHandel
            {
                Navn = "Tesla",
                Antal = -10,
                HandelsPris = 250m
            };

            handel.Validate();
        }

        [TestMethod]
        public void Validate_NavnErNull_KasterException()
        {
            AktieHandel handel = new AktieHandel
            {
                Navn = null!,
                Antal = 10,
                HandelsPris = 250m
            };

            Assert.ThrowsExactly<ArgumentNullException>(
    () => handel.Validate());
        }

        [TestMethod]
        public void Validate_NavnForKort_KasterException()
        {
            AktieHandel handel = new AktieHandel
            {
                Navn = "IBM",
                Antal = 10,
                HandelsPris = 250m
            };

            
        }

        [TestMethod]
        public void Validate_AntalErNul_KasterException()
        {
            AktieHandel handel = new AktieHandel
            {
                Navn = "Tesla",
                Antal = 0,
                HandelsPris = 250m
            };

            Assert.ThrowsExactly<ArgumentException>(
       () => handel.Validate());
        }

        [TestMethod]
        public void Validate_HandelsPrisErNul_KasterException()
        {
            AktieHandel handel = new AktieHandel
            {
                Navn = "Tesla",
                Antal = 10,
                HandelsPris = 0m
            };

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
       () => handel.Validate());
        }

        [TestMethod]
        public void Validate_HandelsPrisErNegativ_KasterException()
        {
            AktieHandel handel = new AktieHandel
            {
                Navn = "Tesla",
                Antal = 10,
                HandelsPris = -100m
            };

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
    () => handel.Validate());
        }
    }
}