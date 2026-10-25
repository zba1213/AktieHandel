using AktieHandelRepoLib;


namespace AktiehandelRepoLib.Tests
{
    [TestClass]
    public class AktieHandelRepoTests
    {
        private AktieHandelRepo _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            _repository = new AktieHandelRepo();

            _repository.Add(new AktieHandel
            {
                Navn = "Tesla",
                Antal = 10,
                HandelsPris = 250m
            });

            _repository.Add(new AktieHandel
            {
                Navn = "Apple",
                Antal = 20,
                HandelsPris = 180m
            });

            _repository.Add(new AktieHandel
            {
                Navn = "Novo Nordisk",
                Antal = -5,
                HandelsPris = 700m
            });
        }

        [TestMethod]
        public void Search_FiltreOgValidering()
        {
            Assert.HasCount(3, _repository.Search());
            Assert.AreEqual("Novo Nordisk", _repository.Search(substring: "NORD").Single().Navn);
            Assert.AreEqual("Tesla", _repository.Search(antal: 10).Single().Navn);
            Assert.AreEqual("Apple", _repository.Search(antal: 10, antalFilter: "over").Single().Navn);
            Assert.AreEqual("Novo Nordisk", _repository.Search(antal: 10, antalFilter: "under").Single().Navn);
            Assert.IsEmpty(_repository.Search(substring: "Tesla", antal: 20));
            Assert.ThrowsExactly<ArgumentException>(() => _repository.Search(antalFilter: "forkert"));
        }

        [TestMethod]
        public void GetAll_ReturnererAlleHandler()
        {
            List<AktieHandel> result = _repository.GetAll();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void Get_ReturnererAlleHandler()
        {
            List<AktieHandel> result = _repository.Get();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void GetById_EksisterendeId_ReturnererObjekt()
        {
            AktieHandel? result = _repository.GetById(1);

            Assert.IsNotNull(result);
            Assert.AreEqual("Tesla", result.Navn);
        }

        [TestMethod]
        public void GetById_UkendtId_ReturnererNull()
        {
            AktieHandel? result = _repository.GetById(999);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Add_TildelerNytId()
        {
            AktieHandel handel = new AktieHandel
            {
                Navn = "Microsoft",
                Antal = 5,
                HandelsPris = 400m
            };

            AktieHandel result = _repository.Add(handel);

            Assert.AreEqual(4, result.HandelsId);
            Assert.AreEqual(4, _repository.GetAll().Count);
        }

        [TestMethod]
        public void Delete_EksisterendeId_SletterObjekt()
        {
            AktieHandel? result = _repository.Delete(1);

            Assert.IsNotNull(result);
            Assert.AreEqual("Tesla", result.Navn);
            Assert.AreEqual(2, _repository.GetAll().Count);
        }

        [TestMethod]
        public void Delete_UkendtId_ReturnererNull()
        {
            AktieHandel? result = _repository.Delete(999);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Update_EksisterendeId_OpdatererObjekt()
        {
            AktieHandel nyeData = new AktieHandel
            {
                Navn = "Tesla Motors",
                Antal = 30,
                HandelsPris = 300m
            };

            AktieHandel? result =
                _repository.Update(1, nyeData);

            Assert.IsNotNull(result);
            Assert.AreEqual("Tesla Motors", result.Navn);
            Assert.AreEqual(30, result.Antal);
            Assert.AreEqual(300m, result.HandelsPris);
        }

        [TestMethod]
        public void Update_UkendtId_ReturnererNull()
        {
            AktieHandel data = new AktieHandel
            {
                Navn = "Tesla",
                Antal = 10,
                HandelsPris = 300m
            };

            AktieHandel? result =
                _repository.Update(999, data);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Get_Kurs_ReturnererKunHandlerOverKurs()
        {
            List<AktieHandel> result =
                _repository.Get(200m, null);

            Assert.AreEqual(2, result.Count);

            Assert.IsTrue(
                result.All(x => x.HandelsPris > 200m));
        }

        [TestMethod]
        public void Get_Navn_ReturnererKunKorrektNavn()
        {
            List<AktieHandel> result =
                _repository.Get(0m, "Tesla");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Tesla", result[0].Navn);
        }

        [TestMethod]
        public void Get_UkendtNavn_ReturnererTomListe()
        {
            List<AktieHandel> result =
                _repository.Get(0m, "Google");

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Get_SortByPris_SortererStigende()
        {
            List<AktieHandel> result =
                _repository.Get(0m, null, "pris");

            Assert.AreEqual(180m, result[0].HandelsPris);
            Assert.AreEqual(250m, result[1].HandelsPris);
            Assert.AreEqual(700m, result[2].HandelsPris);
        }

        [TestMethod]
        public void Get_SortByPrisDesc_SortererFaldende()
        {
            List<AktieHandel> result =
                _repository.Get(0m, null, "pris_desc");

            Assert.AreEqual(700m, result[0].HandelsPris);
            Assert.AreEqual(250m, result[1].HandelsPris);
            Assert.AreEqual(180m, result[2].HandelsPris);
        }

        [TestMethod]
        public void Get_SortByNavn_SortererAlfabetisk()
        {
            List<AktieHandel> result =
                _repository.Get(0m, null, "navn");

            Assert.AreEqual("Apple", result[0].Navn);
            Assert.AreEqual("Novo Nordisk", result[1].Navn);
            Assert.AreEqual("Tesla", result[2].Navn);
        }

        [TestMethod]
        public void Get_SortByIdDesc_SortererFaldende()
        {
            List<AktieHandel> result =
                _repository.Get(0m, null, "id_desc");

            Assert.AreEqual(3, result[0].HandelsId);
            Assert.AreEqual(2, result[1].HandelsId);
            Assert.AreEqual(1, result[2].HandelsId);
        }
    }
}
